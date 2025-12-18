using System;
using Konfus.Systems.Fx_System.Effects;
using Armored_Felines.Input;
using Armored_Felines.Tanks;
using Konfus.Utility.Attributes;
using UnityEngine;

namespace Armored_Felines.Effects
{
    [Serializable]
    public class PickupBoostEffect : Effect
    {
        [SerializeField, Tooltip("Type of boost pickup")]
        private BoostType boostType;
        [SerializeField, Tooltip("Size of the boost")]
        private float boostAmount;
        [SerializeField, Tooltip("If the boost is temporary or permanent. Heals are permanent, armor can be temp")]
        private bool temporary;
        [SerializeField, Tooltip("How long in seconds until temporary effects go away")]
        [ShowIf(nameof(temporary), true)]
        private float secondsToLast;
        
        private Interactable _interactable;
        private bool _processingInteraction = false;

        public BoostType Type => boostType;
        public float BoostAmount => boostAmount;
        public bool Temporary => temporary;
        public float BoostDurationInSeconds => secondsToLast; // Duration of boost
        public override float Duration => 0; // Effect duration
        
        public override void Initialize(GameObject parentGo)
        {
            _interactable = parentGo.GetComponent<Interactable>();
        }

        public override void Play()
        {
            if (CheckIfCurrentlyInteracting()) return;
            _processingInteraction = true;
            
            Debug.Log("Boost " + Type + " picked up by " + _interactable.LastInteractor.name);
            Tank myTank = _interactable.LastInteractor.GetComponent<Tank>();
            if (!myTank) return;
            
            if (Temporary)
            {
                myTank.AddBoostEffectTimer(Type, BoostDurationInSeconds, BoostAmount);
                return;
            }
            
            switch (Type)
            {
                
                case BoostType.CurrentHealth:   //Big ol' thing of tape (OuchTape)
                {
                    Debug.Log("Increasing current health by " + BoostAmount);
                    Health health = myTank.transform.GetComponent<Health>();
                    if (health) health.Heal(BoostAmount);
                    break;
                }
                //need to consider if max health increases should full heal current health
                case BoostType.MaxHealth:   //Repair Kit
                {   //local scope not inherent to switch statement, hence the extra {}
                    Debug.Log("Increasing max health by " + BoostAmount);
                    Health health = myTank.transform.GetComponent<Health>();
                    if (health)
                    {
                        health.IncreaseMaxHealth(BoostAmount);
                        health.FullHeal();
                    }
                    break;
                }
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

    public enum BoostType
    {
        CurrentHealth, MaxHealth, Armor, BonusDamage, BonusSpeed, FireRate, None
    }
    
}
