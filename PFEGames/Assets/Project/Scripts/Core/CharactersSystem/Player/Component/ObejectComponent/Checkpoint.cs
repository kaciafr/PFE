using System;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.Component
{
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField]private Transform spawnPoint;
        // Numéro d'ordre, à régler dans l'Inspector (0, 1, 2… dans le sens du niveau)
        [SerializeField] private int order;
        private bool activated;
        

        private void OnTriggerEnter(Collider other)
        {
            if (activated) return;
            if (other.GetComponentInParent<PlayerManager>() == null) return;
            
            activated = true;
            Transform point = spawnPoint !=null ? spawnPoint : transform;
            CheckPointSystem.SaveCheckPoint(point.position, point.rotation, order);
            
        }
    }
}