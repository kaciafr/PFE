using System;
using UnityEngine;

namespace Goblfin.CharactersSystem.Player
{
    public class PlayerAnimationEvents : MonoBehaviour
    {
        public event Action ThrowRelease;

        public void OnThrowRelease() => ThrowRelease?.Invoke();
    }
}
