using Goblfin.CharactersSystem.Player.Component;
using UnityEditor;
using UnityEngine;

 
public static  class SnapPointCrateTool
{
    const float DistanceFromFace = 0.2f;

    [MenuItem("Tools/Goblfin/Add SnapPoints to selected crates")]
    static void AddSnapPoints()
    {
        if (Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("SnapPoints", "Selectionne d'abord une ou plusieurs caisses dans la Hierarchy.", "OK");
            return;
        }

        foreach (GameObject crate in Selection.gameObjects)
        {
            AddSnapPointsTo(crate.transform);
        }
    }

    static void AddSnapPointsTo(Transform crate)
    {
        SetupCrate(crate);

        if (!TryGetLocalBounds(crate, out Bounds bounds))
        {
            Debug.LogWarning($"[SnapPoints] {crate.name} n'a ni Collider ni Mesh : impossible de calculer sa taille.");
            return;
        }

        foreach (SnapPoint old in crate.GetComponentsInChildren<SnapPoint>())
        {
            Undo.DestroyObjectImmediate(old.gameObject);
        }

        Vector3 center = crate.TransformPoint(bounds.center);
        Vector3 halfSize = Vector3.Scale(bounds.extents, crate.lossyScale);

        CreatePoint(crate, "SnapPoint_Droite", center, crate.right, halfSize.x);
        CreatePoint(crate, "SnapPoint_Gauche", center, -crate.right, halfSize.x);
        CreatePoint(crate, "SnapPoint_Avant", center, crate.forward, halfSize.z);
        CreatePoint(crate, "SnapPoint_Arriere", center, -crate.forward, halfSize.z);

        Debug.Log($"[SnapPoints] 4 SnapPoints poses sur {crate.name}.");
    }

    static void CreatePoint(Transform crate, string name, Vector3 center, Vector3 faceDirection, float halfSize)
    {
        GameObject point = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(point, "Ajouter SnapPoint");

        point.transform.SetParent(crate, true);
        point.transform.position = center + faceDirection * (Mathf.Abs(halfSize) + DistanceFromFace);
        point.transform.rotation = Quaternion.LookRotation(-faceDirection, Vector3.up);  

        point.AddComponent<SnapPoint>();
    }

    static void SetupCrate(Transform crate)
    {
        GameObject go = crate.gameObject;
        int layer = LayerMask.NameToLayer("Grabable ");
        if (layer != -1 && go.layer != layer)
        {
            Undo.RecordObject(go, "Crate Layer");
            go.layer = layer;
        }

        if (!go.TryGetComponent(out Rigidbody body))
        {
            body = Undo.AddComponent<Rigidbody>(go);
            body.mass = 10f;
            body.linearDamping = 0.5f;
            body.angularDamping = 0.5f;
        }

        if (!go.TryGetComponent(out Collider col))
        {
            col = Undo.AddComponent<BoxCollider>(go);
        }

        Undo.RecordObject(body, "Crate Rigidbody");
        body.mass = 10f;
        body.isKinematic = true;
        body.constraints = RigidbodyConstraints.FreezeRotation;

    }

    static void CrateSetupHeavy(Transform crate)
    {
        SetupCrate(crate);

        Rigidbody body = crate.GetComponent<Rigidbody>();
        Undo.RecordObject(body, "Crate Rigidbody");
        body.mass = 100f;
        body.isKinematic = true;
        body.constraints = RigidbodyConstraints.FreezeRotation;
    }

    static bool TryGetLocalBounds(Transform crate, out Bounds bounds)
    {
        if (crate.TryGetComponent(out BoxCollider box))
        {
            bounds = new Bounds(box.center, box.size);
            return true;
        }

        if (crate.TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh != null)
        {
            bounds = meshFilter.sharedMesh.bounds;
            return true;
        }

        bounds = default;
        return false;
    }
}
