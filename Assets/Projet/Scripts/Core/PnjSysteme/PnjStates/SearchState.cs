using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using Utilities;

namespace PnjStates
{
    public class SearchState : IPnjStates
    {
       private float timer;
       private BrainPnj guard;
       private Tween lookTween;
       private int currentSearchIndex;
       private Vector3 lastTargetPos;
       private List<Vector3> searchPoints = new List<Vector3>();
       private bool isWaiting;

       [SerializeField] private float lookAngle = 45f;
       [SerializeField] private float lookDuration = 2.5f;

       public SearchState(BrainPnj guard, Vector3 lastTargetPos)
       {
          this.guard = guard;
          this.lastTargetPos = lastTargetPos;
       }

       public void EnterState(BrainPnj brainPnj)
       {
          timer = 0f;
          currentSearchIndex = 0;
          isWaiting = false;

          guard.Agent.updateRotation = false;
          guard.Agent.updatePosition = true;
          guard.Agent.speed = GameMetrix.GuardPatrolSpeed;

          searchPoints = GameMetrix.SearchPointsGenerated(lastTargetPos, GameMetrix.SearchRadius, GameMetrix.SearchPointCount);

          if (searchPoints.Count > 0)
             brainPnj.Agent.SetDestination(searchPoints[0]);
          
          Quaternion baseRotation = guard.transform.rotation;
          Quaternion leftRotation = baseRotation * Quaternion.Euler(0, -lookAngle, 0);
          Quaternion rightRotation = baseRotation * Quaternion.Euler(0, lookAngle, 0);

          lookTween = Tween.LocalRotation(guard.transform, leftRotation, rightRotation, lookDuration,
             cycles: -1, cycleMode: CycleMode.Yoyo);
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
          brainPnj.Agent.updateRotation = true;
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