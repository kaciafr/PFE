using UnityEditor;
using UnityEngine;


public static  class SetLayerTool
{
    [MenuItem("Tools/Goblfin/Layers/Grabable")]
    static void SetGrabable() => SetLayerOnSelection("Grabable ");
    
    [MenuItem("Tools/Goblfin/Layers/Interactable")]
    static void SetInteractable() => SetLayerOnSelection("Interactable ");
    
    [MenuItem("Tools/Goblfin/Layers/Ground")]
    static void SetGround() => SetLayerOnSelection("Ground");
    
    [MenuItem("Tools/Goblfin/Layers/Climb")]
    static void SetClimb() => SetLayerOnSelection("Obstacle");
    
    [MenuItem("Tools/Goblfin/Layers/Guard")]
    static void SetGuard() => SetLayerOnSelection("Guard");




    private static void SetLayerOnSelection(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            EditorUtility.DisplayDialog("Layer", $"Le layer \"{layerName}\" n'existe pas.", "OK");
            return;
        }

        if (Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Layer", "Selectionne d'abord un ou plusieurs objets dans la Hierarchy.", "OK");
            return;
        }

        foreach (GameObject gameObject in Selection.gameObjects)
        {
            AddLayerTo(gameObject.transform, layer);
        }
    }


    private static void AddLayerTo(Transform target, int layer)
    {
        Undo.RecordObject(target.gameObject, "Set Layer");
        target.gameObject.layer = layer;

        foreach (Transform child in target)
        {
            AddLayerTo(child, layer);
        }
    }
}