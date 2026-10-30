using Goblfin;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaveTest : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            SaveData data = new SaveData();
            data.hasCheckPoint = true;
            data.checkPointPosition = transform.position;   
            SaveSystem.Save(data);
            Debug.Log("Sauvé dans : " + Application.persistentDataPath);
        }

        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            SaveData data = SaveSystem.Load();

            if (data == null)
            {
                Debug.Log("Aucune sauvegarde");
                return;
            }

            Debug.Log("Position chargée : " + data.checkPointPosition);
        }
    }
}