using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSliding : MonoBehaviour
{


    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody playerRb;
    private PlayerBasicMovement playerMovement;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float StartYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;


    private void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerBasicMovement>();

        StartYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // If slide key is pressed and player moves
        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
            StartSlide();
        if (Input.GetKeyUp(slideKey) && playerMovement.sliding)
            StopSlide();
    }

    private void FixedUpdate() 
    {
        if (playerMovement.sliding)
            SlidingMovement();
    }
    private void StartSlide()
    {
        playerMovement.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        playerRb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    
        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Sliding normal
        if (!playerMovement.OnSlope() || playerRb.velocity.y > -0.1f)
        {
            playerRb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        // Sliding down a slope
        else
        {
            playerRb.AddForce(playerMovement.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
            StopSlide();
    }
    private void StopSlide()
    {
        playerMovement.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, StartYScale, playerObj.localScale.z);
    }
}
