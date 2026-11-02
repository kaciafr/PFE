using System.IO;
using UnityEngine;

namespace Goblfin
{
    public static class SaveSystem
    {
        private static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

        public static void Save(SaveData data)
        {
            string json  = JsonUtility.ToJson(data, true);
            
            File.WriteAllText(FilePath, json);
        }

        public static SaveData Load()
        {
            if (!File.Exists(FilePath)) return null; 
            
            string json = File.ReadAllText(FilePath);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public static void Delete()
        {
            if(File.Exists(FilePath)) File.Delete(FilePath);
        }

        // Remplit data.objects avec l'état de tous les ISaveable de la scène
        public static void CaptureWorld(SaveData data)
        {
            data.objects.Clear();

            // 1. On récupère tous les scripts de la scène
            MonoBehaviour[] all = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            foreach (MonoBehaviour mb in all)
            {
                // 2. On garde seulement ceux qui ont signé le contrat ISaveable
                if (mb is ISaveable saveable)
                {
                    ObjectSave o = new ObjectSave();
                    o.id = saveable.Id;
                    o.state = saveable.CaptureState();
                    data.objects.Add(o);
                }
            }
        }

        // Rend à chaque ISaveable de la scène l'état trouvé dans data.objects
        public static void RestoreWorld(SaveData data)
        {
            MonoBehaviour[] all = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            foreach (MonoBehaviour mb in all)
            {
                if (mb is ISaveable saveable)
                {
                    // 3. On cherche dans la liste la boîte qui a le même ID
                    foreach (ObjectSave o in data.objects)
                    {
                        if (o.id == saveable.Id)
                            saveable.RestoreState(o.state);
                    }
                }
            }
        }
    }
}