using UnityEngine;

namespace Runtime.Project.Scripts.Core.ElementSysteme.RunTime
{
	public class ElementInfo : MonoBehaviour
	{
		protected ElementSimulation elementSim;

		public virtual void Init(ElementSimulation elementSim)
		{
			this.elementSim = elementSim;
		}
	}
}