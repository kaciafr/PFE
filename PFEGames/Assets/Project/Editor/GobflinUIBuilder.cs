using System.Collections.Generic;
using Goblfin;
using Goblfin.CharactersSystem.Player.Input;
using Goblfin.Player;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Monte le reste de l'UI dans la scène ouverte : HUD (pastille E, jauge, notification),
// ControlsPanel (rebind), VictoryPanel + VictoryZone, et branche les références.
// Ne touche pas à ce qui existe déjà (cherche par nom). Ctrl+Z pour annuler.
public static class GobflinUIBuilder
{
    const string InputReaderPath = "Assets/Project/Input/InputReader 1.asset";   // celui du joueur
    const string PrefabFolder = "Assets/Project/Prefabs/UI";

    static readonly List<string> report = new();

    [MenuItem("Tools/Goblfin/UI/Monter le reste de l'UI dans la scène")]
    static void Build()
    {
        report.Clear();

        PauseScreen pause = Object.FindFirstObjectByType<PauseScreen>(FindObjectsInactive.Include);
        if (pause == null)
        {
            EditorUtility.DisplayDialog("UI Gobflin", "Pas trouvé de PausePanel (PauseScreen) dans la scène.", "OK");
            return;
        }
        Canvas canvas = pause.GetComponentInParent<Canvas>(true).rootCanvas;
        Transform root = canvas.transform.parent;   // UI_Game

        InputReader inputReader = AssetDatabase.LoadAssetAtPath<InputReader>(InputReaderPath);
        PlayerManager player = Object.FindFirstObjectByType<PlayerManager>(FindObjectsInactive.Include);
        if (inputReader == null) report.Add("⚠ InputReader introuvable : " + InputReaderPath);
        if (player == null) report.Add("⚠ Pas de PlayerManager dans la scène (pastille E et jauge sans joueur).");

        UIManager uiManager = Object.FindFirstObjectByType<UIManager>(FindObjectsInactive.Include);
        if (uiManager != null)
        {
            if (root != null && uiManager.transform.IsChildOf(canvas.transform))
            {
                Undo.SetTransformParent(uiManager.transform, root, "Déplacer UIManager");
                report.Add("UIManager sorti du Canvas.");
            }
            SetIfEmpty(uiManager, "inputReader", inputReader);
            SetBool(uiManager, "isGameplayScene", true);
        }

        // 3. Les panneaux
        BuildHUD(canvas.transform, player, inputReader);
        ControlsScreen controls = BuildControlsPanel(canvas.transform, inputReader);
        VictoryScreen victory = BuildVictoryPanel(canvas.transform);
        BuildVictoryZone(victory, player);

        // 4. Brancher les Contrôles dans les Options
        UIOption options = Object.FindFirstObjectByType<UIOption>(FindObjectsInactive.Include);
        if (options != null && controls != null) SetIfEmpty(options, "controlsScreen", controls);

        // 5. Le pop-up toujours en dernier (par-dessus tout)
        Transform confirm = canvas.transform.Find("ConfirmPopup");
        if (confirm != null) confirm.SetAsLastSibling();

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Debug.Log("[UI Gobflin]\n" + string.Join("\n", report));
        EditorUtility.DisplayDialog("UI Gobflin", string.Join("\n", report) + "\n\nPense à Ctrl+S !", "OK");
    }

    // Champs déclarés en "UIScreen" (type de base) → quel écran mettre dedans s'ils sont vides
    static readonly Dictionary<string, System.Type> screenByField = new()
    {
        { "pauseScreen", typeof(PauseScreen) },
        { "optionsScreen", typeof(UIOption) },
        { "controlsScreen", typeof(ControlsScreen) },
    };

    // ═══════════════════ TOUT RÉPARER (menu OU jeu) ═══════════════════
    [MenuItem("Tools/Goblfin/UI/Tout réparer (scène ouverte)", priority = 0)]
    static void FixAll()
    {
        // 0. Jamais en Play : tout serait perdu à l'arrêt
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("UI Gobflin", "Arrête le mode Play d'abord (sinon tout est perdu).", "OK");
            return;
        }

        report.Clear();
        InputReader inputReader = AssetDatabase.LoadAssetAtPath<InputReader>(InputReaderPath);

