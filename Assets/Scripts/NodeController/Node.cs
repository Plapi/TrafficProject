using System.Collections.Generic;
using UnityEngine;
using Poly2Tri;

public class Node : MonoBehaviour {

	private const int SINGLE_CONNEXION_VERT_LENGTH = 8;
	private const int MULTIPLE_CONNEXION_VERT_LENGTH = 26;

	private readonly List<Node> connexions = new();
	private Vector3[] meshVertices = new Vector3[0];
	private Vector2[] meshUvs = new Vector2[0];

	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;

	public int ConnexionsCount => connexions.Count;
	public Point2D[] BoundPoints { get; private set; }

	private void Awake() {
		meshRenderer = new GameObject("mesh").AddComponent<MeshRenderer>();
		meshRenderer.transform.parent = transform;
		meshRenderer.transform.localPosition = Vector3.zero;
		meshRenderer.material = Resources.Load<Material>("Materials/Road");
		meshFilter = meshRenderer.gameObject.AddComponent<MeshFilter>();
		BoundPoints = new Point2D[0];
	}

	public List<Node> GetConnexions() {
		return connexions;
	}

	public void Connect(Node node) {
		connexions.Add(node);
		node.connexions.Add(this);
	}

	public void Disconnect(Node node) {
		bool remove0 = connexions.Remove(node);
		bool remove1 = node.connexions.Remove(this);
		if (!remove0 || !remove1) {
			Debug.LogError($"Connexion not found {remove0} {remove1}");
		}
	}

	public bool IsConnectedWith(Node node) {
		return connexions.Contains(node);
	}

	public bool HasConnectionBetween(Vector3 point, out Node connexion) {
		connexion = default;
		for (int i = 0; i < connexions.Count; i++) {
			PerpPointToNode(connexions[i], out Vector3 p0, out Vector3 p1);
			p0 += transform.position;
			p1 += transform.position;

			connexions[i].PerpPointToNode(this, out Vector3 p2, out Vector3 p3);
			p2 += connexions[i].transform.position;
			p3 += connexions[i].transform.position;

			Vector3 forwardLeft = point + (Vector3.forward + Vector3.left) * Config.Instance.RoadHalfWidth;
			Vector3 forwardRight = point + (Vector3.forward + Vector3.right) * Config.Instance.RoadHalfWidth;
			Vector3 backLeft = point + (Vector3.back + Vector3.left) * Config.Instance.RoadHalfWidth;
			Vector3 backRight = point + (Vector3.back + Vector3.right) * Config.Instance.RoadHalfWidth;

			Vector3[] points = new Vector3[5] {
				point, forwardLeft, forwardRight, backLeft, backRight
			};

			if (Utils.PolyContainsAnyPoint(p0, p1, p2, p3, points)) {
				connexion = connexions[i];
				return true;
			}
		}
		return false;
	}

	public bool HasAcceptedDistance(Node node) {
		return Vector3.Distance(transform.position, node.transform.position) >= Config.Instance.RoadWidth;
	}

	public bool HasAcceptedAngle(Node node) {
		if (connexions.Count <= 1) {
			return true;
		}
		for (int i = 0; i < connexions.Count; i++) {
			if (connexions[i] != node) {
				float angle = Utils.GetAngleSigned(node.transform.position, transform.position, connexions[i].transform.position);
				if (Mathf.Abs(angle) < Config.Instance.RoadsMinAngle) {
					return false;
				}
			}
		}
		return true;
	}

