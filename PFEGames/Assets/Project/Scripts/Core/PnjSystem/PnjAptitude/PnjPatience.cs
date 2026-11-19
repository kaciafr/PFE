using System;
using UnityEngine;

namespace Goblfin.PnjSystem
{
    public class PnjPatience : PnjAptitude
    {
        public enum PatienceType
        {
            Calm    = 0,
            Anxious = 1,
            Nervous = 2
        }
        [field:SerializeField] public PatienceType CurrentPatienceType { get; private set; }
        private float currentPatienceValue;

        private void Start()
        {
            currentPatienceValue = 0;
            UpdatePatience(currentPatienceValue);
        }

        public void AddOrRemovePatience(float value)
        {
            currentPatienceValue = Mathf.Clamp(currentPatienceValue + value, 0, 2);
            UpdatePatience(currentPatienceValue);
        }

        private void UpdatePatience(float amount)
        {
            CurrentPatienceType = (PatienceType)amount;
            Debug.Log(CurrentPatienceType);
            Debug.Log(amount);
        }
    }
}