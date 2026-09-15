using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Characters
{
    public class PlayerMovement: MonoBehaviour

    {
    private CharacterController characterController;
    public int moveSpeed =4; 
    public Transform cam;

    public int VerticalVelocity;

    public bool isGrounded;

    
    public void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void Move(Vector2 input)
    {
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        Vector3 move = right * input.x + forward * input.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);
    }

    public void Jump(Vector2 dir)
    {

    }
    }
}