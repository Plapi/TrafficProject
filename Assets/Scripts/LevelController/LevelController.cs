using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour {

	[SerializeField] private BoxCollider map = default;
	[SerializeField] private NodeController nodeController = default;
	[SerializeField] private NavigationController navigationController = default;

	[SerializeField] private MeshRenderer border = default;
	[SerializeField] private bool isRightEdge = default;
	[SerializeField] private bool isBottomEdge = default;

	private bool updateConfirmRoadPanelPos;
	private bool updateNode;

	private bool inPlayMode;
	private List<PointOfInterest> pointOfInterests;
	private List<LinkedNode> linkedNodes;

	private readonly List<UIPointOfInterestPanel> pointOfInterestPanels = new();

	private Action onExit;

	public void Init(Action onExit) {
		this.onExit = onExit;
		nodeController.Init(map);
		enabled = false;
	}

	public bool TouchInputRaycast() {
		return map.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _, float.MaxValue);
	}

	public NodeController GetNodeController() {
		return nodeController;
	}

	private void OnExit() {
		enabled = false;
		ClearPointOfInterestPanels();
		onExit?.Invoke();
	}

	public void OnEnter() {
		SetPointOfInterestPanels(nodeController.GetPointOfInterests());
		UIController.Instance.GetView<UILevelView>().Init(new UILevelView.Data {
			onBack = OnExit,
			onRoadButton = () => {
				UIController.Instance.ShowView<UIBuildRoadView>(() => {
					nodeController.enabled = true;
					nodeController.ShowHighligtsAnim();
				});
				CameraController.Instance.SetMoveEnable(false);
			}, onDemolishButton = () => {
				CameraController.Instance.SetTapAction(() => {
					nodeController.Demolish();
				});
				UIController.Instance.ShowView<UIEraseRoadView>();
			}, onIntersectionButton = () => {
				UIController.Instance.InitAndShowView<UIIntersectionView>(new UIIntersectionView.Data {
					onBack = () => {
						nodeController.SaveDada();
					},
					intersections = nodeController.GetIntersections()
				});
			}, onPlayButton = () => {
				if (CanStartPlayMode()) {
					StartPlayMode();
				} else {
					navigationController.Stop();
				}
			}
		});

		UIController.Instance.GetView<UIBuildRoadView>().Init(new UIBuildRoadView.Data {
			onBack = () => {
				CameraController.Instance.SetMoveEnable(true);
				if (nodeController.CurrentNode != null) {
					if (nodeController.CurrentNodeCanBePlaced) {
						nodeController.ApplyCurrentNode();
					} else {
						nodeController.DismissCurrentNode();
					}
				}
				nodeController.enabled = false;
				if (MapController.Instance.ConfirmRoadPanel.gameObject.activeSelf) {
					MapController.Instance.ConfirmRoadPanel.Hide();
				}
			}
		});

		UIController.Instance.GetView<UIEraseRoadView>().Init(new UIEraseRoadView.Data {
			onBack = () => {
				CameraController.Instance.SetTapAction(null);
			}
		});

		UIController.Instance.GetView<UIPlayModeView>().Init(new UIPlayModeView.Data {
			onBack = StopPlayMode,
			onNormalSpeedButton = () => {

			},
			onFastSpeedButton = () => {

			}
		});

		MapController.Instance.ConfirmRoadPanel.Init(() => {
			MapController.Instance.ConfirmRoadPanel.Hide();
			updateConfirmRoadPanelPos = false;
			nodeController.DismissCurrentNode();
		}, () => {
			MapController.Instance.ConfirmRoadPanel.Hide();
			updateConfirmRoadPanelPos = false;
			nodeController.ApplyCurrentNode();
		});

		nodeController.SetLinkNodes();

		UIController.Instance.ShowView<UILevelView>();
		enabled = true;
	}

	private bool CanStartPlayMode() {
		navigationController.SetPoints(new List<NodeController> { nodeController }, out List<(string, string)> missingPaths, nodeController.GetLinkedNodes());
		if (missingPaths.Count > 0) {
			missingPaths.ForEach(con => {
				Debug.LogError($"Could not find path {con.Item1} {con.Item2}");
			});
		}
		return missingPaths.Count == 0;
	}

	private void StartPlayMode() {
		UIController.Instance.ShowView<UIPlayModeView>();
		nodeController.StartIntersectionsWithSemaphore();

		pointOfInterests = nodeController.GetPointOfInterests();
		linkedNodes = nodeController.GetLinkedNodes();

		pointOfInterests.ForEach(pointOfInterest => {
			pointOfInterest.InitProgressSpawnTime();
			pointOfInterest.CarsCountStartedWithThisDestination = 0;
		});
		linkedNodes.ForEach(linkedNode => {
			if (linkedNode.IsHeadNode) {
				linkedNode.InitProgressSpawnTime();
			}
		});

		inPlayMode = true;
	}

	private void StopPlayMode() {
		navigationController.Stop();
		nodeController.StopIntersectionsWithSemaphores();
		inPlayMode = false;
		pointOfInterests.ForEach(pointOfInterest => pointOfInterest.ResetCarsProgress());
		pointOfInterestPanels.ForEach(pointOfInterestPanel => pointOfInterestPanel.ResetUI());
	}

	private void Update() {
		pointOfInterestPanels.ForEach(panel => panel.UpdatePosition());

		if (inPlayMode) {
			PlayModeUpdate();
			return;
		}

		if (nodeController.enabled) {

			if (!Utils.IsOverUI() && CameraController.Instance.TouchesCount < 2) {
				if (Input.GetMouseButtonDown(0)) {
					updateNode = nodeController.CurrentNode == null || nodeController.IsMouseInputCloseToCurrentNode();
					CameraController.Instance.SetMoveEnable(!updateNode);
					if (updateNode) {
						nodeController.HideHighlightsAnim();
					}
				} else if (updateNode && Input.GetMouseButton(0)) {
					if (MapController.Instance.ConfirmRoadPanel.gameObject.activeSelf) {
						MapController.Instance.ConfirmRoadPanel.Hide();
					}
					nodeController.UpdateController();
				}
			}

			if (updateNode && Input.GetMouseButtonUp(0) && nodeController.CurrentNodesHasMinDistance) {
				MapController.Instance.ConfirmRoadPanel.Show();
				updateConfirmRoadPanelPos = true;
				updateNode = false;
			}
		}

		if (updateConfirmRoadPanelPos && nodeController.CurrentNode != null) {
			MapController.Instance.ConfirmRoadPanel.UpdateAnchorPos(nodeController.CurrentNode.transform);
			MapController.Instance.ConfirmRoadPanel.SetAcceptButtonInteractable(nodeController.CurrentNodeCanBePlaced);
		}
	}

	private void SetPointOfInterestPanels(List<PointOfInterest> pointOfInterests) {
		pointOfInterests.ForEach(pointOfInterest => {
			UIPointOfInterestPanel pointOfInterestPanel = MapController.Instance.PointOfInterestPanel;
			UIPointOfInterestPanel panel = Instantiate(pointOfInterestPanel, pointOfInterestPanel.transform.parent);
			panel.transform.SetSiblingIndex(0);
			panel.Init(pointOfInterest);
			panel.ShowAnim();
			pointOfInterestPanels.Add(panel);
		});
	}

	private void ClearPointOfInterestPanels() {
		pointOfInterestPanels.ForEach(p => {
			p.HideAnim(() => {
				Destroy(p.gameObject);
			});
		});
		pointOfInterestPanels.Clear();
	}

	private void PlayModeUpdate() {
		pointOfInterests.ForEach(pointOfInterest => {
			pointOfInterest.UpdateSpawnTime();
			if (pointOfInterest.CanSpawnCar()) {
				pointOfInterest.InitProgressSpawnTime();
				TravelCar(pointOfInterest.name, pointOfInterest);
			}
		});
		linkedNodes.ForEach(linkedNode => {
			if (linkedNode.IsHeadNode) {
				linkedNode.UpdateSpawnTime();
				if (linkedNode.CanSpawnCar()) {
					linkedNode.InitProgressSpawnTime();
					TravelCar(linkedNode.name);
				}
			}
		});
	}

	private void TravelCar(string startName, PointOfInterest exlude = null) {
		navigationController.TravelAgent(startName, GetDestination(exlude, out PointOfInterest pointOfInterest), () => {
			if (pointOfInterest != null) {
				pointOfInterest.OnCarEnter();
			}
		});
	}

	private string GetDestination(PointOfInterest exlude, out PointOfInterest pointOfInterest) {
		pointOfInterest = null;
		List<PointOfInterest> list = new(pointOfInterests);
		if (exlude != null) {
			list.Remove(exlude);
		}
		if (list.Count > 0) {
			int min = pointOfInterests[0].CarsCountStartedWithThisDestination;
			for (int i = 1; i < pointOfInterests.Count; i++) {
				if (min > pointOfInterests[i].CarsCountStartedWithThisDestination) {
					min = pointOfInterests[i].CarsCountStartedWithThisDestination;
				}
			}
			pointOfInterest = list.FindAll(l => l.CarsCountStartedWithThisDestination == min).Random();
			return pointOfInterest.name;
		}
		return linkedNodes.FindAll(lNode => !lNode.IsHeadNode).Random().name;
	}

	public void DeleteData() {
		nodeController.DeleteData();
	}

