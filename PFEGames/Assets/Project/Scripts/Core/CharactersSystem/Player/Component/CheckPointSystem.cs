using Goblfin.CharactersSystem.Player.Data;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace Goblfin.CharactersSystem.Player.Component
{
    public static class CheckPointSystem
    {
        public static bool HasCheckPoint{ get; private set; }
        public static  Vector3 Position { get; private set; }
        public static Quaternion Rotation { get; private set; }
        
        public static void SaveCheckPoint (Vector3 position, Quaternion rotation)
        {
            HasCheckPoint = true;
            Position = position;
            Rotation = rotation;
        } 
        
        public static void Clear() => HasCheckPoint = false;
    }
} 