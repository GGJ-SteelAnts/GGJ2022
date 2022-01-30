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
    private float speedModifier = 0.0f;
    Vector3 moveDirection = Vector3.zero;
    bool canMove = false;
    Rigidbody rb;
    float moveDirectionY;
    public bool isRunning = false;

    public bool isFalling = false;
    [Header("Jump")]
    public float jumpSpeed = 7.5f;
    [HideInInspector]
    public bool inAir = false;
    private bool isGrounded = false;
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
    // Start is called before the first frame update

    private Vector3 saveDirection;
    private Vector3 downDirection;
    private Vector3 platformForward;
    private Vector3 platformRight;

    private Vector3 follow = Vector3.zero;

    private GameObject pullObject = null;
    private GameObject pushObject = null;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        platformForward = Vector3.forward;
        platformRight = Vector3.right;
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
            currentSpeed += 0.005f;
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
        float curSpeedY = canMove ? (currentSpeed + speedModifier) * Input.GetAxis("Horizontal") : 0;

        moveDirection = (transform.forward * curSpeedX * Time.deltaTime) + (transform.right * curSpeedY * Time.deltaTime);

        if ((currentSpeed + speedModifier) >= maxSpeed && !runningParticles.isPlaying)
        {
            runningParticles.Play(true);
        }
        else if ((currentSpeed + speedModifier) < maxSpeed && runningParticles.isPlaying)
        {
            runningParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            inAir = true;
            isGrounded = false;
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

        rb.MovePosition(rb.position + moveDirection);
    }

    private void FixedUpdate()
    {
        if (inAir)
        {
            // Debug.Log("Jump");
            if (audioSource != null && jumpClips.Count > 0)
            {
                audioSource.PlayOneShot(jumpClips[Random.Range(0, jumpClips.Count)]);
            }
            rb.AddForce(transform.up * jumpSpeed * 100f * Time.deltaTime, ForceMode.Impulse);
            inAir = false;
        }

        if (Vector3.Distance(transform.position, new Vector3(0f, 0f, transform.position.z)) > 10f)
        {
            // Debug.Log("Player is falling :)");
            this.isFalling = true;
        }
        else
        {
            this.isFalling = false;
        }

        if (saveDirection != Vector3.zero)
        {
            Vector3 axis;
            float angle;
            axis = Vector3.Cross(-transform.up, saveDirection);

            angle = Mathf.Atan2(Vector3.Magnitude(axis), Vector3.Dot(-transform.up, saveDirection));
            transform.RotateAround(axis, angle * Time.deltaTime * 8f);
        }

        var distanceFromYAxis = new Vector2(rb.position.x, rb.position.y).magnitude;
        if (distanceFromYAxis > this.maxDistanceFromCenterLine)
        {
            Debug.Log("Player fell out of map.");
            rb.velocity = Vector3.zero;
            Physics.gravity = -Vector3.up * 9.81f;
            UiController.SaveGame();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == "platform")
        {
            Physics.gravity = this.downDirection * 9.81f;

            PlatformManager platform = other.gameObject.GetComponent<PlatformManager>();

            if (platform != null)
            {
                platform.Step();
                float step = platform.speed * Time.deltaTime * 10f;
                switch (platform.type)
                {
                    case PlatformManager.PlatformType.Push:
                        rb.AddForce(-(other.transform.position - transform.position) * step, ForceMode.Impulse);
                        break;
                    case PlatformManager.PlatformType.Pull:
                        rb.AddForce((other.transform.position - transform.position) * step, ForceMode.Impulse);
                        break;
                }
            }
        }
    }

    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag != "platform") return;
        PlatformManager platform = other.gameObject.GetComponent<PlatformManager>();
        if (platform == null)
        {
            // FIXME: Should platforms be allowed to not to have a PlatformManager?
            // Debug.Log("ERROR");
            return;
        }

        if (other.GetContact(0).normal == other.transform.forward
                || other.GetContact(0).normal == -other.transform.forward
                || other.GetContact(0).normal == -other.transform.right
                || other.GetContact(0).normal == other.transform.right
                || (other.GetContact(0).normal != -other.transform.up
                    && other.GetContact(0).normal != other.transform.up)
            )
        {
            return;
        }
        this.downDirection = -transform.up;
        saveDirection = -other.GetContact(0).normal;

        // TODO: Handle other PlatformTypes
        Physics.gravity = this.downDirection * 9.81f;

    }

    void OnCollisionEnter(Collision other)
    {
        isGrounded = true;
        if (other.gameObject.tag == "platform")
        {
            PlatformManager platform = other.gameObject.GetComponent<PlatformManager>();
            if (platform != null)
            {
                platform.Step();
                switch (platform.type)
                {
                    case PlatformManager.PlatformType.Push:
                        break;
                    case PlatformManager.PlatformType.Pull:
                        break;
                    case PlatformManager.PlatformType.RotateY:
                        break;
                    case PlatformManager.PlatformType.RotateZ:
                        break;
                    case PlatformManager.PlatformType.SpeedUp:
                        speedModifier += platform.speed;
                        break;
                    case PlatformManager.PlatformType.SpeedDown:
                        if (speedModifier - platform.speed >= 0)
                        {
                            speedModifier -= platform.speed;
                        }
                        else
                        {
                            speedModifier = 0.0f;
                        }
                        break;
                }
            }
            if (other.GetContact(0).normal == other.transform.forward
                || other.GetContact(0).normal == -other.transform.forward
                || other.GetContact(0).normal == -other.transform.right
                || other.GetContact(0).normal == other.transform.right
                || (other.GetContact(0).normal != -other.transform.up
                    && other.GetContact(0).normal != other.transform.up)
            )
            {
                return;
            }
            saveDirection = -other.GetContact(0).normal;
            Vector3 gDirection = -transform.up;
            if (platform == null)
            {
                gDirection = -transform.up;
            }
            platformForward = other.transform.forward;
            platformRight = other.transform.right;
            this.downDirection = gDirection;
            Physics.gravity = gDirection * 9.81f;
        }
    }

}
