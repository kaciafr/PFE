using System;
using UnityEngine;

namespace Goblfin.PnjSystem
{
    public class OverlapCacheMarker : MonoBehaviour
    {
        private Func<GameObject, bool> destroyAction;
        
        public void Setup(Func<GameObject, bool> destroyCallback)
        {
            destroyAction = destroyCallback;
        }
        
        private void OnDestroy() => destroyAction?.Invoke(gameObject);
    }
}