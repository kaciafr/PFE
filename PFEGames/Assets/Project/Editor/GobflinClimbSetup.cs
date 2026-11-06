using System.Collections.Generic;
using Goblfin.Player.Component;   // namespace de ClimbSurface
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Rend la bannière grimpable DIRECTEMENT DANS SON PREFAB (une fois pour toutes) :
// on garde son Mesh Collider (forme exacte → arrêt pile en haut de la bannière),
// on met le layer d'escalade et un ClimbSurface. Puis on nettoie la scène ouverte.
public static class GobflinClimbSetup
{
    const string BannerPrefabPath = "Assets/Project/Art/Model/Meuble/ObjetUtiliisable/ObjetInteractable/SM - Etendard.prefab";
    const string SettingsPath = "Assets/Project/Scripts/Core/CharactersSystem/Player/ScriptableObject/PlayerSettings.asset";
    const string OldGroupName = "___ CLIMB COLLIDERS ___";

    [MenuItem("Tools/Goblfin/Escalade/Configurer la bannière (prefab + scène)")]
    static void Setup()
    {
        if (EditorApplication.isPlaying || PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            EditorUtility.DisplayDialog("Escalade Gobflin", "Arrête le Play et sors du mode prefab (flèche < de la Hierarchy).", "OK");
            return;
        }

        List<string> report = new List<string>();

        // 1. Le layer d'escalade = PlayerSettings → Climb → Layer
        // Nom complet : Unity a AUSSI une classe "PlayerSettings" (UnityEditor)
        var settings = AssetDatabase.LoadAssetAtPath<Goblfin.CharactersSystem.Player.Data.PlayerSettings>(SettingsPath);
        int climbLayer = settings != null ? FirstLayer(settings.Climb.Layer) : -1;
        if (climbLayer < 0)
        {
            EditorUtility.DisplayDialog("Escalade Gobflin", "Layer d'escalade introuvable dans PlayerSettings → Climb → Layer.", "OK");
            return;
        }

        // 2. LE PREFAB : layer + ClimbSurface sur l'objet qui porte le Mesh Collider
        GameObject bannerAsset = AssetDatabase.LoadAssetAtPath<GameObject>(BannerPrefabPath);
        if (bannerAsset == null)
        {
            EditorUtility.DisplayDialog("Escalade Gobflin", "Prefab introuvable :\n" + BannerPrefabPath, "OK");
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(BannerPrefabPath);
        Collider col = root.GetComponentInChildren<Collider>(true);
        if (col == null)
        {
            col = root.AddComponent<MeshCollider>();   // se cale sur le mesh de la bannière
            report.Add("Prefab : Mesh Collider ajouté.");
        }

        GameObject surface = col.gameObject;
        col.isTrigger = false;   // un trigger est ignoré par le rayon d'escalade

        if (surface.layer != climbLayer)
        {
            surface.layer = climbLayer;
            report.Add($"Prefab : layer = {LayerMask.LayerToName(climbLayer)}.");
        }
        if (surface.GetComponent<ClimbSurface>() == null)
        {
            surface.AddComponent<ClimbSurface>();   // Distance 0.4, Can Climb Over décoché = arrêt en haut
            report.Add("Prefab : ClimbSurface ajouté.");
        }

        PrefabUtility.SaveAsPrefabAsset(root, BannerPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);

        // 3. LA SCÈNE : supprimer les anciens colliders séparés
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (GameObject go in scene.GetRootGameObjects())
        {
            if (go.name != OldGroupName) continue;
            Undo.DestroyObjectImmediate(go);
            report.Add("Scène : ancien groupe de colliders supprimé.");
        }

        // 4. LES BANNIÈRES DÉJÀ POSÉES : enlever ce que l'ancien outil avait changé dessus
        int fixedCount = 0;
        foreach (GameObject inst in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(inst)) continue;
            if (PrefabUtility.GetCorrespondingObjectFromSource(inst) != bannerAsset) continue;

            // a) Composants AJOUTÉS par l'ancien outil (Box Collider penché, ClimbSurface)
            foreach (var added in PrefabUtility.GetAddedComponents(inst))
                if (added.instanceComponent is BoxCollider || added.instanceComponent is ClimbSurface)
                    added.Revert(InteractionMode.UserAction);

            // b) Composants SUPPRIMÉS sur l'instance (ex : le Mesh Collider) → on les remet
            foreach (var removed in PrefabUtility.GetRemovedComponents(inst))
                removed.Revert(InteractionMode.UserAction);

            // c) Le layer : reprendre celui du prefab
            foreach (Transform t in inst.GetComponentsInChildren<Transform>(true))
            {
                SerializedProperty layer = new SerializedObject(t.gameObject).FindProperty("m_Layer");
                if (layer.prefabOverride)
                    PrefabUtility.RevertPropertyOverride(layer, InteractionMode.UserAction);
            }

            fixedCount++;
        }
        if (fixedCount > 0) report.Add($"Scène : {fixedCount} bannière(s) remise(s) comme le prefab.");

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[Escalade Gobflin]\n" + string.Join("\n", report));
        EditorUtility.DisplayDialog("Escalade Gobflin", string.Join("\n", report) + "\n\nPense à Ctrl+S !", "OK");
    }

    // LayerMask (ex : 64) → numéro du premier layer coché (ex : 6)
    static int FirstLayer(LayerMask mask)
    {
        for (int i = 0; i < 32; i++)
            if ((mask.value & (1 << i)) != 0) return i;
        return -1;
    }
}
