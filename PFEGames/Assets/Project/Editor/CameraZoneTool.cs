using Goblfin;
using UnityEditor;
using UnityEngine;

// Outil d'éditeur : Tools > Camera > Créer une zone pour la pièce sélectionnée
// Sélectionne tous les objets d'une pièce (sol, murs...), puis lance l'outil.
// Il crée :
//  - un CameraState "CS_<nom>" qui garde la caméra à l'intérieur de la pièce (Avoid Walls décoché),
//  - un objet "CameraZone_<nom>" avec un Box Collider (trigger) qui couvre la pièce.
public static class CameraZoneTool
{
    private const string StateFolder = "Assets/Project/Editor/Datas/Camera/Rooms";
    private const float WallMargin = 0.6f;
    private const float MaxFov = 70f;

    [MenuItem("Tools/Camera/Créer une zone pour la pièce sélectionnée")]
    private static void CreateRoomZone()
    {
        GameObject[] selection = Selection.gameObjects;
        if (selection.Length == 0)
        {
            EditorUtility.DisplayDialog("Zone caméra", "Sélectionne les objets de la pièce (sol, murs...).", "OK");
            return;
        }

        // Étape 1 : la taille de la pièce = la boîte qui englobe tous les objets sélectionnés
        if (!TryGetBounds(selection, out Bounds room))
        {
            EditorUtility.DisplayDialog("Zone caméra", "Aucun objet visible dans la sélection.", "OK");
            return;
        }

        // Étape 2 : la vue de départ = la vue par défaut du CameraController
        CameraState baseState = FindDefaultState();
        if (baseState == null)
        {
            EditorUtility.DisplayDialog("Zone caméra", "Vue par défaut introuvable (CameraController).", "OK");
            return;
        }

        string roomName = AskName(selection[0].name);
        if (roomName == null) return;

        // Étape 3 : créer la vue de la pièce
        CameraState state = CreateState(baseState, room, roomName);

        // Étape 4 : créer la zone qui déclenche cette vue
        CreateZone(room, state, roomName);

        Debug.Log("Zone caméra créée pour " + roomName + ". Ajuste CS_" + roomName + " si besoin, puis sauvegarde la scène.", state);
    }

    private static bool TryGetBounds(GameObject[] objects, out Bounds bounds)
    {
        bounds = new Bounds();
        bool found = false;

        foreach (GameObject go in objects)
        {
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer) continue;
                if (!found) { bounds = r.bounds; found = true; }
                else bounds.Encapsulate(r.bounds);
            }
        }
        return found;
    }

    private static CameraState FindDefaultState()
    {
        CameraController controller = Object.FindFirstObjectByType<CameraController>();
        if (controller != null)
        {
            SerializedObject so = new SerializedObject(controller);
            CameraState state = so.FindProperty("defaultState").objectReferenceValue as CameraState;
            if (state != null) return state;
        }
        return AssetDatabase.LoadAssetAtPath<CameraState>("Assets/Project/Editor/Datas/Camera/NormalView.asset");
    }

    // La caméra reste dans la pièce : on la rapproche, on la monte un peu, et on élargit le FOV
    // pour garder à peu près le même cadrage.
    private static CameraState CreateState(CameraState baseState, Bounds room, string roomName)
    {
        CameraState state = Object.Instantiate(baseState);
        state.avoidWalls = false;

        Vector3 offset = baseState.followOffset;
        Vector3 flat = new Vector3(offset.x, 0f, offset.z);
        float flatDistance = flat.magnitude;

        // Profondeur de la pièce dans la direction de la caméra
        Vector3 dir = flatDistance > 0.01f ? flat / flatDistance : Vector3.forward;
        float depth = Mathf.Abs(dir.x) * room.size.x + Mathf.Abs(dir.z) * room.size.z;
        float maxDistance = Mathf.Max(1.5f, depth * 0.5f - WallMargin);

        if (flatDistance > maxDistance)
        {
            float scale = maxDistance / flatDistance;

            // Hauteur : un peu plus haut pour voir la pièce, mais sous le plafond
            float maxHeight = Mathf.Max(0.5f, room.size.y - WallMargin - 1f);
            float height = Mathf.Min(Mathf.Max(offset.y, offset.y * scale + 1f), maxHeight);

            state.followOffset = new Vector3(flat.x * scale, height, flat.z * scale);

            // FOV plus large pour compenser la caméra plus proche
            float halfFov = baseState.fov * 0.5f * Mathf.Deg2Rad;
            float fov = 2f * Mathf.Atan(Mathf.Tan(halfFov) / scale) * Mathf.Rad2Deg;
            state.fov = Mathf.Min(fov, MaxFov);
        }

        if (!AssetDatabase.IsValidFolder(StateFolder))
            AssetDatabase.CreateFolder("Assets/Project/Editor/Datas/Camera", "Rooms");

        string path = AssetDatabase.GenerateUniqueAssetPath(StateFolder + "/CS_" + roomName + ".asset");
        AssetDatabase.CreateAsset(state, path);
        AssetDatabase.SaveAssets();
        return state;
    }

    // Boîte trigger qui couvre la pièce, un peu réduite pour ne pas déborder sur la porte
    private static void CreateZone(Bounds room, CameraState state, string roomName)
    {
        GameObject zone = new GameObject("CameraZone_" + roomName);
        Undo.RegisterCreatedObjectUndo(zone, "Créer zone caméra");
        zone.transform.position = room.center;

        BoxCollider box = zone.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(
            Mathf.Max(0.5f, room.size.x - WallMargin * 2f),
            room.size.y,
            Mathf.Max(0.5f, room.size.z - WallMargin * 2f));

        CameraZone cz = zone.AddComponent<CameraZone>();
        SerializedObject so = new SerializedObject(cz);
        so.FindProperty("state").objectReferenceValue = state;
        so.ApplyModifiedPropertiesWithoutUndo();

        Selection.activeGameObject = zone;
    }

    private static string AskName(string fallback)
    {
        string name = fallback.Replace(" ", "");
        return EditorUtility.DisplayDialogComplex("Zone caméra",
            "Créer une zone caméra nommée \"" + name + "\" ?\n(Renomme l'objet CameraZone ensuite si tu veux.)",
            "Créer", "Annuler", "") == 0 ? name : null;
    }
}
