using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
	public class ElementInfo : MonoBehaviour
	{
		protected ElementSimulation elementSim;

		public virtual void Init(ElementSimulation element)
		{
			this.elementSim = element;
		}
	}
}