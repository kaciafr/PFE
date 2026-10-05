using UnityEngine;

namespace PnjDetection
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