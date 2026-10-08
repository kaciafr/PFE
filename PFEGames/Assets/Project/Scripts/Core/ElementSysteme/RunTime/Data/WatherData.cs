using UnityEngine;

namespace Runtime
{
    [CreateAssetMenu(fileName = "WatherData")]
    public class WatherData : ScriptableObject
    {
        [Header("Standar")]
        public float standarDegres = 23f;
        
        [Header("Rainning")]
        public float rainIntensity = 0f;
        
        [Header("Wind")]
        public float windIntensity = 0.1f;
        
        [Header("Humidity")]
        public float humidity = 0.2f;
    }
}
