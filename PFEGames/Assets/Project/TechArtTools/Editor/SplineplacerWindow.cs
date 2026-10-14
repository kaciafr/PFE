// À placer DANS un dossier Editor (ex: Assets/Editor/SplinePlacerWindow.cs).
// Dépend de IosUI (défini dans BrushWindow.cs) et de SplinePlacer.cs.
// Menu : Tools > Spline Placer
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ToolSaverio.Editor
{
    // =====================================================================================
    //  Génération : transforme les données du SplinePlacer en prefabs instanciés
    // =====================================================================================
    internal static class SplineBuilder
    {
        struct Placement
        {
            public GameObject prefab;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }

        struct PrefabDims
        {
            public Vector3 size, center;
        }

        class Poly
        {
            public readonly List<Vector3> v = new List<Vector3>();
            public readonly List<float> frac = new List<float>(); // index fractionnaire de point (pour interpoler rotation / scale)
            public readonly List<float> cum = new List<float>();  // longueur cumulée
            public float Length => cum.Count > 0 ? cum[cum.Count - 1] : 0f;
        }

        // ---------------------------------------------------------------- API

        public static void Rebuild(SplinePlacer p)
        {
            if (p == null) return;

            var dims = new Dictionary<GameObject, PrefabDims>();
            var list = new List<Placement>();

            for (int s = 0; s < p.sections.Count; s++)
            {
                var sec = p.sections[s];
                if (sec.points.Count == 0) continue;

                if (sec.prefab != null && sec.points.Count >= 2)
                    PlaceAlongSection(p, sec, dims, list);

                if (s > 0 && p.breakPrefab != null)
                    PlaceBreak(p, s, list);
            }

            if (list.Count == 0 && p.outputRoot == null) return;

            var root = EnsureRoot(p);
            Apply(root, list);
            EditorSceneManager.MarkSceneDirty(p.gameObject.scene);
        }

        public static List<Vector3> PathVertices(SplinePlacer p, SplineSection sec)
        {
            if (sec.points.Count == 0) return new List<Vector3>();
            return BuildPoly(p, sec).v;
        }

        public static float PathLength(SplinePlacer p, SplineSection sec)
        {
            if (sec.points.Count < 2) return 0f;
            return BuildPoly(p, sec).Length;
        }

        public static Quaternion PointFrame(SplinePlacer p, SplineSection sec, int i)
        {
            var pts = sec.points;
            int n = pts.Count;
            Vector3 W(int k) => p.transform.TransformPoint(pts[Mathf.Clamp(k, 0, n - 1)].position);
            return Frame(p, W(i + 1) - W(i - 1));
        }

        // ---------------------------------------------------------------- Racine de sortie

        static Transform EnsureRoot(SplinePlacer p)
        {
            Transform parent = p.targetParent != null ? p.targetParent : p.transform.parent;

            if (p.outputRoot == null)
            {
                var go = new GameObject(p.name + " (généré)");
                go.transform.SetParent(parent, false);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                p.outputRoot = go.transform;
                EditorUtility.SetDirty(p);
            }
            else if (p.outputRoot.parent != parent)
            {
                p.outputRoot.SetParent(parent, true);
            }
            return p.outputRoot;
        }

        static void Apply(Transform root, List<Placement> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var pl = list[i];
                GameObject go = null;

                // Réutilise l'objet existant si c'est le bon prefab (évite de tout recréer à chaque drag)
                if (i < root.childCount)
                {
                    var existing = root.GetChild(i).gameObject;
                    if (PrefabUtility.GetCorrespondingObjectFromSource(existing) == pl.prefab) go = existing;
                    else UnityEngine.Object.DestroyImmediate(existing);
                }

                if (go == null)
                {
                    go = (GameObject)PrefabUtility.InstantiatePrefab(pl.prefab, root);
                    go.transform.SetSiblingIndex(Mathf.Min(i, root.childCount - 1));
                }

                go.transform.SetPositionAndRotation(pl.position, pl.rotation);
                go.transform.localScale = pl.scale;
            }

            for (int i = root.childCount - 1; i >= list.Count; i--)
                UnityEngine.Object.DestroyImmediate(root.GetChild(i).gameObject);
        }

        // ---------------------------------------------------------------- Courbe

        static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * ((2f * p1)
                           + (-p0 + p2) * t
                           + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
                           + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        static Poly BuildPoly(SplinePlacer p, SplineSection sec)
        {
            var pts = sec.points;
            int n = pts.Count;
            var poly = new Poly();

            Vector3 W(int i) => p.transform.TransformPoint(pts[Mathf.Clamp(i, 0, n - 1)].position);

            poly.v.Add(W(0));
            poly.frac.Add(0f);

            for (int i = 0; i < n - 1; i++)
            {
                if (p.smooth && n > 2)
                {
                    const int steps = 16;
                    for (int k = 1; k <= steps; k++)
                    {
                        float t = k / (float)steps;
                        poly.v.Add(CatmullRom(W(i - 1), W(i), W(i + 1), W(i + 2), t));
                        poly.frac.Add(i + t);
                    }
                }
                else
                {
                    poly.v.Add(W(i + 1));
                    poly.frac.Add(i + 1);
                }
            }

            poly.cum.Add(0f);
            for (int i = 1; i < poly.v.Count; i++)
                poly.cum.Add(poly.cum[i - 1] + Vector3.Distance(poly.v[i - 1], poly.v[i]));

            return poly;
        }

        static void Eval(Poly poly, float d, out Vector3 pos, out Vector3 tangent, out float frac)
        {
            d = Mathf.Clamp(d, 0f, poly.Length);

            int j = 0;
            int last = poly.cum.Count - 2;
            while (j < last && poly.cum[j + 1] < d) j++;

            float segLen = poly.cum[j + 1] - poly.cum[j];
            float t = segLen > 1e-6f ? (d - poly.cum[j]) / segLen : 0f;

            pos = Vector3.Lerp(poly.v[j], poly.v[j + 1], t);
            Vector3 dir = poly.v[j + 1] - poly.v[j];
            tangent = dir.sqrMagnitude > 1e-8f ? dir.normalized : Vector3.forward;
            frac = Mathf.Lerp(poly.frac[j], poly.frac[j + 1], t);
        }

        static void Interp(SplineSection sec, float frac, out Quaternion rot, out Vector3 scale)
        {
            var pts = sec.points;
            int n = pts.Count;
            int i = Mathf.Clamp((int)frac, 0, Mathf.Max(0, n - 2));
            int j = Mathf.Min(i + 1, n - 1);
            float f = Mathf.Clamp01(frac - i);

            rot = Quaternion.Slerp(Quaternion.Euler(pts[i].rotationOffset), Quaternion.Euler(pts[j].rotationOffset), f);
            scale = Vector3.Lerp(pts[i].scale, pts[j].scale, f);
        }

        /// <summary>Rotation qui aligne l'axe de longueur du prefab sur la tangente.</summary>
        public static Quaternion Frame(SplinePlacer p, Vector3 tangent)
        {
            Vector3 t = tangent;
            if (!p.followSlope) t.y = 0f;
            if (t.sqrMagnitude < 1e-6f) t = Vector3.forward;

            Quaternion q = Quaternion.LookRotation(t.normalized, Vector3.up);
            if (p.lengthAxis == LengthAxis.X) q *= Quaternion.Euler(0f, -90f, 0f);
            return q;
        }

        // ---------------------------------------------------------------- Dimensions des prefabs

        static PrefabDims GetDims(GameObject prefab, Dictionary<GameObject, PrefabDims> cache)
        {
            if (cache.TryGetValue(prefab, out var cached)) return cached;

            bool has = false;
            Bounds b = new Bounds(Vector3.zero, Vector3.one);
            Matrix4x4 toRoot = prefab.transform.worldToLocalMatrix;

            foreach (var mf in prefab.GetComponentsInChildren<MeshFilter>())
            {
                if (mf.sharedMesh == null) continue;
                Matrix4x4 m = toRoot * mf.transform.localToWorldMatrix;
                Bounds mb = mf.sharedMesh.bounds;

                for (int sx = -1; sx <= 1; sx += 2)
                for (int sy = -1; sy <= 1; sy += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    Vector3 corner = m.MultiplyPoint3x4(mb.center + Vector3.Scale(mb.extents, new Vector3(sx, sy, sz)));
                    if (!has) { b = new Bounds(corner, Vector3.zero); has = true; }
                    else b.Encapsulate(corner);
                }
            }

            var dims = new PrefabDims { size = b.size, center = b.center };
            cache[prefab] = dims;
            return dims;
        }

        // ---------------------------------------------------------------- Pose le long d'une section

        static void PlaceAlongSection(SplinePlacer p, SplineSection sec, Dictionary<GameObject, PrefabDims> dims, List<Placement> output)
        {
            var poly = BuildPoly(p, sec);
            float total = poly.Length;
            if (total < 0.01f) return;

            var dim = GetDims(sec.prefab, dims);
            int axis = p.lengthAxis == LengthAxis.X ? 0 : 2;
            Vector3 axisVec = axis == 0 ? Vector3.right : Vector3.forward;
            Vector3 baseScale = sec.prefab.transform.localScale;

            float lengthBase = Mathf.Max(0.01f, dim.size[axis] * Mathf.Abs(baseScale[axis]));
            float centerOffset = dim.center[axis] * baseScale[axis]; // pivot -> centre de la pièce
            float gap = Mathf.Max(0f, p.gap);

            // Pose une pièce dont le CENTRE est à la distance centerDist sur la courbe
            void Emit(float centerDist, float stretch)
            {
                Eval(poly, centerDist, out Vector3 pos, out Vector3 tan, out float frac);
                Interp(sec, frac, out Quaternion offRot, out Vector3 mult);
                if (stretch > 0f) mult[axis] = stretch;

                Quaternion rot = Frame(p, tan) * offRot;
                Vector3 pivot = pos - rot * axisVec * (centerOffset * mult[axis]);

                output.Add(new Placement
                {
                    prefab = sec.prefab,
                    position = pivot,
                    rotation = rot,
                    scale = Vector3.Scale(baseScale, mult)
                });
            }

            if (p.fitMode == FitMode.Stretch)
            {
                int count = Mathf.Max(1, Mathf.RoundToInt((total + gap) / (lengthBase + gap)));
                float step = total / count;
                float pieceLen = Mathf.Max(0.01f, step - gap);
                float stretch = pieceLen / lengthBase;

                for (int k = 0; k < count; k++)
                    Emit(k * step + pieceLen * 0.5f, stretch);
            }
            else
            {
                float d = 0f;
                for (int guard = 0; guard < 5000; guard++)
                {
                    Eval(poly, d, out _, out _, out float f0);
                    Interp(sec, f0, out _, out Vector3 s0);

                    float len = lengthBase * Mathf.Max(0.01f, s0[axis]);
                    if (d + len > total + 0.001f) break;

                    Emit(d + len * 0.5f, -1f);
                    d += len + gap;
                }
            }
        }

        // ---------------------------------------------------------------- Point d'arrêt

        static void PlaceBreak(SplinePlacer p, int s, List<Placement> output)
        {
            var sec = p.sections[s];
            var prev = p.sections[s - 1];
            var pt = sec.points[0];

            Vector3 pos = p.transform.TransformPoint(pt.position);

            bool hasIn = false, hasOut = false;
            Vector3 inDir = Vector3.forward, outDir = Vector3.forward;

            if (prev.points.Count >= 2)
            {
                var poly = BuildPoly(p, prev);
                Eval(poly, poly.Length, out _, out inDir, out _);
                hasIn = true;
            }
            if (sec.points.Count >= 2)
            {
                var poly = BuildPoly(p, sec);
                Eval(poly, 0f, out _, out outDir, out _);
                hasOut = true;
            }

            Quaternion frameIn = Frame(p, inDir);
            Quaternion frameOut = Frame(p, outDir);
            Quaternion frame;

            if (!hasIn && !hasOut) frame = Frame(p, Vector3.forward);
            else if (!hasIn) frame = frameOut;
            else if (!hasOut) frame = frameIn;
            else
            {
                switch (p.breakAlign)
                {
                    case BreakAlign.Incoming: frame = frameIn; break;
                    case BreakAlign.Outgoing: frame = frameOut; break;
                    default: frame = Quaternion.Slerp(frameIn, frameOut, 0.5f); break;
                }
            }

            output.Add(new Placement
            {
                prefab = p.breakPrefab,
                position = pos,
                rotation = frame * Quaternion.Euler(pt.rotationOffset),
                scale = Vector3.Scale(p.breakPrefab.transform.localScale, pt.scale)
            });
        }
    }

    // =====================================================================================
    //  Fenêtre iOS + outils de Scene view
    // =====================================================================================
    public class SplinePlacerWindow : EditorWindow
    {
        [SerializeField] private SplinePlacer target;
        [SerializeField] private bool editing;
        [SerializeField] private int mode;            // 0 = Ajouter, 1 = Modifier
        [SerializeField] private int activeSection;
        [SerializeField] private int selectedPoint = -1;

        private Vector2 scroll;
        private System.Action pending;
        private bool toolsHiddenByUs;

        private const float RowH = 48f;
        private const float SectionH = 58f;

        [MenuItem("Tools/TechArtTools/SplineBuilder")]
        public static void ShowWindow()
        {
            var w = GetWindow<SplinePlacerWindow>();
            w.titleContent = new GUIContent("Spline Placer");
            w.minSize = new Vector2(400, 560);
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            SceneView.duringSceneGui += OnSceneGUI;
            Undo.undoRedoPerformed += OnUndoRedo;
            OnSelectionChange();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            Undo.undoRedoPerformed -= OnUndoRedo;
            ReleaseTools();
        }

        private void OnSelectionChange()
        {
            var go = Selection.activeGameObject;
            var p = go != null ? go.GetComponentInParent<SplinePlacer>() : null;
            if (p != null && p != target) SetTarget(p);
            Repaint();
        }

        private void OnUndoRedo()
        {
            if (target != null) SplineBuilder.Rebuild(target);
            Repaint();
            SceneView.RepaintAll();
        }

        private void SetTarget(SplinePlacer p)
        {
            target = p;
            activeSection = 0;
            selectedPoint = -1;
            if (p != null && p.sections.Count == 0) p.sections.Add(new SplineSection());
        }

        // ------------------------------------------------------------------ Accès / édition

        private SplineSection ActiveSection
        {
            get
            {
                if (target == null || target.sections.Count == 0) return null;
                activeSection = Mathf.Clamp(activeSection, 0, target.sections.Count - 1);
                return target.sections[activeSection];
            }
        }

        private SplinePoint SelectedPointData
        {
            get
            {
                var s = ActiveSection;
                if (s == null || selectedPoint < 0 || selectedPoint >= s.points.Count) return null;
                return s.points[selectedPoint];
            }
        }

        /// <summary>Applique une modification avec Undo, puis régénère les prefabs.</summary>
        private void Edit(string label, System.Action change)
        {
            if (target == null) return;
            Undo.RecordObject(target, label);
            change();
            EditorUtility.SetDirty(target);
            SplineBuilder.Rebuild(target);
            Repaint();
            SceneView.RepaintAll();
        }

        private Vector3 SnapXZ(Vector3 w)
        {
            float s = target != null ? target.snap : 0f;
            if (s <= 0.001f) return w;
            w.x = Mathf.Round(w.x / s) * s;
            w.z = Mathf.Round(w.z / s) * s;
            return w;
        }

        private static SplinePoint Clone(SplinePoint p)
        {
            return new SplinePoint { position = p.position, rotationOffset = p.rotationOffset, scale = p.scale };
        }

        // ------------------------------------------------------------------ Actions

        private void NewPlacer()
        {
            var go = new GameObject("Spline Placer");
            Undo.RegisterCreatedObjectUndo(go, "Nouveau tracé");

            var p = go.AddComponent<SplinePlacer>();
            p.sections.Add(new SplineSection());

            Selection.activeGameObject = go;
            SetTarget(p);
            editing = true;
            mode = 0;
        }

        private void AddPointAt(Vector3 world)
        {
            if (ActiveSection == null) return;
            Edit("Ajouter un point", () =>
            {
                var sec = ActiveSection;
                var prev = sec.points.Count > 0 ? sec.points[sec.points.Count - 1] : null;
                sec.points.Add(new SplinePoint
                {
                    position = target.transform.InverseTransformPoint(world),
                    rotationOffset = prev != null ? prev.rotationOffset : Vector3.zero,
                    scale = prev != null ? prev.scale : Vector3.one
                });
                selectedPoint = sec.points.Count - 1;
            });
        }

        /// <summary>
        /// Point d'arrêt : coupe la section au point sélectionné (ou au dernier point),
        /// place le prefab d'arrêt et reprend le tracé depuis ce point dans une nouvelle section.
        /// </summary>
        private void AddBreak()
        {
            var sec = ActiveSection;
            if (sec == null || sec.points.Count == 0) return;

            Edit("Point d'arrêt", () =>
            {
                int idx = (selectedPoint >= 0 && selectedPoint < sec.points.Count) ? selectedPoint : sec.points.Count - 1;

                var next = new SplineSection { prefab = sec.prefab };
                next.points.Add(Clone(sec.points[idx]));
                for (int i = idx + 1; i < sec.points.Count; i++) next.points.Add(sec.points[i]);

                sec.points.RemoveRange(idx + 1, sec.points.Count - idx - 1);
                target.sections.Insert(activeSection + 1, next);

                activeSection++;
                selectedPoint = 0;
            });
        }

        private void InsertPoint()
        {
            var sec = ActiveSection;
            int i = selectedPoint;
            if (sec == null || i < 0 || i >= sec.points.Count - 1) return;

            Edit("Insérer un point", () =>
            {
                var a = sec.points[i];
                var b = sec.points[i + 1];
                sec.points.Insert(i + 1, new SplinePoint
                {
                    position = (a.position + b.position) * 0.5f,
                    rotationOffset = Quaternion.Slerp(Quaternion.Euler(a.rotationOffset), Quaternion.Euler(b.rotationOffset), 0.5f).eulerAngles,
                    scale = (a.scale + b.scale) * 0.5f
                });
                selectedPoint = i + 1;
            });
        }

        private void DeleteSelectedPoint()
        {
            var sec = ActiveSection;
            int i = selectedPoint;
            if (sec == null || i < 0 || i >= sec.points.Count) return;

            Edit("Supprimer le point", () =>
            {
                sec.points.RemoveAt(i);
                if (sec.points.Count == 0 && target.sections.Count > 1)
                {
                    target.sections.RemoveAt(activeSection);
                    activeSection = Mathf.Clamp(activeSection - 1, 0, target.sections.Count - 1);
                }
                selectedPoint = -1;
            });
        }

        private void DeleteSection(int index)
        {
            if (target == null || index < 0 || index >= target.sections.Count) return;

            Edit("Supprimer la section", () =>
            {
                if (target.sections.Count > 1)
                {
                    target.sections.RemoveAt(index);
                    activeSection = Mathf.Clamp(activeSection, 0, target.sections.Count - 1);
                }
                else
                {
                    target.sections[0].points.Clear();
                }
                selectedPoint = -1;
            });
        }

        /// <summary>Détache le résultat : les prefabs deviennent des objets normaux, plus liés au tracé.</summary>
        private void Bake()
        {
            if (target == null || target.outputRoot == null) return;
            Undo.RecordObject(target, "Figer le résultat");
            ShowNotification(new GUIContent("Résultat figé : " + target.outputRoot.name));
            target.outputRoot = null;
            EditorUtility.SetDirty(target);
        }

        // ------------------------------------------------------------------ Fenêtre

        private static int Segmented(Rect r, string[] labels, int selected)
        {
            var P = IosUI.P;
            float w = r.width / labels.Length;

            IosUI.Fill(r, P.fill, 9f);
            if (selected >= 0)
                IosUI.Fill(new Rect(r.x + selected * w + 2f, r.y + 2f, w - 4f, r.height - 4f), P.segSel, 7f);

            for (int i = 0; i < labels.Length; i++)
                IosUI.Text(new Rect(r.x + i * w, r.y, w, r.height), labels[i], IosUI.Center, i == selected ? P.label : P.secondary);

            if (IosUI.Tap(r, out _, out _))
                return Mathf.Clamp((int)((Event.current.mousePosition.x - r.x) / w), 0, labels.Length - 1);
            return selected;
        }

        private void OnGUI()
        {
            IosUI.Ensure();

            var e = Event.current;
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag) Repaint();
            if (e.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), IosUI.P.bg);

            // État figé pour toute la passe (évite les erreurs de layout si l'état change pendant un clic)
            bool hasTarget = target != null;
            bool hasPoint = hasTarget && SelectedPointData != null;

            scroll = GUILayout.BeginScrollView(scroll, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

            GUILayout.Space(18);
            DrawHeader();
            GUILayout.Space(16);

            if (!hasTarget)
            {
                DrawNoTarget();
            }
            else
            {
                DrawControls();
                GUILayout.Space(20);
                DrawPlacerCard();
                GUILayout.Space(20);
                DrawPoseCard();
                if (hasPoint)
                {
                    GUILayout.Space(20);
                    DrawPointCard();
                }
                GUILayout.Space(20);
                DrawSections();
                GUILayout.Space(20);
                DrawFooter();
            }

            GUILayout.Space(24);
            GUILayout.EndScrollView();

            if (pending != null)
            {
                var action = pending;
                pending = null;
                EditorApplication.delayCall += () =>
                {
                    action();
                    Repaint();
                    SceneView.RepaintAll();
                };
            }
        }

        private void DrawHeader()
        {
            var r = IosUI.Block(34);
            IosUI.Text(new Rect(r.x + 4f, r.y, r.width, r.height), "Spline Placer", IosUI.Title, IosUI.P.label);
            var r2 = IosUI.Block(20);
            IosUI.Text(new Rect(r2.x + 4f, r2.y, r2.width, r2.height),
                "Pose murs, toitures et clôtures le long d'un tracé.", IosUI.Body, IosUI.P.secondary);
        }

        private void DrawNoTarget()
        {
            var P = IosUI.P;
            var card = IosUI.Block(150);
            IosUI.Fill(card, P.card, 14f);
            IosUI.Text(new Rect(card.x, card.y + 22f, card.width, 20f), "Aucun tracé sélectionné", IosUI.Center, P.label);
            IosUI.Text(new Rect(card.x, card.y + 44f, card.width, 18f),
                "Sélectionne un objet avec un Spline Placer, ou crées-en un.", IosUI.SmallCenter, P.secondary);

            var btn = new Rect(card.center.x - 90f, card.y + 84f, 180f, 42f);
            if (IosUI.PillButton(btn, "Nouveau tracé", P.blue, Color.white, 14f))
                pending = NewPlacer;
        }

        private void DrawControls()
        {
            var P = IosUI.P;

            var r = IosUI.Block(52);
            if (IosUI.PillButton(r, editing ? "Édition active — Échap pour quitter" : "Activer l'édition",
                    editing ? P.green : P.blue, Color.white, 16f))
            {
                editing = !editing;
                SceneView.RepaintAll();
            }

            GUILayout.Space(10);
            r = IosUI.Block(36);
            mode = Segmented(r, new[] { "Ajouter des points", "Modifier" }, mode);

            GUILayout.Space(8);
            r = IosUI.Block(18);
            IosUI.Text(r,
                mode == 0
                    ? "Clic sur une surface : ajoute un point  ·  B : point d'arrêt"
                    : "Clic : sélectionner  ·  W / E / R : déplacer, tourner, échelle",
                IosUI.SmallCenter, P.secondary);

            GUILayout.Space(10);
            r = IosUI.Block(40);
            const float gap = 8f;
            float w = (r.width - gap * 2f) / 3f;
            var a = new Rect(r.x, r.y, w, r.height);
            var b = new Rect(a.xMax + gap, r.y, w, r.height);
            var c = new Rect(b.xMax + gap, r.y, w, r.height);

            if (IosUI.PillButton(a, "Point d'arrêt", IosUI.Tint(P.orange), P.orange, 12f)) pending = AddBreak;
            if (IosUI.PillButton(b, "Insérer", IosUI.Tint(P.blue), P.blue, 12f)) pending = InsertPoint;
            if (IosUI.PillButton(c, "Supprimer", IosUI.Tint(P.red), P.red, 12f)) pending = DeleteSelectedPoint;
        }

        private void DrawPlacerCard()
        {
            var P = IosUI.P;
            var hdr = IosUI.SectionHeader("Tracé");
            var newBtn = new Rect(hdr.xMax - 16f - 130f, hdr.y, 130f, hdr.height);
            if (IosUI.TextButton(newBtn, "Nouveau tracé", P.blue)) pending = NewPlacer;

            var card = IosUI.BeginCard(4, RowH);
            const float fieldW = 210f;

            // Objet
            var r = card.Next();
            IosUI.RowLabel(r, "Tracé");
            var f = new Rect(r.xMax - 16f - fieldW, r.y + 13f, fieldW, 22f);
            var nt = (SplinePlacer)EditorGUI.ObjectField(f, target, typeof(SplinePlacer), true);
            if (nt != null && nt != target) SetTarget(nt);

            // Parent (EmptyParentObject)
            r = card.Next();
            IosUI.RowLabel(r, "Parent");
            f = new Rect(r.xMax - 16f - fieldW, r.y + 13f, fieldW, 22f);
            var np = (Transform)EditorGUI.ObjectField(f, target.targetParent, typeof(Transform), true);
            // Interdit de ranger la sortie dans elle-même
            bool insideOutput = np != null && target.outputRoot != null && np.IsChildOf(target.outputRoot);
            if (np != target.targetParent && !insideOutput)
                Edit("Parent du tracé", () => target.targetParent = np);

            // Prefab de la section active
            r = card.Next();
            var sec = ActiveSection;
            IosUI.RowLabel(r, "Prefab (section " + (activeSection + 1) + ")", 190f);
            f = new Rect(r.xMax - 16f - 150f, r.y + 13f, 150f, 22f);
            var pf = (GameObject)EditorGUI.ObjectField(f, sec != null ? sec.prefab : null, typeof(GameObject), false);
            if (sec != null && pf != sec.prefab)
                Edit("Prefab de la section", () => sec.prefab = pf);

            // Prefab d'arrêt
            r = card.Next();
            IosUI.RowLabel(r, "Prefab d'arrêt");
            f = new Rect(r.xMax - 16f - fieldW, r.y + 13f, fieldW, 22f);
            var bp = (GameObject)EditorGUI.ObjectField(f, target.breakPrefab, typeof(GameObject), false);
            if (bp != target.breakPrefab)
                Edit("Prefab d'arrêt", () => target.breakPrefab = bp);
        }

        private void DrawPoseCard()
        {
            IosUI.SectionHeader("Pose");
            var card = IosUI.BeginCard(7, RowH);

            // Axe de longueur
            var r = card.Next();
            IosUI.RowLabel(r, "Axe de longueur", 170f);
            int axis = Segmented(new Rect(r.xMax - 16f - 100f, r.y + 8f, 100f, 32f), new[] { "X", "Z" }, (int)target.lengthAxis);
            if (axis != (int)target.lengthAxis) Edit("Axe de longueur", () => target.lengthAxis = (LengthAxis)axis);

            // Remplissage
            r = card.Next();
            IosUI.RowLabel(r, "Remplissage");
            int fit = Segmented(new Rect(r.xMax - 16f - 150f, r.y + 8f, 150f, 32f), new[] { "Étirer", "Répéter" }, (int)target.fitMode);
            if (fit != (int)target.fitMode) Edit("Remplissage", () => target.fitMode = (FitMode)fit);

            // Espacement
            r = card.Next();
            float gv = Mathf.Round(IosUI.SliderRow(r, "Espacement", target.gap, 0f, 2f, IosUI.F(target.gap, "0.00") + " m") * 20f) / 20f;
            if (!Mathf.Approximately(gv, target.gap)) Edit("Espacement", () => target.gap = gv);

            // Courbe lissée
            r = card.Next();
            IosUI.RowLabel(r, "Courbe lissée", 200f);
            bool sm = IosUI.Switch(new Rect(r.xMax - 16f - 51f, r.y + (RowH - 31f) * 0.5f, 51f, 31f), target.smooth);
            if (sm != target.smooth) Edit("Courbe lissée", () => target.smooth = sm);

            // Suivre la pente
            r = card.Next();
            IosUI.RowLabel(r, "Suivre la pente (toitures)", 220f);
            bool fs = IosUI.Switch(new Rect(r.xMax - 16f - 51f, r.y + (RowH - 31f) * 0.5f, 51f, 31f), target.followSlope);
            if (fs != target.followSlope) Edit("Suivre la pente", () => target.followSlope = fs);

            // Aimantation
            r = card.Next();
            float sn = Mathf.Round(IosUI.SliderRow(r, "Aimantation", target.snap, 0f, 2f,
                target.snap <= 0.001f ? "Off" : IosUI.F(target.snap, "0.00") + " m") * 4f) / 4f;
            if (!Mathf.Approximately(sn, target.snap)) Edit("Aimantation", () => target.snap = sn);

            // Orientation du point d'arrêt
            r = card.Next();
            IosUI.RowLabel(r, "Orientation d'arrêt", 140f);
            int ba = Segmented(new Rect(r.xMax - 16f - 190f, r.y + 8f, 190f, 32f), new[] { "Entrant", "Sortant", "Milieu" }, (int)target.breakAlign);
            if (ba != (int)target.breakAlign) Edit("Orientation d'arrêt", () => target.breakAlign = (BreakAlign)ba);
        }

        private void DrawPointCard()
        {
            var pt = SelectedPointData;
            if (pt == null) return;

            IosUI.SectionHeader("Point sélectionné");
            var card = IosUI.BeginCard(2, RowH);

            // Rotation (Y)
            var r = card.Next();
            float yaw = Mathf.DeltaAngle(0f, pt.rotationOffset.y);
            float ny = Mathf.Round(IosUI.SliderRow(r, "Rotation Y", yaw, -180f, 180f, Mathf.RoundToInt(yaw) + "°"));
            if (!Mathf.Approximately(ny, yaw))
                Edit("Rotation du point", () => pt.rotationOffset = new Vector3(pt.rotationOffset.x, ny, pt.rotationOffset.z));

            // Échelle uniforme
            r = card.Next();
            float avg = (pt.scale.x + pt.scale.y + pt.scale.z) / 3f;
            float ns = Mathf.Round(IosUI.SliderRow(r, "Échelle", avg, 0.2f, 3f, "×" + IosUI.F(avg, "0.00")) * 20f) / 20f;
            if (!Mathf.Approximately(ns, Mathf.Round(avg * 20f) / 20f))
                Edit("Échelle du point", () => pt.scale = Vector3.one * ns);
        }

        private void DrawSections()
        {
            var P = IosUI.P;
            IosUI.SectionHeader("Sections");

            var sections = target.sections;
            int count = Mathf.Max(1, sections.Count);
            var card = IosUI.Block(count * SectionH);
            IosUI.Fill(card, P.card, 14f);

            for (int i = 0; i < sections.Count; i++)
            {
                var row = new Rect(card.x, card.y + i * SectionH, card.width, SectionH);
                DrawSectionRow(row, i, sections[i], i > 0);
            }
        }

        private void DrawSectionRow(Rect row, int index, SplineSection sec, bool separator)
        {
            var P = IosUI.P;
            var e = Event.current;
            bool active = index == activeSection;
            bool hover = row.Contains(e.mousePosition) && GUIUtility.hotControl == 0;

            if (separator)
                EditorGUI.DrawRect(new Rect(row.x + 64f, row.y, row.width - 64f, 1f), P.sep);
            if (hover)
                IosUI.Fill(new Rect(row.x + 6f, row.y + 3f, row.width - 12f, row.height - 6f), P.fill, 10f);

            var tile = new Rect(row.x + 14f, row.y + 11f, 36f, 36f);
            IosUI.Fill(tile, active ? P.blue : P.fill, 9f);
            IosUI.Text(tile, (index + 1).ToString(), IosUI.Center, active ? Color.white : P.secondary);

            int idx = index;
            var del = new Rect(row.xMax - 14f - 86f, row.y + 14f, 86f, 30f);
            if (IosUI.PillButton(del, "Supprimer", IosUI.Tint(P.red), P.red, 15f))
                pending = () => DeleteSection(idx);

            float textX = tile.xMax + 14f;
            float textW = del.x - 10f - textX;
            string title = index == 0 ? "Section 1" : "Section " + (index + 1) + " (après arrêt)";
            string prefabName = sec.prefab != null ? sec.prefab.name : "aucun prefab";
            string info = sec.points.Count + " pts  ·  " + IosUI.F(SplineBuilder.PathLength(target, sec), "0.0") + " m  ·  " + prefabName;

            IosUI.Text(new Rect(textX, row.y + 9f, textW, 20f), title, IosUI.Body, P.label);
            IosUI.Text(new Rect(textX, row.y + 29f, textW, 16f), IosUI.TruncEnd(info, textW, 5.8f), IosUI.Small, P.secondary);

            if (IosUI.Tap(row, out _, out _))
                pending = () => { activeSection = idx; selectedPoint = -1; };
        }

        private void DrawFooter()
        {
            var P = IosUI.P;
            var r = IosUI.Block(44);
            const float gap = 10f;
            var a = new Rect(r.x, r.y, (r.width - gap) * 0.5f, r.height);
            var b = new Rect(a.xMax + gap, r.y, a.width, r.height);

            if (IosUI.PillButton(a, "Reconstruire", IosUI.Tint(P.blue), P.blue, 14f))
                pending = () => SplineBuilder.Rebuild(target);
            if (IosUI.PillButton(b, "Figer le résultat", IosUI.Tint(P.green), P.green, 14f))
                pending = Bake;

            GUILayout.Space(8);
            var hint = IosUI.Block(30);
            IosUI.Text(hint, "Figer détache les prefabs du tracé pour les modifier à la main.", IosUI.SmallCenter, P.secondary);
        }

        // ------------------------------------------------------------------ Scene view

        private void HideTools()
        {
            Tools.hidden = true;
            toolsHiddenByUs = true;
        }

        private void ReleaseTools()
        {
            if (!toolsHiddenByUs) return;
            Tools.hidden = false;
            toolsHiddenByUs = false;
        }

        private void OnSceneGUI(SceneView sv)
        {
            if (target == null)
            {
                ReleaseTools();
                return;
            }

            Event e = Event.current;

            if (!editing)
            {
                ReleaseTools();
                DrawGizmos(e, false);
                return;
            }

            HideTools();
            int id = GUIUtility.GetControlID(FocusType.Passive);
            if (e.type == EventType.Layout) HandleUtility.AddDefaultControl(id);
            if (e.type == EventType.MouseMove) sv.Repaint();

            // Raccourcis clavier
            if (e.type == EventType.KeyDown && !e.control && !e.alt)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Escape:
                        editing = false;
                        Repaint();
                        sv.Repaint();
                        e.Use();
                        return;
                    case KeyCode.B:
                        AddBreak();
                        e.Use();
                        break;
                    case KeyCode.Delete:
                    case KeyCode.Backspace:
                        DeleteSelectedPoint();
                        e.Use();
                        break;
                }
            }

            DrawGizmos(e, mode == 1);

            if (mode == 0) HandleAddMode(e, id);
            else DrawPointHandles();
        }

        private void DrawGizmos(Event e, bool useButtons)
        {
            var blue = new Color(0.04f, 0.52f, 1f, 1f);

            for (int s = 0; s < target.sections.Count; s++)
            {
                var sec = target.sections[s];
                bool active = s == activeSection;

                var verts = SplineBuilder.PathVertices(target, sec);
                if (verts.Count >= 2 && e.type == EventType.Repaint)
                {
                    Handles.color = active ? blue : new Color(blue.r, blue.g, blue.b, 0.4f);
                    Handles.DrawAAPolyLine(active ? 4f : 2.5f, verts.ToArray());
                }

                if (!editing) continue;

                for (int i = 0; i < sec.points.Count; i++)
                {
                    Vector3 world = target.transform.TransformPoint(sec.points[i].position);
                    float size = HandleUtility.GetHandleSize(world);
                    bool selected = active && i == selectedPoint;
                    bool isBreak = s > 0 && i == 0;

                    Color col = selected ? new Color(0.19f, 0.82f, 0.35f) : isBreak ? new Color(1f, 0.62f, 0.04f) : Color.white;

                    if (isBreak && e.type == EventType.Repaint)
                        Handles.Label(world + Vector3.up * size * 0.35f, "Arrêt");

                    if (useButtons && !selected)
                    {
                        Handles.color = col;
                        if (Handles.Button(world, Quaternion.identity, size * 0.1f, size * 0.14f, Handles.SphereHandleCap))
                        {
                            activeSection = s;
                            selectedPoint = i;
                            Repaint();
                        }
                    }
                    else if (e.type == EventType.Repaint)
                    {
                        Handles.color = col;
                        Handles.SphereHandleCap(0, world, Quaternion.identity, size * 0.12f, EventType.Repaint);
                    }
                }
            }
        }

        private void DrawPointHandles()
        {
            var sec = ActiveSection;
            var pt = SelectedPointData;
            if (sec == null || pt == null) return;

            Vector3 world = target.transform.TransformPoint(pt.position);
            Quaternion frame = SplineBuilder.PointFrame(target, sec, selectedPoint);
            Quaternion rot = frame * Quaternion.Euler(pt.rotationOffset);
            float size = HandleUtility.GetHandleSize(world);

            switch (Tools.current)
            {
                case Tool.Rotate:
                {
                    EditorGUI.BeginChangeCheck();
                    Quaternion nr = Handles.RotationHandle(rot, world);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Vector3 off = (Quaternion.Inverse(frame) * nr).eulerAngles;
                        Edit("Rotation du point", () => pt.rotationOffset = off);
                    }
                    break;
                }
                case Tool.Scale:
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 ns = Handles.ScaleHandle(pt.scale, world, rot, size);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ns = new Vector3(Mathf.Max(0.05f, ns.x), Mathf.Max(0.05f, ns.y), Mathf.Max(0.05f, ns.z));
                        Edit("Échelle du point", () => pt.scale = ns);
                    }
                    break;
                }
                default:
                {
                    EditorGUI.BeginChangeCheck();
                    Quaternion hr = Tools.pivotRotation == PivotRotation.Local ? rot : Quaternion.identity;
                    Vector3 np = Handles.PositionHandle(world, hr);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Vector3 snapped = SnapXZ(np);
                        Edit("Déplacer le point", () => pt.position = target.transform.InverseTransformPoint(snapped));
                    }
                    break;
                }
            }
        }

        private void HandleAddMode(Event e, int id)
        {
            if (!TryGetSurface(e, out Vector3 hit)) return;

            Vector3 p = SnapXZ(hit);
            var sec = ActiveSection;

            if (e.type == EventType.Repaint)
            {
                float size = HandleUtility.GetHandleSize(p);
                var blue = new Color(0.04f, 0.52f, 1f, 1f);

                if (sec != null && sec.points.Count > 0)
                {
                    Vector3 last = target.transform.TransformPoint(sec.points[sec.points.Count - 1].position);
                    Handles.color = new Color(blue.r, blue.g, blue.b, 0.7f);
                    Handles.DrawDottedLine(last, p, 4f);
                }

                Handles.color = new Color(blue.r, blue.g, blue.b, 0.35f);
                Handles.DrawSolidDisc(p, Vector3.up, size * 0.18f);
                Handles.color = blue;
                Handles.DrawWireDisc(p, Vector3.up, size * 0.18f);
            }

            if (e.GetTypeForControl(id) == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                AddPointAt(p);
                e.Use();
            }
        }

        private bool TryGetSurface(Event e, out Vector3 point)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            var hits = Physics.RaycastAll(ray, 2000f);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var h in hits)
            {
                // Ignore les pièces générées pour ne pas empiler les points dessus
                if (target.outputRoot != null && h.transform.IsChildOf(target.outputRoot)) continue;
                point = h.point;
                return true;
            }

            // Sans collider : plan horizontal à la hauteur du dernier point (ou du tracé)
            Vector3 origin = target.transform.position;
            var sec = ActiveSection;
            if (sec != null && sec.points.Count > 0)
                origin = target.transform.TransformPoint(sec.points[sec.points.Count - 1].position);

            var plane = new Plane(Vector3.up, origin);
            if (plane.Raycast(ray, out float dist))
            {
                point = ray.GetPoint(dist);
                return true;
            }

            point = default;
            return false;
        }
    }
}