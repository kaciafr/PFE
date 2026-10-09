using UnityEngine;

namespace Goblfin.PuzzleSystem
{
	public class PuzzleLink : MonoBehaviour
	{
		[SerializeField] private ConditionBase condition;
		[SerializeField] private ActionBase action;

		private bool running;

		private void OnEnable()  => condition.Changed += OnChanged;
		private void OnDisable() => condition.Changed -= OnChanged;

		private void OnChanged()
		{
			if (condition.isMet && !running)
			{
				running = true;
				action.Enter();
			}
		}

		private void Update()
		{
			if (!running) return;

			action.Tick();

			if (action.isFinished)
			{
				action.Exit();
				running = false;
			}
		}
	}
}