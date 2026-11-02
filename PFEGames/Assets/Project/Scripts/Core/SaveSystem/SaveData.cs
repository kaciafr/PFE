using System;
using System.Collections.Generic;
using UnityEngine;

namespace Goblfin
{
    // Une boîte : l'ID d'un objet, et son état
    [Serializable]
    public class ObjectSave
    {
        public string id;
        public string state;
    }

    [Serializable ]
    public class SaveData
    {

        public int version = 1;

        public Quaternion checkPointRotation;
        public bool hasCheckPoint;
        public Vector3 checkPointPosition;
        public int checkPointOrder = -1;

        // L'état de tous les objets du château
        public List<ObjectSave> objects = new List<ObjectSave>();
    }
}