	public void UpdateMesh() {
		if (connexions.Count == 1) {
			if (meshVertices.Length != SINGLE_CONNEXION_VERT_LENGTH) {
				meshVertices = new Vector3[SINGLE_CONNEXION_VERT_LENGTH];
			}

			PerpPointToNode(connexions[0], out meshVertices[0], out meshVertices[6]);
			meshVertices[1] = meshVertices[7] = Vector3.zero;
			PerpMidPointToNode(connexions[0], out meshVertices[2], out meshVertices[4]);
			meshVertices[3] = meshVertices[5] = Utils.MidPoint(meshVertices[2], meshVertices[4]);

		} else if (connexions.Count >= 2) {
			int verticesLength = connexions.Count * MULTIPLE_CONNEXION_VERT_LENGTH;
			if (meshVertices.Length != verticesLength) {
				meshVertices = new Vector3[verticesLength];
			}

			int index = 0;
			for (int i = 0; i < connexions.Count; i++) {
				Node from = connexions[i];
				Node to = GetClosestNodeFor(from);

				Vector3 fromDir = (from.transform.position - transform.position).normalized;
				Vector3 toDir = (to.transform.position - transform.position).normalized;

				Vector3 fromPos = fromDir * Config.Instance.RoadCurveDist;
				Vector3 toPos = toDir * Config.Instance.RoadCurveDist;

				Vector3 fromRightPos = fromPos + Vector3.Cross(fromPos, Vector3.up).normalized * Config.Instance.RoadHalfWidth;
				Vector3 toRightPos = toPos + Vector3.Cross(Vector3.zero - toPos, Vector3.up).normalized * Config.Instance.RoadHalfWidth;

				Vector3 fromMiddlePoint = Utils.MidPoint(transform.InverseTransformPoint(from.transform.position), Vector3.zero);
				Vector3 toMiddlePoint = Utils.MidPoint(transform.InverseTransformPoint(to.transform.position), Vector3.zero);

				meshVertices[index++] = fromMiddlePoint + Vector3.Cross(fromMiddlePoint, Vector3.up).normalized * Config.Instance.RoadHalfWidth;
				meshVertices[index++] = fromMiddlePoint;

				meshVertices[index++] = fromRightPos;
				meshVertices[index++] = fromPos;

				Vector3 intersection = Intersection(fromRightPos, -fromDir, toRightPos, -toDir);
				const int halfCurvePoints = 4;
				for (int j = 1; j <= halfCurvePoints; j++) {
					meshVertices[index++] = Bezier.GetPoint(fromRightPos, intersection, toRightPos, 0.1f * j);
					meshVertices[index++] = Vector3.zero;
				}
				meshVertices[index++] = Bezier.GetPoint(fromRightPos, intersection, toRightPos, 0.5f);
				meshVertices[index++] = Vector3.zero;
				for (int j = 1; j <= halfCurvePoints; j++) {
					meshVertices[index++] = Bezier.GetPoint(fromRightPos, intersection, toRightPos, 0.1f * (halfCurvePoints + 1 + j));
					meshVertices[index++] = Vector3.zero;
				}

				meshVertices[index++] = toRightPos;
				meshVertices[index++] = toPos;

				meshVertices[index++] = toMiddlePoint - Vector3.Cross(toMiddlePoint, Vector3.up).normalized * Config.Instance.RoadHalfWidth;
				meshVertices[index++] = toMiddlePoint;
			}
		}

		int groups = Mathf.Max(2, connexions.Count);
		int groupLength = meshVertices.Length / groups;

		if (meshFilter.mesh == null || meshFilter.mesh.vertices.Length != meshVertices.Length) {

			int trianglesPerGroup = (groupLength - 2) * 3;

			List<int> triangles = new(trianglesPerGroup * groups);
			int triangleIndex = 0;

			for (int i = 0; i < groups; i++) {
				int index = groupLength * i;
				for (int j = 0; j < trianglesPerGroup; j += 6) {
					triangles.Add(index + 0);
					triangles.Add(index + 1);
					triangles.Add(index + 2);
					triangles.Add(index + 3);
					triangles.Add(index + 2);
					triangles.Add(index + 1);
					index += 2;
				}
				triangleIndex += 6;
			}
			Vector3[] normals = new Vector3[meshVertices.Length];
			for (int i = 0; i < normals.Length; i++) {
				normals[i] = Vector3.up;
			}
			meshFilter.mesh = new Mesh {
				vertices = meshVertices,
				triangles = triangles.ToArray(),
				normals = normals
			};
		}

#if UNITY_EDITOR
		int[] debugTriangles = meshFilter.mesh.triangles;
		for (int i = 0; i < debugTriangles.Length - 2; i += 3) {
			Debug.DrawLine(transform.position + meshVertices[debugTriangles[i]], transform.position + meshVertices[debugTriangles[i + 1]], Color.yellow);
			Debug.DrawLine(transform.position + meshVertices[debugTriangles[i + 1]], transform.position + meshVertices[debugTriangles[i + 1]], Color.yellow);
			Debug.DrawLine(transform.position + meshVertices[debugTriangles[i + 2]], transform.position + meshVertices[debugTriangles[i]], Color.yellow);
		}
#endif

		if (meshUvs.Length != meshVertices.Length) {
			meshUvs = new Vector2[meshVertices.Length];
		}
		for (int i = 0; i < groups; i++) {
			int start = i * groupLength;
			int end = (i + 1) * groupLength;
			float topWidth = 0f;
			float bottomWidth = 0f;
			for (int j = start; j < end - 2; j += 2) {
				topWidth += Vector3.Distance(meshVertices[j], meshVertices[j + 2]);
				bottomWidth += Vector3.Distance(meshVertices[j + 1], meshVertices[j + 3]);
			}
			float tw = 0f;
			float bw = 0f;
			for (int j = start; j < end - 2; j += 2) {
				meshUvs[j] = new Vector2(tw / topWidth, 0f);
				meshUvs[j + 1] = new Vector2(bw / bottomWidth, 1f);
				tw += Vector3.Distance(meshVertices[j], meshVertices[j + 2]);
				bw += Vector3.Distance(meshVertices[j + 1], meshVertices[j + 3]);
			}
			meshUvs[end - 2] = new Vector2(1f, 0f);
			meshUvs[end - 1] = new Vector2(1f, 1f);
		}

		meshFilter.mesh.vertices = meshVertices;
		meshFilter.mesh.uv = meshUvs;
		meshFilter.mesh.RecalculateBounds();

		int globalBoundsPointsLength = meshVertices.Length / 2;
		if (BoundPoints.Length != globalBoundsPointsLength) {
			BoundPoints = new Point2D[globalBoundsPointsLength];
			for (int i = 0; i < BoundPoints.Length; i++) {
				BoundPoints[i] = new();
			}
		}
		for (int i = 0; i < BoundPoints.Length; i++) {
			BoundPoints[i].X = transform.position.x + meshVertices[i * 2].x;
			BoundPoints[i].Y = transform.position.z + meshVertices[i * 2].z;
		}
	}

