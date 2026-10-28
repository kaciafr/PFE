using Goblfin.CharactersSystem.Player;
using UnityEngine;

namespace Goblfin
{
    public class PlayerLean : MonoBehaviour
    {
        [SerializeField] private float MaxLean = 12f;
        [SerializeField] private float fullLeanTurnSpeed = 360f;
        [SerializeField] private float leanSmooth = 8f;

        private PlayerManager player;
        private Transform model; 
        private Quaternion modelStartRotation;
        private Vector3 lastForward;
        private float targetLean; 
        private float currentLean;
        
        
        
        void Start()
        {
            player = GetComponent<PlayerManager>();
            model = player.Motor.Model; 
            modelStartRotation = model.localRotation;
            lastForward = player.Facing;
        }

        // FixedUpdate : on mesure au même rythme que la rotation du perso (physique)
        void FixedUpdate()
        {
            if (player.IsClimbing)
            {
                targetLean = 0f; 
                lastForward = player.Facing;
                return; 
            }
            
            
            float turned = Vector3.SignedAngle(lastForward, player.Facing, Vector3.up);
            float turnSpeed = turned / Time.fixedDeltaTime;
            lastForward = player.Facing;
            
            float turned01 = Mathf.Clamp( turnSpeed/fullLeanTurnSpeed, -1f, 1f );
            float speed01 = Mathf.Clamp01(player.HorizontalSpeed / player.Settings.Movement.SprintSpeed);
            targetLean = -turned01 * MaxLean * speed01;
        }


        private void LateUpdate()
        {
            if (player.IsClimbing) return;
            
            currentLean = Mathf.Lerp(currentLean, targetLean, leanSmooth * Time.deltaTime);
            
            model.localRotation = modelStartRotation * Quaternion.Euler(0,0,currentLean);
        }
    }
}
