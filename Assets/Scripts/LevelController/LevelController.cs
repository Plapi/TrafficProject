using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour {

	[SerializeField] private BoxCollider map = default;
	[SerializeField] private NodeController nodeController = default;
	[SerializeField] private NavigationController navigationController = default;

	[SerializeField] private UIConfirmRoadPanel confirmRoadPanel = default;

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
}
