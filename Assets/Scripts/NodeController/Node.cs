using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {

	private const int SINGLE_CONNEXION_VERT_LENGTH = 14;
	private const int MULTIPLE_CONNEXION_VERT_LENGTH = 24;

	private readonly List<Node> connexions = new();
	private Vector3[] meshVertices = new Vector3[0];

	private NavigationPoint[] navigationRightPoints = new NavigationPoint[0];
	private NavigationPoint[] navigationLeftPoints = new NavigationPoint[0];
	private Vector3[] navigationDirectionRightPoints = new Vector3[0];
	private Vector3[] navigationDirectionLeftPoints = new Vector3[0];

	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;
	private NodeMeshLines nodeMeshLines;

	public int ConnexionsCount => connexions.Count;

	[HideInInspector] public bool IsStaticNode { get; private set; }
	[HideInInspector] public bool IsHeadNode { get; private set; }

	private SemaphoreData semaphoreData;
	private int semaphoreIndex;
	private Coroutine semaphoresRoutine;

	private readonly List<GameObject> giveWayObjects = new();
	private readonly List<Semaphore> semaphores = new();

	private void Awake() {
		meshRenderer = new GameObject("mesh").AddComponent<MeshRenderer>();
		meshRenderer.transform.parent = transform;
		meshRenderer.transform.localPosition = Vector3.zero;
		UpdateHighlightColor(true);
		meshFilter = meshRenderer.gameObject.AddComponent<MeshFilter>();
		nodeMeshLines = gameObject.AddComponent<NodeMeshLines>();
	}

	public void SetNode(bool isStatic, bool isHead) {
		IsStaticNode = isStatic;
		IsHeadNode = isHead;
	}

	public List<Node> GetConnexions() {
		return connexions;
	}

	public void UpdateSemaphore(SemaphoreData semaphoreData) {
		this.semaphoreData = semaphoreData;
		UpdateSemaphores();
	}

	public void StartSemaphores() {
		semaphoresRoutine = StartCoroutine(SemaphoresRoutine());
	}

	public void StopSemaphores() {
		if (semaphoresRoutine != null) {
			StopCoroutine(semaphoresRoutine);
			semaphoresRoutine = null;

			NavigationPoint[] inputPoints = Config.Instance.RightDriving ? navigationRightPoints : navigationLeftPoints;
			for (int i = 0; i < inputPoints.Length; i++) {
				inputPoints[i].UpdateStopedbySemaphore(false);
			}
		}
	}

	private IEnumerator SemaphoresRoutine() {
		while (true) {
			NavigationPoint[] inputPoints = Config.Instance.RightDriving ? navigationRightPoints : navigationLeftPoints;
			for (int i = 0; i < inputPoints.Length; i++) {
				inputPoints[i].UpdateStopedbySemaphore(i != semaphoreIndex);
			}

			yield return new WaitForSeconds(semaphoreData.timers[semaphoreIndex]);

			semaphoreIndex++;
			if (semaphoreIndex >= inputPoints.Length) {
				semaphoreIndex = 0;
			}
		}
	}

	public void UpdateSemaphores() {
		SemaphoreData semaphoreData = GetSemaphoreData();
		NavigationPoint[] inputPoints = Config.Instance.RightDriving ? navigationRightPoints : navigationLeftPoints;
		NavigationPoint[] outputPoints = Config.Instance.RightDriving ? navigationLeftPoints : navigationRightPoints;

		for (int i = 0; i < inputPoints.Length; i++) {
			Semaphore semaphore = GetSemaphore(i);
			if (semaphoreData.isOn) {
				Vector3 dir = (inputPoints[i].Position - outputPoints[i].Position).normalized;
				semaphore.transform.position = inputPoints[i].Position + dir * (Config.Instance.RoadHalfWidth / 2f + 0.4f);
				semaphore.transform.localRotation = Quaternion.LookRotation(dir);
				semaphore.transform.SetLocalAngleY(semaphore.transform.localEulerAngles.y + 90f);
				semaphore.gameObject.SetActive(true);
			} else {
				semaphore.gameObject.SetActive(false);
			}
		}
	}

	public SemaphoreData GetSemaphoreData() {
		if (semaphoreData == null || semaphoreData.timers.Length != connexions.Count) {
			semaphoreData = new();
			semaphoreData.timers = new int[connexions.Count];
			for (int i = 0; i < semaphoreData.timers.Length; i++) {
				semaphoreData.timers[i] = Config.Instance.DefaultSemaphoreTimer;
			}
		}
		return semaphoreData;
	}

	private Semaphore GetSemaphore(int index) {
		for (int i = semaphores.Count; i <= index; i++) {
			Semaphore semaphore = Instantiate(Resources.Load<Semaphore>("RoadSigns/Semaphore"), transform);
			semaphore.name = $"Semaphore{index}";
			semaphore.gameObject.SetActive(false);
			semaphores.Add(semaphore);
		}
		return semaphores[index];
	}

	public void UpdateGiveWay(int pointIndex, bool giveWay) {
		NavigationPoint[] inputPoints = Config.Instance.RightDriving ? navigationRightPoints : navigationLeftPoints;
		inputPoints[pointIndex].UpdateGiveWay(giveWay);
		UpdateGiveWay(pointIndex);
	}

	private void UpdateGiveWay(int pointIndex) {
		NavigationPoint[] inputPoints = Config.Instance.RightDriving ? navigationRightPoints : navigationLeftPoints;
		NavigationPoint[] outputPoints = Config.Instance.RightDriving ? navigationLeftPoints : navigationRightPoints;
		GameObject giveWayObj = GetGiveWayObject(pointIndex);
		if (!GetSemaphoreData().isOn && inputPoints[pointIndex].GivesWay) {
			giveWayObj.SetActive(true);
			Vector3 dir = (inputPoints[pointIndex].Position - outputPoints[pointIndex].Position).normalized;
			giveWayObj.transform.position = inputPoints[pointIndex].Position + dir * (Config.Instance.RoadHalfWidth / 2f + 0.4f);
			giveWayObj.transform.localRotation = Quaternion.LookRotation(dir);
			giveWayObj.transform.SetLocalAngleY(giveWayObj.transform.localEulerAngles.y + 90f);
		} else {
			giveWayObj.SetActive(false);
		}
	}

	private GameObject GetGiveWayObject(int index) {
		for (int i = giveWayObjects.Count; i <= index; i++) {
			GameObject obj = Instantiate(Resources.Load<GameObject>("RoadSigns/GiveWay"), transform);
			obj.name = $"GiveWay{index}";
			obj.SetActive(false);
			giveWayObjects.Add(obj);
		}
		return giveWayObjects[index];
	}

	public void UpdateGiveWaysObjects() {
		for (int i = 0; i < giveWayObjects.Count; i++) {
			giveWayObjects[i].SetActive(false);
		}
		for (int i = 0; i < navigationRightPoints.Length; i++) {
			UpdateGiveWay(i);
		}
	}

	public NavigationPoint[] GetNavigationRightPoints() {
		return navigationRightPoints;
	}

	public NavigationPoint[] GetNavigationLeftPoints() {
		return navigationLeftPoints;
	}

	public Vector3[] GetNavigationDirectionRightPoints() {
		return navigationDirectionRightPoints;
	}

	public Vector3[] GetNavigationDirectionLeftPoints() {
		return navigationDirectionLeftPoints;
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
				navigationRightPoints = new NavigationPoint[1];
				navigationLeftPoints = new NavigationPoint[1];
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

			navigationRightPoints[0] = new(transform.position + Utils.MidPoint(Vector3.zero, meshVertices[3]));
			navigationLeftPoints[0] = new(transform.position + Utils.MidPoint(Vector3.zero, meshVertices[2]));
		} else if (connexions.Count >= 2) {

			Vector3 c0 = transform.InverseTransformPoint(connexions[0].transform.position);
			Vector3 c1 = transform.InverseTransformPoint(connexions[1].transform.position);

			if (connexions.Count == 2 && !Utils.Intersection2D(c0, -c0.normalized, c1, -c1.normalized, out _)) {
				if (meshVertices.Length != SINGLE_CONNEXION_VERT_LENGTH) {
					meshVertices = new Vector3[SINGLE_CONNEXION_VERT_LENGTH];
					navigationRightPoints = new NavigationPoint[1];
					navigationLeftPoints = new NavigationPoint[1];
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

				Utils.PerpendicularPoints(transform.position, connexions[0].transform.position,
					out Vector3 nvrp0, out Vector3 nvrp1, Config.Instance.RoadHalfWidth / 2f);
				navigationRightPoints[0] = new(nvrp0);
				navigationLeftPoints[0] = new(nvrp1);
			} else {

				List<Node> connexionsClockwise = GetConnexionsClockwise();
				int verticesLength = (connexionsClockwise.Count - 1) * MULTIPLE_CONNEXION_VERT_LENGTH;
				if (meshVertices.Length != verticesLength) {
					meshVertices = new Vector3[verticesLength];
					navigationRightPoints = new NavigationPoint[connexions.Count];
					navigationLeftPoints = new NavigationPoint[connexions.Count];
					navigationDirectionRightPoints = new Vector3[connexions.Count];
					navigationDirectionLeftPoints = new Vector3[connexions.Count];
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

					Vector3 interRight = Utils.Intersection(cp0Right, (cp0Right - c0Right).normalized, cp1Left, (cp1Left - c1Left).normalized);
					Vector3 interLeft = Utils.Intersection(cp1Right, (cp1Right - c1Right).normalized, cp0Left, (cp0Left - c0Left).normalized);

					navigationRightPoints[i] = new(transform.position + Utils.MidPoint(cp0, cp0Right));
					navigationLeftPoints[i] = new(transform.position + Utils.MidPoint(cp0, cp0Left));

					navigationDirectionRightPoints[i] = -cp0.normalized;
					navigationDirectionLeftPoints[i] = -cp0.normalized;

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
						navigationRightPoints[1] = new(transform.position + Utils.MidPoint(cp1, cp1Right));
						navigationLeftPoints[1] = new(transform.position + Utils.MidPoint(cp1, cp1Left));

						navigationDirectionRightPoints[1] = -cp1.normalized;
						navigationDirectionLeftPoints[1] = -cp1.normalized;
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

		UpdateGiveWaysObjects();
		UpdateSemaphores();
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
	[SerializeField] protected bool drawGizmos = default;
	protected virtual void OnDrawGizmos() {
		if (!drawGizmos) {
			return;
		}

		Gizmos.color = Color.green;
		for (int i = 0; i < navigationRightPoints.Length; i++) {
			Gizmos.DrawCube(navigationRightPoints[i].Position, Vector3.one * 0.25f);
		}
		Gizmos.color = Color.red;
		for (int i = 0; i < navigationLeftPoints.Length; i++) {
			Gizmos.DrawCube(navigationLeftPoints[i].Position, Vector3.one * 0.25f);
		}

		Gizmos.color = Color.red;
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
		}
	}
#endif
}
