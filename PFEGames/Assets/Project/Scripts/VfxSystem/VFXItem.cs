using System;
using UnityEngine;

namespace Goblfin.VfxSystem
{
    [Serializable]
    public class VFXItem
    {
        public VFXType type;
        public ParticleSystem particleSystem;
        public bool loop;
    }
}