        // 1. Trouver le Canvas principal (celui du menu ou celui de la pause)
        Canvas canvas = FindMainCanvas();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("UI Gobflin", "Aucun Canvas dans la scène.", "OK");
            return;
        }

        // 2. Les écrans posés EN DEHORS du Canvas → on les range dedans
        foreach (UIScreen screen in Object.FindObjectsByType<UIScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (screen.GetComponentInParent<Canvas>(true) != null) continue;
            Undo.SetTransformParent(screen.transform, canvas.transform, "Ranger écran dans Canvas");
            screen.transform.localScale = Vector3.one;
            Stretch(screen.gameObject);
            report.Add($"{screen.name} rangé dans le Canvas.");
        }

        // 3. UIManager : le créer s'il manque
        UIManager uiManager = Object.FindFirstObjectByType<UIManager>(FindObjectsInactive.Include);
        if (uiManager == null)
        {
            GameObject go = new GameObject("UIManager");
            Undo.RegisterCreatedObjectUndo(go, "Créer UIManager");
            uiManager = go.AddComponent<UIManager>();
            report.Add("UIManager créé.");
        }
        if (uiManager.name == "GameObject") { Undo.RecordObject(uiManager.gameObject, "Renommer"); uiManager.name = "UIManager"; }

        // Scène de jeu = il y a un joueur. Sinon (menu) : pas de pause, souris libre.
        bool isGameplay = Object.FindFirstObjectByType<PlayerManager>(FindObjectsInactive.Include) != null;
        SetBool(uiManager, "isGameplayScene", isGameplay);
        report.Add("UIManager → Is Gameplay Scene = " + (isGameplay ? "oui (scène de jeu)" : "non (menu)"));

        // 4. Tous les champs "inputReader" vides → InputReader du joueur
        if (inputReader != null)
            foreach (MonoBehaviour mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (mb != null && mb.GetType().Namespace != null && mb.GetType().Namespace.StartsWith("Goblfin"))
                    SetIfEmpty(mb, "inputReader", inputReader);

        // 5. Écrans actifs, CanvasGroup, références vers les écrans, scènes du build
        FixReferencesCore();

        // 6. Le pop-up toujours en dernier (dessiné par-dessus tout)
        ConfirmPopup popup = Object.FindFirstObjectByType<ConfirmPopup>(FindObjectsInactive.Include);
        if (popup != null) popup.transform.SetAsLastSibling();

        // 7. Ce qui reste VIDE (à remplir à la main)
        ReportEmptyFields(canvas, uiManager);

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Debug.Log("[UI Gobflin] Tout réparer\n" + string.Join("\n", report));
        EditorUtility.DisplayDialog("UI Gobflin", string.Join("\n", report) + "\n\nPense à Ctrl+S !", "OK");
    }

    // Canvas du menu (UIMenuStart) ou du jeu (PauseScreen), sinon le premier trouvé
    static Canvas FindMainCanvas()
    {
        Component anchor = Object.FindFirstObjectByType<UIMenuStart>(FindObjectsInactive.Include);
        if (anchor == null) anchor = Object.FindFirstObjectByType<PauseScreen>(FindObjectsInactive.Include);

        Canvas c = anchor != null ? anchor.GetComponentInParent<Canvas>(true) : null;
        if (c == null) c = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        return c != null ? c.rootCanvas : null;
    }

    // Liste les champs encore sur "None" dans les scripts UI → cause des NullReference
    static void ReportEmptyFields(Canvas canvas, UIManager uiManager)
    {
        List<MonoBehaviour> scripts = new List<MonoBehaviour>(canvas.GetComponentsInChildren<MonoBehaviour>(true));
        scripts.Add(uiManager);

        foreach (MonoBehaviour mb in scripts)
        {
            if (mb == null || mb.GetType().Namespace != "Goblfin") continue;

            SerializedProperty p = new SerializedObject(mb).GetIterator();
            while (p.NextVisible(true))
            {
                if (p.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (p.name == "m_Script" || p.objectReferenceValue != null) continue;
                if (p.name == "pauseScreen" && mb is UIManager) continue;   // normal dans le menu
                report.Add($"⚠ VIDE : {mb.name} ({mb.GetType().Name}) → {p.displayName}");
            }
        }
    }

    [MenuItem("Tools/Goblfin/UI/Réparer les références UI")]
    static void FixReferences()
    {
        report.Clear();
        FixReferencesCore();

        if (report.Count == 0) report.Add("Rien à réparer : toutes les références sont bonnes.");

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[UI Gobflin] Réparation\n" + string.Join("\n", report));
        EditorUtility.DisplayDialog("UI Gobflin", string.Join("\n", report) + "\n\nPense à Ctrl+S !", "OK");
    }

    static void FixReferencesCore()
    {
        // 1. Tous les écrans doivent être ACTIFS (sinon leur Awake ne tourne jamais)
        foreach (UIScreen screen in Object.FindObjectsByType<UIScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (screen.gameObject.activeSelf) continue;
            Undo.RecordObject(screen.gameObject, "Activer écran");
            screen.gameObject.SetActive(true);
            report.Add($"{screen.name} réactivé.");
        }

        // 1a. Le carnet de quêtes aussi (il est DANS la pause : c'est la pause qui le cache)
        foreach (QuestJournal journal in Object.FindObjectsByType<QuestJournal>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            // Il doit être DANS le PausePanel, sinon il reste affiché tout le temps
            PauseScreen pause = Object.FindFirstObjectByType<PauseScreen>(FindObjectsInactive.Include);
            if (pause != null && !journal.transform.IsChildOf(pause.transform))
            {
                Undo.SetTransformParent(journal.transform, pause.transform, "Ranger carnet dans la pause");
                report.Add($"{journal.name} rangé dans {pause.name}.");
            }

            if (journal.gameObject.activeSelf) continue;
            Undo.RecordObject(journal.gameObject, "Activer carnet");
            journal.gameObject.SetActive(true);
            report.Add($"{journal.name} réactivé.");
        }

        // 1a-bis. Le DialogueManager doit être ACTIF : son Awake crée "instance" puis cache le panneau tout seul
        foreach (DialogueManager dm in Object.FindObjectsByType<DialogueManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (dm.gameObject.activeSelf) continue;
            Undo.RecordObject(dm.gameObject, "Activer dialogue");
            dm.gameObject.SetActive(true);
            report.Add($"{dm.name} (DialogueManager) réactivé.");
        }

        // 1b. Les éléments du HUD ont besoin d'un CanvasGroup (posés avant le RequireComponent)
        List<Component> hudParts = new List<Component>();
        hudParts.AddRange(Object.FindObjectsByType<QuestNotification>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        hudParts.AddRange(Object.FindObjectsByType<PowerGauge>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        hudParts.AddRange(Object.FindObjectsByType<InteractPrompt>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        foreach (Component part in hudParts)
        {
            if (part.GetComponent<CanvasGroup>() != null) continue;
            Undo.AddComponent<CanvasGroup>(part.gameObject);
            report.Add($"CanvasGroup ajouté sur {part.name}.");
        }

        // 2. Les champs qui pointent vers un PREFAB du dossier (ou vides) → l'écran de la scène
        foreach (MonoBehaviour mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mb == null || mb.GetType().Namespace == null || !mb.GetType().Namespace.StartsWith("Goblfin")) continue;

            SerializedObject so = new SerializedObject(mb);
            SerializedProperty p = so.GetIterator();
            bool changed = false;

            while (p.NextVisible(true))
            {
                if (p.propertyType != SerializedPropertyType.ObjectReference) continue;

                System.Type wanted = null;
                Object current = p.objectReferenceValue;

                if (current is UIScreen || current is QuestJournal)
                {
                    // Pointe vers un fichier du dossier Project → mauvais
                    if (!EditorUtility.IsPersistent(current)) continue;
                    wanted = current.GetType();
                }
                else if (current == null && screenByField.TryGetValue(p.name, out System.Type t))
                {
                    wanted = t;
                }
                else if (current == null)
                {
                    // Champ vide typé directement (ex : ConfirmPopup confirmPopup) → "PPtr<$ConfirmPopup>"
                    wanted = ScreenTypeFromName(p.type);
                }

                if (wanted == null) continue;

                Object found = Object.FindFirstObjectByType(wanted, FindObjectsInactive.Include);
                if (found == null || found == current) continue;

                p.objectReferenceValue = found;
                changed = true;
                report.Add($"{mb.name} → {p.name} = {found.name}");
            }

            if (changed) so.ApplyModifiedProperties();
        }

        // 3. Les scènes dans la liste du build (MenuScene en premier)
        AddScenesToBuild();
    }

    // MenuScene en index 0, puis les scènes de jeu (sans toucher aux autres déjà présentes)
    static void AddScenesToBuild()
    {
        string[] wanted =
        {
            "Assets/Project/Scenes/MenuScene.unity",
            "Assets/Project/Scenes/KaciScene.unity",
            "Assets/Project/Scenes/FinalScene.unity",
        };

        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool changed = false;

        for (int i = wanted.Length - 1; i >= 0; i--)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(wanted[i]) == null) continue;
            if (scenes.Exists(s => s.path == wanted[i])) continue;
            scenes.Insert(0, new EditorBuildSettingsScene(wanted[i], true));
            changed = true;
            report.Add("Scène ajoutée au build : " + System.IO.Path.GetFileNameWithoutExtension(wanted[i]));
        }

        // MenuScene toujours en premier (c'est la scène de lancement)
        int menu = scenes.FindIndex(s => s.path == wanted[0]);
        if (menu > 0)
        {
            EditorBuildSettingsScene m = scenes[menu];
            scenes.RemoveAt(menu);
            scenes.Insert(0, m);
            changed = true;
            report.Add("MenuScene mise en premier dans le build.");
        }

        if (changed) EditorBuildSettings.scenes = scenes.ToArray();
    }

    // "PPtr<$ConfirmPopup>" → typeof(ConfirmPopup), seulement pour les écrans UI
    static System.Type ScreenTypeFromName(string ptr)
    {
        if (!ptr.StartsWith("PPtr<$")) return null;
        string name = ptr.Substring(6, ptr.Length - 7);
        foreach (System.Type t in TypeCache.GetTypesDerivedFrom<UIScreen>())
            if (t.Name == name) return t;
        return name == nameof(QuestJournal) ? typeof(QuestJournal) : null;
    }

    [MenuItem("Tools/Goblfin/UI/Créer le prefab PF_UI_Game")]
    static void MakePrefab()
    {
        PauseScreen pause = Object.FindFirstObjectByType<PauseScreen>(FindObjectsInactive.Include);
        Transform root = pause != null ? pause.GetComponentInParent<Canvas>(true).rootCanvas.transform.parent : null;
        if (root == null)
        {
            EditorUtility.DisplayDialog("UI Gobflin", "Pas trouvé l'objet racine UI_Game (parent du Canvas).", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder(PrefabFolder))
            AssetDatabase.CreateFolder("Assets/Project/Prefabs", "UI");

        PrefabUtility.SaveAsPrefabAssetAndConnect(root.gameObject, PrefabFolder + "/PF_UI_Game.prefab", InteractionMode.UserAction);
        EditorUtility.DisplayDialog("UI Gobflin", "PF_UI_Game créé dans " + PrefabFolder +
            "\n\nDans une AUTRE scène : re-glisser le Player sur InteractPrompt et PowerGauge.", "OK");
    }

    // ───────────────────────────── HUD ─────────────────────────────

    static void BuildHUD(Transform canvas, PlayerManager player, InputReader inputReader)
    {
        Transform hud = canvas.Find("HUD");
        if (hud == null)
        {
            GameObject go = CreateUI("HUD", canvas);
            Stretch(go);
            hud = go.transform;
            report.Add("HUD créé.");
        }
        hud.SetAsFirstSibling();   // le HUD est dessiné SOUS les menus

        // Pastille E
        if (hud.Find("InteractPrompt") == null)
        {
            GameObject prompt = CreateUI("InteractPrompt", hud);
            Place(prompt, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(50, 50));
            AddImage(prompt, Color.white, BuiltinSprite("UI/Skin/Knob.psd"), false);
            TextMeshProUGUI key = AddText(prompt.transform, "TxtKey", "E", 30, Color.black);
            Stretch(key.gameObject);
            key.fontStyle = FontStyles.Bold;

            InteractPrompt script = Undo.AddComponent<InteractPrompt>(prompt);
            Set(script, "player", player);
            Set(script, "inputReader", inputReader);
            Set(script, "txtKey", key);
            report.Add("Pastille E créée.");
        }

        BuildPowerGauge(hud, player);

        // Notification de quête
        if (hud.Find("QuestNotification") == null)
        {
            GameObject notif = CreateUI("QuestNotification", hud);
            Place(notif, new Vector2(0.5f, 1f), new Vector2(0, -80), new Vector2(700, 80));
            AddImage(notif, new Color(0f, 0f, 0f, 0.6f), BuiltinSprite("UI/Skin/UISprite.psd"), false);
            TextMeshProUGUI msg = AddText(notif.transform, "TxtMessage", "Une quête a été ajoutée dans mon carnet", 30, Color.white);
            Stretch(msg.gameObject, 10);

            QuestNotification script = Undo.AddComponent<QuestNotification>(notif);
            Set(script, "txtMessage", msg);
            report.Add("Notification de quête créée.");
        }
    }

    // ─────────────────────────── Jauge de lancer seule ───────────────────────────

    [MenuItem("Tools/Goblfin/UI/Ajouter la jauge de lancer (scène ouverte)")]
    static void AddPowerGauge()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("UI Gobflin", "Arrête le Play d'abord.", "OK");
            return;
        }
        report.Clear();

        // 1. Le joueur : la jauge lit sa puissance de lancer
        PlayerManager player = Object.FindFirstObjectByType<PlayerManager>(FindObjectsInactive.Include);
        if (player == null)
        {
            EditorUtility.DisplayDialog("UI Gobflin", "Pas de joueur (PlayerManager) dans la scène.", "OK");
            return;
        }

        // 2. Le Canvas : celui de la scène, sinon on en crée un
        Canvas canvas = FindMainCanvas();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasGo, "Créer Canvas");
            canvasGo.layer = LayerMask.NameToLayer("UI");
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            report.Add("Canvas créé.");
        }

        // 3. Le HUD (dessiné sous les menus), puis la jauge dedans
        Transform hud = canvas.transform.Find("HUD");
        if (hud == null)
        {
            GameObject go = CreateUI("HUD", canvas.transform);
            Stretch(go);
            hud = go.transform;
            report.Add("HUD créé.");
        }
        hud.SetAsFirstSibling();

        BuildPowerGauge(hud, player);
        if (report.Count == 0) report.Add("La jauge existe déjà dans HUD.");

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Selection.activeGameObject = hud.Find("PowerGauge").gameObject;
        EditorUtility.DisplayDialog("UI Gobflin", string.Join("\n", report) + "\n\nPense à Ctrl+S !", "OK");
    }

    static void BuildPowerGauge(Transform hud, PlayerManager player)
    {
        if (hud.Find("PowerGauge") == null)
        {
            GameObject gauge = CreateUI("PowerGauge", hud);
            Place(gauge, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30, 150));
            AddImage(gauge, new Color(0f, 0f, 0f, 0.6f), BuiltinSprite("UI/Skin/UISprite.psd"), false);

            GameObject fillGo = CreateUI("Fill", gauge.transform);
            Stretch(fillGo, 4);
            Image fill = AddImage(fillGo, Color.white, BuiltinSprite("UI/Skin/UISprite.psd"), false);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Vertical;
            fill.fillOrigin = (int)Image.OriginVertical.Bottom;
            fill.fillAmount = 0.5f;

            PowerGauge script = Undo.AddComponent<PowerGauge>(gauge);
            Set(script, "player", player);
            Set(script, "fill", fill);
            SetGradient(script, "colors", GreenToRed());
            report.Add("Jauge de puissance créée.");
        }
    }

    // ─────────────────────────── Contrôles ───────────────────────────

    static ControlsScreen BuildControlsPanel(Transform canvas, InputReader inputReader)
    {
        Transform existing = canvas.Find("ControlsPanel");
        if (existing != null)
        {
            report.Add("ControlsPanel existe déjà : pas touché.");
            return existing.GetComponent<ControlsScreen>();
        }

        GameObject panel = CreateUI("ControlsPanel", canvas);
        Stretch(panel);
        InsertBeforeConfirm(panel.transform, canvas);

        GameObject bg = CreateUI("Background", panel.transform);
        Stretch(bg);
        AddImage(bg, new Color(0f, 0f, 0f, 0.9f), null, true);

        TextMeshProUGUI title = AddText(panel.transform, "Title", "Contrôles", 60, Color.white);
        Place(title.gameObject, new Vector2(0.5f, 1f), new Vector2(0, -100), new Vector2(800, 80));
        title.fontStyle = FontStyles.Bold;

        // La colonne des lignes
        GameObject rows = CreateUI("Rows", panel.transform);
        Place(rows, new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(900, 650));
        VerticalLayoutGroup vlg = Undo.AddComponent<VerticalLayoutGroup>(rows);
        vlg.spacing = 10;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

        // Une ligne par touche modifiable
        foreach (RebindEnums rebind in System.Enum.GetValues(typeof(RebindEnums)))
        {
            GameObject row = CreateUI("Row_" + rebind, rows.transform);
            HorizontalLayoutGroup hlg = Undo.AddComponent<HorizontalLayoutGroup>(row);
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            Layout(row, -1, 50);

            TextMeshProUGUI label = AddText(row.transform, "Label", rebind.ToString(), 28, Color.white);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            Layout(label.gameObject, 400, 50);

            Button key = CreateButton(row.transform, "BtnKey", "?");
            Layout(key.gameObject, 300, 50);
            TextMeshProUGUI txtKey = key.GetComponentInChildren<TextMeshProUGUI>();
            txtKey.gameObject.name = "TxtKey";

            RebindButton rb = Undo.AddComponent<RebindButton>(key.gameObject);
            Set(rb, "inputReader", inputReader);
            SetEnum(rb, "rebind", (int)rebind);
            Set(rb, "txtLabel", label);
            Set(rb, "txtKey", txtKey);
        }

        // Boutons du bas
        GameObject buttons = CreateUI("Buttons", panel.transform);
        Place(buttons, new Vector2(0.5f, 0f), new Vector2(0, 100), new Vector2(700, 80));
        HorizontalLayoutGroup bh = Undo.AddComponent<HorizontalLayoutGroup>(buttons);
        bh.spacing = 40;
        bh.childAlignment = TextAnchor.MiddleCenter;
        bh.childControlWidth = true; bh.childControlHeight = true;
        bh.childForceExpandWidth = true; bh.childForceExpandHeight = true;

        Button reset = CreateButton(buttons.transform, "BtnReset", "Réinitialiser");
        Button back = CreateButton(buttons.transform, "BtnBack", "Retour");

        ControlsScreen script = Undo.AddComponent<ControlsScreen>(panel);
        Set(script, "btnBack", back);
        Set(script, "btnReset", reset);
        Set(script, "inputReader", inputReader);

        report.Add("ControlsPanel créé (10 lignes de rebind).");
        return script;
    }

    // ─────────────────────────── Victoire ───────────────────────────

    static VictoryScreen BuildVictoryPanel(Transform canvas)
    {
        Transform existing = canvas.Find("VictoryPanel");
        if (existing != null)
        {
            report.Add("VictoryPanel existe déjà : pas touché.");
            return existing.GetComponent<VictoryScreen>();
        }

        GameObject panel = CreateUI("VictoryPanel", canvas);
        Stretch(panel);
        InsertBeforeConfirm(panel.transform, canvas);

        GameObject bg = CreateUI("Background", panel.transform);
        Stretch(bg);
        AddImage(bg, new Color(0f, 0f, 0f, 0.9f), null, true);

        TextMeshProUGUI title = AddText(panel.transform, "Title", "BRAVO JEUNE GOBELIN,\nTU AS RÉUSSI TA MISSION", 72, new Color(1f, 0.85f, 0.3f));
        Place(title.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0, 100), new Vector2(1400, 300));
        title.fontStyle = FontStyles.Bold;

        Button menu = CreateButton(panel.transform, "BtnMenu", "Retour au menu principal");
        Place(menu.gameObject, new Vector2(0.5f, 0f), new Vector2(0, 150), new Vector2(500, 80));

        VictoryScreen script = Undo.AddComponent<VictoryScreen>(panel);
        Set(script, "btnMenu", menu);
        Set(script, "title", title.transform);

        report.Add("VictoryPanel créé.");
        return script;
    }

    static void BuildVictoryZone(VictoryScreen victory, PlayerManager player)
    {
        VictoryTrigger trigger = Object.FindFirstObjectByType<VictoryTrigger>(FindObjectsInactive.Include);
        if (trigger != null)
        {
            SetIfEmpty(trigger, "victoryScreen", victory);
            report.Add("VictoryZone existe déjà : Victory Screen vérifié.");
            return;
        }

        GameObject zone = new GameObject("VictoryZone");
        Undo.RegisterCreatedObjectUndo(zone, "Créer VictoryZone");
        zone.transform.position = player != null ? player.transform.position + player.transform.forward * 3f : Vector3.zero;

        BoxCollider box = Undo.AddComponent<BoxCollider>(zone);
        box.isTrigger = true;
        box.size = new Vector3(2f, 2f, 2f);

        VictoryTrigger script = Undo.AddComponent<VictoryTrigger>(zone);
        Set(script, "victoryScreen", victory);

        report.Add("VictoryZone créée à 3 m devant le joueur : DÉPLACE-LA vers l'artefact.");
    }

    // ─────────────────────────── Outils ───────────────────────────

    static GameObject CreateUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Créer UI");
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");
        return go;
    }

    // Plein parent (avec une marge optionnelle)
    static void Stretch(GameObject go, float margin = 0f)
    {
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(margin, margin);
        rt.offsetMax = new Vector2(-margin, -margin);
    }

    // Ancre en un point + position + taille
    static void Place(GameObject go, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void Layout(GameObject go, float width, float height)
    {
        LayoutElement le = Undo.AddComponent<LayoutElement>(go);
        if (width > 0) le.preferredWidth = width;
        if (height > 0) le.preferredHeight = height;
    }

    static Image AddImage(GameObject go, Color color, Sprite sprite, bool raycast)
    {
        Image img = Undo.AddComponent<Image>(go);
        img.sprite = sprite;
        if (sprite != null) img.type = Image.Type.Sliced;
        img.color = color;
        img.raycastTarget = raycast;
        return img;
    }

    static TextMeshProUGUI AddText(Transform parent, string name, string text, float size, Color color)
    {
        GameObject go = CreateUI(name, parent);
        TextMeshProUGUI tmp = Undo.AddComponent<TextMeshProUGUI>(go);
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    static Button CreateButton(Transform parent, string name, string label)
    {
        TMP_DefaultControls.Resources res = new TMP_DefaultControls.Resources { standard = BuiltinSprite("UI/Skin/UISprite.psd") };
        GameObject go = TMP_DefaultControls.CreateButton(res);
        Undo.RegisterCreatedObjectUndo(go, "Créer bouton");
        go.name = name;
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");

        TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28;
        tmp.color = Color.black;
        tmp.gameObject.layer = go.layer;

        Undo.AddComponent<UIButtonFX>(go);
        return go.GetComponent<Button>();
    }

    static void InsertBeforeConfirm(Transform panel, Transform canvas)
    {
        Transform confirm = canvas.Find("ConfirmPopup");
        if (confirm != null) panel.SetSiblingIndex(confirm.GetSiblingIndex());
        else panel.SetAsLastSibling();
    }

    static Sprite BuiltinSprite(string path) => AssetDatabase.GetBuiltinExtraResource<Sprite>(path);

    static Gradient GreenToRed()
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.green, 0f), new GradientColorKey(Color.yellow, 0.5f), new GradientColorKey(Color.red, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return g;
    }

    // Remplir un champ [SerializeField] privé
    static void Set(Object target, string field, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null) { report.Add($"⚠ Champ '{field}' introuvable sur {target.GetType().Name}"); return; }
        p.objectReferenceValue = value;
        so.ApplyModifiedProperties();
    }

    // Remplir seulement si le champ est vide (ne pas écraser ce que tu as réglé)
    static void SetIfEmpty(Object target, string field, Object value)
    {
        if (value == null) return;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null || p.objectReferenceValue != null) return;
        p.objectReferenceValue = value;
        so.ApplyModifiedProperties();
        report.Add($"{target.GetType().Name} → {field} branché.");
    }

    static void SetBool(Object target, string field, bool value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null) return;
        p.boolValue = value;
        so.ApplyModifiedProperties();
    }

    static void SetEnum(Object target, string field, int index)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null) return;
        p.enumValueIndex = index;
        so.ApplyModifiedProperties();
    }

    static void SetGradient(Object target, string field, Gradient value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null) return;
        p.gradientValue = value;
        so.ApplyModifiedProperties();
    }
}
