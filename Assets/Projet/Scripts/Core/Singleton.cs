using UnityEngine;

namespace DefaultNamespace
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                    if (instance == null)
                    {
                        
                        instance = FindObjectOfType<T>();

                        if (instance == null)
                        {
                            
                            var singletonObject = new GameObject(typeof(T).Name + " (Singleton)");
                            instance = singletonObject.AddComponent<T>();
                        }
                    }

                    return instance;
                
            }
        }
        protected virtual void Awake()
        {
	        if (instance != null && instance != this as T)
	        {
		        Destroy(gameObject);
		        return;
	        }
	        instance = this as T;
	        DontDestroyOnLoad(gameObject);
        }
        
    }
    
}