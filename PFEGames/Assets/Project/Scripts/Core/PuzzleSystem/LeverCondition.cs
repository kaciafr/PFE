using Goblfin.CharactersSystem.Player.Interface;
using UnityEngine;
using PrimeTween;

public class LeverCondition : ConditionBase , IInteractable
{
    private bool isPull;
    public override bool isMet => isPull;

    [SerializeField] private Transform handle; 
    
    [SerializeField] private float offset;

    [SerializeField] private float onAngle; 
    
    [SerializeField] private float offAngle;
    
    [SerializeField] private float duration;

    public bool CanInteract => true;


    public void Start()
    {
        handle.localRotation = Quaternion.Euler(offAngle, 0, 0); 
    }

  

    public void Interact()
    {
        isPull = !isPull;
        NotifyChanged();
        Tween.LocalRotation(handle,
            Quaternion.Euler(isMet ? onAngle : offAngle, 0f, 0f),
            duration, Ease.OutBack);
    }
    

}
