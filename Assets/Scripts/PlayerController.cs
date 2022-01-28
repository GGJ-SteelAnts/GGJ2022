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
    CharacterController characterController;
    public float rotationX = 0;
    // Start is called before the first frame update
    [HideInInspector]
    bool canMove = true;
    Rigidbody rb;

    float moveDirectionY;

    public Vector3 jump;
    public float jumpForce = 2.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Jump");
            rb.AddForce(transform.up * jumpSpeed * 500 * Time.deltaTime, ForceMode.Impulse);
        }

        rb.MovePosition(rb.position + moveDirection);

        //characterController.Move(moveDirection * Time.deltaTime);
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "platform")
        {
            this.gameObject.transform.rotation = collision.gameObject.transform.rotation;

            Vector3 direction = collision.gameObject.transform.rotation * Vector3.down;

            Physics.gravity = direction * 9.81f;
            Debug.Log(Physics.gravity);
            //Physics.gravity = -Physics.gravity;
        }
    }
}
