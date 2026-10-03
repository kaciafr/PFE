using System;
using UnityEngine;

namespace Characters
{
    // À poser sur le même GameObject que l'Animator : Unity n'envoie les Animation Events qu'aux scripts de cet objet
    public class PlayerAnimationEvents : MonoBehaviour
    {
        public event Action ThrowRelease;

        // Appelé par l'Animation Event du clip Throw, à la frame où la main lâche l'objet
        public void OnThrowRelease() => ThrowRelease?.Invoke();
    }
}
