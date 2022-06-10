using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviourSingleton<MapController> {

	[SerializeField] private Button cameraButton = default;
	[SerializeField] private UIPointOfInterestPanel pointOfInterestPanel = default;
	[SerializeField] private UIConfirmRoadPanel confirmRoadPanel = default;

	[SerializeField] private NavigationController mainNavigationController = default;
	[SerializeField] private LevelController[] levels = default;

	public UIPointOfInterestPanel PointOfInterestPanel => pointOfInterestPanel;
	public UIConfirmRoadPanel ConfirmRoadPanel => confirmRoadPanel;

	private readonly List<NodeController> nodeControllers = new();

	private void Start() {
		base.Awake();

		cameraButton.SetAction(() => {
			CameraController.Instance.ChangeOrto();
		});

		for (int i = 0; i < levels.Length; i++) {
			levels[i].Init(OnEnter);
			nodeControllers.Add(levels[i].GetNodeController());
		}
		//StartNavigation();

		UIController.Instance.ShowView<UIMapView>();

		OnEnter();
	}

	private void OnEnter() {
		CameraController.Instance.SetTapAction(() => {
			//StopNavigation();
			for (int i = 0; i < levels.Length; i++) {
				if (levels[i].TouchInputRaycast()) {
					CameraController.Instance.SetTapAction(null);
					levels[i].OnEnter();
					break;
				}
			}
		});
	}

	private void StartNavigation() {
		mainNavigationController.SetPoints(nodeControllers, out _);
		nodeControllers.ForEach(n => {
			n.StartIntersectionsWithSemaphore();
		});
	}

	private void StopNavigation() {
		mainNavigationController.Stop();
		nodeControllers.ForEach(n => {
			n.StopIntersectionsWithSemaphores();
		});
	}
}
