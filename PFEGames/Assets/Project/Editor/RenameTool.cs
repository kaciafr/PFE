using UnityEditor;
using UnityEngine;

namespace Projet.Tools.Datas
{
    public class RenameTool : EditorWindow
    {
        [MenuItem("Tools/Goblfin/Nomenclature")]
        static void Open() => GetWindow<RenameTool>("Nomenclature");

        private static readonly string[] Prefixes =
        {
            "(aucun)","MSH_", "PROPS_", "SK_", "T_", "M_", "MV_", "PF_", "PFV_", "A_", "AC_", "AVT_", "AVM_",
            "SH_", "SUB_", "VFX_", "PS_", "SFX_", "MUS_", "VO_", "MIX_", "UI_", "ICO_", "F_",
            "SC_", "SO_", "RT_", "PP_", "PHM_", "TS_"
        };

        private static readonly string[] Suffixes =
        {
            "(aucun)", "_BC", "_N", "_MS", "_MAOS", "_AO", "_E", "_H", "_MSK"
        };

        private int prefixIndex;
        private string assetName = "Object";
        private string variant = "";
        private int suffixIndex;
        private bool AddNumber;

        private string BuildName(int number)
        {
            string name = assetName;
            if (prefixIndex > 0) name = Prefixes[prefixIndex] + name;
            if (!string.IsNullOrEmpty(variant)) name += "_" + variant;
            if (AddNumber) name += "_" + number.ToString("00");
            if (suffixIndex > 0) name += Suffixes[suffixIndex];

            return name;
        }

        private void OnGUI()
        {
            prefixIndex = EditorGUILayout.Popup("Prefix", prefixIndex, Prefixes);
            assetName = EditorGUILayout.TextField("Asset Name", assetName);
            variant = EditorGUILayout.TextField("Variant", variant);
            suffixIndex = EditorGUILayout.Popup("Suffix", suffixIndex, Suffixes);
            AddNumber = EditorGUILayout.Toggle("Add Number", AddNumber);

            EditorGUILayout.HelpBox("Resultat : " + BuildName(1), MessageType.None);

            if (GUILayout.Button("Rename"))
                RenameSelection();
        }

        private void RenameSelection()
        {
            Object[] selection = Selection.objects;

            if (selection.Length == 0)
            {
                EditorUtility.DisplayDialog("Rename", "Please select an object to rename.", "OK");
                return;
            }

            for (int i = 0; i < selection.Length; i++)
            {
                string newName = BuildName(i + 1);
                string path = AssetDatabase.GetAssetPath(selection[i]);
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.RenameAsset(path, newName);
                }
                else
                {
                    Undo.RecordObject(selection[i], "Rename Object");
                    selection[i].name = newName;
                }
            }
        }
    }
}
