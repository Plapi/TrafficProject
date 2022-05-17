using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {

	private const float WIDTH = 2.5f;
	private const float HALF_WIDTH = WIDTH / 2f;
	private const float CURVE_DIST = 5f;

	private readonly List<Node> connexions = new();
	private Vector3[] meshVertices = new Vector3[0];

	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;

	public int ConnexionsCount => connexions.Count;

	private void Awake() {
		meshRenderer = new GameObject("mesh").AddComponent<MeshRenderer>();
		meshRenderer.transform.parent = transform;
		meshRenderer.transform.localPosition = Vector3.zero;
		meshRenderer.material = Resources.Load<Material>("Materials/Road");

		meshFilter = meshRenderer.gameObject.AddComponent<MeshFilter>();
	}

	public void AddConnexion(Node node) {
		connexions.Add(node);
		node.connexions.Add(this);
	}

	public void RemoveConnexion(Node node) {
		connexions.Remove(node);
		node.connexions.Remove(this);
	}

	public void UpdateMesh() {
		if (connexions.Count == 1) {
			if (meshVertices.Length != 5) {
				meshVertices = new Vector3[5];
				meshVertices[0] = Vector3.zero;
			}
			PerpPointToNode(connexions[0], out meshVertices[2], out meshVertices[1]);
			PerpMidPointToNode(connexions[0], out meshVertices[3], out meshVertices[4]);
		} else if (connexions.Count == 2) {
			if (meshVertices.Length != 27) {
				meshVertices = new Vector3[27];
			}
			PerpMidPointToNode(connexions[0], out Vector3 v1, out Vector3 v2);
			PerpMidPointToNode(connexions[1], out Vector3 v4, out Vector3 v5);
			IntersectPointBetweenNodes(connexions[0], connexions[1], out Vector3 v6, out Vector3 v3);

			Vector3 v32 = GetMinCurvePos(v3, v2);
			Vector3 v34 = GetMinCurvePos(v3, v4);
			Vector3 v65 = GetMinCurvePos(v6, v5);
			Vector3 v61 = GetMinCurvePos(v6, v1);

			List<Vector3> points = Bezier.GetBezierPoints(new() { v32, v3, v34 });
			meshVertices[1] = v2;
			for (int i = 0; i < points.Count; i++) {
				meshVertices[i + 2] = points[i];
			}

			meshVertices[13] = v4;
			meshVertices[14] = v5;
			points = Bezier.GetBezierPoints(new() { v65, v6, v61 });
			for (int i = 0; i < points.Count; i++) {
				meshVertices[i + 15] = points[i];
			}
			meshVertices[26] = v1;
			meshVertices[0] = Utils.MidPoint(meshVertices[8], meshVertices[18]);
		}

		if (meshFilter.mesh == null || meshFilter.mesh.vertices.Length != meshVertices.Length) {
			meshFilter.mesh = new();

			int[] triangles = new int[(meshVertices.Length - 1) * 3];
			for (int i = 0; i < meshVertices.Length - 1; i++) {
				triangles[i * 3] = 0;
				triangles[i * 3 + 1] = i + 1;
				if (i + 2 < meshVertices.Length) {
					triangles[i * 3 + 2] = i + 2;
				} else {
					triangles[i * 3 + 2] = 1;
				}
			}
			meshFilter.mesh.vertices = meshVertices;
			meshFilter.mesh.triangles = triangles;

			meshFilter.mesh.normals = new Vector3[meshVertices.Length];
			for (int i = 0; i < meshFilter.mesh.normals.Length; i++) {
				meshFilter.mesh.normals[i] = Vector3.up;
			}
		}
		meshFilter.mesh.vertices = meshVertices;
	}

	private Vector3 GetMinCurvePos(Vector3 v0, Vector3 v1) {
		return Vector3.Lerp(v0, v1, CURVE_DIST / Vector3.Distance(v0, v1)); ;
	}

	private void PerpPointToNode(Node node, out Vector3 left, out Vector3 right) {
		Vector3 dir = node.transform.position - transform.position;
		Vector3 cross = Vector3.Cross(dir, Vector3.up).normalized;
		left = cross * HALF_WIDTH;
		right = -cross * HALF_WIDTH;
	}

	private void PerpMidPointToNode(Node node, out Vector3 left, out Vector3 right) {
		Vector3 dir = node.transform.position - transform.position;
		Vector3 cross = Vector3.Cross(dir, Vector3.up).normalized;
		Vector3 midPoint = transform.InverseTransformPoint(Utils.MidPoint(transform.position, node.transform.position));
		left = midPoint + cross * HALF_WIDTH;
		right = midPoint - cross * HALF_WIDTH;
	}

	private void IntersectPointBetweenNodes(Node node0, Node node1, out Vector3 left, out Vector3 right) {
		PerpPointToNode(node0, out Vector3 ml0, out Vector3 mr0);
		node0.PerpMidPointToNode(this, out Vector3 l0, out Vector3 r0);
		Vector3 dirl0 = ((transform.position + ml0) - (node0.transform.position + r0)).normalized;
		Vector3 dirr0 = ((transform.position + mr0) - (node0.transform.position + l0)).normalized;

		PerpPointToNode(node1, out Vector3 ml1, out Vector3 mr1);
		node1.PerpMidPointToNode(this, out Vector3 l1, out Vector3 r1);
		Vector3 dirl1 = ((transform.position + mr1) - (node1.transform.position + l1)).normalized;
		Vector3 dirr1 = ((transform.position + ml1) - (node1.transform.position + r1)).normalized;

		if (Math3d.LineLineIntersection(out left, node0.transform.position + r0, dirl0, node1.transform.position + l1, dirl1)) {
			left = transform.InverseTransformPoint(left);
		} else {
			left = ml0;
		}

		if (Math3d.LineLineIntersection(out right, node0.transform.position + l0, dirr0, node1.transform.position + r1, dirr1)) {
			right = transform.InverseTransformPoint(right);
		} else {
			right = mr0;
		}
	}

#if UNITY_EDITOR
	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawCube(transform.position, Vector3.one * 0.25f);

		connexions.ForEach(cn => {
			Gizmos.DrawLine(transform.position, cn.transform.position);
		});

		bool showLabel = UnityEditor.Selection.activeGameObject == gameObject;
		Gizmos.color = Color.green;
		for (int i = 0; i < meshVertices.Length; i++) {
			Gizmos.DrawCube(transform.position + meshVertices[i], Vector3.one * 0.15f);
			if (showLabel) {
				UnityEditor.Handles.Label(transform.position + meshVertices[i], i.ToString());
			}
		}
	}
#endif
}
