using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.Pool; 



namespace Goblfin.VfxSystem
{
    public class VfxManager : MonoBehaviour
    {
        public static VfxManager Instance;

        [Header("Tous les VFX")]
        [SerializeField] private VFXItem[] vfxItems;
        private Dictionary<VFXType, VFXItem> vfxItemDictionary;
        
        private Dictionary<VFXType, ObjectPool<ParticleSystem>> pools = new Dictionary<VFXType, ObjectPool<ParticleSystem>>();
        
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitDictionariesVFX();
        }

        private void InitDictionariesVFX()
        {
            vfxItemDictionary = new Dictionary<VFXType, VFXItem>();
            foreach (VFXItem vfxItem in vfxItems)
            {
                if (!vfxItemDictionary.ContainsKey(vfxItem.type))
                {
                    vfxItemDictionary.Add(vfxItem.type, vfxItem);
                    CreatePool(vfxItem);
                }
                else
                {
                    Debug.LogWarning("Duplicate VFX Type: " + vfxItem.type);
                }
            }
        }

        private void CreatePool(VFXItem item)
        {
            ObjectPool<ParticleSystem> pool = null;

            pool = new ObjectPool<ParticleSystem>(
                createFunc: () =>
                {
                    ParticleSystem vfx = Instantiate(item.particleSystem, transform);

                    var main = vfx.main;
                    main.stopAction = ParticleSystemStopAction.Callback;
                    main.loop = item.loop;

                    VFXReturnToPool returner = vfx.gameObject.AddComponent<VFXReturnToPool>();
                    returner.vfx = vfx;
                    returner.pool = pool;

                    return vfx;
                },
                actionOnGet: vfx => vfx.gameObject.SetActive(true),
                actionOnRelease: vfx => vfx.gameObject.SetActive(false),
                actionOnDestroy: vfx => Destroy(vfx.gameObject),
                defaultCapacity: 5,
                maxSize: 20);

            pools.Add(item.type, pool);
        }

        public ParticleSystem PlayVFX(VFXType type, Vector3 position)
        {
            if (!vfxItemDictionary.ContainsKey(type))
            {
                Debug.LogWarning("VFX Type " + type + " not found");
                return null;
            }

            VFXItem item = vfxItemDictionary[type];

            if (item.particleSystem == null) return null ;

            ParticleSystem vfx = pools[type].Get();
            vfx.transform.position = position;
            vfx.Clear();
            vfx.Play();
            return vfx;
        }
    }
}
