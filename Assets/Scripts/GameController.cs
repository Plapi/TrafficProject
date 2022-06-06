using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviourSingleton<GameController> {

	[SerializeField] private Button cameraButton = default;
	[SerializeField] private NavigationController mainNavigationController = default;
	[SerializeField] private LevelController[] levels = default;

	private readonly List<NodeController> nodeControllers = new();

	private void Start() {
		base.Awake();

		cameraButton.SetAction(() => {
			CameraController.Instance.ChangeOrto();
		});

		for (int i = 0; i < levels.Length; i++) {
			levels[i].Init(OnEnterMap);
			nodeControllers.Add(levels[i].GetNodeController());
		}
		StartNavigation();

		UIController.Instance.ShowView<UIMapView>();

		OnEnterMap();
	}

	private void OnEnterMap() {
		CameraController.Instance.SetTapAction(() => {
			StopNavigation();
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
		mainNavigationController.SetPoints(nodeControllers);
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
