using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public float movementSpeed = 12f;
    public float jumpHeight = 3f;
    public float playerGravity = -9.81f;
    public Vector2 playerCollide = new Vector2(1f, -2.5f);
    public LayerMask groundMask;
    public Transform playerBody;
    public Transform playerCamera;

    public CharacterController characterController;
    public bool drawGizmos = false;
    public Color playerGroundGizmoColor = Color.red;

    private float yRotation = 0f;
    public Vector3 playerVelocity;
    public bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        ApplyGravity();
        MouseLook();
        MovementControls();

        AuxiliaryControls();
    }

    public void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation -= mouseY;
        yRotation = Mathf.Clamp(yRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up, mouseX);
    }

    public void MovementControls()
    {
        float xMove = Input.GetAxis("Horizontal");
        float zMove = Input.GetAxis("Vertical");

        Vector3 move = playerBody.transform.right * xMove + playerBody.transform.forward * zMove;

        characterController.Move(move * movementSpeed * Time.deltaTime);

        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * playerGravity);
        }
    }

    public void AuxiliaryControls()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    public void ApplyGravity()
    {
        isGrounded = Physics.CheckBox(new Vector3(playerBody.transform.position.x, playerBody.transform.position.y + playerCollide.y, playerBody.transform.position.z), new Vector3(playerCollide.x, 0.1f, playerCollide.x), Quaternion.identity, groundMask);

        if(isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        if(!isGrounded)
        {
            playerVelocity.y += playerGravity * Time.deltaTime;
            if (playerVelocity.y > 0)
            {
                if (Physics.CheckBox(new Vector3(playerBody.transform.position.x, playerBody.transform.position.y - playerCollide.y, playerBody.transform.position.z), new Vector3(playerCollide.x, 0.1f, playerCollide.x), playerBody.rotation, groundMask))
                {
                    playerVelocity.y = -(playerVelocity.y / 3);
                }
            }
        }

        characterController.Move(playerVelocity * Time.deltaTime);
    }

    public void OnDrawGizmos()
    {
        if(drawGizmos)
        {
            Gizmos.color = playerGroundGizmoColor;
            Gizmos.DrawCube(new Vector3(playerBody.transform.position.x, playerBody.transform.position.y + playerCollide.y, playerBody.transform.position.z), new Vector3(playerCollide.x * 2, 0.1f, playerCollide.x * 2));
            Gizmos.DrawCube(new Vector3(playerBody.transform.position.x, playerBody.transform.position.y - playerCollide.y, playerBody.transform.position.z), new Vector3(playerCollide.x * 2, 0.1f, playerCollide.x * 2));
        }
    }
}
