using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class ColorComponent : MonoBehaviour {

	[SerializeField] [Color] private string id = default;
	[SerializeField] private Object obj = default;

	public ColorId Id {
		get {
			Enum.TryParse(id, out ColorId value);
			return value;
		}
		set => id = value.ToString();
	}

	private void Awake() {
		ApplyColor();
	}

	private void ApplyColor() {
		if (obj == null) {
			if (TryGetComponent(out TextMeshPro textMeshPro)) {
				obj = textMeshPro;
			} else if (TryGetComponent(out Image image)) {
				obj = image;
			} else if (TryGetComponent(out Renderer renderer)) {
				obj = renderer;
			}
		}

		if (obj != null) {
			Color color = ColorPalette.Get(Id);
			if (obj is TextMeshPro textMeshPro) {
				textMeshPro.color = color;
			} else if (obj is Image image) {
				image.color = color;
			} else if (obj is Renderer renderer) {
				Material mat = new(Application.isPlaying ? renderer.material : renderer.sharedMaterial);
				if (mat != null) {
					mat.color = color;
					renderer.material = mat;
				}
			}
		}
	}

#if UNITY_EDITOR
	private void LateUpdate() {
		if (Application.isPlaying) {
			return;
		}
		ApplyColor();
	}

	public void OnInspectorGUI() {
		EditorGUILayout.BeginHorizontal();
		Color color = ColorPalette.Get(Id);
		EditorGUILayout.ColorField("Color", color);
		EditorGUILayout.TextField(color.ToHex(), GUILayout.Width(65));
		EditorGUILayout.EndHorizontal();
	}
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(ColorComponent))]
public class ColorComponentEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		((ColorComponent)target).OnInspectorGUI();
	}
}
#endif

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ColorAttribute : PropertyAttribute {

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ColorAttribute))]
public class ColorDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		Enum.TryParse(property.stringValue, out ColorId value);
		property.stringValue = EditorGUI.EnumPopup(position, property.displayName, value).ToString();
	}
}
#endif
