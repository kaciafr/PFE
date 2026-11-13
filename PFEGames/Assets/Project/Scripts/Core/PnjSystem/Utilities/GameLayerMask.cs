using UnityEngine;

namespace Goblfin.PnjSystem
{
    public static class GameLayerMask
    {
        public static readonly LayerMask Wall = LayerMask.GetMask("Wall");
        public static readonly LayerMask Vaultable = LayerMask.GetMask("Vault");
        public static readonly LayerMask Player = LayerMask.GetMask("Player");
        public static LayerMask Ground = LayerMask.GetMask("Ground");
        public static LayerMask Climb = LayerMask.GetMask("Climb");
        public static LayerMask Door = LayerMask.GetMask("Door");
    }
}