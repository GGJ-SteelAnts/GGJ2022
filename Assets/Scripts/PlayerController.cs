using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxDistanceFromCenterLine;
    [Header("Move")]
    public float speed = 7.5f;
    public float maxSpeed = 15.0f;
    public float minSpeed = 5.0f;
    [HideInInspector]
    public float currentSpeed = 0f;
    private float modifier = 0.0f;
    Vector3 moveDirection = Vector3.zero;
    bool canMove = false;
    Rigidbody rb;
    float moveDirectionY;
    public bool isRunning = false;

    public bool isFalling = false;
    [Header("Jump")]
    public float jumpSpeed = 7.5f;
    [HideInInspector]
    public bool jump = false;
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

    private Vector3 follow = Vector3.zero;

    private GameObject pullObject = null;
    private GameObject pushObject = null;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        platformForward = Vector3.forward;
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

        float curSpeedX = canMove ? (currentSpeed + modifier) : 0;
        float curSpeedY = canMove ? (currentSpeed + 3.5f + modifier) * Input.GetAxis("Horizontal") : 0;

        moveDirection = (platformForward * curSpeedX * Time.deltaTime) + (transform.right * curSpeedY * Time.deltaTime);

        if ((currentSpeed + modifier) >= maxSpeed && !runningParticles.isPlaying)
        {
            runningParticles.Play(true);
        }
        else if ((currentSpeed + modifier) < maxSpeed && runningParticles.isPlaying)
        {
            runningParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jump = true;
            isGrounded = false;
        }

        if (audioSource != null && audioClips.Count > 0) {
            if ((currentSpeed + modifier) > speed * 3f && (currentSpeed + modifier) < speed * 7f)
            {
                if (audioClips.Count > 1 && audioSource.clip != audioClips[1]) {
                    audioSource.Stop();
                    audioSource.clip = audioClips[1];
                    audioSource.Play();
                }
            } else if ((currentSpeed + modifier) <= speed * 3f) {
                if (audioSource.clip != audioClips[0]) {
                    audioSource.Stop();
                    audioSource.clip = audioClips[0];
                    audioSource.Play();
                }
            } else if ((currentSpeed + modifier) >= speed * 7f) {
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
        if (jump)
        {
            // Debug.Log("Jump");
            rb.AddForce(transform.up * jumpSpeed * 100 * Time.deltaTime, ForceMode.Impulse);
            jump = false;
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
            transform.RotateAround(axis, angle * Time.deltaTime * 5f);
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
        if (distanceFromYAxis > maxDistanceFromCenterLine)
        {
            Debug.Log("Player fell out of map.");
            rb.velocity = Vector3.zero;
            Physics.gravity = -Vector3.up * 9.81f;

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
                || (other.GetContact(0).normal != -other.transform.up
                    && other.GetContact(0).normal != other.transform.up
                    && other.GetContact(0).normal != other.transform.right
                    && other.GetContact(0).normal != -other.transform.right)
            )
        {
            return;
        }
        this.downDirection = -other.GetContact(0).normal;
        saveDirection = -other.GetContact(0).normal;

        // TODO: Handle other PlatformTypes
        Physics.gravity = -other.GetContact(0).normal * 9.81f;

    }

    void OnCollisionEnter(Collision other)
    {
        isGrounded = true;
        if (other.gameObject.tag == "platform")
        {

            if (other.GetContact(0).normal == other.transform.forward
                || other.GetContact(0).normal == -other.transform.forward
                || (other.GetContact(0).normal != -other.transform.up
                    && other.GetContact(0).normal != other.transform.up
                    && other.GetContact(0).normal != other.transform.right
                    && other.GetContact(0).normal != -other.transform.right)
            )
            {
                return;
            }

            saveDirection = -other.GetContact(0).normal;

            Vector3 gDirection = -other.GetContact(0).normal;
            PlatformManager platform = other.gameObject.GetComponent<PlatformManager>();
            if (platform != null)
            {
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
                        modifier += platform.speed;
                        break;
                    case PlatformManager.PlatformType.SpeedDown:
                        if (modifier - platform.speed >= 0)
                        {
                            modifier -= platform.speed;
                        }
                        else
                        {
                            modifier = 0.0f;
                        }
                        break;
                    default:
                        gDirection = -other.GetContact(0).normal;
                        break;
                }
            }
            else
            {
                gDirection = -transform.up;
            }
            platformForward = other.transform.forward;
            this.downDirection = gDirection;
            Physics.gravity = gDirection * 9.81f;
        }
    }

}
