using UnityEngine;

namespace Goblfin.PnjSystem
{
    public class CanBeDetected : MonoBehaviour , IDetected
    {
        public float VisibilityState { get; }
        public float speed { get; }
    }
}