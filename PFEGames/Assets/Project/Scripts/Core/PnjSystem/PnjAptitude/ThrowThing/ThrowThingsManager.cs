using UnityEngine;
using UnityEngine.AI;

namespace Goblfin.PnjSystem
{
    public class ThrowThingsManager : PnjAptitude
    {
        [Header("Object Throw")]
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private Transform hand;
        [SerializeField] private int maxRocks = 15;
        [SerializeField] private float throwCooldown = 1.5f;
        [SerializeField] private float rockLifeTime = 6f;
        
        [Header("Tir")]
        [SerializeField] private float throwAngular = 12f;
        [SerializeField] private float maxThrowRange = 15f;
        [SerializeField] private float minThrowRange = 4f;
        [SerializeField] private float vagueness = 0.6f;
        [SerializeField] private float aimHeight = 1f;
        
        public GameObject RockPrefab => rockPrefab;
        public int MaxRocks => maxRocks;
        public int RocksLeft { get; private set; }
        public bool CanThrow => RocksLeft > 0 && Time.time >= nextThrowTime;
        public float MinThrowRange => minThrowRange;

        private NavMeshAgent agent;
        private NavMeshPath path;
        private float nextThrowTime;
        [field:SerializeField] public Vector3 TargetPos{get; private set;}
        private void OnEnable() => RocksLeft = maxRocks;
        
        public bool InRange(Vector3 target, float margin = 1f) =>
            (target - transform.position).sqrMagnitude <= (maxThrowRange * margin) * (maxThrowRange * margin);

        public bool CanReach(Vector3 target)
        {
            if (!NavMesh.SamplePosition(target, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
                return false;                                 

            if (Mathf.Abs(hit.position.y - target.y) > 0.6f)
                return false;                                  

            agent ??= GetComponent<NavMeshAgent>();
            path ??= new NavMeshPath();

            return agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete;
        }

        public bool TryThrow(Vector3 targetPos)
        {
            if (!CanThrow || rockPrefab == null) return false;

            Vector3 origin = hand != null ? hand.position : transform.position + Vector3.up * 1.5f;

            Vector2 error = Random.insideUnitCircle * vagueness;
            Vector3 aim = targetPos + Vector3.up * aimHeight + new Vector3(error.x, 0f, error.y);

            Vector3 displacement = aim - origin;
            float flight = Mathf.Max(0.4f, new Vector2(displacement.x, displacement.z).magnitude / throwAngular);
            Vector3 velocity = displacement / flight - Physics.gravity * (0.5f * flight);

            GameObject rock = PrefabPool.Get(rockPrefab, origin, Random.rotation);
            if (rock.TryGetComponent(out ThrowRock thrown))
            {
                thrown.Launch(velocity, rockLifeTime);
            }
               

            RocksLeft--;
            nextThrowTime = Time.time + throwCooldown;
            return true;
        }
        public void SetTarget(Vector3 target)
        {
            TargetPos = target;
        }
    }
}