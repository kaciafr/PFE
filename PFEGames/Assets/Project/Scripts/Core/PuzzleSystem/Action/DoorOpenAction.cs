using PrimeTween;
using UnityEngine;

namespace Goblfin.PuzzleSystem
{
	public class DoorOpenAction : ActionBase
	{
		[SerializeField]private Transform door ;
        
		[SerializeField] private Tween tween;
		private Vector3 openoffset = new Vector3(0f,3f,0f);
		private Vector3 closedoffset;
		private float duration = 2f;

		private void  Awake()
		{
			closedoffset = door.localPosition;
		}
		public override void Enter()
		{
			base.Enter();
			tween = Tween.LocalPosition(door, closedoffset + openoffset, duration, Ease.InOutSine)
				.OnComplete(() => isFinished = true);        
		}

		public override void Undo()
		{
			tween.Stop();
			isFinished = false;
			tween = Tween.LocalPosition(door, closedoffset, duration, Ease.InOutSine);
		}
	}
}
        
