using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {

	private const int SINGLE_CONNEXION_VERT_LENGTH = 14;
	private const int MULTIPLE_CONNEXION_VERT_LENGTH = 24;

	private readonly List<Node> connexions = new();
	private Vector3[] meshVertices = new Vector3[0];

	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;
	private NodeMeshLines nodeMeshLines;

	public int ConnexionsCount => connexions.Count;

	private void Awake() {
		meshRenderer = new GameObject("mesh").AddComponent<MeshRenderer>();
		meshRenderer.transform.parent = transform;
		meshRenderer.transform.localPosition = Vector3.zero;
		UpdateHighlightColor(true);
		meshFilter = meshRenderer.gameObject.AddComponent<MeshFilter>();

		nodeMeshLines = gameObject.AddComponent<NodeMeshLines>();
		nodeMeshLines.transform.parent = transform;
		nodeMeshLines.transform.localPosition = Vector3.zero;
	}

	public List<Node> GetConnexions() {
		return connexions;
	}

	public void Connect(Node node) {
		if (!connexions.Contains(node)) {
			connexions.Add(node);
		}
		if (!node.connexions.Contains(this)) {
			node.connexions.Add(this);
		}
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

			NodeMeshLines.Line line = new() {
				points = new Vector3[SINGLE_CONNEXION_VERT_LENGTH]
			};

			PerpMidPointToNode(connexions[0], out meshVertices[1], out meshVertices[0]);
			PerpPointToNode(connexions[0], out meshVertices[3], out meshVertices[2]);

			//meshVertices[2] += (meshVertices[0] - meshVertices[2]).normalized * Config.Instance.RoadHalfWidth;
			//meshVertices[3] += (meshVertices[1] - meshVertices[3]).normalized * Config.Instance.RoadHalfWidth;

			line.points[0] = meshVertices[0];
			line.points[1] = meshVertices[2];

			Vector3 pLeft = meshVertices[2] + (meshVertices[2] - meshVertices[0]).normalized * Config.Instance.RoadHalfWidth;
			Vector3 pRight = meshVertices[3] + (meshVertices[3] - meshVertices[1]).normalized * Config.Instance.RoadHalfWidth;
			Vector3 pMiddle = Utils.MidPoint(pLeft, pRight);

			int index = 4;
			for (int j = 1; j <= 4; j++) {
				Vector3 leftPoint = Bezier.GetPoint(meshVertices[2], pLeft, pMiddle, j * 0.2f);
				Vector3 rightPoint = Bezier.GetPoint(meshVertices[3], pRight, pMiddle, j * 0.2f);

				meshVertices[index++] = leftPoint;
				meshVertices[index++] = rightPoint;

				line.points[j + 1] = leftPoint;
				line.points[12 - j] = rightPoint;
			}

			meshVertices[^2] = Bezier.GetPoint(meshVertices[2], pLeft, pMiddle, 0.98f);
			meshVertices[^1] = Bezier.GetPoint(meshVertices[3], pRight, pMiddle, 0.98f);

			line.points[6] = meshVertices[^2];
			line.points[7] = meshVertices[^1];

			line.points[^2] = meshVertices[3];
			line.points[^1] = meshVertices[1];

			nodeMeshLines.UpdateExpanderLines(new NodeMeshLines.Line[1] { line }, true);
			nodeMeshLines.UpdateMiddleLines(new Vector3[2] {
				Utils.MidPoint(meshVertices[0], meshVertices[1]),
				pMiddle
			});
		} else if (connexions.Count >= 2) {

			Vector3 c0 = transform.InverseTransformPoint(connexions[0].transform.position);
			Vector3 c1 = transform.InverseTransformPoint(connexions[1].transform.position);

			if (connexions.Count == 2 && !Utils.Intersection2D(c0, -c0.normalized, c1, -c1.normalized, out _)) {
				if (meshVertices.Length != SINGLE_CONNEXION_VERT_LENGTH) {
					meshVertices = new Vector3[SINGLE_CONNEXION_VERT_LENGTH];
				}
				PerpMidPointToNode(connexions[0], out meshVertices[1], out meshVertices[0]);
				PerpMidPointToNode(connexions[1], out meshVertices[2], out meshVertices[3]);

				nodeMeshLines.UpdateExpanderLines(new NodeMeshLines.Line[2] {
					new() {
						points = new Vector3[] { meshVertices[2], meshVertices[0] }
					},
					new() {
						points = new Vector3[] { meshVertices[1], meshVertices[3] }
					}
				});
				nodeMeshLines.UpdateMiddleLines(new Vector3[2] {
					Utils.MidPoint(meshVertices[0], meshVertices[1]),
					Utils.MidPoint(meshVertices[2], meshVertices[3]),
				});
			} else {

				List<Node> connexionsClockwise = GetConnexionsClockwise();
				int verticesLength = (connexionsClockwise.Count - 1) * MULTIPLE_CONNEXION_VERT_LENGTH;
				if (meshVertices.Length != verticesLength) {
					meshVertices = new Vector3[verticesLength];
				}
				int index = 0;

				NodeMeshLines.Line[] lines = new NodeMeshLines.Line[connexions.Count];

				List<Vector3[]> listMiddleLines = null;

				for (int i = 0; i < connexionsClockwise.Count - 1; i++) {

					lines[i] = new NodeMeshLines.Line {
						points = new Vector3[12]
					};
					Vector3[] middleLinesPoints = connexions.Count == 2 ? new Vector3[12] : null;

					Node from = connexionsClockwise[i];
					Node to = connexionsClockwise[i + 1];

					c0 = transform.InverseTransformPoint(from.transform.position);
					c1 = transform.InverseTransformPoint(to.transform.position);

					Vector3 c0Mid = Utils.MidPoint(c0, Vector3.zero);
					Vector3 c1Mid = Utils.MidPoint(c1, Vector3.zero);

					if (middleLinesPoints != null) {
						middleLinesPoints[0] = c0Mid;
					}

					Vector3 cp0 = c0.normalized * Mathf.Min(Vector3.Distance(c0Mid, Vector3.zero) - 0.1f, Config.Instance.RoadCurveDist);
					Vector3 cp1 = c1.normalized * Mathf.Min(Vector3.Distance(c1Mid, Vector3.zero) - 0.1f, Config.Instance.RoadCurveDist);

					PerpMidPointToNode(from, out Vector3 c0Left, out Vector3 c0Right);
					Utils.PerpendicularPoints(cp0, c0, out Vector3 cp0Right, out Vector3 cp0Left, Config.Instance.RoadHalfWidth);

					if (middleLinesPoints != null) {
						middleLinesPoints[1] = Utils.MidPoint(c0Left, c0Right);
					}

					PerpMidPointToNode(to, out Vector3 c1Left, out Vector3 c1Right);
					Utils.PerpendicularPoints(cp1, c1, out Vector3 cp1Right, out Vector3 cp1Left, Config.Instance.RoadHalfWidth);

					Vector3 interRight = Intersection(cp0Right, (cp0Right - c0Right).normalized, cp1Left, (cp1Left - c1Left).normalized);
					Vector3 interLeft = Intersection(cp1Right, (cp1Right - c1Right).normalized, cp0Left, (cp0Left - c0Left).normalized);

					meshVertices[index++] = c0Right;
					meshVertices[index++] = c0Left;
					meshVertices[index++] = cp0Right;
					meshVertices[index++] = cp0Left;

					lines[i].points[^1] = c0Left;
					lines[i].points[^2] = cp0Left;

					if (connexions.Count == 2) {
						lines[1] = new NodeMeshLines.Line {
							points = new Vector3[12]
						};
						lines[1].points[0] = c0Right;
						lines[1].points[1] = cp0Right;
					}

					for (int j = 1; j <= 8; j++) {
						Vector3 rightBPoint = Bezier.GetPoint(cp0Right, interRight, cp1Left, j * 0.1f);
						Vector3 leftBPoint = Bezier.GetPoint(cp0Left, interLeft, cp1Right, j * 0.1f);
						meshVertices[index++] = rightBPoint;
						meshVertices[index++] = leftBPoint;

						lines[i].points[10 - j] = leftBPoint;
						if (connexions.Count == 2) {
							lines[1].points[1 + j] = rightBPoint;
						}

						if (middleLinesPoints != null) {
							middleLinesPoints[j + 1] = Utils.MidPoint(rightBPoint, leftBPoint);
						}
					}

					meshVertices[index++] = cp1Left;
					meshVertices[index++] = cp1Right;
					meshVertices[index++] = c1Left;
					meshVertices[index++] = c1Right;

					lines[i].points[1] = cp1Right;
					lines[i].points[0] = c1Right;

					if (connexions.Count == 2) {
						lines[1].points[^2] = cp1Left;
						lines[1].points[^1] = c1Left;
					}

					if (middleLinesPoints != null) {
						middleLinesPoints[^2] = Utils.MidPoint(c1Left, c1Right);
						middleLinesPoints[^1] = c1Mid;

						nodeMeshLines.UpdateMiddleLines(middleLinesPoints);
					} else {
						if (listMiddleLines == null) {
							listMiddleLines = new();
						}
						listMiddleLines.Add(new Vector3[2] {
							c0Mid,
							cp0
						});
					}
				}

				nodeMeshLines.UpdateExpanderLines(lines, true);

				if (listMiddleLines != null) {
					nodeMeshLines.UpdateMiddleLines(listMiddleLines);
				}
			}
		}

		if (meshFilter.mesh == null || meshFilter.mesh.vertices.Length != meshVertices.Length) {

			int[] triangles = new int[(meshVertices.Length - 2) * 3];
			int index = 0;
			for (int i = 0; i < triangles.Length; i += 6) {
				triangles[i] = index;
				triangles[i + 1] = index + 1;
				triangles[i + 2] = index + 2;
				triangles[i + 3] = index + 3;
				triangles[i + 4] = index + 2;
				triangles[i + 5] = index + 1;
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

		//#if UNITY_EDITOR
		//		int[] debugTriangles = meshFilter.mesh.triangles;
		//		for (int i = 0; i < debugTriangles.Length - 2; i += 3) {
		//			Debug.DrawLine(transform.position + meshVertices[debugTriangles[i]], transform.position + meshVertices[debugTriangles[i + 1]], Color.yellow);
		//			Debug.DrawLine(transform.position + meshVertices[debugTriangles[i + 1]], transform.position + meshVertices[debugTriangles[i + 2]], Color.yellow);
		//			Debug.DrawLine(transform.position + meshVertices[debugTriangles[i + 2]], transform.position + meshVertices[debugTriangles[i]], Color.yellow);
		//		}
		//#endif

		meshFilter.mesh.vertices = meshVertices;
		meshFilter.mesh.RecalculateBounds();
	}

	private List<Node> GetConnexionsClockwise() {
		if (connexions.Count == 2) {
			return connexions;
		}
		List<Node> list = new() { connexions[0] };
		List<Node> tList = new(connexions);
		tList.Remove(list[^1]);
		while (tList.Count > 0) {
			list.Add(GetClosestNode(list[^1], tList));
			tList.Remove(list[^1]);
		}
		list.Add(list[0]);
		return list;
	}

	private Node GetClosestNode(Node node, List<Node> nodes) {
		Node closestNode = default;
		float minAngle = float.MaxValue;
		for (int i = 0; i < nodes.Count; i++) {
			float angle = Utils.GetAngle360(node.transform.position, transform.position, nodes[i].transform.position);
			if (minAngle > angle) {
				minAngle = angle;
				closestNode = nodes[i];
			}
		}
		return closestNode != null ? closestNode : nodes[0];
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
		meshRenderer.material = correct ? Config.Instance.RoadMaterial : Config.Instance.RoadWrongMaterial;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos() {
		/*Gizmos.color = Color.red;
		Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);

		connexions.ForEach(cn => {
			Gizmos.DrawLine(transform.position, cn.transform.position);
		});

		bool showLabel = UnityEditor.Selection.activeGameObject == gameObject;
		Gizmos.color = Color.green;
		for (int i = 0; i < meshVertices.Length; i++) {
			//Gizmos.DrawCube(transform.position + meshVertices[i], Vector3.one * 0.15f);
			if (showLabel) {
				UnityEditor.Handles.Label(transform.position + meshVertices[i], i.ToString());
			}
		}*/
	}
#endif
}
