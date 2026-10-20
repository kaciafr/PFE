using System;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.Component
{
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField]private Transform spawnPoint;
        private bool activated;
        

        private void OnTriggerEnter(Collider other)
        {
            if (activated) return;
            if (other.GetComponentInParent<PlayerManager>() == null) return;
            
            activated = true;
            Transform point = spawnPoint !=null ? spawnPoint : transform;
            CheckPointSystem.SaveCheckPoint(point.position, point.rotation);
            Debug.Log($"Checkpoint {point.name} has been activated");
            
        }
    }
}