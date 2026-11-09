using Goblfin.PnjSystem.Utilities;
using UnityEngine;

namespace Goblfin.PnjSystem
{
    public class ResolveState : IPnjStates
    {
       private enum Phase { GoToSource, GoToProblem }

       public StatePriority Priority => StatePriority.Resolve;
       public float AgentSpeed => GameMetrix.GuardChaseSpeed;

       private readonly ElementSimulation elementSim;
       private readonly ResolutionSource resolutionSource;
       private GrabThings grabThings;
       private ResolveProblem resolveProblem;
       private Phase phase;
       private float arrivedTimer;

       public ResolveState(ElementSimulation elementSim, ResolutionSource resolutionSource)
       {
          this.elementSim = elementSim;
          this.resolutionSource = resolutionSource;
       }

       public void EnterState(BrainPnj brain)
       {
          grabThings = brain.GetAptitude<GrabThings>();
          resolveProblem = brain.GetAptitude<ResolveProblem>();

          brain.Agent.speed = AgentSpeed;
          brain.Agent.isStopped = false;
          brain.Agent.SetDestination(resolutionSource.transform.position);
          phase = Phase.GoToSource;
          arrivedTimer = 0f;
       }

       public void UpdateState(BrainPnj brain)
       {
          if (grabThings == null || resolveProblem == null || elementSim == null || resolutionSource == null
              || elementSim.CurrentState is not BurningState)
          {
             brain.PnjGoTo(new PatrolState(brain));
             return;
          }

          if (brain.Agent.remainingDistance <= brain.Agent.stoppingDistance + 0.5f)
          {
             switch (phase)
             {
                case Phase.GoToSource:
                   if (grabThings.Grab(resolutionSource.gameObject))
                   {
                      phase = Phase.GoToProblem;
                      brain.Agent.SetDestination(elementSim.transform.position);
                   }
                   else if (Arrived(brain))
                   {
                      arrivedTimer += Time.deltaTime;
                      if (arrivedTimer > 1f)
                      {
                         brain.PnjGoTo(new SearchState(brain, brain.transform.position));
                      }
                   }
                   break;

                case Phase.GoToProblem:
                   if (resolveProblem.CloseToTheProbleme(elementSim))
                   {
                      brain.Agent.speed = 0;
                      resolveProblem.Resolve(elementSim);
                   }

                   break;
             }
          }
       }

       public void ExitState(BrainPnj brain)
       {
          grabThings?.UnGrab();
          brain.Agent.isStopped = false;
          Communication.EndAction(brain);
       }

       private static bool Arrived(BrainPnj brain) =>
          !brain.Agent.pathPending && brain.Agent.remainingDistance <= brain.Agent.stoppingDistance + 0.5f;
    }
}