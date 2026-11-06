using UnityEngine;

namespace Goblfin.Player.Component
{
    public class ClimbSurface : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Distance entre le centre du gobelin et la surface.")]
        private float distance = 0.4f;

        [SerializeField, Tooltip("Coché : en haut, le gobelin passe sur le rebord (comme une échelle).\nDécoché : il s'arrête en haut (bannière).")]
        private bool canClimbOver = false;

        [SerializeField, Min(0f), Tooltip("Si Can Climb Over est décoché : le centre du gobelin s'arrête à cette distance SOUS le haut de la surface.\nPlus petit = il monte plus haut.")]
        private float stopBelowTop = 0.3f;

        public float Distance => distance;
        public bool CanClimbOver => canClimbOver;
        public float StopBelowTop => stopBelowTop;
    }
}
