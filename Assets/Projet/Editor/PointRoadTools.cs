using Routine;
using UnityEditor;
using UnityEngine;

namespace Projet.Tools
{
    [CustomEditor(typeof(BrainPnj))]
    public class PointRoadTools : Editor
    {
       private bool placingPoints = false;
       private Transform routeParent;

       public override void OnInspectorGUI()
       {
          DrawDefaultInspector();

          BrainPnj brain = (BrainPnj)target;

          EditorGUILayout.Space();

          if (!placingPoints)
          {
             if (GUILayout.Button("Activer placement de points (clic dans la Scene)"))
             {
                placingPoints = true;
                EnsureRouteParent(brain);
             }
          }
          else
          {
             EditorGUILayout.HelpBox("Clique dans la Scene View pour ajouter un point. Appuie sur Echap pour arrêter.", MessageType.Info);
             if (GUILayout.Button("Arrêter le placement"))
                placingPoints = false;
          }

          if (GUILayout.Button("Vider la routine"))
          {
             // détruit aussi les GameObjects, pas juste la liste
             foreach (PointTime pt in brain.firstRoutine)
             {
                if (pt != null)
                   Undo.DestroyObjectImmediate(pt.gameObject);
             }

             brain.firstRoutine.Clear();
             EditorUtility.SetDirty(brain);
          }
       }

       private void EnsureRouteParent(BrainPnj brain)
       {
          // si un parent existe déjà (nommé d'après le garde), on le réutilise
          string parentName = $"{brain.gameObject.name}_PatrolRoute";
          GameObject existing = GameObject.Find(parentName);

          if (existing != null)
          {
             routeParent = existing.transform;
          }
          else
          {
             GameObject parentObj = new GameObject(parentName);
             Undo.RegisterCreatedObjectUndo(parentObj, "Créer parent de route");
             routeParent = parentObj.transform;
          }
       }

       private void OnSceneGUI()
       {
          if (!placingPoints) return;

          BrainPnj brain = (BrainPnj)target;
          Event e = Event.current;

          if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
          {
             placingPoints = false;
             e.Use();
          }

          if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
          {
             Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

             if (Physics.Raycast(ray, out RaycastHit hit))
             {
                GameObject point = new GameObject($"PatrolPoint_{brain.firstRoutine.Count}");
                point.transform.position = hit.point;
                point.transform.SetParent(routeParent); // <- rangé dans le parent

                PointTime pt = point.AddComponent<PointTime>();
                brain.firstRoutine.Add(pt);

                Undo.RegisterCreatedObjectUndo(point, "Créer point de patrouille");
                EditorUtility.SetDirty(brain);
             }

             e.Use();
          }
       }
    }
}