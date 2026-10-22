// À placer HORS d'un dossier Editor (ex: Assets/Scripts/SplinePlacer.cs).
// Ce composant ne contient que les DONNÉES du tracé : c'est ce qui permet de
// le rééditer à tout moment. La génération des prefabs se fait dans l'éditeur
// (voir SplinePlacerWindow.cs).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ToolSaverio
{
    /// <summary>Axe local du prefab qui représente sa longueur (celui qui suit la spline).</summary>
    public enum LengthAxis { X, Z }

    /// <summary>Stretch : les pièces s'étirent pour remplir exactement. Repeat : taille fixe, on s'arrête avant de dépasser.</summary>
    public enum FitMode { Stretch, Repeat }

    /// <summary>Orientation du prefab de point d'arrêt (poteau / angle).</summary>
    public enum BreakAlign { Incoming, Outgoing, Bisector }

    [Serializable]
    public class SplinePoint
    {
        public Vector3 position;              // espace local du SplinePlacer
        public Vector3 rotationOffset;        // euler (°), appliqué après l'orientation sur la tangente
        public Vector3 scale = Vector3.one;   // multiplicateur
    }

    [Serializable]
    public class SplineSection
    {
        public GameObject prefab;             // prefab posé en répétition sur cette section
        public List<SplinePoint> points = new List<SplinePoint>();
    }

    [DisallowMultipleComponent]
    public class SplinePlacer : MonoBehaviour
    {
        // Tracé : une section = une spline. Un point d'arrêt coupe la section en deux.
        public List<SplineSection> sections = new List<SplineSection>();

        // Prefab posé à chaque point d'arrêt (début de chaque section après la première)
        public GameObject breakPrefab;

        // EmptyParentObject dans lequel les prefabs générés sont rangés
        public Transform targetParent;

        // Objet qui contient les prefabs générés (créé et géré par l'outil)
        public Transform outputRoot;

        // Réglages de pose
        public LengthAxis lengthAxis = LengthAxis.X;
        public FitMode fitMode = FitMode.Stretch;
        public float gap = 0f;
        public bool smooth = true;
        public bool followSlope = false;
        public float snap = 0f;
        public BreakAlign breakAlign = BreakAlign.Bisector;
    }
}