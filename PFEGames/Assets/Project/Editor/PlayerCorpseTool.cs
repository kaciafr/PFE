using Goblfin.CharactersSystem.Player.Component;
using Goblfin.Player.Component;
using Goblfin.RagdollSystem;
using UnityEditor;
using UnityEngine;

// Fabrique le prefab du cadavre à partir du modèle du joueur, puis le branche sur PlayerRagdoll
public static class PlayerCorpseTool
{
    const string PlayerPath = "Assets/Project/Prefabs/Characters/Player .prefab";
    const string CorpsePath = "Assets/Project/Prefabs/Characters/PlayerCorpse.prefab";

    [MenuItem("Tools/Goblfin/Créer le prefab Cadavre du joueur")]
    static void CreateCorpse()
    {
        // 1. Ouvrir le prefab du joueur et trouver le modèle (l'Animator qui a un avatar)
        GameObject player = PrefabUtility.LoadPrefabContents(PlayerPath);
        Animator model = FindCharacterAnimator(player.transform);
        PlayerRagdoll ragdoll = player.GetComponentInChildren<PlayerRagdoll>();

        if (model == null || ragdoll == null)
        {
            EditorUtility.DisplayDialog("Cadavre", "Pas trouvé l'Animator ou le PlayerRagdoll dans le prefab Player.", "OK");
            PrefabUtility.UnloadPrefabContents(player);
            return;
        }

        SetupRagdollGrab(player);
        SetupCarryArms(player, model);

        // 2. Copier le modèle (on garde tous les GameObjects pour que les os restent dans le même ordre)
        GameObject corpse = Object.Instantiate(model.gameObject);
        corpse.name = "PlayerCorpse";
        corpse.transform.localScale = model.transform.lossyScale;

        // 3. Enlever les scripts du joueur, l'Animator, les triggers et les joints du grab
        // (2 passages : un script qui en "requiert" un autre doit partir avant lui)
        for (int pass = 0; pass < 2; pass++)
            foreach (MonoBehaviour script in corpse.GetComponentsInChildren<MonoBehaviour>(true))
                Object.DestroyImmediate(script);
        Object.DestroyImmediate(corpse.GetComponent<Animator>());

        foreach (SpringJoint spring in corpse.GetComponentsInChildren<SpringJoint>(true))
            Object.DestroyImmediate(spring);

        foreach (Collider col in corpse.GetComponentsInChildren<Collider>(true))
        {
            if (col.isTrigger) Object.DestroyImmediate(col);
            else col.enabled = true;
        }

        // 4. Activer la physique des os (un Rigidbody sans collider reste figé, sinon il tombe dans le vide)
        //    Layer Default : pas Guard, sinon le grab de caisses le prend pour une caisse
        foreach (Rigidbody bone in corpse.GetComponentsInChildren<Rigidbody>(true))
        {
            bone.isKinematic = bone.GetComponent<Collider>() == null;
            bone.gameObject.layer = LayerMask.NameToLayer("Default");
        }

        // 5. Les hanches (premier os) sur le layer que le système de lancer détecte :
        //    on ramasse le cadavre par les hanches, et PlayerCorpse fige le reste du corps avec elles
        Rigidbody hips = corpse.GetComponentsInChildren<Rigidbody>(true)[0];
        hips.gameObject.layer = LayerMask.NameToLayer("Interactable ");

        corpse.AddComponent<PlayerCorpse>();

        // 6. Enregistrer le prefab du cadavre
        GameObject corpseAsset = PrefabUtility.SaveAsPrefabAsset(corpse, CorpsePath);
        Object.DestroyImmediate(corpse);

        // 7. Le brancher dans le champ "Corpse Prefab" de PlayerRagdoll
        SerializedObject ragdollData = new SerializedObject(ragdoll);
        ragdollData.FindProperty("corpsePrefab").objectReferenceValue = corpseAsset.GetComponent<PlayerCorpse>();
        ragdollData.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(player, PlayerPath);
        PrefabUtility.UnloadPrefabContents(player);

        Selection.activeObject = corpseAsset;
        EditorUtility.DisplayDialog("Cadavre", "PlayerCorpse.prefab créé et branché sur PlayerRagdoll.\nGrabAnchor (SpringJoint) branché sur Grab.", "OK");
    }

