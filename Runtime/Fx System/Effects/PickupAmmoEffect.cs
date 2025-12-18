using System;
using Armored_Felines.Fireables;
using Konfus.Systems.Fx_System.Effects;
using Armored_Felines.Input;
using Armored_Felines.Tanks;
using UnityEngine;

namespace Armored_Felines.Effects
{
    [Serializable]
    public class PickupAmmoEffect : Effect
    {
        [SerializeField, Tooltip("Type of ammo pickup")]
        private Fireable ammo;
        [SerializeField, Tooltip("Amount of ammo included")]
        private float ammoAmount;
        
        private GameObject _go;
        private Interactable _interactable;
        private bool _processingInteraction = false;

        public Fireable Ammo => ammo;
        public float AmmoAmount => ammoAmount;

        public override float Duration => 0;
        
        public override void Initialize(GameObject parentGo)
        {
            _go = parentGo;
            _interactable = parentGo.GetComponent<Interactable>();
        }

        public override void Play()
        {
            if (CheckIfCurrentlyInteracting()) return;
            _processingInteraction = true;
            
            float amount = ammoAmount;
            Debug.Log("Ammo picked up by " + _interactable.LastInteractor.name);
            Tank myTank = _interactable.LastInteractor.GetComponent<Tank>();
            if (!myTank) return;
            switch (Ammo)
            {
                case Projectile projectile: //Ammo gained is a projectile
                {
                    Debug.Log("Gained " + amount + " projectile(s): " + projectile.name);
                    myTank.SetSecondaryFire(projectile);
                    break;
                }
                case Explosive explosive:   //Ammo gained is an explosive
                    Debug.Log("Gained " + amount + " explosive(s): " + explosive.name);
                    myTank.SetSecondaryFire(explosive);
                    break;
                default:
                    Debug.Log("Ammo box was empty");
                    break;
            }
        }

        public override void Stop()
        {
            
        }

        public bool CheckIfCurrentlyInteracting()
        {
            return _processingInteraction;
        }
    }
}
