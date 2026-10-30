using Goblfin.CharactersSystem.Player.Interface;
using PrimeTween;
using UnityEngine;

namespace Goblfin.PuzzleSystem
{
	public class LeverCondition : ConditionBase , IInteractable, ISaveable
	{
		// ID unique, généré automatiquement dans l'éditeur
		[SerializeField] private string id;
		public string Id => id;

		private bool isPull;
		public override bool isMet => isPull;

		[SerializeField] private Transform handle; 
    
		[SerializeField] private float offset;

		[SerializeField] private float onAngle; 
    
		[SerializeField] private float offAngle;
    
		[SerializeField] private float duration;

		public bool CanInteract => true;


		private void OnValidate()
		{
			// Si pas encore d'ID, on en crée un
			if (string.IsNullOrEmpty(id))
				id = System.Guid.NewGuid().ToString();
		}

		public void Start()
		{
			// Dépend de isPull, sinon Start pourrait écraser l'état restauré
			handle.localRotation = Quaternion.Euler(isMet ? onAngle : offAngle, 0, 0);
		}

  

		public void Interact()
		{
			isPull = !isPull;
			NotifyChanged();
			Tween.LocalRotation(handle,
				Quaternion.Euler(isMet ? onAngle : offAngle, 0f, 0f),
				duration, Ease.OutBack);
		}

		// Donner son état : "True" ou "False"
		public string CaptureState() => isPull.ToString();

		// Reprendre son état
		public void RestoreState(string state)
		{
			isPull = bool.Parse(state);
			handle.localRotation = Quaternion.Euler(isMet ? onAngle : offAngle, 0, 0);  // sans animation
			NotifyChanged();   // prévenir le PuzzleController → la porte s'ouvre toute seule
		}
    

	}
}