    // Branche le SpringJoint qui manquait au Grab du joueur (sans lui, TryGrab n'accroche rien)
    static void SetupRagdollGrab(GameObject player)
    {
        Grab grab = player.GetComponentInChildren<Grab>(true);
        if (grab == null) return;

        // 1. Une zone de détection sur Grab pour voir les corps à portée
        if (grab.GetComponent<Collider>() == null)
        {
            SphereCollider zone = grab.gameObject.AddComponent<SphereCollider>();
            zone.isTrigger = true;
            zone.radius = 0.6f;
        }

        // Layer "Ignore Raycast" : sinon la KillZone des gardes voit cette zone comme le joueur
        // et le tue de plus loin (mort en boucle au respawn)
        grab.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        // 2. Le point qui tire le corps : sous la racine (pas dans le squelette, sinon PlayerRagdoll le prend pour un os)
        Transform anchor = player.transform.Find("GrabAnchor");
        if (anchor == null)
        {
            anchor = new GameObject("GrabAnchor").transform;
            anchor.SetParent(player.transform, false);
            anchor.localPosition = new Vector3(0f, 0.8f, 0.6f);
        }

        // 3. Rigidbody kinematic : il suit le joueur sans être tiré par le ressort
        if (!anchor.TryGetComponent(out Rigidbody body))
            body = anchor.gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        // 4. Le ressort qui relie ce point au corps attrapé
        if (!anchor.TryGetComponent(out SpringJoint spring))
            spring = anchor.gameObject.AddComponent<SpringJoint>();
        spring.spring = 800f;
        spring.damper = 50f;
        spring.maxDistance = 0.2f;
        spring.anchor = Vector3.zero;
        spring.autoConfigureConnectedAnchor = false;
        spring.connectedAnchor = Vector3.zero;

        // 5. Mettre ce ressort dans le champ "Joint" du Grab
        SerializedObject grabData = new SerializedObject(grab);
        grabData.FindProperty("joint").objectReferenceValue = spring;
        grabData.ApplyModifiedPropertiesWithoutUndo();
    }

    // Ajoute CarryArms sur le joueur et le branche sur les rigs des bras qui existent déjà
    static void SetupCarryArms(GameObject player, Animator model)
    {
        if (!player.TryGetComponent(out CarryArms arms))
            arms = player.AddComponent<CarryArms>();

        SerializedObject data = new SerializedObject(arms);
        data.FindProperty("rightArmRig").objectReferenceValue = FindByName(player, "RigBrasDroit")?.GetComponent<UnityEngine.Animations.Rigging.Rig>();
        data.FindProperty("leftArmRig").objectReferenceValue = FindByName(player, "RigBrasGauche")?.GetComponent<UnityEngine.Animations.Rigging.Rig>();
        data.FindProperty("rightHandTarget").objectReferenceValue = FindByName(player, "MainDroiteTarget");
        data.FindProperty("leftHandTarget").objectReferenceValue = FindByName(player, "MainGaucheTarget");
        data.FindProperty("head").objectReferenceValue = model.GetBoneTransform(HumanBodyBones.Head);
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    // Cherche un enfant par son nom (Trim : certains noms finissent par un espace, ex "RigBrasDroit ")
    static Transform FindByName(GameObject root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Trim() == name) return child;
        }

        Debug.LogWarning($"PlayerCorpseTool : \"{name}\" introuvable dans le Player");
        return null;
    }

    static Animator FindCharacterAnimator(Transform root)
    {
        foreach (Animator candidate in root.GetComponentsInChildren<Animator>(true))
        {
            if (candidate.avatar != null) return candidate;
        }

        return null;
    }
}
