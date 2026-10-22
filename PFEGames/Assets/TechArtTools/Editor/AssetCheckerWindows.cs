using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ToolSaverio.Editor
{
	public class AssetCheckerWindow : EditorWindow
	{
		enum Severity { Warning, Error }

		class Issue
		{
			public string path;
			public string category;
			public string message;
			public Severity severity;
			public Action fix;
		}

		static readonly int[] TextureSizes = { 256, 512, 1024, 2048, 4096 };
		static readonly string[] TextureSizeLabels = { "256", "512", "1K", "2K", "4K" };

		// Réglages
		int maxTextureSize = 2048;
		int maxTriangles = 10000;
		bool checkTextures = true;
		bool checkMeshes = true;
		bool checkMaterials = true;

		// Résultats
		readonly List<Issue> issues = new List<Issue>();
		bool hasScanned;
		string lastScanLabel = "";
		string[] lastFolders = { "Assets" };

		// État UI
		Vector2 scroll;
		Action pending; // action différée (évite de modifier la liste pendant le dessin)

		[MenuItem("Tools/TechArtTools/AssetChecker")]
		static void Open()
		{
			var w = GetWindow<AssetCheckerWindow>();
			w.titleContent = new GUIContent("Asset Checker");
			w.minSize = new Vector2(480, 440);
		}

		void OnEnable() { wantsMouseMove = true; }

		// ------------------------------------------------------------------ Thème iOS

		struct Palette
		{
			public Color bg, card, fill, sep, label, secondary, blue, green, red, orange, segSel, switchOff;
		}

		Palette P;
		bool paletteReady, paletteIsPro;
		GUIStyle sTitle, sBody, sBodyRight, sSmall, sCenter, sSection, sBig, sChevron;

		const float M = 16f;      // marge extérieure
		const float RowH = 48f;   // hauteur d'une ligne de réglage
		const float IssueH = 60f; // hauteur d'une ligne de résultat

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

		void EnsureUI()
		{
			bool pro = EditorGUIUtility.isProSkin;
			if (paletteReady && pro == paletteIsPro && sTitle != null) return;

			paletteReady = true;
			paletteIsPro = pro;

			if (pro)
			{
				P = new Palette
				{
					bg = Hex("#000000"), card = Hex("#1C1C1E"), fill = Hex("#2C2C2E"), sep = Hex("#38383A"),
					label = Color.white, secondary = Hex("#8E8E93"),
					blue = Hex("#0A84FF"), green = Hex("#30D158"), red = Hex("#FF453A"), orange = Hex("#FF9F0A"),
					segSel = Hex("#636366"), switchOff = Hex("#39393D")
				};
			}
			else
			{
				P = new Palette
				{
					bg = Hex("#F2F2F7"), card = Color.white, fill = Hex("#EEEEF0"), sep = Hex("#E0E0E4"),
					label = Color.black, secondary = Hex("#8E8E93"),
					blue = Hex("#007AFF"), green = Hex("#34C759"), red = Hex("#FF3B30"), orange = Hex("#FF9500"),
					segSel = Color.white, switchOff = Hex("#E9E9EA")
				};
			}

			sTitle = MakeStyle(26, FontStyle.Bold, TextAnchor.MiddleLeft);
			sBody = MakeStyle(13, FontStyle.Normal, TextAnchor.MiddleLeft);
			sBodyRight = MakeStyle(13, FontStyle.Normal, TextAnchor.MiddleRight);
			sSmall = MakeStyle(11, FontStyle.Normal, TextAnchor.MiddleLeft);
			sCenter = MakeStyle(12, FontStyle.Bold, TextAnchor.MiddleCenter);
			sSection = MakeStyle(11, FontStyle.Normal, TextAnchor.MiddleLeft);
			sBig = MakeStyle(18, FontStyle.Normal, TextAnchor.MiddleCenter);
			sChevron = MakeStyle(20, FontStyle.Normal, TextAnchor.MiddleCenter);
		}

		// ------------------------------------------------------------------ Helpers de dessin

		static bool IsRepaint => Event.current.type == EventType.Repaint;

		static void Fill(Rect r, Color c, float radius)
		{
			if (!IsRepaint) return;
			radius = Mathf.Min(radius, Mathf.Min(r.width, r.height) * 0.5f);
			GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, c, 0f, radius);
		}

		static void Text(Rect r, string s, GUIStyle style, Color c)
		{
			var old = GUI.contentColor;
			GUI.contentColor = c;
			GUI.Label(r, s, style);
			GUI.contentColor = old;
		}

		static Color Tint(Color c, float a = 0.18f) => new Color(c.r, c.g, c.b, a);

		static Color Shade(Color c, bool hover, bool pressed)
		{
			if (pressed) return new Color(c.r * 0.78f, c.g * 0.78f, c.b * 0.78f, Mathf.Min(1f, c.a + 0.08f));
			if (hover) return new Color(Mathf.Lerp(c.r, 1f, 0.1f), Mathf.Lerp(c.g, 1f, 0.1f), Mathf.Lerp(c.b, 1f, 0.1f), Mathf.Min(1f, c.a + 0.05f));
			return c;
		}

		static Rect Block(float height)
		{
			var r = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));
			r.x += M;
			r.width -= M * 2f;
			return r;
		}

		static string Num(long v) => v.ToString("#,0", CultureInfo.InvariantCulture).Replace(',', ' ');

		static string TruncStart(string s, float width, float charW)
		{
			int max = Mathf.Max(8, (int)(width / charW));
			return s.Length <= max ? s : "…" + s.Substring(s.Length - (max - 1));
		}

		static string TruncEnd(string s, float width, float charW)
		{
			int max = Mathf.Max(8, (int)(width / charW));
			return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
		}

		/// <summary>Zone cliquable avec états hover / pressed. Retourne true au relâchement du clic.</summary>
		static bool Tap(Rect r, out bool hover, out bool pressed)
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

		bool PillButton(Rect r, string label, Color bg, Color fg, float radius)
		{
			bool tap = Tap(r, out bool hover, out bool pressed);
			Fill(r, Shade(bg, hover, pressed), radius);
			Text(r, label, sCenter, fg);
			return tap;
		}

		bool TextButton(Rect r, string label, Color color)
		{
			bool tap = Tap(r, out bool hover, out bool pressed);
			Text(r, label, sBodyRight, pressed ? Tint(color, 0.5f) : hover ? Color.Lerp(color, Color.white, 0.2f) : color);
			return tap;
		}

		bool Switch(Rect r, bool on)
		{
			if (Tap(r, out _, out _)) on = !on;
			Fill(r, on ? P.green : P.switchOff, r.height * 0.5f);
			float d = r.height - 4f;
			var knob = new Rect(on ? r.xMax - d - 2f : r.x + 2f, r.y + 2f, d, d);
			Fill(knob, Color.white, d * 0.5f);
			return on;
		}

		int Segmented(Rect r, string[] labels, int selected)
		{
			float w = r.width / labels.Length;
			Fill(r, P.fill, 9f);
			if (selected >= 0)
				Fill(new Rect(r.x + selected * w + 2f, r.y + 2f, w - 4f, r.height - 4f), P.segSel, 7f);

			for (int i = 0; i < labels.Length; i++)
				Text(new Rect(r.x + i * w, r.y, w, r.height), labels[i], sCenter, i == selected ? P.label : P.secondary);

			if (Tap(r, out _, out _))
			{
				int i = Mathf.Clamp((int)((Event.current.mousePosition.x - r.x) / w), 0, labels.Length - 1);
				return i;
			}
			return selected;
		}

		int Stepper(Rect r)
		{
			Fill(r, P.fill, 9f);
			EditorGUI.DrawRect(new Rect(r.center.x - 0.5f, r.y + 7f, 1f, r.height - 14f), P.sep);

			var minus = new Rect(r.x, r.y, r.width * 0.5f, r.height);
			var plus = new Rect(r.center.x, r.y, r.width * 0.5f, r.height);
			Text(minus, "−", sBig, P.label);
			Text(plus, "+", sBig, P.label);

			if (Tap(minus, out _, out _)) return -1;
			if (Tap(plus, out _, out _)) return 1;
			return 0;
		}

		Rect SectionHeader(string title)
		{
			var r = Block(26);
			var t = new Rect(r.x + 16f, r.y, r.width - 32f, r.height);
			Text(t, title.ToUpperInvariant(), sSection, P.secondary);
			return r;
		}
	
		void OnGUI()
		{
			EnsureUI();

			var e = Event.current;
			if (e.type == EventType.MouseMove) Repaint();
			if (e.type == EventType.Repaint)
				EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), P.bg);

			scroll = GUILayout.BeginScrollView(scroll, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

			GUILayout.Space(18);
			DrawHeader();
			GUILayout.Space(16);
			DrawSettings();
			GUILayout.Space(18);
			DrawScanButtons();
			GUILayout.Space(22);
			DrawResults();
			GUILayout.Space(24);

			GUILayout.EndScrollView();

			if (pending != null)
			{
				var action = pending;
				pending = null;
				EditorApplication.delayCall += () => { action(); Repaint(); };
			}
		}

		void DrawHeader()
		{
			var r = Block(34);
			Text(new Rect(r.x + 4f, r.y, r.width, r.height), "Asset Checker", sTitle, P.label);

			var r2 = Block(20);
			Text(new Rect(r2.x + 4f, r2.y, r2.width, r2.height),
				"Valide tes textures, meshes et matériaux en un coup d'œil.", sBody, P.secondary);
		}

		void DrawSettings()
		{
			SectionHeader("Règles");

			int rows = 3 + (checkTextures ? 1 : 0) + (checkMeshes ? 1 : 0);
			var card = Block(rows * RowH);
			Fill(card, P.card, 14f);

			int index = 0;
			Rect NextRow()
			{
				var rr = new Rect(card.x, card.y + index * RowH, card.width, RowH);
				if (index > 0) EditorGUI.DrawRect(new Rect(rr.x + 16f, rr.y, rr.width - 16f, 1f), P.sep);
				index++;
				return rr;
			}
			void RowLabel(Rect rr, string s) =>
				Text(new Rect(rr.x + 16f, rr.y, rr.width - 200f, rr.height), s, sBody, P.label);
			Rect SwitchRect(Rect rr) => new Rect(rr.xMax - 16f - 51f, rr.y + (RowH - 31f) * 0.5f, 51f, 31f);

			// Textures
			var r = NextRow();
			RowLabel(r, "Textures");
			checkTextures = Switch(SwitchRect(r), checkTextures);

			if (checkTextures)
			{
				r = NextRow();
				RowLabel(r, "Résolution max");
				var seg = new Rect(r.xMax - 16f - 230f, r.y + 8f, 230f, 32f);
				int idx = Mathf.Max(0, Array.IndexOf(TextureSizes, maxTextureSize));
				idx = Segmented(seg, TextureSizeLabels, idx);
				maxTextureSize = TextureSizes[idx];
			}

			// Meshes
			r = NextRow();
			RowLabel(r, "Meshes / FBX");
			checkMeshes = Switch(SwitchRect(r), checkMeshes);

			if (checkMeshes)
			{
				r = NextRow();
				RowLabel(r, "Triangles max");
				var st = new Rect(r.xMax - 16f - 94f, r.y + 8f, 94f, 32f);
				Text(new Rect(st.x - 14f - 90f, r.y, 90f, r.height), Num(maxTriangles), sBodyRight, P.secondary);
				int d = Stepper(st);
				if (d != 0)
				{
					int step = Event.current.shift ? 10000 : 1000;
					maxTriangles = Mathf.Clamp(maxTriangles + d * step, 1000, 1000000);
				}
			}

			// Matériaux
			r = NextRow();
			RowLabel(r, "Matériaux & Shaders");
			checkMaterials = Switch(SwitchRect(r), checkMaterials);
		}

		void DrawScanButtons()
		{
			var r = Block(46);
			const float gap = 10f;
			var a = new Rect(r.x, r.y, (r.width - gap) * 0.5f, r.height);
			var b = new Rect(a.xMax + gap, r.y, a.width, r.height);

			if (PillButton(a, "Scan Project", P.blue, Color.white, 14f))
				pending = () => Scan(new[] { "Assets" }, "Projet entier");

			if (PillButton(b, "Scan Selected Folder", Tint(P.blue), P.blue, 14f))
			{
				pending = () =>
				{
					string folder = GetSelectedFolder();
					if (folder == null)
						EditorUtility.DisplayDialog("Asset Checker", "Sélectionne un dossier dans la fenêtre Project.", "OK");
					else
						Scan(new[] { folder }, folder);
				};
			}
		}

		void DrawResults()
		{
			int errors = issues.Count(i => i.severity == Severity.Error);
			int warnings = issues.Count - errors;
			int fixable = issues.Count(i => i.fix != null);

			var hdr = SectionHeader("Résultats");

			if (hasScanned && issues.Count > 0)
			{
				float x = hdr.x + 16f + 84f;
				x = Badge(x, hdr, P.red, errors);
				Badge(x, hdr, P.orange, warnings);

				if (fixable > 0)
				{
					var fr = new Rect(hdr.xMax - 16f - 170f, hdr.y, 170f, hdr.height);
					if (TextButton(fr, $"Tout corriger ({fixable})", P.blue))
						pending = FixAll;
				}
			}

			if (!hasScanned)
			{
				var card = Block(104);
				Fill(card, P.card, 14f);
				Text(new Rect(card.x, card.y + 30f, card.width, 20f), "Prêt à scanner", sCenter, P.label);
				Text(new Rect(card.x, card.y + 52f, card.width, 18f),
					"Lance un scan pour analyser tes assets.", sSmall.Centered(), P.secondary);
				return;
			}

			if (issues.Count == 0)
			{
				var card = Block(124);
				Fill(card, P.card, 14f);
				var badge = new Rect(card.center.x - 20f, card.y + 22f, 40f, 40f);
				Fill(badge, Tint(P.green, 0.2f), 20f);
				Text(badge, "OK", sCenter, P.green);
				Text(new Rect(card.x, card.y + 70f, card.width, 20f), "Tout est propre", sCenter, P.label);
				Text(new Rect(card.x, card.y + 90f, card.width, 18f),
					$"Aucun problème détecté — {lastScanLabel}", sSmall.Centered(), P.secondary);
				return;
			}

			var list = Block(issues.Count * IssueH);
			Fill(list, P.card, 14f);

			for (int i = 0; i < issues.Count; i++)
			{
				var row = new Rect(list.x, list.y + i * IssueH, list.width, IssueH);
				DrawIssueRow(row, issues[i], i > 0);
			}
		}

		float Badge(float x, Rect hdr, Color c, int count)
		{
			Fill(new Rect(x, hdr.y + hdr.height * 0.5f - 4f, 8f, 8f), c, 4f);
			Text(new Rect(x + 13f, hdr.y, 40f, hdr.height), count.ToString(), sSmall, P.secondary);
			return x + 13f + 8f + (count >= 10 ? 14f : 8f) + 12f;
		}

		void DrawIssueRow(Rect row, Issue issue, bool separator)
		{
			var e = Event.current;
			var hit = new Rect(row.x, row.y, row.width, row.height);
			bool hover = hit.Contains(e.mousePosition) && GUIUtility.hotControl == 0;

			if (separator)
				EditorGUI.DrawRect(new Rect(row.x + 64f, row.y, row.width - 64f, 1f), P.sep);

			if (hover)
				Fill(new Rect(row.x + 6f, row.y + 3f, row.width - 12f, row.height - 6f), P.fill, 10f);

			// Icône de l'asset + pastille de sévérité
			var tile = new Rect(row.x + 14f, row.y + 12f, 36f, 36f);
			Fill(tile, P.fill, 9f);
			var icon = AssetDatabase.GetCachedIcon(issue.path);
			if (icon != null && IsRepaint)
				GUI.DrawTexture(new Rect(tile.x + 6f, tile.y + 6f, 24f, 24f), icon, ScaleMode.ScaleToFit);

			var sevColor = issue.severity == Severity.Error ? P.red : P.orange;
			Fill(new Rect(tile.xMax - 9f, tile.yMax - 9f, 14f, 14f), P.card, 7f);
			Fill(new Rect(tile.xMax - 7f, tile.yMax - 7f, 10f, 10f), sevColor, 5f);

			// Bouton "Corriger" (traité avant la ligne pour avoir la priorité au clic)
			var fixRect = new Rect(row.xMax - 14f - 82f, row.y + 15f, 82f, 30f);
			bool fixTap = false;
			if (issue.fix != null)
				fixTap = PillButton(fixRect, "Corriger", Tint(P.blue), P.blue, 15f);
			else
				Text(new Rect(row.xMax - 34f, row.y, 20f, row.height), "›", sChevron, P.secondary);

			// Textes
			float textX = tile.xMax + 14f;
			float textW = (issue.fix != null ? fixRect.x - 10f : row.xMax - 38f) - textX;

			Text(new Rect(textX, row.y + 11f, textW, 20f), TruncEnd(issue.message, textW, 7f), sBody, P.label);
			Text(new Rect(textX, row.y + 31f, textW, 16f),
				$"{issue.category} · {TruncStart(issue.path, textW - 70f, 5.8f)}", sSmall, P.secondary);

			if (Tap(hit, out _, out _))
			{
				var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(issue.path);
				Selection.activeObject = obj;
				EditorGUIUtility.PingObject(obj);
			}

			if (fixTap)
				pending = () => { issue.fix(); Rescan(); };
		}
		void Rescan() => Scan(lastFolders, lastScanLabel);

		static string GetSelectedFolder()
		{
			var obj = Selection.activeObject;
			if (obj == null) return null;
			string path = AssetDatabase.GetAssetPath(obj);
			return AssetDatabase.IsValidFolder(path) ? path : null;
		}

		void Scan(string[] folders, string label)
		{
			lastFolders = folders;
			lastScanLabel = label;
			issues.Clear();

			try
			{
				if (checkTextures) ScanTextures(folders);
				if (checkMeshes) ScanMeshes(folders);
				if (checkMaterials) ScanMaterials(folders);
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}

			issues.Sort((a, b) => b.severity.CompareTo(a.severity));
			hasScanned = true;
			Repaint();
		}

		static string[] FindPaths(string filter, string[] folders)
		{
			return AssetDatabase.FindAssets(filter, folders)
				.Select(AssetDatabase.GUIDToAssetPath)
				.Where(p => p.StartsWith("Assets/"))
				.Distinct()
				.ToArray();
		}
	
		void ScanTextures(string[] folders)
		{
			var paths = FindPaths("t:Texture2D", folders);
			int limit = maxTextureSize;

			for (int i = 0; i < paths.Length; i++)
			{
				string path = paths[i];
				if (i % 20 == 0)
					EditorUtility.DisplayProgressBar("Asset Checker", "Textures…", i / (float)paths.Length);

				var importer = AssetImporter.GetAtPath(path) as TextureImporter;
				if (importer == null) continue;

				// Résolution
				var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				if (tex != null && Mathf.Max(tex.width, tex.height) > limit)
				{
					issues.Add(new Issue
					{
						path = path, category = "Texture", severity = Severity.Warning,
						message = $"Résolution {tex.width}×{tex.height} > {limit}px",
						fix = () => Modify(importer, imp => imp.maxTextureSize = limit)
					});
				}

				// MipMaps (inutiles pour sprites / UI / curseurs)
				bool needsMips = importer.textureType == TextureImporterType.Default
				                 || importer.textureType == TextureImporterType.NormalMap
				                 || importer.textureType == TextureImporterType.SingleChannel;
				if (needsMips && !importer.mipmapEnabled)
				{
					issues.Add(new Issue
					{
						path = path, category = "Texture", severity = Severity.Warning,
						message = "MipMaps désactivées",
						fix = () => Modify(importer, imp => imp.mipmapEnabled = true)
					});
				}

				// Compression
				if (importer.textureCompression == TextureImporterCompression.Uncompressed
				    && importer.textureType != TextureImporterType.GUI
				    && importer.textureType != TextureImporterType.Cursor)
				{
					issues.Add(new Issue
					{
						path = path, category = "Texture", severity = Severity.Warning,
						message = "Aucune compression appliquée",
						fix = () => Modify(importer, imp => imp.textureCompression = TextureImporterCompression.Compressed)
					});
				}
			}
		}
		void ScanMeshes(string[] folders)
		{
			var paths = FindPaths("t:Model", folders);
			for (int i = 0; i < paths.Length; i++)
			{
				string path = paths[i];
				EditorUtility.DisplayProgressBar("Asset Checker", "Meshes…", i / (float)paths.Length);

				var importer = AssetImporter.GetAtPath(path) as ModelImporter;
				if (importer == null) continue;

				long tris = 0;
				foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
				{
					if (obj is Mesh mesh)
						for (int s = 0; s < mesh.subMeshCount; s++)
							if (mesh.GetTopology(s) == MeshTopology.Triangles)
								tris += mesh.GetIndexCount(s) / 3;
				}

				if (tris > maxTriangles)
				{
					issues.Add(new Issue
					{
						path = path, category = "Mesh", severity = Severity.Warning,
						message = $"{Num(tris)} triangles > {Num(maxTriangles)}"
					});
				}

				if (importer.isReadable)
				{
					issues.Add(new Issue
					{
						path = path, category = "Mesh", severity = Severity.Warning,
						message = "Read/Write activé (double la mémoire)",
						fix = () => Modify(importer, imp => imp.isReadable = false)
					});
				}
			}
		}


		static readonly string[] DiscouragedShaderPrefixes = { "Legacy Shaders/", "Mobile/Diffuse", "Particles/Standard" };
		static readonly string[] MainTexProps = { "_BaseMap", "_MainTex" };

		void ScanMaterials(string[] folders)
		{
			var paths = FindPaths("t:Material", folders);
			for (int i = 0; i < paths.Length; i++)
			{
				string path = paths[i];
				EditorUtility.DisplayProgressBar("Asset Checker", "Matériaux…", i / (float)paths.Length);

				var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
				if (mat == null) continue;

				// Shader manquant / en erreur
				if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
				{
					issues.Add(new Issue
					{
						path = path, category = "Material", severity = Severity.Error,
						message = "Shader manquant ou en erreur (rose)"
					});
					continue;
				}

				// Shader legacy / non optimisé
				string shaderName = mat.shader.name;
				if (DiscouragedShaderPrefixes.Any(p => shaderName.StartsWith(p)))
				{
					issues.Add(new Issue
					{
						path = path, category = "Material", severity = Severity.Warning,
						message = $"Shader legacy : {shaderName}"
					});
				}

				// Slot de texture principal vide
				foreach (var prop in MainTexProps)
				{
					if (mat.HasProperty(prop) && mat.GetTexture(prop) == null)
					{
						issues.Add(new Issue
						{
							path = path, category = "Material", severity = Severity.Warning,
							message = $"Slot de texture principal vide ({prop})"
						});
						break;
					}
				}
			}
		}
	
		static void Modify<T>(T importer, Action<T> change) where T : AssetImporter
		{
			change(importer);
			importer.SaveAndReimport();
		}

		void FixAll()
		{
			var fixes = issues.Where(i => i.fix != null).ToList();
			if (!EditorUtility.DisplayDialog("Tout corriger",
				    $"Appliquer {fixes.Count} correction(s) ? Les assets seront réimportés.", "Appliquer", "Annuler"))
				return;

			try
			{
				for (int i = 0; i < fixes.Count; i++)
				{
					EditorUtility.DisplayProgressBar("Tout corriger", fixes[i].path, i / (float)fixes.Count);
					fixes[i].fix();
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}

			AssetDatabase.SaveAssets();
			Rescan();
		}
	}

	static class GUIStyleExtensions
	{
		/// <summary>Copie d'un style avec alignement centré.</summary>
		public static GUIStyle Centered(this GUIStyle s)
		{
			return new GUIStyle(s) { alignment = TextAnchor.MiddleCenter };
		}
	}
}