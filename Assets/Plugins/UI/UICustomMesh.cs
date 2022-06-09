using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class UICustomMesh : UIItem {

	[SerializeField] private List<Vector2> vertices = new();
	[SerializeField] private List<Color> colors = new();
	[SerializeField] private Mesh mesh = default;

	private Material mat;

	private Material GetMaterial() {
		if (mat == null) {
			mat = new Material(Shader.Find("UI/Default"));
		}
		return mat;
	}

	private void Start() {
		if (TryGetComponent(out CanvasRenderer canvasRenderer)) {
			canvasRenderer.materialCount = 1;
			canvasRenderer.SetMaterial(GetMaterial(), 0);
			canvasRenderer.SetMesh(mesh);
		}
	}

#if UNITY_EDITOR
	public void OnSceneGUI() {
		for (int i = 0; i < vertices.Count; i++) {
			EditorGUI.BeginChangeCheck();
			Vector3 newPos = Handles.PositionHandle(transform.TransformPoint(vertices[i]), Quaternion.identity);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(this, "Change Look At Target Position");
				vertices[i] = transform.InverseTransformPoint(newPos);
			}
			Handles.Label(transform.TransformPoint(vertices[i]), i.ToString());
		}

		if (mesh == null) {
			mesh = new();
		}
		mesh.Clear();

		Vector3[] verticesV2 = new Vector3[vertices.Count];
		for (int i = 0; i < vertices.Count; i++) {
			verticesV2[i] = new Vector3(vertices[i].x, vertices[i].y, 0f);
		}
		mesh.vertices = verticesV2;

		if (vertices.Count >= 3) {
			int[] triangles = new int[(vertices.Count - 2) * 3];
			for (int i = 0; i < vertices.Count - 2; i++) {
				if (i % 2 == 0) {
					triangles[i * 3] = i;
					triangles[i * 3 + 1] = i + 1;
					triangles[i * 3 + 2] = i + 2;
				} else {
					triangles[i * 3] = i;
					triangles[i * 3 + 1] = i + 2;
					triangles[i * 3 + 2] = i + 1;
				}
			}
			mesh.triangles = triangles;
		}

		while (colors.Count != vertices.Count) {
			if (colors.Count < vertices.Count) {
				colors.Add(Color.white);
			} else {
				colors.RemoveAt(colors.Count - 1);
			}
		}
		mesh.colors = colors.ToArray();

		if (!TryGetComponent(out CanvasRenderer canvasRenderer)) {
			canvasRenderer = gameObject.AddComponent<CanvasRenderer>();
		}
		canvasRenderer.SetMesh(mesh);
	}
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(UICustomMesh)), CanEditMultipleObjects]
public class UICustomMeshEditor : Editor {
	private void OnSceneGUI() {
		((UICustomMesh)target).OnSceneGUI();
	}
}
#endif