	private Node GetClosestNodeFor(Node node) {
		Node closestNode = default;
		float minAngle = float.MaxValue;
		for (int i = 0; i < connexions.Count; i++) {
			if (node != connexions[i]) {
				float angle = Utils.GetAngle360(node.transform.position, transform.position, connexions[i].transform.position);
				if (minAngle > angle) {
					minAngle = angle;
					closestNode = connexions[i];
				}
			}
		}
		return closestNode != null ? closestNode : connexions[0];
	}

	private Vector3 Intersection(Vector3 v0, Vector3 d0, Vector3 v1, Vector3 d1) {
		if (Math3d.LineLineIntersection(out Vector3 intersection, v0, d0, v1, d1)) {
			return intersection;
		}
		return Utils.MidPoint(v0, v1);
	}

	private void PerpPointToNode(Node node, out Vector3 right, out Vector3 left) {
		Vector3 dir = node.transform.position - transform.position;
		Vector3 cross = Vector3.Cross(dir, Vector3.up).normalized;
		right = -cross * Config.Instance.RoadHalfWidth;
		left = cross * Config.Instance.RoadHalfWidth;
	}

	private void PerpMidPointToNode(Node node, out Vector3 right, out Vector3 left) {
		Vector3 dir = node.transform.position - transform.position;
		Vector3 cross = Vector3.Cross(dir, Vector3.up).normalized;
		Vector3 midPoint = transform.InverseTransformPoint(Utils.MidPoint(transform.position, node.transform.position));
		right = midPoint - cross * Config.Instance.RoadHalfWidth;
		left = midPoint + cross * Config.Instance.RoadHalfWidth;
	}

	public void UpdateHighlightColor(bool correct) {
		meshRenderer.material.color = correct ? Color.white : Color.red;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);

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
