using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

// Outils d'éditeur pour que la caméra ne passe plus derrière les murs, sans casser le cadrage.
// 1) Tools > Camera > Ajouter Deoccluder : règle le Deoccluder de CM_PlayerCam (prefab ___Camera___).
//    Il ne réagit qu'au layer CameraBlock.
// 2) Tools > Camera > Bloquer la caméra sur la sélection : ajoute un bloqueur invisible
//    (enfant "CameraBlocker", layer CameraBlock) sur les murs sélectionnés.
// 3) Tools > Camera > Retirer le blocage de la sélection : enlève ces bloqueurs.
public static class CameraDeoccluderTool
{
    private const string PrefabPath = "Assets/Project/Prefabs/___Camera___ .prefab";
    private const string CameraName = "CM_PlayerCam";
    private const string BlockLayer = "CameraBlock";
    private const string BlockerName = "CameraBlocker";

    [MenuItem("Tools/Camera/Ajouter Deoccluder")]
    private static void AddDeoccluder()
    {
        int layer = EnsureBlockLayer();
        if (layer < 0) return;

        // Étape 1 : ouvrir le prefab de la caméra
        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        if (root == null)
        {
            EditorUtility.DisplayDialog("Caméra", "Prefab introuvable : " + PrefabPath, "OK");
            return;
        }

        try
        {
            // Étape 2 : trouver la caméra Cinemachine du joueur
            CinemachineCamera cam = FindCamera(root);
            if (cam == null)
            {
                EditorUtility.DisplayDialog("Caméra", CameraName + " introuvable dans le prefab.", "OK");
                return;
            }

            // Étape 3 : ajouter le Deoccluder (ou reprendre celui qui existe)
            CinemachineDeoccluder deoccluder = cam.GetComponent<CinemachineDeoccluder>();
            if (deoccluder == null)
                deoccluder = cam.gameObject.AddComponent<CinemachineDeoccluder>();

            // Étape 4 : il ne réagit qu'aux murs marqués CameraBlock
            deoccluder.CollideAgainst = 1 << layer;
            deoccluder.IgnoreTag = "Player";
            deoccluder.TransparentLayers = 0;
            deoccluder.MinimumDistanceFromTarget = 4f;

            // Étape 5 : réglages doux pour ne pas écraser le cadrage des CameraStates
            var avoid = deoccluder.AvoidObstacles;
            avoid.Enabled = true;
            avoid.Strategy = CinemachineDeoccluder.ObstacleAvoidance.ResolutionStrategy.PullCameraForward;
            avoid.DistanceLimit = 0f;
            avoid.MinimumOcclusionTime = 0.3f;
            avoid.CameraRadius = 0.3f;
            avoid.MaximumEffort = 4;
            avoid.SmoothingTime = 0.4f;
            avoid.Damping = 1.2f;
            avoid.DampingWhenOccluded = 0.5f;
            deoccluder.AvoidObstacles = avoid;

            // Étape 6 : enregistrer le prefab
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Debug.Log("Deoccluder réglé sur " + CameraName + " : il réagit seulement au layer " + BlockLayer + ".");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [MenuItem("Tools/Camera/Bloquer la caméra sur la sélection")]
    private static void AddBlockers()
    {
        int layer = EnsureBlockLayer();
        if (layer < 0) return;

        int count = 0;
        foreach (GameObject wall in Selection.gameObjects)
        {
            // Étape 1 : la forme du mur (bounds du mesh, dans le repère du mur)
            MeshFilter mf = wall.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
                Debug.LogWarning(wall.name + " n'a pas de mesh, ignoré.", wall);
                continue;
            }

            // Étape 2 : un seul bloqueur par mur (on remplace l'ancien)
            Transform old = wall.transform.Find(BlockerName);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

            // Étape 3 : enfant invisible avec un BoxCollider de la taille du mur, sur le layer CameraBlock
            GameObject blocker = new GameObject(BlockerName);
            Undo.RegisterCreatedObjectUndo(blocker, "Bloquer la caméra");
            blocker.transform.SetParent(wall.transform, false);
            blocker.layer = layer;

            // Si le mur a une échelle négative, on la compense pour que le BoxCollider reste positif
            Vector3 s = wall.transform.lossyScale;
            Vector3 sign = new Vector3(Mathf.Sign(s.x), Mathf.Sign(s.y), Mathf.Sign(s.z));
            blocker.transform.localScale = sign;

            Bounds b = mf.sharedMesh.bounds;
            BoxCollider box = blocker.AddComponent<BoxCollider>();
            box.center = Vector3.Scale(b.center, sign);
            box.size = new Vector3(Mathf.Abs(b.size.x), Mathf.Abs(b.size.y), Mathf.Abs(b.size.z));

            count++;
        }

        Debug.Log(count + " mur(s) bloquent maintenant la caméra. Pense à sauvegarder la scène.");
    }

    // Trouve tout seul les murs de séparation (fins, hauts et longs) et les sélectionne,
    // puis leur ajoute un bloqueur.
    [MenuItem("Tools/Camera/Bloquer automatiquement tous les murs")]
    private static void AutoBlockWalls()
    {
        const float maxThickness = 1f;
        const float minHeight = 2.5f;
        const float minLength = 1.5f;

        var walls = new System.Collections.Generic.List<GameObject>();

        foreach (MeshRenderer r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude))
        {
            GameObject go = r.gameObject;
            if (go.name == BlockerName) continue;
            if (go.GetComponent<Collider>() == null) continue;
            if (go.GetComponentInParent<Rigidbody>() != null) continue;

            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            // Taille réelle du mur dans son propre repère
            Vector3 s = Vector3.Scale(mf.sharedMesh.bounds.size, go.transform.lossyScale);
            float x = Mathf.Abs(s.x), y = Mathf.Abs(s.y), z = Mathf.Abs(s.z);

            bool thin = Mathf.Min(x, z) <= maxThickness;
            bool tall = y >= minHeight;
            bool longEnough = Mathf.Max(x, z) >= minLength;

            if (thin && tall && longEnough)
                walls.Add(go);
        }

        if (walls.Count == 0)
        {
            EditorUtility.DisplayDialog("Caméra", "Aucun mur trouvé.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Caméra",
                walls.Count + " murs trouvés. Ils sont sélectionnés dans la Hierarchy.\nAjouter un bloqueur de caméra sur chacun ?",
                "Ajouter", "Annuler"))
        {
            Selection.objects = walls.ToArray();
            return;
        }

        Selection.objects = walls.ToArray();
        AddBlockers();
    }

