using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ColorPalette : ScriptableObject {

	[SerializeField] private ColorGroup[] m_colors;

	private static ColorPalette instance;
	public static ColorPalette Instance {
		get {
			if (instance == null) {
				instance = Resources.Load<ColorPalette>("ColorPalette");
			}
			return instance;
		}
	}

	public static Color Get(ColorId id) {
		return Instance.GetColor(id);
	}

	public static string GetHex(ColorId id) {
		return Instance.GetColor(id).ToHex();
	}

	public static string GetWithRichText(string text, ColorId id) {
		return $"<color={GetHex(id)}>{text}</color>";
	}

	private Color GetColor(ColorId id) {
		int index = (int)id;
		if (index < m_colors.Length) {
			return m_colors[index].color;
		}
		return Color.clear;
	}

	[Serializable]
	private class ColorGroup {
		public string id;
		public Color color;
	}

#if UNITY_EDITOR
	public void Save() {
		string path = Application.dataPath.Replace("Assets", AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(typeof(ColorId).ToString())[0]));
		string script = "public enum " + typeof(ColorId) + " {";
		for (int i = 0; i < m_colors.Length; i++) {
			script += "\n\t" + m_colors[i].id + ",";
		}
		script += "\n\tCount\n}";
		File.WriteAllText(path, script);
		AssetDatabase.Refresh();
		AssetDatabase.SaveAssets();
	}
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(ColorPalette))]
public class ColorPaletteEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		if (GUILayout.Button("Save")) {
			((ColorPalette)target).Save();
		}
	}
}
#endif
