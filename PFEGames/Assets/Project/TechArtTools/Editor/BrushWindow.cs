// Place ce fichier dans un dossier "Editor" (ex: Assets/Editor/BrushWindow.cs)
// Menu : Tools > Scatter Brush Tool

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ToolSaverio.Editor
{
    // =====================================================================================
    //  Mini-bibliothèque d'UI "iOS" (cartes arrondies, switch, slider, boutons pilule…)
    // =====================================================================================
    internal static class IosUI
    {
        public struct Palette
        {
            public Color bg, card, fill, sep, label, secondary, blue, green, red, orange, segSel, switchOff;
        }

        public static Palette P;
        static bool ready, isPro;
        public static GUIStyle Body, BodyRight, Small, SmallCenter, Title, Center, Section;

        public const float M = 16f;

        public class Card
        {
            public Rect rect;
            readonly float rowH;
            int index;

            public Card(Rect rect, float rowH) { this.rect = rect; this.rowH = rowH; }

            public Rect Next()
            {
                var r = new Rect(rect.x, rect.y + index * rowH, rect.width, rowH);
                if (index > 0) EditorGUI.DrawRect(new Rect(r.x + 16f, r.y, r.width - 16f, 1f), P.sep);
                index++;
                return r;
            }
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        static GUIStyle MakeStyle(int size, FontStyle fs, TextAnchor anchor)
        {
            var s = new GUIStyle(GUI.skin.label)
            {
                fontSize = size,
                fontStyle = fs,
                alignment = anchor,
                clipping = TextClipping.Clip,
                wordWrap = false,
                padding = new RectOffset(),
                margin = new RectOffset()
            };
            s.normal.textColor = Color.white; // la vraie couleur passe par GUI.contentColor
            return s;
        }

        public static void Ensure()
        {
            bool pro = EditorGUIUtility.isProSkin;
            if (ready && pro == isPro && Body != null) return;
            ready = true;
            isPro = pro;

            P = pro
                ? new Palette
                {
                    bg = Hex("#000000"), card = Hex("#1C1C1E"), fill = Hex("#2C2C2E"), sep = Hex("#38383A"),
                    label = Color.white, secondary = Hex("#8E8E93"),
                    blue = Hex("#0A84FF"), green = Hex("#30D158"), red = Hex("#FF453A"), orange = Hex("#FF9F0A"),
                    segSel = Hex("#636366"), switchOff = Hex("#39393D")
                }
                : new Palette
                {
                    bg = Hex("#F2F2F7"), card = Color.white, fill = Hex("#EEEEF0"), sep = Hex("#E0E0E4"),
                    label = Color.black, secondary = Hex("#8E8E93"),
                    blue = Hex("#007AFF"), green = Hex("#34C759"), red = Hex("#FF3B30"), orange = Hex("#FF9500"),
                    segSel = Color.white, switchOff = Hex("#E9E9EA")
                };

            Title = MakeStyle(26, FontStyle.Bold, TextAnchor.MiddleLeft);
            Body = MakeStyle(13, FontStyle.Normal, TextAnchor.MiddleLeft);
            BodyRight = MakeStyle(13, FontStyle.Normal, TextAnchor.MiddleRight);
            Small = MakeStyle(11, FontStyle.Normal, TextAnchor.MiddleLeft);
            SmallCenter = MakeStyle(11, FontStyle.Normal, TextAnchor.MiddleCenter);
            Center = MakeStyle(12, FontStyle.Bold, TextAnchor.MiddleCenter);
            Section = MakeStyle(11, FontStyle.Normal, TextAnchor.MiddleLeft);
        }

        // ---------------------------------------------------------------- Dessin

        public static bool IsRepaint => Event.current.type == EventType.Repaint;

        public static void Fill(Rect r, Color c, float radius)
        {
            if (!IsRepaint) return;
            radius = Mathf.Min(radius, Mathf.Min(r.width, r.height) * 0.5f);
            GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, c, 0f, radius);
        }

        public static void Text(Rect r, string s, GUIStyle style, Color c)
        {
            var old = GUI.contentColor;
            GUI.contentColor = c;
            GUI.Label(r, s, style);
            GUI.contentColor = old;
        }

        public static Color Tint(Color c, float a = 0.18f) => new Color(c.r, c.g, c.b, a);

        static Color Shade(Color c, bool hover, bool pressed)
        {
            if (pressed) return new Color(c.r * 0.78f, c.g * 0.78f, c.b * 0.78f, Mathf.Min(1f, c.a + 0.08f));
            if (hover) return new Color(Mathf.Lerp(c.r, 1f, 0.1f), Mathf.Lerp(c.g, 1f, 0.1f), Mathf.Lerp(c.b, 1f, 0.1f), Mathf.Min(1f, c.a + 0.05f));
            return c;
        }

        public static Rect Block(float height)
        {
            var r = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));
            r.x += M;
            r.width -= M * 2f;
            return r;
        }

        public static Card BeginCard(int rows, float rowH)
        {
            var r = Block(rows * rowH);
            Fill(r, P.card, 14f);
            return new Card(r, rowH);
        }

        public static Rect SectionHeader(string title)
        {
            var r = Block(26);
            Text(new Rect(r.x + 16f, r.y, r.width - 32f, r.height), title.ToUpperInvariant(), Section, P.secondary);
            return r;
        }

        public static void RowLabel(Rect row, string s, float width = 150f) =>
            Text(new Rect(row.x + 16f, row.y, width, row.height), s, Body, P.label);

        public static string TruncStart(string s, float width, float charW)
        {
            int max = Mathf.Max(8, (int)(width / charW));
            return s.Length <= max ? s : "…" + s.Substring(s.Length - (max - 1));
        }

        public static string TruncEnd(string s, float width, float charW)
        {
            int max = Mathf.Max(8, (int)(width / charW));
            return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
        }

        // ---------------------------------------------------------------- Contrôles

        public static bool Tap(Rect r, out bool hover, out bool pressed)
        {
            int id = GUIUtility.GetControlID(FocusType.Passive, r);
            var e = Event.current;
            EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);

            pressed = GUIUtility.hotControl == id;
            hover = r.Contains(e.mousePosition) && (GUIUtility.hotControl == 0 || pressed);

            switch (e.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (e.button == 0 && r.Contains(e.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        e.Use();
                        return r.Contains(e.mousePosition);
                    }
                    break;
            }
            return false;
        }

        public static bool PillButton(Rect r, string label, Color bg, Color fg, float radius)
        {
            bool tap = Tap(r, out bool hover, out bool pressed);
            Fill(r, Shade(bg, hover, pressed), radius);
            Text(r, label, Center, fg);
            return tap;
        }

        public static bool TextButton(Rect r, string label, Color color)
        {
            bool tap = Tap(r, out bool hover, out bool pressed);
            Text(r, label, BodyRight, pressed ? Tint(color, 0.5f) : hover ? Color.Lerp(color, Color.white, 0.2f) : color);
            return tap;
        }

        public static bool Switch(Rect r, bool on)
        {
            if (Tap(r, out _, out _)) on = !on;
            Fill(r, on ? P.green : P.switchOff, r.height * 0.5f);
            float d = r.height - 4f;
            Fill(new Rect(on ? r.xMax - d - 2f : r.x + 2f, r.y + 2f, d, d), Color.white, d * 0.5f);
            return on;
        }

        /// <summary>Slider iOS : piste fine, partie remplie bleue, poignée blanche.</summary>
        public static float Slider(Rect r, float value, float min, float max)
        {
            int id = GUIUtility.GetControlID(FocusType.Passive);
            var e = Event.current;
            var hit = new Rect(r.x - 10f, r.y, r.width + 20f, r.height);
            float From(float x) => Mathf.Lerp(min, max, Mathf.Clamp01((x - r.x) / r.width));

            switch (e.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (e.button == 0 && hit.Contains(e.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        value = From(e.mousePosition.x);
                        e.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        value = From(e.mousePosition.x);
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                    break;
            }

            EditorGUIUtility.AddCursorRect(hit, MouseCursor.Link);

            float t = Mathf.InverseLerp(min, max, value);
            var track = new Rect(r.x, r.center.y - 2f, r.width, 4f);
            Fill(track, P.fill == P.card ? P.sep : P.switchOff, 2f);
            Fill(new Rect(track.x, track.y, track.width * t, track.height), P.blue, 2f);

            const float k = 24f;
            var knob = new Rect(r.x + r.width * t - k * 0.5f, r.center.y - k * 0.5f, k, k);
            Fill(new Rect(knob.x - 1f, knob.y + 1f, k + 2f, k + 2f), new Color(0f, 0f, 0f, 0.22f), (k + 2f) * 0.5f);
            Fill(knob, Color.white, k * 0.5f);
            return value;
        }

        /// <summary>Ligne complète : libellé à gauche, slider au milieu, valeur à droite.</summary>
        public static float SliderRow(Rect row, string label, float value, float min, float max, string valueText)
        {
            RowLabel(row, label, 120f);
            Text(new Rect(row.xMax - 16f - 54f, row.y, 54f, row.height), valueText, BodyRight, P.secondary);
            float sx = row.x + 16f + 124f;
            var sr = new Rect(sx, row.y + (row.height - 28f) * 0.5f, row.xMax - 16f - 54f - 12f - sx, 28f);
            return Slider(sr, value, min, max);
        }

        public static string F(float v, string fmt = "0.0") => v.ToString(fmt, CultureInfo.InvariantCulture);
    }

    // =====================================================================================
    //  Fenêtre du Scatter Brush
    // =====================================================================================
    public class BrushWindow : EditorWindow
    {
        // Réglages (sérialisés : conservés après un rechargement de domaine)
        [SerializeField] private float brushRadius = 2.0f;
        [SerializeField] private int spawnDensity = 5;
        [SerializeField] private float minScale = 0.8f;
        [SerializeField] private float maxScale = 1.2f;
        [SerializeField] private float alignStrength = 1f; // 0 = toujours droit, 1 = suit la pente
        [SerializeField] private LayerMask surfaceLayer = ~0; // Tout par défaut
        [SerializeField] private float maxSlope = 45f;        // Pente max en degrés (90 = aucune limite)
        [SerializeField] private List<GameObject> prefabsToSpawn = new List<GameObject>();

        private bool isBrushActive;
        private Transform parentContainer;

        // État UI
        private Vector2 scroll;
        private Action pending;
        private bool anyPreviewLoading;

        // État du pinceau (Scene view)
        private Vector3 lastPaintPoint;
        private bool strokeIsErase; // mode du trait en cours (fixé au clic)

        private const float ROW_H = 48f;
        private const float PALETTE_ROW_H = 52f;

        [MenuItem("Tools/TechArtTools/BrushAsset")]
        public static void ShowWindow()
        {
            var window = GetWindow<BrushWindow>();
            window.titleContent = new GUIContent("Scatter Brush");
            window.minSize = new Vector2(380, 480);
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnInspectorUpdate()
        {
            // Les miniatures de prefabs se chargent en asynchrone
            if (anyPreviewLoading) Repaint();
        }

        // ------------------------------------------------------------------ Fenêtre

        [Obsolete("Obsolete")]
        private void OnGUI()
        {
            IosUI.Ensure();

            var e = Event.current;
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag) Repaint();
            if (e.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), IosUI.P.bg);
                anyPreviewLoading = false;
            }

            scroll = GUILayout.BeginScrollView(scroll, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

            GUILayout.Space(18);
            DrawHeader();
            GUILayout.Space(16);
            DrawToggle();
            GUILayout.Space(20);
            DrawBrushSettings();
            GUILayout.Space(20);
            DrawRandomization();
            GUILayout.Space(20);
            DrawContainer();
            GUILayout.Space(20);
            DrawPalette();
            GUILayout.Space(24);

            GUILayout.EndScrollView();

            if (pending != null)
            {
                var action = pending;
                pending = null;
                EditorApplication.delayCall += () => { action(); Repaint(); };
            }
        }

        private void DrawHeader()
        {
            var r = IosUI.Block(34);
            IosUI.Text(new Rect(r.x + 4f, r.y, r.width, r.height), "Scatter Brush", IosUI.Title, IosUI.P.label);
            var r2 = IosUI.Block(20);
            IosUI.Text(new Rect(r2.x + 4f, r2.y, r2.width, r2.height),
                "Peins des prefabs directement sur tes surfaces.", IosUI.Body, IosUI.P.secondary);
        }

        private void DrawToggle()
        {
            var rect = IosUI.Block(52);
            var palette = IosUI.P;
            string label = isBrushActive ? "Pinceau actif — Échap pour quitter" : "Activer le pinceau";
            if (IosUI.PillButton(rect, label, isBrushActive ? palette.green : palette.blue, Color.white, 16f))
            {
                isBrushActive = !isBrushActive;
                SceneView.RepaintAll();
            }

            GUILayout.Space(8);
            var hint = IosUI.Block(18);
            IosUI.Text(hint, "Clic : peindre  ·  Shift + clic : effacer  ·  Shift + molette : rayon",
                IosUI.SmallCenter, palette.secondary);
        }

        private void DrawBrushSettings()
        {
            IosUI.SectionHeader("Pinceau");
            var card = IosUI.BeginCard(4, ROW_H);

            var r = card.Next();
            brushRadius = IosUI.SliderRow(r, "Rayon", brushRadius, 0.5f, 10f, IosUI.F(brushRadius) + " m");

            r = card.Next();
            spawnDensity = Mathf.RoundToInt(IosUI.SliderRow(r, "Objets / clic", spawnDensity, 1, 20, spawnDensity.ToString()));

            r = card.Next();
            maxSlope = Mathf.Round(IosUI.SliderRow(r, "Pente max", maxSlope, 0f, 90f,
                maxSlope >= 90f ? "Aucune" : Mathf.RoundToInt(maxSlope) + "°"));

            r = card.Next();
            IosUI.RowLabel(r, "Surface (Layers)");
            var field = new Rect(r.xMax - 16f - 170f, r.y + 13f, 170f, 22f);
            var layers = InternalEditorUtility.layers;
            int mask = InternalEditorUtility.LayerMaskToConcatenatedLayersMask(surfaceLayer);
            mask = EditorGUI.MaskField(field, mask, layers);
            surfaceLayer = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(mask);
        }

        private void DrawRandomization()
        {
            IosUI.SectionHeader("Randomisation");
            var card = IosUI.BeginCard(3, ROW_H);

            var r = card.Next();
            float newMin = IosUI.SliderRow(r, "Scale min", minScale, 0.1f, 3f, "×" + IosUI.F(minScale, "0.00"));
            minScale = Mathf.Min(newMin, maxScale);

            r = card.Next();
            float newMax = IosUI.SliderRow(r, "Scale max", maxScale, 0.1f, 3f, "×" + IosUI.F(maxScale, "0.00"));
            maxScale = Mathf.Max(newMax, minScale);

            r = card.Next();
            alignStrength = Mathf.Round(IosUI.SliderRow(r, "Alignement pente", alignStrength, 0f, 1f,
                Mathf.RoundToInt(alignStrength * 100f) + "%") * 20f) / 20f;
        }

        private void DrawContainer()
        {
            IosUI.SectionHeader("Conteneur");
            var card = IosUI.BeginCard(1, ROW_H);
            var r = card.Next();
            IosUI.RowLabel(r, "Parent");
            var field = new Rect(r.xMax - 16f - 200f, r.y + 13f, 200f, 22f);
            parentContainer = (Transform)EditorGUI.ObjectField(field, parentContainer, typeof(Transform), true);
        }

        // ------------------------------------------------------------------ Palette

        [Obsolete("Obsolete")]
        private void DrawPalette()
        {
            var palette = IosUI.P;
            var hdr = IosUI.SectionHeader("Palette");

            if (prefabsToSpawn.Count > 0)
            {
                var clear = new Rect(hdr.xMax - 16f - 120f, hdr.y, 120f, hdr.height);
                if (IosUI.TextButton(clear, "Tout retirer", palette.red))
                    pending = () => prefabsToSpawn.Clear();
            }

            int count = prefabsToSpawn.Count;
            var card = IosUI.Block(count * PALETTE_ROW_H + 68f);
            IosUI.Fill(card, palette.card, 14f);

            for (int i = 0; i < count; i++)
            {
                var row = new Rect(card.x, card.y + i * PALETTE_ROW_H, card.width, PALETTE_ROW_H);
                DrawPrefabRow(row, i, i > 0);
            }

            // Zone de dépôt
            var zone = new Rect(card.x + 12f, card.yMax - 56f, card.width - 24f, 44f);
            DrawDropZone(zone);
        }

        [Obsolete("Obsolete")]
        private void DrawPrefabRow(Rect row, int index, bool separator)
        {
            var P = IosUI.P;
            var prefab = prefabsToSpawn[index];
            var e = Event.current;

            if (separator)
                EditorGUI.DrawRect(new Rect(row.x + 64f, row.y, row.width - 64f, 1f), P.sep);

            // Miniature
            var tile = new Rect(row.x + 14f, row.y + 8f, 36f, 36f);
            IosUI.Fill(tile, P.fill, 9f);
            if (prefab != null && IosUI.IsRepaint)
            {
                Texture2D thumb = AssetPreview.GetAssetPreview(prefab);
                if (thumb == null)
                {
                    if (AssetPreview.IsLoadingAssetPreview(prefab.GetInstanceID())) anyPreviewLoading = true;
                    thumb = AssetPreview.GetMiniThumbnail(prefab);
                }
                if (thumb != null)
                    GUI.DrawTexture(new Rect(tile.x + 3f, tile.y + 3f, 30f, 30f), thumb, ScaleMode.ScaleToFit);
            }

            // Bouton "Retirer" (traité avant la ligne pour avoir la priorité au clic)
            var remove = new Rect(row.xMax - 14f - 76f, row.y + 11f, 76f, 30f);
            int idx = index;
            if (IosUI.PillButton(remove, "Retirer", IosUI.Tint(P.red), P.red, 15f))
                pending = () => { if (idx < prefabsToSpawn.Count) prefabsToSpawn.RemoveAt(idx); };

            // Textes
            float textX = tile.xMax + 14f;
            float textW = remove.x - 10f - textX;
            string name = prefab != null ? prefab.name : "(prefab manquant)";
            string path = prefab != null ? AssetDatabase.GetAssetPath(prefab) : "";
            IosUI.Text(new Rect(textX, row.y + 8f, textW, 20f), IosUI.TruncEnd(name, textW, 7f), IosUI.Body, P.label);
            IosUI.Text(new Rect(textX, row.y + 27f, textW, 16f), IosUI.TruncStart(path, textW, 5.8f), IosUI.Small, P.secondary);

            // Clic sur la ligne = ping de l'asset
            if (IosUI.Tap(row, out _, out _) && prefab != null)
            {
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
        }

        private void DrawDropZone(Rect zone)
        {
            var P = IosUI.P;
            var e = Event.current;

            bool dragOver = zone.Contains(e.mousePosition)
                            && DragAndDrop.objectReferences != null
                            && DragAndDrop.objectReferences.Length > 0
                            && (e.type == EventType.DragUpdated || e.type == EventType.Repaint);

            bool tap = IosUI.Tap(zone, out bool hover, out _);
            IosUI.Fill(zone, dragOver ? IosUI.Tint(P.blue, 0.25f) : hover ? P.sep : P.fill, 10f);
            IosUI.Text(zone, "Glisse des prefabs ici — ou clique pour ajouter la sélection",
                IosUI.SmallCenter, dragOver ? P.blue : P.secondary);

            // Glisser-déposer
            if (zone.Contains(e.mousePosition))
            {
                if (e.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    e.Use();
                }
                else if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    var dropped = DragAndDrop.objectReferences.OfType<GameObject>().Where(IsPrefabAsset).ToArray();
                    pending = () => AddPrefabs(dropped);
                    e.Use();
                }
            }

            // Clic : ajoute la sélection courante
            if (tap)
            {
                var selected = Selection.gameObjects.Where(IsPrefabAsset).ToArray();
                pending = () => AddPrefabs(selected);
            }
        }

        private static bool IsPrefabAsset(GameObject go)
        {
            return go != null
                   && EditorUtility.IsPersistent(go)
                   && PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab;
        }

        private void AddPrefabs(IEnumerable<GameObject> objs)
        {
            int added = 0;
            foreach (var go in objs)
            {
                if (go == null || prefabsToSpawn.Contains(go)) continue;
                prefabsToSpawn.Add(go);
                added++;
            }
            if (added == 0)
                ShowNotification(new GUIContent("Aucun prefab valide à ajouter"));
        }

        // ------------------------------------------------------------------ Scene view

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!isBrushActive) return;

            Event e = Event.current;
            int id = GUIUtility.GetControlID(FocusType.Passive);

            // Empêche la sélection par défaut d'Unity tant que le pinceau est actif
            if (e.type == EventType.Layout)
                HandleUtility.AddDefaultControl(id);

            // Échap : désactive le pinceau
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                isBrushActive = false;
                Repaint();
                sceneView.Repaint();
                e.Use();
                return;
            }

            // Shift + molette : change le rayon
            if (e.type == EventType.ScrollWheel && e.shift)
            {
                brushRadius = Mathf.Clamp(brushRadius - e.delta.y * 0.15f, 0.5f, 10f);
                Repaint();
                e.Use();
                return;
            }

            if (e.type == EventType.MouseMove || e.type == EventType.KeyDown || e.type == EventType.KeyUp)
                sceneView.Repaint();

            // Raycast depuis la souris
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            bool hasHit = Physics.Raycast(ray, out RaycastHit hit, 1000f, surfaceLayer);

            // Cercle du pinceau
            if (hasHit && e.type == EventType.Repaint)
            {
                // Bleu = peindre, rouge = effacer (Shift)
                bool erasing = GUIUtility.hotControl == id ? strokeIsErase : e.shift;
                Color baseColor = erasing ? new Color(1f, 0.27f, 0.23f) : new Color(0.04f, 0.52f, 1f);
                Handles.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.22f);
                Handles.DrawSolidDisc(hit.point, hit.normal, brushRadius);
                Handles.color = baseColor;
                Handles.DrawWireDisc(hit.point, hit.normal, brushRadius);
            }

            switch (e.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (e.button == 0 && !e.alt && hasHit)
                    {
                        GUIUtility.hotControl = id;
                        strokeIsErase = e.shift;
                        Undo.IncrementCurrentGroup();
                        Stamp(hit, strokeIsErase);
                        lastPaintPoint = hit.point;
                        e.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        // Trait continu : nouveau "tampon" tous les ~60 % du rayon (30 % pour l'effacement)
                        float spacing = brushRadius * (strokeIsErase ? 0.3f : 0.6f);
                        if (hasHit && Vector3.Distance(hit.point, lastPaintPoint) > spacing)
                        {
                            Stamp(hit, strokeIsErase);
                            lastPaintPoint = hit.point;
                        }
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                    break;
            }
        }

        private void Stamp(RaycastHit hit, bool erase)
        {
            if (erase) ErasePrefabs(hit.point);
            else PaintPrefabs(hit.point, hit.normal);

            Undo.SetCurrentGroupName(erase ? "Scatter Brush Erase" : "Scatter Brush");
        }

        /// <summary>
        /// Supprime les instances de prefabs de la palette situées dans le rayon du pinceau.
        /// Si un conteneur parent est défini, seuls ses enfants directs sont concernés.
        /// </summary>
        private void ErasePrefabs(Vector3 center)
        {
            var sources = new HashSet<GameObject>(prefabsToSpawn.Where(p => p != null));
            if (sources.Count == 0)
            {
                ShowNotification(new GUIContent("Aucun prefab dans la palette !"));
                return;
            }

            List<GameObject> candidates;
            if (parentContainer != null)
            {
                candidates = parentContainer.Cast<Transform>().Select(t => t.gameObject).ToList();
            }
            else
            {
                candidates = UnityEngine.Object
                    .FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                    .Where(go => !EditorUtility.IsPersistent(go))
                    .ToList();
            }

            foreach (var go in candidates)
            {
                if (go == null) continue;
                if (!PrefabUtility.IsOutermostPrefabInstanceRoot(go)) continue;

                var source = PrefabUtility.GetCorrespondingObjectFromSource(go);
                if (source == null || !sources.Contains(source)) continue;

                if (Vector3.Distance(go.transform.position, center) <= brushRadius)
                    Undo.DestroyObjectImmediate(go);
            }
        }

        private void PaintPrefabs(Vector3 center, Vector3 normal)
        {
            var valid = prefabsToSpawn.Where(p => p != null).ToList();
            if (valid.Count == 0)
            {
                ShowNotification(new GUIContent("Aucun prefab dans la palette !"));
                return;
            }

            // Repère tangent au plan du pinceau (fonctionne aussi sur les pentes)
            Vector3 tangent = Vector3.Cross(normal, Vector3.up);
            if (tangent.sqrMagnitude < 0.001f) tangent = Vector3.right;
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(normal, tangent);

            for (int i = 0; i < spawnDensity; i++)
            {
                Vector2 rnd = Random.insideUnitCircle * brushRadius;
                Vector3 origin = center + tangent * rnd.x + bitangent * rnd.y;

                // Raycast le long de la normale pour plaquer sur le relief exact
                if (!Physics.Raycast(origin + normal * 5f, -normal, out RaycastHit subHit, 10f, surfaceLayer))
                    continue;

                // Pente trop forte : on ne place rien (angle entre la normale locale et le haut)
                if (maxSlope < 90f && Vector3.Angle(subHit.normal, Vector3.up) > maxSlope)
                    continue;

                GameObject prefab = valid[Random.Range(0, valid.Count)];

                GameObject obj = parentContainer != null
                    ? (GameObject)PrefabUtility.InstantiatePrefab(prefab, parentContainer)
                    : (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                Undo.RegisterCreatedObjectUndo(obj, "Scatter Brush Spawn");

                obj.transform.position = subHit.point;

                Quaternion yaw = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                // 0 % = reste vertical (herbe), 100 % = suit la pente (rochers), entre les deux = mélange
                Vector3 up = Vector3.Slerp(Vector3.up, subHit.normal, alignStrength);
                obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, up) * yaw;

                obj.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
            }
        }
    }
}