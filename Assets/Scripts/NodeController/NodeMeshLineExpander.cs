using UnityEngine;

public class NodeMeshLineExpander : MonoBehaviour {

	private Vector3[] meshVertices = new Vector3[0];
	private Vector3[] expandPoints = new Vector3[0];

	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;

	private void Awake() {
		meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = Config.Instance.RoadLineMaterial;
		meshFilter = meshRenderer.gameObject.AddComponent<MeshFilter>();
	}

	public void SetColor(Color color) {
		meshRenderer.material.color = color;
	}

	public Vector3[] UpdateMesh(Vector3[] edgePoints, float value, bool oSide = false, bool goDown = false) {

		int vLength = edgePoints.Length * 2;
		if (meshVertices.Length != vLength) {
			meshVertices = new Vector3[vLength];
			expandPoints = new Vector3[edgePoints.Length];
		}

		if (!goDown) {
			for (int j = 0; j < edgePoints.Length - 1; j++) {
				meshVertices[j * 2] = edgePoints[j];
				if (oSide) {
					Utils.PerpendicularPoints(edgePoints[j], edgePoints[j + 1], out _, out meshVertices[j * 2 + 1], value);
				} else {
					Utils.PerpendicularPoints(edgePoints[j], edgePoints[j + 1], out meshVertices[j * 2 + 1], out _, value);
				}
				expandPoints[j] = meshVertices[j * 2 + 1];
			}
			meshVertices[^2] = edgePoints[^1];
			if (oSide) {
				Utils.PerpendicularPoints(edgePoints[^1], edgePoints[^2], out meshVertices[^1], out _, value);
			} else {
				Utils.PerpendicularPoints(edgePoints[^1], edgePoints[^2], out _, out meshVertices[^1], value);
			}

			expandPoints[^1] = meshVertices[^1];
		} else {
			for (int j = 0; j < edgePoints.Length; j++) {
				meshVertices[j * 2] = edgePoints[j];
				meshVertices[j * 2 + 1] = edgePoints[j] + Vector3.down * 1f;
			}
		}

		if (meshFilter.mesh == null || meshFilter.mesh.vertices.Length != meshVertices.Length) {

			int[] triangles = new int[(meshVertices.Length - 2) * 3];
			int index = 0;
			for (int i = 0; i < triangles.Length; i += 6) {
				if (oSide) {
					triangles[i] = index + 2;
					triangles[i + 1] = index + 1;
					triangles[i + 2] = index;
					triangles[i + 3] = index + 1;
					triangles[i + 4] = index + 2;
					triangles[i + 5] = index + 3;
				} else {
					triangles[i] = index;
					triangles[i + 1] = index + 1;
					triangles[i + 2] = index + 2;
					triangles[i + 3] = index + 3;
					triangles[i + 4] = index + 2;
					triangles[i + 5] = index + 1;
				}
				index += 2;
			}

			Vector3[] normals = new Vector3[meshVertices.Length];
			for (int i = 0; i < normals.Length; i++) {
				normals[i] = Vector3.up;
			}
			meshFilter.mesh = new Mesh {
				vertices = meshVertices,
				triangles = triangles,
				normals = normals
			};
		}

		meshFilter.mesh.vertices = meshVertices;
		meshFilter.mesh.RecalculateBounds();

		return expandPoints;
	}

	/*Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c) {
		Vector3 side0 = b - a;
		Vector3 side1 = c - a;
		return Vector3.Cross(side0, side1).normalized;
	}*/

#if UNITY_EDITOR
	private void OnDrawGizmos() {
		bool showLabel = UnityEditor.Selection.activeGameObject == gameObject;
		Gizmos.color = Color.blue;
		for (int i = 0; i < meshVertices.Length; i++) {
			//Gizmos.DrawCube(transform.position + meshVertices[i], Vector3.one * 0.15f);
			if (showLabel) {
				UnityEditor.Handles.Label(transform.position + meshVertices[i], i.ToString());
			}
		}
	}
#endif
}
