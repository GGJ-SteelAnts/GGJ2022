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
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        isRunning = Input.GetKey(KeyCode.LeftShift);
        if (canMove && currentSpeed < minSpeed)
        {
            currentSpeed += 0.0005f;
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

        float curSpeedX = canMove ? (currentSpeed + speedModifier) : 0;
        float curSpeedY = canMove ? (currentSpeed + currentSpeed + speedModifier) * Input.GetAxis("Horizontal") : 0;

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

        if (audioSource != null && audioClips.Count > 0) {
            if ((currentSpeed + speedModifier) > speed * 3f && (currentSpeed + speedModifier) < speed * 6f)
            {
                if (audioClips.Count > 1 && audioSource.clip != audioClips[1]) {
                    audioSource.Stop();
                    audioSource.clip = audioClips[1];
                    audioSource.Play();
                }
            } else if ((currentSpeed + speedModifier) <= speed * 3f) {
                if (audioSource.clip != audioClips[0]) {
                    audioSource.Stop();
                    audioSource.clip = audioClips[0];
                    audioSource.Play();
                }
            } else if ((currentSpeed + speedModifier) >= speed * 6f) {
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
            rb.AddForce(transform.up * jumpSpeed * 100f * Time.deltaTime, ForceMode.Impulse);
            inAir = false;
        }

        if (rb.velocity.magnitude != 0 && Vector3.Dot(rb.velocity.normalized, Physics.gravity.normalized) > 0)
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

        if (pullObject != null)
        {
            PlatformManager platform = pullObject.GetComponent<PlatformManager>();
            if (platform != null)
            {
                float step = platform.speed * Time.deltaTime * 10f;
                rb.AddForce((pullObject.transform.position - transform.position) * step, ForceMode.Force);
            }
            if (Vector3.Distance(pullObject.transform.position, transform.position) > 5f)
            {
                pullObject = null;
            }
        }

        if (pushObject != null)
        {
            PlatformManager platform = pushObject.GetComponent<PlatformManager>();
            if (platform != null)
            {
                float step = platform.speed * Time.deltaTime * 10f;
                rb.AddForce(-(pushObject.transform.position - transform.position) * step, ForceMode.Force);
            }
            if (Vector3.Distance(pushObject.transform.position, transform.position) > 5f)
            {
                pushObject = null;
            }
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
        Physics.gravity = -other.GetContact(0).normal * 9.81f;

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
                        pushObject = other.gameObject;
                        pullObject = null;
                        break;
                    case PlatformManager.PlatformType.Pull:
                        pullObject = other.gameObject;
                        pushObject = null;
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

            Vector3 gDirection = -other.GetContact(0).normal;
            saveDirection = gDirection;
            if (platform == null)
            {
                gDirection = -other.GetContact(0).normal;
            }
            platformForward = other.transform.forward;
            platformRight = other.transform.right;
            this.downDirection = gDirection;
            Physics.gravity = gDirection * 9.81f;
        }
    }

}
