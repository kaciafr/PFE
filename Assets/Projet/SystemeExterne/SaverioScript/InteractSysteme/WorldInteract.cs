using System.Collections.Generic;
using CharacterController.Script;
using Script.PlayerControllers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InteractSysteme.Script
{
	public class WorldInteract : MonoBehaviour
	{
		[SerializeField] private CharacterSetup characterSetup;

		private readonly HashSet<IInteractable> insideTrigger = new();

		private readonly HashSet<IInteractable> interactablesInRange = new();

		private void FixedUpdate()
		{
			foreach (var interactable in interactablesInRange)
				if (!insideTrigger.Contains(interactable))
					interactable.OnPlayerExit(characterSetup);

			foreach (var interactable in insideTrigger)
				if (!interactablesInRange.Contains(interactable))
					interactable.OnPlayerEnter(characterSetup);

			interactablesInRange.Clear();

			foreach (var interactable in insideTrigger)
				interactablesInRange.Add(interactable);

			insideTrigger.Clear();
		}

		private void OnTriggerStay(Collider other)
		{
			if (other.TryGetComponent(out IInteractable interactable)) insideTrigger.Add(interactable);
		}

		public void OnInteractInput(InputAction.CallbackContext context)
		{
			if (context.performed)
			{
				var priorityTarget = GetPriorityInteractable();

				if (priorityTarget != null)
				{
					priorityTarget.Interact(characterSetup);
					Debug.Log(priorityTarget);
				}
			}
		}

		private IInteractable GetPriorityInteractable()
		{
			if (interactablesInRange == null || interactablesInRange.Count == 0) return null;

			IInteractable highestPriorityTarget = null;
			var maxPriority = int.MinValue;

			foreach (var interactable in interactablesInRange)
				if (interactable.Priority > maxPriority)
				{
					maxPriority = interactable.Priority;
					highestPriorityTarget = interactable;
				}

			return highestPriorityTarget;
		}
	}
}