using Goblfin.CharactersSystem.Player;
using UnityEngine;
using UnityEngine.Events;

namespace Goblfin.QuestSystem
{
    // Une zone (Collider en "Is Trigger") qui déclenche des actions quand le joueur entre
    [RequireComponent(typeof(Collider))]
    public class TriggerZone : MonoBehaviour
    {
        [SerializeField] private bool triggerOnce = true;
        [SerializeField] private UnityEvent onEnter;

        private bool triggered;

        private void OnTriggerEnter(Collider other)
        {
            // 1. Déjà déclenchée une fois : on ne refait rien
            if (triggerOnce && triggered) return;

            // 2. Seulement le joueur (le collider touché peut être un enfant du Player)
            if (other.GetComponentInParent<PlayerManager>() == null) return;

            // 3. On fait toutes les actions de l'Inspector
            triggered = true;
            onEnter.Invoke();
        }
    }
}