    [MenuItem("Tools/Camera/Retirer le blocage de la sélection")]
    private static void RemoveBlockers()
    {
        int count = 0;
        foreach (GameObject wall in Selection.gameObjects)
        {
            Transform old = wall.transform.Find(BlockerName);
            if (old == null) continue;
            Undo.DestroyObjectImmediate(old.gameObject);
            count++;
        }
        Debug.Log(count + " bloqueur(s) retiré(s).");
    }

    // Crée le layer CameraBlock s'il n'existe pas, et l'empêche de toucher quoi que ce soit physiquement
    private static int EnsureBlockLayer()
    {
        int layer = LayerMask.NameToLayer(BlockLayer);

        if (layer < 0)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(slot.stringValue))
                {
                    slot.stringValue = BlockLayer;
                    tagManager.ApplyModifiedProperties();
                    layer = i;
                    break;
                }
            }

            if (layer < 0)
            {
                EditorUtility.DisplayDialog("Caméra", "Plus de place pour créer le layer " + BlockLayer + ".", "OK");
                return -1;
            }
        }

        // Le bloqueur ne doit pas gêner le joueur, les gardes ou les objets : aucune collision physique
        for (int i = 0; i < 32; i++)
            Physics.IgnoreLayerCollision(layer, i, true);

        return layer;
    }

    private static CinemachineCamera FindCamera(GameObject root)
    {
        foreach (CinemachineCamera c in root.GetComponentsInChildren<CinemachineCamera>(true))
            if (c.name == CameraName) return c;
        return null;
    }
}
