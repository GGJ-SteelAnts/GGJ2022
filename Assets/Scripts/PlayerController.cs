using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 7.5f;
    public float runningSpeed = 7.5f;
    public float jumpSpeed = 7.5f;
    public float lookSpeed = 7.5f;
    public float lookXLimit = 7.5f;
    public Camera playerCamera;
    public ParticleSystem runningParticles;
    Vector3 moveDirection = Vector3.zero;
    public float rotationX = 0;
    public Transform mainObject;
    // Start is called before the first frame update
    [HideInInspector]
    bool canMove = true;
    Rigidbody rb;
    float moveDirectionY;
    public bool jump = false;
    private bool isGrounded = false;
    public bool isRunning = false;

    private Vector3 saveDirection;

    private Vector3 downDirection;

    private Vector3 follow = Vector3.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;

        moveDirection = (transform.forward * curSpeedX * Time.deltaTime) + (transform.right * curSpeedY * Time.deltaTime);

        if (isRunning && !runningParticles.isPlaying && Input.GetKey(KeyCode.W))
        {
            runningParticles.Play(true);
        }
        else if ((!isRunning && runningParticles.isPlaying) || !Input.GetKey(KeyCode.W))
        {
            runningParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jump = true;
            isGrounded = false;
        }

        rb.MovePosition(rb.position + moveDirection);
    }

    private void FixedUpdate()
    {
        if (jump)
        {
            Debug.Log("Jump");
            rb.AddForce(transform.up * jumpSpeed * 100 * Time.deltaTime, ForceMode.Impulse);
            jump = false;
        }

        if (saveDirection != Vector3.zero)
        {
            Vector3 axis;
            float angle;
            axis = Vector3.Cross(-transform.up, saveDirection);

            angle = Mathf.Atan2(Vector3.Magnitude(axis), Vector3.Dot(-transform.up, saveDirection));
            transform.RotateAround(axis, angle * Time.deltaTime * 5f);
        }
    }

    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == "platform")
        {
            Physics.gravity = this.downDirection * 9.81f;
        }
    }

    void OnCollisionStay(Collision other) {
        if (other.gameObject.tag != "platform") return;
        PlatformManager platform = other.gameObject.GetComponent<PlatformManager>();
        if (platform == null)
        {
            // FIXME: Should platforms be allowed to not to have a PlatformManager?
            // Debug.Log("ERROR");
            return;
        }

        if (other.GetContact(0).normal == other.transform.forward || other.GetContact(0).normal == -other.transform.forward)
        {
            return;
        }

        // TODO: Handle other PlatformTypes
        Physics.gravity = -other.GetContact(0).normal * 9.81f;

    }

    void OnCollisionEnter(Collision other)
    {
        isGrounded = true;
        if (other.gameObject.tag == "platform")
        {

            if (other.GetContact(0).normal == other.transform.forward || other.GetContact(0).normal == -other.transform.forward)
            {
                return;
            }

            saveDirection = -other.GetContact(0).normal;

            Vector3 gDirection;
            PlatformManager platform = other.gameObject.GetComponent<PlatformManager>();
            gDirection = -transform.up;
            if (platform == null)
            {
                // FIXME: remove
                this.downDirection = -transform.up;
                return;
            }
            if (platform.type == PlatformManager.PlatformType.Pull)
            {
                gDirection = -other.GetContact(0).normal;
            }
            else if (platform.type == PlatformManager.PlatformType.Push)
            {
                gDirection = -other.GetContact(0).normal;
            }
            else if ((platform.type == PlatformManager.PlatformType.RotateY || platform.type == PlatformManager.PlatformType.RotateZ))
            {
                gDirection = -other.GetContact(0).normal;
            }
            else if (platform.type == PlatformManager.PlatformType.SpeedUp)
            {
                rb.AddForce(other.transform.forward * platform.speed * Time.deltaTime, ForceMode.Impulse);
                gDirection = -other.GetContact(0).normal;
            }
            else if (platform.type == PlatformManager.PlatformType.SpeedDown)
            {
                rb.AddForce(other.transform.forward * platform.speed * 10 * Time.deltaTime, ForceMode.Impulse);
                gDirection = -other.GetContact(0).normal;
            }
            this.downDirection = gDirection;

            Physics.gravity = gDirection * 9.81f;
        }
    }

}
