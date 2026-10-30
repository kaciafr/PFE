using Goblfin.CharactersSystem.Player;
using Goblfin.CharactersSystem.Player.Component;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goblfin
{
    public class SaveLoader : MonoBehaviour
    {
        [SerializeField] private PlayerManager player;


        private void Start()
        {
            // Rendre leur état aux objets du château
            SaveData data = SaveSystem.Load();
            if (data != null) SaveSystem.RestoreWorld(data);

            CheckPointSystem.LoadFromSave();

            if (CheckPointSystem.HasCheckPoint)
            {
                player.Respawn(0f);

            }
        }

        private void Update()
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                SaveSystem.Delete();
                CheckPointSystem.Clear();   
                Debug.Log("Sauvegarde effacée");
            }
        }
    }
    
}