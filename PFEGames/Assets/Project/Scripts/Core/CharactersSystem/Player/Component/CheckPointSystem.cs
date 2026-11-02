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


        public static int Order { get; private set; } = -1;
        
        public static void SaveCheckPoint (Vector3 position, Quaternion rotation, int order)
        {
            if (order <= Order) return;

            HasCheckPoint = true;
            Position = position;
            Rotation = rotation;
            Order = order;

            SaveData data = new SaveData();
            data.hasCheckPoint = true;
            data.checkPointPosition = position;
            data.checkPointRotation = rotation;
            data.checkPointOrder = order;

            // Ajouter l'état des objets du château (leviers, caisses…)
            SaveSystem.CaptureWorld(data);
            SaveSystem.Save(data);
        }



        public static void LoadFromSave()
        {
            SaveData data = SaveSystem.Load();
            if (data == null) return;
            HasCheckPoint = data.hasCheckPoint;
            Position = data.checkPointPosition;
            Rotation = data.checkPointRotation;
            Order = data.checkPointOrder;
        }

        // Clear remet aussi le numéro à "aucun"
        public static void Clear()
        {
            HasCheckPoint = false;
            Order = -1;
        }
    }
} 