using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private AnimationCurve accelerationCurve;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private Transform cameraTransform;

    private Rigidbody _rb;
    private Vector3 _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Get movement input from player
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        _moveInput = AdjustInputToBeRelativeToCamera(new Vector2(horizontalInput, verticalInput));
    }
    
    /// <summary>
    /// Adjusts the input, so that the movement matches input regardless of camera rotation.
    /// </summary>
    /// <param name="moveInput">The player movement input.</param>
    /// <returns>The camera corrected movement input.</returns>
    private Vector3 AdjustInputToBeRelativeToCamera(Vector2 moveInput)
    {
        // Get movement input
        float horizontalInput = moveInput.x;
        float verticalInput = moveInput.y;
            
        // Calculate movement direction relative to the camera
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();
            
        Vector3 movementDirection = (cameraForward * verticalInput) + (cameraRight * horizontalInput);
        return movementDirection;
    } 
    
    private void FixedUpdate()
    {
       
        Move(_moveInput);
        Rotate(_moveInput);
    }
    
    /// <summary>
    /// Rotates based on rotation option.
    /// </summary>
    private void Rotate(Vector3 dir)
    {
        if (dir != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, 10 * Time.deltaTime);
        }
    }

    private void Move(Vector3 dir)
    {
        // Calculate desired velocity based on input and max speed
        Vector3 desiredVelocity = dir * maxSpeed;

        // Calculate current velocity and speed
        Vector3 currentVelocity = _rb.velocity;
        float currentSpeed = currentVelocity.magnitude;

        // Calculate acceleration based on animation curve
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxSpeed);
        float currentAcceleration = acceleration * accelerationCurve.Evaluate(normalizedSpeed);

        // Apply acceleration to reach the desired velocity
        Vector3 velocityChange = desiredVelocity - currentVelocity;
        velocityChange.y = 0f; // Ignore changes in the vertical direction
        _rb.AddForce(velocityChange.normalized * currentAcceleration, ForceMode.Acceleration);

        // Apply deceleration to gradually slow down when there is no input
        if (dir.magnitude < 0.01f)
        {
            Vector3 decelerationForce = -currentVelocity.normalized * deceleration;
            decelerationForce.y = 0f; // Ignore deceleration in the vertical direction
            _rb.AddForce(decelerationForce, ForceMode.Acceleration);
        }

        // Clamp the velocity to the maximum speed
        _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, maxSpeed);
    }
}