#if UNITY_EDITOR
	private void OnDrawGizmos() {
		if (!Application.isPlaying) {
			//CreateLevelBorders();
		}
	}

	public void CreateLevelBorders() {
		if (border == null) {
			border = new GameObject("border").AddComponent<MeshRenderer>();
			border.material = Resources.Load<Material>("LevelBorder");
			border.transform.parent = transform;
			border.transform.localPosition = Vector3.zero;

			border.gameObject.AddComponent<MeshFilter>().mesh = new Mesh();
		}

		MeshFilter borderFilter = border.GetComponent<MeshFilter>();
		Mesh mesh = borderFilter.sharedMesh;

		float width = map.bounds.size.x;
		float height = map.bounds.size.z;

		Vector3 bottomLeft = new(-width / 2f, -2f, -height / 2f);
		Vector3 bottomRight = new(-width / 2f, -2f, height / 2f);
		Vector3 topLeft = new(width / 2f, -2f, -height / 2f);
		Vector3 topRight = new(width / 2f, -2f, height / 2f);

		mesh.Clear();
		mesh.vertices = new Vector3[] {
			bottomLeft + Vector3.down * 2f,
			bottomLeft + Vector3.up * 10f,
			bottomRight + Vector3.down * 2f,
			bottomRight + Vector3.up * 10f,
			topRight + Vector3.down * 2f,
			topRight + Vector3.up * 10f,
			topLeft + Vector3.down * 2f,
			topLeft + Vector3.up * 10f
		};

		List<int> triangles = new() {
			0,
			1,
			2,
			2,
			1,
			0,
			1,
			2,
			3,
			3,
			2,
			1,
			2,
			3,
			4,
			4,
			2,
			3,
			3,
			4,
			5,
			5,
			4,
			3
		};
		if (isRightEdge) {
			triangles.AddRange(new int[] { 4, 5, 6 });
			triangles.AddRange(new int[] { 6, 5, 4 });
			triangles.AddRange(new int[] { 5, 6, 7 });
			triangles.AddRange(new int[] { 7, 6, 5 });
		}
		if (isBottomEdge) {
			triangles.AddRange(new int[] { 6, 7, 0 });
			triangles.AddRange(new int[] { 0, 7, 6 });
			triangles.AddRange(new int[] { 7, 0, 1 });
			triangles.AddRange(new int[] { 1, 0, 7 });
		}
		mesh.triangles = triangles.ToArray();

		Color[] colors = new Color[mesh.vertices.Length];
		for (int i = 0; i < colors.Length; i += 2) {
			colors[i] = Config.Instance.LevelBorderColor;
			colors[i + 1] = Config.Instance.LevelBorderColor;
			colors[i + 1].a = 0f;
		}
		mesh.colors = colors;

		Vector3[] normals = new Vector3[mesh.vertices.Length];
		for (int i = 0; i < mesh.vertices.Length; i++) {
			normals[i] = Vector3.up;
		}
		mesh.normals = normals;

		mesh.RecalculateBounds();
		borderFilter.sharedMesh = mesh;
	}
#endif
}
