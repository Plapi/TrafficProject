using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {

	private const float WIDTH = 2.5f;
	private const float HALF_WIDTH = WIDTH / 2f;
	private const float CURVE_DIST = 5f;

	private readonly List<Node> connexions = new();
	private Vector3[] meshVertices = new Vector3[0];
	private Vector2[] meshUvs = new Vector2[0];

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
			if (meshVertices.Length != 4) {
				meshVertices = new Vector3[4];
			}
			PerpPointToNode(connexions[0], out meshVertices[0], out meshVertices[1]);
			PerpMidPointToNode(connexions[0], out meshVertices[2], out meshVertices[3]);
		} else if (connexions.Count == 2) {
			if (meshVertices.Length != 24) {
				meshVertices = new Vector3[24];
				meshUvs = new Vector2[24];
			}

			Vector3 c0 = transform.InverseTransformPoint(connexions[0].transform.position);
			Vector3 p0 = GetMinCurvePos(Vector3.zero, c0);
			Vector3 c1 = transform.InverseTransformPoint(connexions[1].transform.position);
			Vector3 p1 = GetMinCurvePos(Vector3.zero, c1);
			List<Vector3> points = Bezier.GetPoints(p0, Vector3.zero, p1);

			PerpMidPointToNode(connexions[0], out meshVertices[1], out meshVertices[0]);
			PerpMidPointToNode(connexions[1], out meshVertices[^2], out meshVertices[^1]);
			for (int i = 0; i < points.Count - 1; i++) {
				Debug.DrawLine(transform.position + points[i], transform.position + points[i + 1], Color.blue);
				Utils.PerpendicularPoints(points[i], points[i + 1], out meshVertices[i * 2 + 2], out meshVertices[i * 2 + 3], HALF_WIDTH);
			}
		}

		if (meshFilter.mesh == null || meshFilter.mesh.vertices.Length != meshVertices.Length) {
			int index = 0;
			bool even = true;
			int[] triangles = new int[(meshVertices.Length - 2) * 3];
			for (int i = 0; i < triangles.Length; i += 3) {
				if (even) {
					triangles[i] = index + 2;
					triangles[i + 1] = index + 1;
					triangles[i + 2] = index;
				} else {
					triangles[i] = index;
					triangles[i + 1] = index + 1;
					triangles[i + 2] = index + 2;
				}
				even = !even;
				index++;
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

		if (meshUvs.Length != meshVertices.Length) {
			meshUvs = new Vector2[meshVertices.Length];
		}
		float topWidth = 0f;
		float bottomWidth = 0f;
		for (int i = 0; i < meshVertices.Length - 2; i += 2) {
			topWidth += Vector3.Distance(meshVertices[i], meshVertices[i + 2]);
			bottomWidth += Vector3.Distance(meshVertices[i + 1], meshVertices[i + 3]);
		}
		float tw = 0f;
		float bw = 0f;
		for (int i = 0; i < meshVertices.Length - 2; i += 2) {
			meshUvs[i] = new Vector2(tw / topWidth, 1f);
			meshUvs[i + 1] = new Vector2(bw / bottomWidth, 0f);
			tw += Vector3.Distance(meshVertices[i], meshVertices[i + 2]);
			bw += Vector3.Distance(meshVertices[i + 1], meshVertices[i + 3]);
		}
		meshUvs[^2] = new Vector2(1f, 1f);
		meshUvs[^1] = new Vector2(1f, 0f);

		meshFilter.mesh.vertices = meshVertices;
		meshFilter.mesh.uv = meshUvs;
	}

	private Vector3 GetMinCurvePos(Vector3 v0, Vector3 v1) {
		float dist = Vector3.Distance(v0, v1);
		if (dist / 2f < CURVE_DIST) {
			return Utils.MidPoint(v0, v1);
		}
		return Vector3.Lerp(v0, v1, CURVE_DIST / dist);
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
