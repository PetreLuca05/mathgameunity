using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.InputSystem; // Add this for the new Input System


public class PlayerMovement : MonoBehaviour
{
    public float followOrientationSpeed;
    public Transform playerModel;
    public Transform Orientation;

    public PhysicsMaterial groundedMaterial;
    public PhysicsMaterial inAirMaterial;

    [Header("Movement")]
    public float moveSpeed;
    public float groundDrag;
    public float velocity;

    [Header("Jumping")]
    public float inAirMovementSpeed;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float inAirGravity;
    bool readyToJump;

    [Header("Ground Check")]
    public Transform GroundCheckTranform;
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;
    float horizontalInput;
    float verticalInput;
    [HideInInspector] public Vector3 moveDirection;
    Rigidbody rb;
    private PlayerInput playerInput;
    [HideInInspector] public InputAction moveAction;
    private InputAction jumpAction;
    private InputAction teleportAction;

    Transform cameraToUseForOrientation;


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            teleportAction = playerInput.actions["D_Teleport"];
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        cameraToUseForOrientation = GetComponent<PlayerManager>().DefaultCamera.transform;
    }

    private void Update()
    {
        grounded = Physics.Raycast(GroundCheckTranform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();
        UpdateMoveDirection();
        UpdatePlayerModelOrientation();

        // Update physics material based on grounded state
        Collider playerCollider = GetComponent<Collider>();
        if (playerCollider != null)
        {
            if (grounded)
                playerCollider.material = groundedMaterial;
            else
                playerCollider.material = inAirMaterial;
        }

        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void OnDisable()
    {
        horizontalInput = 0f;
        verticalInput = 0f;
    }

    private void MyInput()
    {
        // Read movement input from the new Input System
        Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;

        // Jump input (hold to keep jumping)
        if (jumpAction != null && jumpAction.ReadValue<float>() > 0 && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        // Third-person movement relative to camera
        Vector3 camForward = cameraToUseForOrientation.transform.forward;
        Vector3 camRight = cameraToUseForOrientation.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        moveDirection = camForward * verticalInput + camRight * horizontalInput;

        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
            rb.AddForce(Vector3.down * inAirGravity, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float speedLimit = grounded ? moveSpeed : inAirMovementSpeed;

        if (flatVel.magnitude > speedLimit)
        {
            Vector3 limitedVel = flatVel.normalized * speedLimit;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    public void UpdateMoveDirection()
    {
        // Orientation always matches camera forward (y axis only)
        Vector3 forward = cameraToUseForOrientation.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Orientation.forward = forward;
    }

    public void UpdatePlayerModelOrientation()
    {
        Vector3 lookDir = moveDirection;
        lookDir.y = 0f;

        if (lookDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, followOrientationSpeed * Time.deltaTime);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}