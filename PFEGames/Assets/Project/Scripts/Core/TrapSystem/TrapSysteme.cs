using System;
using UnityEngine;

namespace Goblfin.TrapSystem
{
    public class TrapSysteme : MonoBehaviour
    {
        public event Action TrapTriggered;
        public event Action TrapOpen;
        private bool isClosed;
        [SerializeField] private float cooldown;
        private float currentTime;

        private void Start()
        {
            currentTime = 0;
        }

        private void Update()
        {
            if (isClosed)
            {
                currentTime += Time.deltaTime;
                if (currentTime >= cooldown)
                {
                    TrapOpen?.Invoke();
                    isClosed = false;
                    currentTime = 0;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponentInChildren<IPrey>() != null && isClosed == false)
            {
                var prey = other.gameObject.GetComponentInChildren<IPrey>();
                prey.Catches();
                isClosed = true;
                TrapTriggered?.Invoke();
            }

        }
    }
}
