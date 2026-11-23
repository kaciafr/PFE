using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Outil d'éditeur : sélectionne le fromage puis Tools > FX > Ajouter vapeur + mouches.
// Crée FX_Stink (vapeur verte) et FX_Flies (mouches) en enfants de l'objet sélectionné.
public static class CheeseStinkFxTool
{
    private const string MaterialFolder = "Assets/Project/Art/FX";

    [MenuItem("Tools/FX/Ajouter vapeur + mouches")]
    private static void AddFx()
    {
        GameObject target = Selection.activeGameObject;
        if (target == null)
        {
            EditorUtility.DisplayDialog("FX fromage", "Sélectionne d'abord le fromage dans la Hierarchy.", "OK");
            return;
        }

        // Si on a sélectionné un FX par erreur, on remonte au fromage
        while (target.name.StartsWith("FX_") && target.transform.parent != null)
            target = target.transform.parent.gameObject;

        // Étape 1 : trouver le haut du fromage (en coordonnées monde)
        Vector3 top = target.transform.position;
        Renderer rend = target.GetComponent<Renderer>();
        if (rend != null)
            top = new Vector3(rend.bounds.center.x, rend.bounds.max.y, rend.bounds.center.z);

        // Étape 2 : créer les matériaux (vert transparent et noir)
        // Si FX_Stink existe déjà, on garde son matériau (et donc ta texture)
        Material stinkMat = GetExistingMaterial(target.transform, "FX_Stink");
        if (stinkMat == null)
            stinkMat = GetOrCreateMaterial("M_FX_Stink", new Color32(0xC4, 0xD9, 0x8A, 255));
        Material fliesMat = GetOrCreateMaterial("M_FX_Flies", Color.black);

        // Étape 3 : créer les deux effets
        CreateStink(target.transform, top, stinkMat);
        CreateFlies(target.transform, top, fliesMat);

        EditorUtility.SetDirty(target);
        Debug.Log("FX_Stink et FX_Flies ajoutés sur " + target.name + ". Pense à sauvegarder la scène.");
    }

    private const uint StinkSeed = 12345;

    private static void CreateStink(Transform parent, Vector3 top, Material mat)
    {
        // Bords nets (style cartoon)
        mat.SetFloat("_AlphaClip", 1f);
        mat.SetFloat("_Cutoff", 0.3f);
        mat.EnableKeyword("_ALPHATEST_ON");
        EditorUtility.SetDirty(mat);

        // La vapeur verte, dessinée devant
        ParticleSystem ps = CreateParticleObject("FX_Stink", parent, top + Vector3.up * 0.05f, mat);
        ConfigureStink(ps, 1f, new Color32(0xD8, 0xFF, 0x7A, 255));
        ps.GetComponent<ParticleSystemRenderer>().sortingOrder = 1;

        // Le contour : même vapeur, plus grosse et vert foncé, dessinée derrière
        ParticleSystem outline = CreateParticleObject("FX_StinkOutline", ps.transform, ps.transform.position, mat);
        outline.transform.localRotation = Quaternion.identity;
        ConfigureStink(outline, 1.15f, new Color32(0x2F, 0x5A, 0x12, 255));
        outline.GetComponent<ParticleSystemRenderer>().sortingOrder = 0;
    }

    // Réglages communs à la vapeur et à son contour
    private static void ConfigureStink(ParticleSystem ps, float sizeMultiplier, Color color)
    {
        // Même seed = mêmes particules, donc le contour suit exactement la vapeur
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.useAutoRandomSeed = false;
        ps.randomSeed = StinkSeed;

        // Style cartoon : bouffées opaques, couleur franche, bien séparées
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.8f, 2.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.25f, 0.3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.18f * sizeMultiplier, 0.26f * sizeMultiplier);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.startColor = color;
        main.gravityModifier = 0f;

        var emission = ps.emission;
        emission.rateOverTime = 7f;

        // Serré à la base, s'ouvre en V vers le haut
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 0.03f;

        // Opaque presque toute sa vie, juste un fondu très court au début et à la fin
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.08f), new GradientAlphaKey(1f, 0.85f), new GradientAlphaKey(0f, 1f) });
        col.color = gradient;

        // "Pop" : grossit d'un coup, gonfle un peu en montant, puis rétrécit jusqu'à disparaître
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        AnimationCurve pop = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.1f, 0.8f),
            new Keyframe(0.75f, 1.25f),
            new Keyframe(1f, 0f));
        size.size = new ParticleSystem.MinMaxCurve(1f, pop);

        // Chaque bouffée tourne sur elle-même
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-45f * Mathf.Deg2Rad, 45f * Mathf.Deg2Rad);

        // Petit tortillement en montant
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.08f;
        noise.frequency = 0.8f;
        noise.scrollSpeed = 0.4f;
    }

    private static void CreateFlies(Transform parent, Vector3 top, Material mat)
    {
        ParticleSystem ps = CreateParticleObject("FX_Flies", parent, top + Vector3.up * 0.2f, mat);
        ps.transform.rotation = Quaternion.identity;

        var main = ps.main;
        main.startLifetime = 5f;
        main.startSpeed = 0f;
        main.startSize = 0.03f;
        main.startColor = Color.black;
        main.maxParticles = 4;
        main.prewarm = true;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 1f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        // Les mouches tournent autour du fromage
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.y = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.orbitalY = new ParticleSystem.MinMaxCurve(3f, 6f);
        velocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f, 0f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.5f;
        noise.frequency = 2f;
    }

    // Crée un GameObject avec un Particle System, tourné vers le haut (Rotation X = -90)
    private static ParticleSystem CreateParticleObject(string name, Transform parent, Vector3 worldPos, Material mat)
    {
        Transform old = parent.Find(name);
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Ajouter " + name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        // La taille de l'effet ne dépend pas de l'échelle du fromage
        var main = ps.main;
        main.scalingMode = ParticleSystemScalingMode.Local;

        ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = mat;

        return ps;
    }

    // Récupère le matériau d'un FX déjà présent sous le fromage (null s'il n'existe pas)
    private static Material GetExistingMaterial(Transform parent, string fxName)
    {
        Transform fx = parent.Find(fxName);
        if (fx == null) return null;

        ParticleSystemRenderer psr = fx.GetComponent<ParticleSystemRenderer>();
        return psr != null ? psr.sharedMaterial : null;
    }

    // Génère une texture ronde avec un bord flou (blanc au centre, transparent sur les bords)
    private static Texture2D GetOrCreateSoftDot()
    {
        string path = MaterialFolder + "/T_FX_SoftDot.png";
        Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (existing != null) return existing;

        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder("Assets/Project/Art", "FX");

        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size - 1, size - 1) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / (size * 0.5f);
                float alpha = Mathf.Clamp01(1f - dist);
                alpha = alpha * alpha;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    // Matériau URP Particles/Unlit en Transparent
    private static Material GetOrCreateMaterial(string matName, Color color)
    {
        string path = MaterialFolder + "/" + matName + ".mat";
        // Si le matériau existe déjà, on n'y touche pas (pour garder ta texture)
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;

        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder("Assets/Project/Art", "FX");
        mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));

        // Texture ronde et floue (sinon les particules sont des carrés)
        Texture2D softDot = GetOrCreateSoftDot();

        mat.SetColor("_BaseColor", color);
        mat.SetTexture("_BaseMap", softDot);

        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)RenderQueue.Transparent;

        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        return mat;
    }
}
