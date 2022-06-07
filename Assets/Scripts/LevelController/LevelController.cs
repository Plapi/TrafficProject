using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour {

	[SerializeField] private BoxCollider map = default;
	[SerializeField] private NodeController nodeController = default;
	[SerializeField] private NavigationController navigationController = default;

	[SerializeField] private UIConfirmRoadPanel confirmRoadPanel = default;

	[SerializeField] private MeshRenderer border = default;
	[SerializeField] private bool isRightEdge = default;
	[SerializeField] private bool isBottomEdge = default;

	private bool updateConfirmRoadPanelPos;
	private bool updateNode;

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
		onExit?.Invoke();
	}

	public void OnEnter() {
		UIController.Instance.GetView<UILevelView>().Init(new UILevelView.Data {
			onBack = OnExit,
			onRoadButton = () => {
				UIController.Instance.ShowView<UIBuildRoadView>(() => {
					nodeController.enabled = true;
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
				UIController.Instance.ShowView<UIPlayModeView>();
				navigationController.SetPoints(new List<NodeController> { nodeController });
				nodeController.StartIntersectionsWithSemaphore();
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
				if (confirmRoadPanel.gameObject.activeSelf) {
					confirmRoadPanel.Hide();
				}
			}
		});

		UIController.Instance.GetView<UIEraseRoadView>().Init(new UIEraseRoadView.Data {
			onBack = () => {
				CameraController.Instance.SetTapAction(null);
			}
		});

		UIController.Instance.GetView<UIPlayModeView>().Init(new UIPlayModeView.Data {
			onBack = () => {
				navigationController.Stop();
				nodeController.StopIntersectionsWithSemaphores();
			},
			onNormalSpeedButton = () => {

			},
			onFastSpeedButton = () => {

			}
		});

		confirmRoadPanel.Init(() => {
			confirmRoadPanel.Hide();
			updateConfirmRoadPanelPos = false;
			nodeController.DismissCurrentNode();
		}, () => {
			confirmRoadPanel.Hide();
			updateConfirmRoadPanelPos = false;
			nodeController.ApplyCurrentNode();
		});

		nodeController.SetLinkNodes();

		UIController.Instance.ShowView<UILevelView>();
		enabled = true;
	}

	private void Update() {
		if (nodeController.enabled) {

			if (!Utils.IsOverUI() && CameraController.Instance.TouchesCount < 2) {
				if (Input.GetMouseButtonDown(0)) {
					updateNode = nodeController.CurrentNode == null || nodeController.IsMouseInputCloseToCurrentNode();
					CameraController.Instance.SetMoveEnable(!updateNode);
				} else if (updateNode && Input.GetMouseButton(0)) {
					if (confirmRoadPanel.gameObject.activeSelf) {
						confirmRoadPanel.Hide();
					}
					nodeController.UpdateController();
				}
			}

			if (updateNode && Input.GetMouseButtonUp(0) && nodeController.CurrentNodesHasMinDistance) {
				confirmRoadPanel.Show();
				updateConfirmRoadPanelPos = true;
				updateNode = false;
			}
		}

		if (updateConfirmRoadPanelPos && nodeController.CurrentNode != null) {
			confirmRoadPanel.UpdateAnchorPos(nodeController.CurrentNode.transform);
			confirmRoadPanel.SetAcceptButtonInteractable(nodeController.CurrentNodeCanBePlaced);
		}
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
			0, 1, 2,
			2, 1, 0,
			1, 2, 3,
			3, 2, 1,
			2, 3, 4,
			4, 2, 3,
			3, 4, 5,
			5, 4, 3
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
