

// À placer dans un dossier "Editor", ex: Assets/Editor/AnimationCreatorWindow.cs

using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Editor
{
	public class AnimationCreatorWindow : EditorWindow
	{
		const string PrefPrefabGuid = "AnimCreator.PrefabGuid";
		const string PrefFolder = "AnimCreator.Folder";
		const string DefaultFolder = "Assets/Projet/Arts/Animation";
 
		const string PrefSceneGuid = "AnimCreator.SceneGuid";
 
		ObjectField prefabField;
		ObjectField sceneField;
		TextField folderField;
		TextField nameField;
		HelpBox status;
		string lastAnimFolder;
 
		[MenuItem("Tools/Animation Creator")]
		public static void Open()
		{
			var window = GetWindow<AnimationCreatorWindow>("Animation Creator");
			window.minSize = new Vector2(340, 220);
		}
 
		void CreateGUI()
		{
			var root = rootVisualElement;
 
			// --- Prefab AnimBase ---
			prefabField = new ObjectField("Prefab AnimBase")
			{
				objectType = typeof(GameObject),
				allowSceneObjects = false
			};
			prefabField.value = LoadSavedPrefab();
			prefabField.RegisterValueChangedCallback(evt =>
			{
				var path = AssetDatabase.GetAssetPath(evt.newValue);
				EditorPrefs.SetString(PrefPrefabGuid, string.IsNullOrEmpty(path) ? "" : AssetDatabase.AssetPathToGUID(path));
			});
			root.Add(prefabField);
 
			// --- Scène d'animation ---
			sceneField = new ObjectField("Scène d'animation")
			{
				objectType = typeof(SceneAsset),
				allowSceneObjects = false
			};
			sceneField.value = LoadSavedScene();
			sceneField.RegisterValueChangedCallback(evt =>
			{
				var path = AssetDatabase.GetAssetPath(evt.newValue);
				EditorPrefs.SetString(PrefSceneGuid, string.IsNullOrEmpty(path) ? "" : AssetDatabase.AssetPathToGUID(path));
			});
			root.Add(sceneField);
 
			// --- Dossier de sortie ---
			var folderRow = new VisualElement { style = { flexDirection = FlexDirection.Row } };
			folderField = new TextField("Dossier de sortie") { value = EditorPrefs.GetString(PrefFolder, DefaultFolder) };
			folderField.style.flexGrow = 1;
			folderField.RegisterValueChangedCallback(evt => EditorPrefs.SetString(PrefFolder, evt.newValue));
			var browse = new Button(BrowseFolder) { text = "..." };
			folderRow.Add(folderField);
			folderRow.Add(browse);
			root.Add(folderRow);
 
			// --- Nom de l'animation ---
			nameField = new TextField("Nom de l'animation");
			root.Add(nameField);
 
			// --- Boutons ---
			var createButton = new Button(OnCreate) { text = "Create" };
			createButton.style.height = 30;
			root.Add(createButton);
 
			var showButton = new Button(ShowFolder) { text = "Afficher le dossier" };
			root.Add(showButton);
 
			// --- Statut ---
			status = new HelpBox("Entre un nom puis clique sur Create.", HelpBoxMessageType.Info);
			root.Add(status);
		}
 
		// ------------------------------------------------------------------
 
		void OnCreate()
		{
			var prefab = prefabField.value as GameObject;
			if (prefab == null)
			{
				SetStatus("Prefab AnimBase introuvable. Assigne-le dans le champ.", HelpBoxMessageType.Error);
				return;
			}
 
			string animName = Sanitize(nameField.value);
			if (string.IsNullOrEmpty(animName))
			{
				SetStatus("Donne un nom valide à l'animation.", HelpBoxMessageType.Error);
				return;
			}
 
			string root = folderField.value.Replace('\\', '/').TrimEnd('/');
			if (root != "Assets" && !root.StartsWith("Assets/"))
			{
				SetStatus("Le dossier de sortie doit être dans Assets/.", HelpBoxMessageType.Error);
				return;
			}
 
			string animFolder = root + "/" + animName;
			string clipPath = animFolder + "/" + animName + ".anim";
			if (File.Exists(clipPath))
			{
				SetStatus("Une animation \"" + animName + "\" existe déjà dans ce dossier.", HelpBoxMessageType.Error);
				return;
			}
 
			// 0) Bascule sur la scène d'animation (propose de sauvegarder la scène actuelle)
			var sceneAsset = sceneField.value as SceneAsset;
			if (sceneAsset != null)
			{
				string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
				if (SceneManager.GetActiveScene().path != scenePath)
				{
					if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
					{
						SetStatus("Annulé : la scène actuelle n'a pas été sauvegardée.", HelpBoxMessageType.Warning);
						return;
					}
					EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
				}
			}
 
			// 1) Dossier + clip vide
			EnsureFolder(animFolder);
			var clip = new AnimationClip { frameRate = 30f, name = animName };
			AssetDatabase.CreateAsset(clip, clipPath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			lastAnimFolder = animFolder;
 
			// 2) AnimBase dans la scène (réutilise celui qui existe déjà)
			var instance = FindInstanceInScene(prefab);
			if (instance == null)
			{
				instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());
				Undo.RegisterCreatedObjectUndo(instance, "Create AnimBase");
				instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			Selection.activeGameObject = instance;
			EditorGUIUtility.PingObject(instance);
 
			// 3) Pratique pour le dialogue de sauvegarde UMotion
			GUIUtility.systemCopyBuffer = animFolder;
			EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(animFolder));
 
			SetStatus("Prêt ! Dossier : " + animFolder + " (chemin copié dans le presse-papier).\n" +
			          "Glisse AnimBase dans le Pose Editor d'UMotion et anime.", HelpBoxMessageType.Info);
		}
 
		void ShowFolder()
		{
			string path = string.IsNullOrEmpty(lastAnimFolder) ? folderField.value : lastAnimFolder;
			var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
			if (obj != null) EditorGUIUtility.PingObject(obj);
			else SetStatus("Dossier introuvable : " + path, HelpBoxMessageType.Warning);
		}
 
		void BrowseFolder()
		{
			string picked = EditorUtility.OpenFolderPanel("Dossier de sortie des animations", "Assets", "");
			if (string.IsNullOrEmpty(picked)) return;
 
			picked = picked.Replace('\\', '/');
			string dataPath = Application.dataPath.Replace('\\', '/');
			if (!picked.StartsWith(dataPath))
			{
				SetStatus("Choisis un dossier à l'intérieur du projet (Assets/).", HelpBoxMessageType.Warning);
				return;
			}
			folderField.value = "Assets" + picked.Substring(dataPath.Length);
		}
 
		// ------------------------------------------------------------------
 
		static GameObject LoadSavedPrefab()
		{
			string guid = EditorPrefs.GetString(PrefPrefabGuid, "");
			if (!string.IsNullOrEmpty(guid))
			{
				var saved = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
				if (saved != null) return saved;
			}
 
			// Auto-détection par le nom
			foreach (var found in AssetDatabase.FindAssets("AnimBase t:Prefab"))
			{
				var path = AssetDatabase.GUIDToAssetPath(found);
				if (Path.GetFileNameWithoutExtension(path) == "AnimBase")
					return AssetDatabase.LoadAssetAtPath<GameObject>(path);
			}
			return null;
		}
 
		static SceneAsset LoadSavedScene()
		{
			string guid = EditorPrefs.GetString(PrefSceneGuid, "");
			if (!string.IsNullOrEmpty(guid))
			{
				var saved = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(guid));
				if (saved != null) return saved;
			}
 
			// Auto-détection : ta scène actuelle d'anim
			foreach (var found in AssetDatabase.FindAssets("UiMotionScene t:Scene"))
			{
				var path = AssetDatabase.GUIDToAssetPath(found);
				if (Path.GetFileNameWithoutExtension(path) == "UiMotionScene")
					return AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
			}
			return null;
		}
 
		static GameObject FindInstanceInScene(GameObject prefab)
		{
			foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
			{
				if (PrefabUtility.GetCorrespondingObjectFromSource(go) == prefab)
					return go;
			}
			return null;
		}
 
		static void EnsureFolder(string path)
		{
			var parts = path.Split('/');
			string current = parts[0]; // "Assets"
			for (int i = 1; i < parts.Length; i++)
			{
				string next = current + "/" + parts[i];
				if (!AssetDatabase.IsValidFolder(next))
					AssetDatabase.CreateFolder(current, parts[i]);
				current = next;
			}
		}
 
		static string Sanitize(string raw)
		{
			if (string.IsNullOrWhiteSpace(raw)) return "";
			return Regex.Replace(raw.Trim(), @"[^\w\-]+", "_");
		}
 
		void SetStatus(string message, HelpBoxMessageType type)
		{
			status.text = message;
			status.messageType = type;
		}
	}
}
 
