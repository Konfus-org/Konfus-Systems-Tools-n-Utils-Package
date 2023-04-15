using Konfus.Utility.Extensions;
using UnityEngine;

public class MagnifyingGlass : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("Sets the amount the magnifying glass magnifies things")] 
    private float magnification = 5;
    
    [Header("Dependencies")]
    [SerializeField] 
    private Camera mainCamera;
    [SerializeField]
    private Camera magnifyingCamera;

    /// <summary>
    /// Sets the amount the magnifying glass magnifies things.
    /// </summary>
    /// <param name="zoom"></param>
    private void SetMagnification(float amount)
    {
        magnification = amount;
    }

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Update()
    {
        // Set camera pos to main camera pos so we always start at the same magnification as the main camera...
        magnifyingCamera.transform.position = mainCamera.transform.position;
        
        // Look at magnifying glass position and update magnification level
        magnifyingCamera.transform.Face(transform.position, magnifyingCamera.transform.up);
        magnifyingCamera.fieldOfView = mainCamera.fieldOfView - magnification;
    }
}
