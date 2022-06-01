using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour {

	[SerializeField] private NodeController nodeController = default;
	[SerializeField] private NavigationController navigationController = default;

	[SerializeField] private UIConfirmRoadPanel confirmRoadPanel = default;
	[SerializeField] private Button cameraButton = default;

	private bool updateConfirmRoadPanelPos;
	private bool updateNode;

	private void Start() {
		UIController.Instance.GetView<UILevelView>().Init(new UILevelView.Data {
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
				SetNavigationPoints();
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

		cameraButton.onClick.AddListener(() => {
			CameraController.Instance.ChangeOrto();
		});

		nodeController.Init();

		UIController.Instance.ShowView<UILevelView>();
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

	private void SetNavigationPoints() {
		List<NavigationPoint> points = new();
		nodeController.GetAllNodes().ForEach(n => {
			points.AddRange(n.GetNavigationRightPoints());
			points.AddRange(n.GetNavigationLeftPoints());
		});

		Dictionary<string, string> alreadyProcessedConnexions = new();

		nodeController.IterateAllConnexions((Node n0, Node n1) => {

			string n0n1Name = n0.name + n1.name;
			string n1n0Name = n1.name + n0.name;

			if (!alreadyProcessedConnexions.ContainsKey(n0n1Name) && !alreadyProcessedConnexions.ContainsKey(n1n0Name)) {

				alreadyProcessedConnexions.Add(n0n1Name, null);
				alreadyProcessedConnexions.Add(n1n0Name, null);

				nodeController.GetNavigationPointsBetween(n0, n1, out NavigationPoint[] navPoints);

				Vector3 mid0 = Utils.MidPoint(navPoints[0].Position, navPoints[2].Position);
				Vector3 mid1 = Utils.MidPoint(navPoints[1].Position, navPoints[3].Position);
				Vector3 dir = (mid1 - mid0).normalized;

				Vector3 delta = (navPoints[0].Position - mid0).normalized;
				Vector3 cross = Vector3.Cross(delta, dir);

				if (Config.Instance.RightDriving) {
					if (cross.y > 0) {
						navPoints[1].AddNextNode(navPoints[0]);
						navPoints[2].AddNextNode(navPoints[3]);
					} else {
						navPoints[0].AddNextNode(navPoints[1]);
						navPoints[3].AddNextNode(navPoints[2]);
					}
				} else {
					if (cross.y > 0) {
						navPoints[0].AddNextNode(navPoints[1]);
						navPoints[3].AddNextNode(navPoints[2]);
					} else {
						navPoints[1].AddNextNode(navPoints[0]);
						navPoints[2].AddNextNode(navPoints[3]);
					}
				}
			}
		});

		nodeController.GetAllNodes().ForEach(node => {
			if (node.GetNavigationRightPoints().Length >= 2) {

				NavigationPoint[] rightPoints = node.GetNavigationRightPoints();
				NavigationPoint[] leftPoints = node.GetNavigationLeftPoints();

				Vector3[] rightDirections = node.GetNavigationDirectionRightPoints();
				Vector3[] leftDirections = node.GetNavigationDirectionLeftPoints();

				if (Config.Instance.RightDriving) {
					for (int i = 0; i < rightPoints.Length; i++) {
						for (int j = 0; j < leftPoints.Length; j++) {
							if (i != j) {
								Vector3 cPoint = Utils.Intersection(rightPoints[i].Position, rightDirections[i], leftPoints[j].Position, leftDirections[j]);
								rightPoints[i].AddNextNodeWithCurvePoints(leftPoints[j], cPoint);
							}
						}
					}
				} else {
					for (int i = 0; i < leftPoints.Length; i++) {
						for (int j = 0; j < rightPoints.Length; j++) {
							if (i != j) {
								Vector3 cPoint = Utils.Intersection(leftPoints[i].Position, leftDirections[i], rightPoints[j].Position, rightDirections[j]);
								leftPoints[i].AddNextNodeWithCurvePoints(rightPoints[j], cPoint);
							}
						}
					}
				}
			}
		});

		navigationController.SetPoints(points, nodeController.GetPointOfInterests());
	}
}
