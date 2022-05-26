using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	[SerializeField] private CameraController cameraController = default;
	[SerializeField] private NodeController nodeController = default;
	[SerializeField] private NavigationController navigationController = default;

	[SerializeField] private UITopPanel topPanel = default;
	[SerializeField] private UIBuildRoadPanel buildingRoadPanel = default;
	[SerializeField] private UIConfirmRoadPanel confirmRoadPanel = default;
	[SerializeField] private UIEraseRoadPanel eraseRoadPanel = default;

	[SerializeField] private Button cameraButton = default;

	private bool updateConfirmRoadPanelPos;
	private bool updateNode;

	private void Awake() {
		topPanel.Init(() => {
			topPanel.Hide();
			buildingRoadPanel.Show(() => {
				nodeController.enabled = true;
			});
			cameraController.SetMoveEnable(false);
		}, () => {
			topPanel.Hide();
			cameraController.SetTapAction(() => {
				nodeController.Demolish();
			});
			eraseRoadPanel.Show();
		}, () => {

		});

		buildingRoadPanel.Init(() => {
			topPanel.Show(() => {
				cameraController.SetMoveEnable(true);
			});
			buildingRoadPanel.Hide();
			nodeController.enabled = false;
			nodeController.ApplyCurrentNode();
			if (confirmRoadPanel.gameObject.activeSelf) {
				confirmRoadPanel.Hide();
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

		eraseRoadPanel.Init(() => {
			topPanel.Show(null);
			eraseRoadPanel.Hide();
			cameraController.SetTapAction(null);
		});

		cameraButton.onClick.AddListener(() => {
			cameraController.ChangeOrto();
		});

		nodeController.Init();

		SetNavigationPoints();
	}

	private void Update() {
		if (nodeController.enabled) {

			if (!Utils.IsOverUI()) {
				if (Input.GetMouseButtonDown(0)) {
					updateNode = nodeController.CurrentNode == null || nodeController.IsMouseInputCloseToCurrentNode();
					cameraController.SetMoveEnable(!updateNode);
				} else if (updateNode && Input.GetMouseButton(0)) {
					if (confirmRoadPanel.gameObject.activeSelf) {
						confirmRoadPanel.Hide();
					}
					nodeController.UpdateController();
				}
			}

			if (updateNode && Input.GetMouseButtonUp(0)) {
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

		navigationController.Init(points);
	}
}
