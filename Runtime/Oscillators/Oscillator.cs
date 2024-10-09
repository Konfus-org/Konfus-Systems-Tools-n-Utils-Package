using UnityEngine;

/// <summary>
///     A dampened oscillator using the objects transform local position.
/// </summary>
[DisallowMultipleComponent]
public class Oscillator : MonoBehaviour
{
    [Header("Settings:")]
    [Tooltip("The local position about which oscillations are centered."), SerializeField]
    private Vector3 localEquilibriumPosition = Vector3.zero;

    [Tooltip("The axes over which the oscillator applies force. Within range [0, 1]."), SerializeField]
    private Vector3 forceScale = Vector3.one;

    [Tooltip("The greater the stiffness constant, the lesser the amplitude of oscillations."), SerializeField]
    private float stiffness = 100f;

    [Tooltip("The greater the damper constant, the faster that oscillations will dissapear."), SerializeField]
    private float damper = 2f;

    [Tooltip("The greater the mass, the lesser the amplitude of oscillations."), SerializeField]
    private float mass = 1f;
    
    [Header("Debug:"), SerializeField]
    private bool drawDebugVisualization;
    
    private Vector3 _previousDisplacement = Vector3.zero;
    private Vector3 _previousVelocity = Vector3.zero;
    private Rigidbody _rb;

    public Vector3 ForceScale
    {
        get => forceScale; 
        internal set => forceScale = value;
    }
    
    public Vector3 LocalEquilibriumPosition => localEquilibriumPosition;
    public float Stiffness => stiffness;
    public float Mass => mass;
    public bool DrawDebugVisualization => drawDebugVisualization;

    /// <summary>
    ///     Adds a force to the oscillator. Updates the transform's local position.
    /// </summary>
    /// <param name="force">The force to be applied.</param>
    public void ApplyForce(Vector3 force)
    {
        if (_rb != null)
        {
            _rb.AddForce(Vector3.Scale(force, forceScale));
        }
        else
        {
            Vector3 displacement = CalculateDisplacementDueToForce(force);
            transform.localPosition += Vector3.Scale(displacement, forceScale);
        }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    ///     Update the position of the oscillator, by calculating and applying the restorative force.
    /// </summary>
    private void FixedUpdate()
    {
        Vector3 restoringForce = CalculateRestoringForce();
        ApplyForce(restoringForce);
    }

    /// <summary>
    ///     Returns the displacement that results from applying a force over a single fixed update.
    /// </summary>
    /// <param name="force">The causative force.</param>
    /// <returns>Displacement over a single fixed update.</returns>
    private Vector3 CalculateDisplacementDueToForce(Vector3 force)
    {
        Vector3 acceleration = force / mass; // Newton's second law.
        Vector3 deltaVelocity = acceleration * Time.fixedDeltaTime; // Kinematics. Acceleration is the change in velocity over time.
        Vector3 velocity = deltaVelocity + _previousVelocity; // Calculating the updated velocity.
        _previousVelocity = velocity;
        Vector3 displacement = velocity * Time.fixedDeltaTime; // Kinematics. Velocity is the change-in-position over time.
        return displacement;
    }

    /// <summary>
    ///     Returns the damped restorative force of the oscillator.
    ///     The magnitude of the restorative force is 0 at the equilibrium position and maximum at the amplitude of the
    ///     oscillation.
    /// </summary>
    /// <returns>Damped restorative force of the oscillator.</returns>
    private Vector3 CalculateRestoringForce()
    {
        Vector3 displacement = transform.localPosition - localEquilibriumPosition; // Displacement from the rest point. Displacement is the difference in position.
        Vector3 deltaDisplacement = displacement - _previousDisplacement;
        _previousDisplacement = displacement;
        Vector3 velocity = deltaDisplacement / Time.fixedDeltaTime; // Kinematics. Velocity is the change-in-position over time.
        Vector3 force = HookesLaw(displacement, velocity);
        return force;
    }

    /// <summary>
    ///     Returns the damped Hooke's force for a given displacement and velocity.
    /// </summary>
    /// <param name="displacement">The displacement of the oscillator from the equilibrium position.</param>
    /// <param name="velocity">The local velocity of the oscillator.</param>
    /// <returns>Damped Hooke's force</returns>
    private Vector3 HookesLaw(Vector3 displacement, Vector3 velocity)
    {
        Vector3 force = stiffness * displacement + damper * velocity; // Damped Hooke's law
        force = -force; // Take the negative of the force, since the force is restorative (attractive)
        return force;
    }
}