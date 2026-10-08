using System;
using System.Collections.Generic;
using Runtime.Project.Scripts.Core;
using Runtime.Project.Scripts.Core.ElementSysteme.RunTime;
using UnityEngine;

namespace Runtime
{
    public class ElementChain : ElementInfo
    {
        public List<ElementSimulation> neighbours = new List<ElementSimulation>();
        private void OnTriggerEnter(Collider other)
        {
            ElementSimulation element = other.GetComponent<ElementSimulation>();

            if (element != null && !neighbours.Contains(element))
            {
                neighbours.Add(element);
            }
                
        }
        private void OnTriggerExit(Collider other)
        {
            ElementSimulation element = other.GetComponent<ElementSimulation>();

            if (element != null)
            {
                neighbours.Remove(element);
            }

        }
    
    }
}
