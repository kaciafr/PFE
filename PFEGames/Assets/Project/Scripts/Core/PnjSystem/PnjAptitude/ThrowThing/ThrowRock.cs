using UnityEngine;

namespace Goblfin.PnjSystem
{
    public class ThrowRock : MonoBehaviour
    {
        private Rigidbody rb;
        private float releaseAt;
        private bool launched;
        
        private void Awake() => rb = GetComponent<Rigidbody>();

        private void OnEnable()
        {
            launched = false;
            rb.linearVelocity =  Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        public void Launch(Vector3 velocity, float lifeTime)
        {
            rb.linearVelocity = velocity;
            rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.VelocityChange);
            releaseAt = Time.time + lifeTime;
        }

        private void FixedUpdate()
        {
            if (!launched || Time.time < releaseAt) return;
            
            launched = false;
            PrefabPool.Release(gameObject);
        }
    }
}