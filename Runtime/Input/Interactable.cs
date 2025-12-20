using System;
using UnityEngine;
using UnityEngine.Events;

namespace Konfus.Input
{
    public class Interactable : MonoBehaviour
    {
        [Header("Events")]
        public InteractionEvent onInteract;
        
        [Header("Settings")]
        [SerializeField, Tooltip("Name of the interactable for display purposes")]
        public string interactableName;
        [SerializeField]
        public InteractableType interactableType;
        
        public Interactor LastInteractor { get; private set; }
        
        public void Interact(Interactor interactor)
        {
            LastInteractor = interactor;
            onInteract.Invoke(interactor);
            
        }
    }

    public enum InteractableType
    {
        Input, Touch, Hybrid
    }
    
    /// <summary>
    /// Event fired on interaction with an interactable, event carries the <see cref="Interactor"/> that triggered the interaction
    /// </summary>
    [Serializable]
    public class InteractionEvent : UnityEvent<Interactor>
    {
    }
}