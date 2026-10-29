using System.Collections.Generic;
using Goblfin.PnjSystem.Utilities;
using PrimeTween;
using UnityEngine;

namespace Goblfin.PnjSystem
{
    public class SearchState : IPnjStates
    {
	    public StatePriority Priority => StatePriority.Searching;
       private float timer;
       private BrainPnj guard;
       private Tween lookTween;
       private int currentSearchIndex;
       private Vector3 lastTargetPos;
       private List<Vector3> searchPoints = new List<Vector3>();
       private bool isWaiting;
       public SearchState(BrainPnj guard, Vector3 lastTargetPos)
       {
          this.guard = guard;
          this.lastTargetPos = lastTargetPos;
       }

       public void EnterState(BrainPnj brainPnj)
       { 
	       //A ENLEVER
	       brainPnj.material.color = Color.yellow;
          
          timer = 0f;
          currentSearchIndex = 0;
          isWaiting = false;
          
          guard.Agent.speed = GameMetrix.GuardPatrolSpeed;

          searchPoints = GameMetrix.SearchPointsGenerated(lastTargetPos, GameMetrix.SearchRadius, GameMetrix.SearchPointCount);

          if (searchPoints.Count > 0)
             brainPnj.Agent.SetDestination(searchPoints[0]);
          
       }

       public void UpdateState(BrainPnj brainPnj)
       {
          if (searchPoints.Count == 0)
          {
             brainPnj.PnjGoTo(new PatrolState(guard));
             return;
          }

          if (isWaiting)
          {
             timer -= Time.deltaTime;
             if (timer <= 0f)
                GoToNextPoint(brainPnj);
          }
          else
          {
             if (!brainPnj.Agent.pathPending && brainPnj.Agent.remainingDistance < 0.5f)
             {
                isWaiting = true;
                timer = GameMetrix.SearchWaitPerPoint;
             }
          }
       }

       public void ExitState(BrainPnj brainPnj)
       {
          lookTween.Stop();
          guard.Agent.speed = GameMetrix.GuardPatrolSpeed;
       }
       
       private void GoToNextPoint(BrainPnj brainPnj)
       {
          currentSearchIndex++;

          if (currentSearchIndex >= searchPoints.Count)
          {
             brainPnj.PnjGoTo(new PatrolState(guard));
             return;
          }

          isWaiting = false;
          brainPnj.Agent.SetDestination(searchPoints[currentSearchIndex]);
       }
    }
}