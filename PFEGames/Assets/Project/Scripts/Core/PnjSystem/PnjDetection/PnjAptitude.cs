using UnityEngine;

namespace Goblfin.PnjSystem.PnjDetection
{
	public class PnjAptitude : MonoBehaviour
	{
		protected BrainPnj brain;
		
		public virtual void Init(BrainPnj brain)
		{
			this.brain = brain;
		}
		
	}
}