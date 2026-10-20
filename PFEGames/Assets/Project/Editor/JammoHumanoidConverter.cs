using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Outil a lancer une fois : Tools > Jammo > Convertir en Humanoid
// 1. Passe Jammo_LowPoly.fbx en Humanoid (Create From This Model)
// 2. Passe toutes les animations (dossier Jammo + clips utilises par l'Animator du Player) en Humanoid
//    avec "Copy From Other Avatar" = l'avatar de Jammo, et bake le root (anim sur place)
// 3. Assigne l'avatar dans le prefab Player et coupe Apply Root Motion (c'est le Rigidbody qui deplace)
public static class JammoHumanoidConverter
{
    const string ModelPath = "Assets/Jammo-Character/Models/Jammo_LowPoly.fbx";
    const string JammoFolder = "Assets/Jammo-Character";
    const string PlayerPrefabPath = "Assets/Project/Prefabs/Characters/Player .prefab";

    // Les clips dont le nom contient un de ces mots tournent en boucle
    static readonly string[] LoopKeywords = { "Idle", "Walk", "Run", "Sprint", "Push", "Pull", "Climbing Up", "Crouch" };

    [MenuItem("Tools/Jammo/Convertir en Humanoid")]
    static void Convert()
    {
        // ---------- 1. Le modele ----------
        ModelImporter modelImporter = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
        if (modelImporter == null)
        {
            Debug.LogError($"[Jammo] Modele introuvable : {ModelPath}");
            return;
        }

        modelImporter.animationType = ModelImporterAnimationType.Human;
        modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        modelImporter.SaveAndReimport();

        Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<Avatar>().FirstOrDefault();
        if (avatar == null || !avatar.isValid || !avatar.isHuman)
        {
            Debug.LogError("[Jammo] L'avatar Humanoid n'a pas pu etre cree. Ouvre Jammo_LowPoly > Rig > Configure pour voir les os manquants.");
            return;
        }
        Debug.Log($"[Jammo] Avatar Humanoid cree : {avatar.name}");

        // ---------- 2. Les animations ----------
        HashSet<string> animationPaths = new HashSet<string>();

        foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { JammoFolder }))
        {
            animationPaths.Add(AssetDatabase.GUIDToAssetPath(guid));
        }

        foreach (string clipPath in GetPlayerClipPaths())
        {
            animationPaths.Add(clipPath);
        }

        animationPaths.Remove(ModelPath);

        int converted = 0;
        foreach (string path in animationPaths)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) continue;

            ConvertAnimation(importer, avatar);
            converted++;
            Debug.Log($"[Jammo] Animation convertie : {path}");
        }

        // ---------- 3. Le prefab Player ----------
        UpdatePlayerPrefab(avatar);

        AssetDatabase.SaveAssets();
        Debug.Log($"[Jammo] Termine : modele + {converted} fichier(s) d'animation en Humanoid.");
    }

    static void ConvertAnimation(ModelImporter importer, Avatar avatar)
    {
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
        importer.sourceAvatar = avatar;

        // On garde les clips deja configures (loop, decoupe...), sinon on part des clips par defaut
        ModelImporterClipAnimation[] clips = importer.clipAnimations;
        bool hadCustomClips = clips != null && clips.Length > 0;
        if (!hadCustomClips)
        {
            clips = importer.defaultClipAnimations;
        }

        foreach (ModelImporterClipAnimation clip in clips)
        {
            // Root Transform : Rotation et Y "Bake Into Pose", XZ NON bake :
            // le deplacement horizontal part dans le root motion, ignore car Apply Root Motion est desactive
            // -> l'anim reste sur place (si XZ etait bake, le corps glisserait puis reviendrait a chaque boucle)
            clip.lockRootRotation = true;      // Rotation
            clip.keepOriginalOrientation = false; // Based Upon : Body Orientation
            clip.lockRootHeightY = true;       // Position (Y)
            clip.keepOriginalPositionY = false;
            clip.heightFromFeet = true;        // Based Upon : Feet
            clip.lockRootPositionXZ = false;   // Position (XZ)
            clip.keepOriginalPositionXZ = false;

            if (!hadCustomClips)
            {
                bool loop = LoopKeywords.Any(keyword => importer.assetPath.Contains(keyword) || clip.name.Contains(keyword));
                clip.loopTime = loop;
                clip.loopPose = loop;
            }
        }

        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    // Chemins des FBX dont viennent les clips de l'Animator du Player
    static IEnumerable<string> GetPlayerClipPaths()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[Jammo] Prefab Player introuvable : {PlayerPrefabPath}");
            yield break;
        }

        foreach (Animator animator in prefab.GetComponentsInChildren<Animator>(true))
        {
            if (animator.runtimeAnimatorController == null) continue;

            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                string path = AssetDatabase.GetAssetPath(clip);
                if (path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                {
                    yield return path;
                }
            }
        }
    }

    static void UpdatePlayerPrefab(Avatar avatar)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        if (root == null) return;

        foreach (Animator animator in root.GetComponentsInChildren<Animator>(true))
        {
            animator.avatar = avatar;
            animator.applyRootMotion = false;
            Debug.Log($"[Jammo] Animator mis a jour : {animator.gameObject.name}");
        }

        PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }
}
