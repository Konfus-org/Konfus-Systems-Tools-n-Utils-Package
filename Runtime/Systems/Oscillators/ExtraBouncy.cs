using UnityEngine;

/// <summary>
///     Makes a material feel extra bouncy, for when a bounce co-efficient > 1 is desired.
/// </summary>
public class ExtraBouncy : MonoBehaviour
{
    [SerializeField] 
    private float extraBounceMultiplier = 10f;
    
    [SerializeField] 
    private bool shouldBounceBack = true;
    
    [SerializeField, Tooltip("Applies a force to this rigid body such as to mimic a greater bounciness\n" +
                             "Additionally applies the force to any attached oscillator, such as for squash and stretch")] 
    private Oscillator optionalOscillator;
    
    private Rigidbody _rb;


    /// <summary>
    ///     Define the rigid body.
    /// </summary>
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    ///     Apply the ExtraBounce method for all collisions of this rigid body / collider with another.
    /// </summary>
    /// <param name="collision">The other rigid body / collider involved in this collision.</param>
    private void OnCollisionEnter(Collision collision)
    {
        ExtraBounce(collision);
    }

    private void ExtraBounce(Collision collision)
    {
        Vector3 impulse = collision.impulse;

        float minImp = Mathf.Log(2f);
        float imp = Mathf.Log(impulse.magnitude);

        Vector3 force;

        imp = Mathf.Clamp(imp, minImp, Mathf.Infinity);
        force = collision.GetContact(0).normal * imp / Time.fixedDeltaTime;

        Vector3
            extraBounceForce =
                force * extraBounceMultiplier; // * collision.gameObject.GetComponent<Collider>().material.bounciness;

        _rb.AddForceAtPosition(extraBounceForce, collision.GetContact(0).point);
        if (shouldBounceBack)
            try
            {
                collision.rigidbody.AddForce(-extraBounceForce);
            }
            catch
            {
            }


        if (optionalOscillator != null)
        {
            // Squash and Stretch stuff.
            Vector3 oscillatorForce = optionalOscillator.transform.InverseTransformDirection(extraBounceForce);
            for (int i = 0; i < 3; i++)
                // Make the extraBounceForce applied to the oscillator in the negative direction (should compress first).
                if (oscillatorForce[i] < 0)
                    oscillatorForce[i] *= -1;
            optionalOscillator.ApplyForce(oscillatorForce);
        }
    }
}