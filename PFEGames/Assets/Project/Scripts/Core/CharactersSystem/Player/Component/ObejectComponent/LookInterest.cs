
using UnityEngine;

namespace Goblfin.CharactersSystem.Player.Component
{
    public class LookInterest : MonoBehaviour
    {
        [SerializeField] private int priority = 1;
        [SerializeField] private Transform lookPoint;

        public int Priority => priority;

        // 1. Le point à regarder : le Look Point s'il existe, sinon le centre de l'objet
        public Vector3 LookPosition => lookPoint != null ? lookPoint.position : transform.position;
    }
}
