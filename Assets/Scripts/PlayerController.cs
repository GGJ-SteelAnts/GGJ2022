using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxDistanceFromCenterLine = 30.0f;
    [Header("Move")]
    public float speed = 7.5f;
    public float maxSpeed = 15.0f;
    public float minSpeed = 5.0f;
    [HideInInspector]
    public float currentSpeed = 0f;
    [HideInInspector]
    public float speedModifier = 0.0f;
    Vector3 moveDirection = Vector3.zero;
    bool canMove = false;
    [HideInInspector]
    public Rigidbody rb;
    [HideInInspector]
    public CharacterController chc;
    float moveDirectionY;
    public bool isRunning = false;

    public bool isFalling = false;
    [Header("Jump")]
    public float jumpSpeed = 7.5f;
    [HideInInspector]
    public float jumpModifier = 0.0f;
    [HideInInspector]
    public bool inAir = false;
    private bool isGrounded = false;
    private float jumpTimer = 0.1f;
    private float jumpTime = 0f;
    [Header("Camera")]
    public float lookSpeed = 7.5f;
    public float lookXLimit = 40.0f;
    public Camera playerCamera;
    public float rotationX = 0;
    [Header("Others")]
    public ParticleSystem runningParticles;
    public Transform mainObject;
    public AudioSource audioSource;
    public List<AudioClip> jumpClips = new List<AudioClip>();
    public List<AudioClip> audioClips = new List<AudioClip>();
    PlatformManager collidePlatform;
    string platformStatus = "exit";
    // Start is called before the first frame update

    [HideInInspector]
    public Vector3 saveDirection;
    private Vector3 downDirection;
    Vector3 _velocity = Vector3.zero;

    private Vector3 follow = Vector3.zero;

    private GameObject pullObject = null;
    private GameObject pushObject = null;

    void Start()
    {
        //rb = GetComponent<Rigidbody>();
        chc = GetComponent<CharacterController>();
    }

    public void disableCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (UiController.isInMenu) return;
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        isRunning = Input.GetKey(KeyCode.LeftShift);
        if (canMove && currentSpeed < minSpeed)
        {
            currentSpeed += 0.1f;
        }

        if (Input.GetAxis("Vertical") > 0 && currentSpeed < maxSpeed)
        {
            if (!canMove)
            {
                canMove = true;
            }
            currentSpeed += 0.01f;
        }
        else if (Input.GetAxis("Vertical") < 0 && currentSpeed > minSpeed)
        {
            currentSpeed -= 0.01f;
        }

        float curSpeedX = canMove ? (currentSpeed + speedModifier): 0;
        float curSpeedY = canMove ? speed * Input.GetAxis("Horizontal") : 0;

        moveDirection = (transform.forward * curSpeedX * Time.deltaTime) + (transform.right * curSpeedY * Time.deltaTime);

        if ((currentSpeed + speedModifier) >= maxSpeed && !runningParticles.isPlaying)
        {
            runningParticles.Play(true);
        }
        else if ((currentSpeed + speedModifier) < maxSpeed && runningParticles.isPlaying)
        {
            runningParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (audioSource != null && audioClips.Count > 0)
        {
            if ((currentSpeed + speedModifier) > speed * 3f && (currentSpeed + speedModifier) < speed * 6f)
            {
                if (audioClips.Count > 1 && audioSource.clip != audioClips[1])
                {
                    audioSource.Stop();
                    audioSource.clip = audioClips[1];
                    audioSource.Play();
                }
            }
            else if ((currentSpeed + speedModifier) <= speed * 3f)
            {
                if (audioSource.clip != audioClips[0])
                {
                    audioSource.Stop();
                    audioSource.clip = audioClips[0];
                    audioSource.Play();
                }
            }
            else if ((currentSpeed + speedModifier) >= speed * 6f)
            {
                if (audioClips.Count > 2 && audioSource.clip != audioClips[2])
                {
                    audioSource.Stop();
                    audioSource.clip = audioClips[2];
                    audioSource.Play();
                }
            }
        }

        PlatformManager localP = null;
        if (collidePlatform != null)
        {
            collidePlatform.Step();
            collidePlatform.Action(this, platformStatus);
            localP = collidePlatform;
            if (platformStatus == "enter")
            {
                platformStatus = "stay";
                collidePlatform = null;
            }
        }
        else
        {
            if (platformStatus == "stay")
            {
                platformStatus = "exit";
                if (localP != null)
                {
                    localP.Action(this, platformStatus);
                    localP = null;
                }
            }
        }

        if (jumpTime <= Time.time)
        {
            jumpModifier = 0.0f;
            Vector3 locGrav = (Physics.gravity * Time.deltaTime);
            if (!isGrounded)
            {
                _velocity += Physics.gravity / 50 * Time.deltaTime;
            }
            else
            {
                _velocity = Physics.gravity * Time.deltaTime;
            }
        }
        else
        {
            _velocity += transform.up * (jumpSpeed + jumpModifier) / 5 * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            //inAir = true;
            if (audioSource != null && jumpClips.Count > 0)
            {
                audioSource.PlayOneShot(jumpClips[Random.Range(0, jumpClips.Count)]);
            }
            _velocity = Vector3.zero;
            jumpTime = jumpTimer + Time.time;
            isGrounded = false;
        }

        chc.Move(moveDirection + _velocity);

        //rb.MovePosition(rb.position + moveDirection);
    }

    private void FixedUpdate()
    {
        downDirection = -transform.up;
        if (Physics.gravity != (downDirection * 9.81f)) {
            if (downDirection == Vector3.forward)
            {
                downDirection = Vector3.down;
            }
            Physics.gravity = downDirection * 9.81f;
        }

        /*if (inAir)
        {
            // Debug.Log("Jump");
            if (audioSource != null && jumpClips.Count > 0)
            {
                audioSource.PlayOneShot(jumpClips[Random.Range(0, jumpClips.Count)]);
            }
            chc.Move(transform.up * jumpSpeed * 100f * Time.deltaTime);
            //rb.AddForce(transform.up * jumpSpeed * 100f * Time.deltaTime, ForceMode.Impulse);
            inAir = false;
        }*/

        if (Vector3.Distance(transform.position, new Vector3(0f, 0f, transform.position.z)) > 10f)
        {
            // Debug.Log("Player is falling :)");
            this.isFalling = true;
        }
        else
        {
            this.isFalling = false;
        }

        if (saveDirection != -transform.up)
        {
            Vector3 axis;
            float angle;
            axis = Vector3.Cross(-transform.up, saveDirection);

            angle = Mathf.Atan2(Vector3.Magnitude(axis), Vector3.Dot(-transform.up, saveDirection));
            transform.RotateAround(axis, angle * Time.deltaTime * 8f);
        }

        var distanceFromYAxis = new Vector2(transform.position.x, transform.position.y).magnitude;
        if (UiController.isInMenu == false && distanceFromYAxis > this.maxDistanceFromCenterLine)
        {
            Debug.Log("Player fell out of map.");
            //rb.velocity = Vector3.zero;
            UiController.isInMenu = true;
            Physics.gravity = Vector3.down * 9.81f;
            UiController.SaveGame();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    /*void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag != "platform") return;

        PlatformManager platform = other.gameObject.GetComponent<PlatformManager>();

        if (platform == null) return;

        platform.Step();
        platform.Action(this, "exit");
    }

    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag != "platform") return;

        Vector3 contact = other.GetContact(other.contacts.Length - 1).normal;
        if (contact != -other.transform.up && contact != other.transform.up) {
            return;
        }

        saveDirection = -contact;
    }

    void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collide :D");
        isGrounded = true;
        if (other.gameObject.tag != "platform") return;

        PlatformManager platform = other.gameObject.GetComponent<PlatformManager>();

        if (platform != null)
        {
            platform.Step();
            platform.Action(this, "enter");
        }

        Vector3 contact = other.GetContact(other.contacts.Length - 1).normal;
        if (contact != -other.transform.up &&
            contact != other.transform.up) {
            return;
        }

        saveDirection = -contact;
    } */

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        isGrounded = true;
        if (platformStatus == "exit") {
            platformStatus = "enter";
        }
        if (hit.gameObject.tag != "platform") return;

        collidePlatform = hit.gameObject.GetComponent<PlatformManager>();

        Vector3 contact = hit.normal;
        if (contact != -hit.gameObject.transform.up &&
            contact != hit.gameObject.transform.up)
        {
            return;
        }

        saveDirection = -contact;
    }

}
