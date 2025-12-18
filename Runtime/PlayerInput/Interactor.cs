using System.Linq;
using Konfus.Sensor_Toolkit;
using UnityEngine;
using TMPro;

namespace Konfus.PlayerInput
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField, Tooltip("Optional interaction prompt prefab, on AI this will be empty as they don't require a prompt.")]
        private GameObject interactionPromptPrefab;
        [SerializeField, Tooltip("Optional data display for interactable prefabs.")]
        private GameObject interactableDataPrefab;
        [SerializeField, Tooltip("Primary sensor used to scan for on-input interactables to interact with.")]
        private ScanSensor primarySensor;
        [SerializeField, Tooltip("Secondary sensor used to scan for on-input interactables to interact with.")]
        private ScanSensor secondarySensor;

        [SerializeField] private string promptText = "'E'";
        
        private GameObject _interactionPromptInstance;
        private GameObject _interactableDataInstance;
        private TextMeshProUGUI _interactableDataDisplay;
        private bool _interactableInRange;
        private Transform _currentFirstHit;
        
        public bool TryToInteract()
        {
            if (!_interactableInRange || !_currentFirstHit) return false;
            
            Interactable interactable = _currentFirstHit.gameObject.GetComponent<Interactable>();
            if (!interactable || interactable.interactableType == InteractableType.Touch) return false;
                
            return TryToInteract(interactable);
        }

        private bool TryToInteract(Interactable interactable)
        {
            if (!interactable) return false;

            interactable.Interact(this);
            return true;
        }

        private void Start()
        {
            _interactionPromptInstance = Instantiate(interactionPromptPrefab);
            if (_interactionPromptInstance)
            {
                _interactionPromptInstance.SetActive(false);
                Transform prompt = _interactionPromptInstance.transform.GetChild(0);
                if (prompt)
                {
                    prompt.GetComponent<TextMeshProUGUI>()?.SetText(promptText);
                }
            }
            _interactableDataInstance = Instantiate(interactableDataPrefab);
            if (_interactableDataInstance)
            {
                _interactableDataInstance.SetActive(false);
                Transform display = _interactableDataInstance.transform.GetChild(1);
                if (display)
                {
                    _interactableDataDisplay = display.GetComponent<TextMeshProUGUI>();
                    _interactableDataDisplay.SetText("None Selected");
                }
            }
        }

        private void Update()
        {
            _currentFirstHit = null;
            _interactableInRange = false;
            if (!_interactionPromptInstance || !_interactableDataInstance) return;
            _interactionPromptInstance.SetActive(false);
            _interactableDataInstance.SetActive(false);
        }

        private bool TryScan(ScanSensor scanSensor)
        {
            if (!scanSensor) return false;
            if (scanSensor.Scan())
            {
                _interactableInRange = true;
                if (!_interactionPromptInstance) return true;
                Transform foundInteractable = scanSensor.hits.First().gameObject.transform;
                Interactable interactable = foundInteractable.GetComponent<Interactable>();
                //no prompt if interactable doesn't use input interactions
                if (interactable)
                {
                    Vector3 camPosition = Camera.main.transform.position;
                    
                    SetInteractableDataDisplay(interactable.interactableName);
                    _interactableDataInstance.transform.position =
                        foundInteractable.position + new Vector3(0, 1, 1);
                    _interactableDataInstance.transform.LookAt(camPosition);

                    if (interactable.interactableType != InteractableType.Touch)
                    {
                        _interactionPromptInstance.transform.position =
                            foundInteractable.position + Vector3.up;
                        _interactionPromptInstance.transform.LookAt(camPosition);
                        _interactionPromptInstance.SetActive(true);
                        _currentFirstHit = foundInteractable;
                    }
                }

                return true;
            }

            return false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null) return;
            
            Interactable interactable = collision.transform.GetComponent<Interactable>();
            //donut allow touch interactions if interaction type is input only
            if (!interactable || interactable.interactableType == InteractableType.Input) return;
            
            TryToInteract(interactable);
            
        }

        private void SetInteractableDataDisplay(string data)
        {
            if (_interactableDataDisplay)
            {
                _interactableDataDisplay.text = data;
            }
            _interactableDataInstance.SetActive(true);
        }
    }
}