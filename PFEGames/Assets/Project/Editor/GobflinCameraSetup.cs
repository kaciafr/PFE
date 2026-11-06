using System.Collections.Generic;
using Goblfin;
using Goblfin.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// OUTIL TEMPORAIRE : monte le système de caméra (Brain + CinemachineCamera + CameraController)
// dans la scène ouverte, comme dans KaciScene. Ctrl+Z pour annuler. À supprimer quand c'est fini.
public static class GobflinCameraSetup
{
    const string DefaultStatePath = "Assets/Project/Editor/Datas/Camera/NormalView.asset";
    static readonly List<string> report = new();

    [MenuItem("Tools/Goblfin/Camera/Monter la caméra (scène ouverte)")]
    static void Setup()
    {
        // 0. Jamais en Play (tout serait perdu à l'arrêt)
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Caméra Gobflin", "Arrête le mode Play d'abord.", "OK");
            return;
        }

        report.Clear();

        // 1. Le joueur (la cible à suivre) + son tag
        PlayerManager player = Object.FindFirstObjectByType<PlayerManager>(FindObjectsInactive.Include);
        if (player == null)
        {
            EditorUtility.DisplayDialog("Caméra Gobflin", "Pas de joueur (PlayerManager) dans la scène.", "OK");
            return;
        }
        if (!player.CompareTag("Player"))
        {
            Undo.RecordObject(player.gameObject, "Tag Player");
            player.tag = "Player";
            report.Add("Tag 'Player' mis sur le gobelin (sinon les zones ne marchent pas).");
        }

        // 2. Main Camera + Cinemachine Brain
        Camera cam = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            GameObject camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            Undo.RegisterCreatedObjectUndo(camGo, "Créer Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.GetComponent<Camera>();
            report.Add("Main Camera créée.");
        }
        if (cam.GetComponent<CinemachineBrain>() == null)
        {
            Undo.AddComponent<CinemachineBrain>(cam.gameObject);
            report.Add("Cinemachine Brain ajouté sur " + cam.name + ".");
        }

        // 3. La Cinemachine Camera (on garde celle qui existe, sinon on la crée)
        CinemachineCamera vcam = Object.FindFirstObjectByType<CinemachineCamera>(FindObjectsInactive.Include);
        if (vcam == null)
        {
            GameObject go = new GameObject("CM_PlayerCam");
            Undo.RegisterCreatedObjectUndo(go, "Créer CM_PlayerCam");
            go.transform.position = player.transform.position + new Vector3(0f, 1.2f, -6f);
            vcam = go.AddComponent<CinemachineCamera>();
            report.Add("CM_PlayerCam créée.");
        }

        // Suivre le gobelin
        CameraTarget target = vcam.Target;
        if (target.TrackingTarget != player.transform)
        {
            Undo.RecordObject(vcam, "Tracking Target");
            target.TrackingTarget = player.transform;
            vcam.Target = target;
            report.Add("Tracking Target = " + player.name + ".");
        }

        // Position Control = Follow, Rotation Control = Rotation Composer (+ murs + tremblements)
        AddIfMissing<CinemachineFollow>(vcam.gameObject);
        AddIfMissing<CinemachineRotationComposer>(vcam.gameObject);
        AddIfMissing<CinemachineDecollider>(vcam.gameObject);
        AddIfMissing<CinemachineImpulseListener>(vcam.gameObject);

        // Mêmes réglages que KaciScene :
        // Binding Mode = World Space → la caméra reste toujours du même côté du monde (elle ne tourne pas avec le gobelin)
        SerializedObject followSo = new SerializedObject(vcam.GetComponent<CinemachineFollow>());
        SerializedProperty binding = followSo.FindProperty("TrackerSettings.BindingMode");
        if (binding != null && binding.enumValueIndex != 4)
        {
            binding.enumValueIndex = 4;   // 4 = World Space
            followSo.FindProperty("TrackerSettings.PositionDamping").vector3Value = new Vector3(0.5f, 0.5f, 0.5f);
            followSo.ApplyModifiedProperties();
            report.Add("Follow → Binding Mode = World Space (comme KaciScene).");
        }

        // 4. Le CameraController (on garde celui qui existe, sinon on le crée)
        CameraController controller = Object.FindFirstObjectByType<CameraController>(FindObjectsInactive.Include);
        if (controller == null)
        {
            GameObject go = new GameObject("CameraManager");
            Undo.RegisterCreatedObjectUndo(go, "Créer CameraManager");
            controller = go.AddComponent<CameraController>();
            report.Add("CameraManager créé.");
        }
        if (!controller.gameObject.activeSelf)
        {
            Undo.RecordObject(controller.gameObject, "Activer CameraManager");
            controller.gameObject.SetActive(true);
            report.Add("CameraManager réactivé.");
        }

        SerializedObject so = new SerializedObject(controller);
        SerializedProperty pVcam = so.FindProperty("vcam");
        SerializedProperty pState = so.FindProperty("defaultState");

        if (pVcam.objectReferenceValue != vcam)
        {
            pVcam.objectReferenceValue = vcam;
            report.Add("CameraController → Vcam = " + vcam.name + ".");
        }
        if (pState.objectReferenceValue == null)
        {
            // "Goblfin." devant : Cinemachine a AUSSI une classe CameraState → on précise laquelle
            Goblfin.CameraState normal = AssetDatabase.LoadAssetAtPath<Goblfin.CameraState>(DefaultStatePath);
            if (normal != null)
            {
                pState.objectReferenceValue = normal;
                report.Add("CameraController → Default State = NormalView.");
            }
            else report.Add("⚠ NormalView introuvable : " + DefaultStatePath);
        }
        so.ApplyModifiedProperties();

        // 5. Les zones déjà posées sans vue
        foreach (CameraZone zone in Object.FindObjectsByType<CameraZone>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            SerializedObject zso = new SerializedObject(zone);
            if (zso.FindProperty("state").objectReferenceValue == null)
                report.Add("⚠ VIDE : " + zone.name + " → State (choisis une vue).");

            Collider col = zone.GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                Undo.RecordObject(col, "Trigger");
                col.isTrigger = true;
                report.Add(zone.name + " → Is Trigger coché.");
            }
        }

        if (report.Count == 0) report.Add("Tout était déjà bon.");

        EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        Debug.Log("[Caméra Gobflin]\n" + string.Join("\n", report));
        EditorUtility.DisplayDialog("Caméra Gobflin", string.Join("\n", report) + "\n\nPense à Ctrl+S !", "OK");
    }

    static void AddIfMissing<T>(GameObject go) where T : Component
    {
        if (go.GetComponent<T>() != null) return;
        Undo.AddComponent<T>(go);
        report.Add(typeof(T).Name + " ajouté sur " + go.name + ".");
    }
}
