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
    Vector3 moveDirection = Vector3.zero;
    public float rotationX = 0;
    // Start is called before the first frame update
    [HideInInspector]
    bool canMove = true;
    Rigidbody rb;

    float moveDirectionY;

    public Vector3 jump;
    public float jumpForce = 2.0f;
    private bool isGoundet = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        jump = new Vector3(0.0f, 2.0f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;

        moveDirection = (transform.forward * curSpeedX * Time.deltaTime) + (transform.right * curSpeedY * Time.deltaTime);

        if (Input.GetKeyUp(KeyCode.Space) && isGoundet)
        {
            Debug.Log("Jump");
            rb.AddForce(transform.up * jumpSpeed * 500 * Time.deltaTime, ForceMode.Impulse);
            isGoundet = false;
        }

        rb.MovePosition(rb.position + moveDirection);
    }

    void OnCollisionEnter(Collision other)
    {
        isGoundet = true;
        if (other.gameObject.tag == "platform")
        {
            var thisTransform = this.gameObject.transform;

            Vector3 axis; 
            float angle;

            axis = Vector3.Cross(-transform.up, -other.transform.up);
            if (axis != Vector3.zero)
            {
                angle = Mathf.Atan2(Vector3.Magnitude(axis), Vector3.Dot(-transform.up, -other.transform.up));
                transform.RotateAround(axis, angle);
            }
            Vector3 gDirection;
            PlatformManager platform = other.gameObject.GetComponent<PlatformManager>();
            if (platform != null && platform.type == PlatformManager.PlatformType.Pull) {
                gDirection = new Quaternion(0.0f, 0.0f, other.gameObject.transform.rotation.z, 1.0f) * Vector3.down;
            } else if (platform != null && platform.type == PlatformManager.PlatformType.Push) {
                gDirection = new Quaternion(0.0f, 0.0f, other.gameObject.transform.rotation.z, 1.0f) * Vector3.up;
            } else {
                gDirection = new Quaternion(0.0f, 0.0f, other.gameObject.transform.rotation.z, 1.0f) * Vector3.down;
            }

            Physics.gravity = gDirection * 9.81f;
        }
    }
}
