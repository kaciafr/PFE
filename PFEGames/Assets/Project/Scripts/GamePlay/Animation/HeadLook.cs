using System;
using Goblfin.PnjSystem;
using PrimeTween;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Goblfin.Gameplay.Animation
{
    public class HeadLook : MonoBehaviour
    {
       [Header("Références")]
       [SerializeField] private Rig headRig;           
       [SerializeField] private Transform lookTarget;  
       [SerializeField] private Transform headBone;     
       [SerializeField] private Transform bodyRef; 
       [SerializeField] private BrainPnj brainPnj;
       
       [Header("Réglages")]
       [SerializeField] private float weightSpeed = 5f;
       [SerializeField] private float followSharpness = 10f;
       [SerializeField] private float maxAngle = 100f;
       [SerializeField] private float targetHeight = 1.5f;
       [SerializeField, Range(0f, 1f)] private float maxWeight = 0.7f;
       
       [SerializeField] private float angleSweep = 1.5f;
       [SerializeField] private float speedSweep = 1.5f;
       

       private Transform followTarget;
       private Vector3 fixedPoint;
       private bool hasTarget;
       private bool sweeping;
       private float sweepAngle;
       private float sweepPeriod;
       private float sweepStart;
       private VisionCone visionCone;
       private Tween currentTween;

       private void Awake()
       {
          if (headRig == null || lookTarget == null || headBone == null || bodyRef == null)
          {
             Debug.LogError("HeadLook : une référence est vide dans l'Inspector.", this);
             enabled = false;
             return;
          }
          headRig.weight = 0f;
       }

       private void Start()
       {
          visionCone = brainPnj.GetAptitude<VisionCone>();
          visionCone.Target += LookAt;
          visionCone.LostTarget += Clear;
          Clear();
       }

       private void OnDisable()
       {
          Clear();
          visionCone.Target -= LookAt;
          visionCone.LostTarget -= Clear;
       }

       public void LookAt(Transform target)
       {
          Begin();
          followTarget = target;
          sweeping = false;
       }

       public void LookAt(Vector3 point)
       {
          Begin();
          followTarget = null;
          fixedPoint = point;
          sweeping = false;
       }

       public void Sweep(float angle, float period)
       {
          if (sweeping) return;

          Begin();
          followTarget = null;
          sweeping = true;
          sweepAngle = angle;
          sweepPeriod = period;
          sweepStart = Time.time;
    
          currentTween.Stop();
       }

       public void Clear()
       {
          hasTarget = false;
          sweeping = false;
          followTarget = null;
       }

       private void Begin()
       {
          if (hasTarget) return;
          hasTarget = true;
          lookTarget.position = bodyRef.position + Vector3.up * targetHeight + bodyRef.forward * 5f;
       }

       private void Update()
       {
          if (brainPnj.PNJStates is SearchState)
          {
             Sweep(angleSweep, speedSweep);
          }
          else
          {
             Clear();
          }
       }

       private void LateUpdate()
       {
          float desiredWeight = 0f;
   
          if (hasTarget)
          {
             Vector3 wanted = GetWantedPoint();

             // On enlève temporairement la condition de l'angle pour tester
             desiredWeight = maxWeight;
             float k = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
             lookTarget.position = Vector3.Lerp(lookTarget.position, wanted, k);
          }

          headRig.weight = Mathf.MoveTowards(headRig.weight, desiredWeight, weightSpeed * Time.deltaTime);
       }

       private Vector3 GetWantedPoint()
       {
          if (followTarget != null)
             return followTarget.position + Vector3.up * targetHeight;

          if (sweeping)
          {
             float phase = (Time.time - sweepStart) * Mathf.PI * 2f / sweepPeriod;
             float angle = Mathf.Sin(phase) * sweepAngle;
             Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * bodyRef.forward;
             
             return bodyRef.position + Vector3.up * targetHeight + dir * 5f;
          }

          return fixedPoint;
       }
    }
}