using UnityEngine;
using UnityEngine.Pool;

namespace Goblfin.VfxSystem
{
    public class VFXReturnToPool : MonoBehaviour
    {
        public ParticleSystem vfx;
        public ObjectPool<ParticleSystem> pool;

        private void OnParticleSystemStopped()
        {
            pool.Release(vfx);
        }
    }
}
