using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	[SerializeField] private CameraController cameraController = default;
	[SerializeField] private NodeController nodeController = default;

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
}
