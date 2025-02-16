using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement settings")]
    public float moveSpeed = 8f;
    public float sprintSpeed = 12f;
    public float crouchSpeed = 2.5f;
    public float mouseSensitivity = 1.5f;
    public float jumpForce = 5f;
    [Header("WallJump/Wallslide settings")]
    public float wallJumpForce = 12f;
    public float wallJumpBoost = 1.5f;
    public float wallJumpAcceleration = 4f;
    public float wallJumpCooldown = 1f;
    public float wallSlideSpeed = 1f;
    [Header("FOV changing")]
    public float normalFOV = 60f;
    public float sprintFOV = 75f;
    public float crouchFOV = 50f;
    public float fovChangeSpeed = 5f;
    [Header("Uncategorized")]
    public float gravity = 11.5f;
    public float fastFallMultiplier = 2.5f;
    public float crouchHeight = 1f;
    public float standingHeight = 2f;
    public float coyoteTime = 0.1f;
    public float slideSpeed = 12f;
    

    private CharacterController controller;
    private Vector3 moveDirection;
    private float verticalVelocity;
    private Transform cameraTransform;
    private Camera playerCamera;
    private float xRotation = 0f;
    private bool isCrouching = false;
    private bool wallJumpOnCooldown = false;
    private bool isWallSliding = false;
    private float lastGroundedTime;
    private float lastWallJumpTime;
    private float slideDuration = 0.8f;
    private float slideCooldown = 1.0f;
    private float lastSlideTime = -1.0f;
    private bool isSliding = false;
    private float slideTimeLeft = 0f;
    private bool offWall = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        cameraTransform = playerCamera.transform;
        Cursor.lockState = CursorLockMode.Locked;
        playerCamera.fieldOfView = normalFOV;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleCrouch();
        HandleFOV();
        HandleSlide();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) && controller.isGrounded)
        {
            currentSpeed = sprintSpeed;
        }
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        if (isSliding){
            currentSpeed = slideSpeed;
        }
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        moveDirection = move * currentSpeed;


        if (controller.isGrounded)
        {
            lastGroundedTime = Time.time;
            lastWallJumpTime = 0;
            wallJumpOnCooldown = false;
            isWallSliding = false;
            verticalVelocity = -gravity * Time.deltaTime;
            if (Input.GetButtonDown("Jump") && !isCrouching)
            {
                verticalVelocity += jumpForce;
                if (Input.GetKey(KeyCode.LeftShift)) 
                {
                    Vector3 horizontalBoost = transform.forward * (moveSpeed * 0.5f); 
                    moveDirection += horizontalBoost;
                }
            }
        }
        else
        {
            
            HandleWallSlide();
            if (Time.time - lastGroundedTime <= coyoteTime && Input.GetButtonDown("Jump"))
            {
                verticalVelocity = jumpForce;
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                verticalVelocity -= gravity * fastFallMultiplier * Time.deltaTime;
            }
            else if (!isWallSliding)
            {
                verticalVelocity -= gravity * Time.deltaTime;
            }
            HandleWallJump();
        }

        moveDirection.y = verticalVelocity;
        controller.Move(moveDirection * Time.deltaTime);
    }

    void HandleWallSlide()
    {
        RaycastHit hit;
        bool touchingWall = Physics.Raycast(transform.position, transform.right, out hit, 0.8f) ||
                            Physics.Raycast(transform.position, -transform.right, out hit, 0.8f) ||
                            Physics.Raycast(transform.position, transform.forward, out hit, 0.8f);
        
        if (touchingWall && !controller.isGrounded && verticalVelocity < 0)
        {
            isWallSliding = true;
            verticalVelocity = -wallSlideSpeed;
        }
        else
        {
            isWallSliding = false;
        }
    }

    void HandleWallJump()
    {
        RaycastHit hit;
        bool touchingWall = Physics.Raycast(transform.position, transform.right, out hit, 0.8f) ||
                            Physics.Raycast(transform.position, -transform.right, out hit, 0.8f) ||
                            Physics.Raycast(transform.position, transform.forward, out hit, 0.8f);
        if (!touchingWall)
        {
            offWall = true; 
        }

        if (touchingWall && offWall)
        {
            if (Input.GetButtonDown("Jump"))
            {
                Vector3 wallJumpDirection = hit.normal * wallJumpForce + Vector3.up * jumpForce;
                
                moveDirection += hit.normal * wallJumpForce; 
                verticalVelocity = jumpForce; 
                offWall = false; 
                wallJumpOnCooldown = true;
                lastWallJumpTime = Time.time;
            }
        }
    }


    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            controller.height = crouchHeight;
            isCrouching = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            controller.height = standingHeight;
            isCrouching = false;
        }
    }

    void HandleFOV()
    {
        float targetFOV = normalFOV;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            targetFOV = sprintFOV;
        }
        else if (isCrouching)
        {
            targetFOV = crouchFOV;
        }

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
    }
    public void speedPickup(float moveMultiplier){
        moveSpeed += moveMultiplier;
        sprintSpeed += moveMultiplier;
    }
    void HandleSlide()
    {
        if (isSliding)
        {
            slideTimeLeft -= Time.deltaTime;
            if (slideTimeLeft <= 0)
            {
                isSliding = false;
                controller.height = standingHeight; // Restore height after slide
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftShift) && controller.isGrounded && Time.time > lastSlideTime + slideCooldown)
        {
            isSliding = true;
            slideTimeLeft = slideDuration;
            controller.height = crouchHeight; // Lower player collider
            lastSlideTime = Time.time;
            Debug.Log("Sliding!");
        }
    }
}
