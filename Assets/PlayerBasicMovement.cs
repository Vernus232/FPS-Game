using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBasicMovement : MonoBehaviour
{
    [Header("Basic")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;

    [Header("Advanced")]
    public float groundDrag;
    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;
    

    private float desiredMoveSpeed;
    private float lastdesiredMoveSpeed;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool readyToJump;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whaitIsGround;
    private bool grounded;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;


    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode SprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [SerializeField] private Transform orientation;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody playerRb;
    [HideInInspector] public MovementState state;
    [HideInInspector] public bool sliding;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        air
    }

#region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerRb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        GroundCheck();
        GetInput();
        SpeedControl();
        StateHandler();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }
#endregion
    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // When to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Start crouch
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3 (transform.localScale.x, crouchYScale, transform.localScale.z);
            playerRb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // Stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3 (transform.localScale.x, startYScale, transform.localScale.z);
        }
    }
    private void MovePlayer()
    {
        // Calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // On slope
        if (OnSlope() && !exitingSlope)
        {
            playerRb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);
            if (playerRb.velocity.y > 0)
                playerRb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // On ground
        else if (grounded)
            playerRb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // In air
        else if (!grounded)
            playerRb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        
        // Turn gravity off while on slope
        playerRb.useGravity = !OnSlope();
    }
    private void GroundCheck()
    {
        // Raycast into ground
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whaitIsGround);
        if (grounded)
        playerRb.drag = groundDrag;
        else
        playerRb.drag = 0;
    }
    private void SpeedControl()
    {
        // Limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (playerRb.velocity.magnitude > moveSpeed)
                playerRb.velocity = playerRb.velocity.normalized * moveSpeed;
        }

        // Limiting speed on ground or in air
        else
        {
            Vector3 flatVelocity = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z);
            
            // Limit velocity if needed
            if (flatVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity  = flatVelocity.normalized * moveSpeed;
                playerRb.velocity = new Vector3(limitedVelocity.x, playerRb.velocity.y, limitedVelocity.z);
            }
        }
    }
    private void Jump()
    {
        exitingSlope = true;

        // Reset y velocity
        playerRb.velocity = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z);

        playerRb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }
    private void StateHandler()
    {
        // For sliding
        if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && playerRb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }

        // For crouching
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // For sprinting
        else if(grounded && Input.GetKey(SprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        // For walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // For flying
        else
        {
            state = MovementState.air;
        }

        // Check if desiredMoveSpeed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastdesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastdesiredMoveSpeed = desiredMoveSpeed;
    }
    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // Smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }
}
