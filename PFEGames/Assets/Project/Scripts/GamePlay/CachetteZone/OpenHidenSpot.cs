using System;
using System.Collections.Generic;
using Codice.CM.Common;
using Goblfin.Player;
using UnityEngine;

namespace Goblfin.Gameplay.CachetteZone
{
    public class OpenHidenSpot : MonoBehaviour
    {
        [SerializeField] private List<GameObject> doors;
        [SerializeField] private Quaternion openRotation;
        private Quaternion intRotation;

        private void Start()
        {
            foreach (GameObject door in doors)
                intRotation =  door.transform.rotation;
        }

        private void OnTriggerStay(Collider other)
        {
            if(!other.gameObject.GetComponent<PlayerManager>()) return;

            foreach (GameObject door in doors)
            {
                door.transform.rotation = Quaternion.Lerp(door.transform.rotation, openRotation, Time.fixedDeltaTime);
            }
                
        }
        private void OnTriggerExit(Collider other)
        {
            if (!other.gameObject.GetComponent<PlayerManager>()) return;
            foreach (GameObject door in doors)
                {
                    door.transform.rotation = intRotation;
                }
        }
    }
}
