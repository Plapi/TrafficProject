using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviourSingleton<GameController> {

	[SerializeField] private Button cameraButton = default;
	[SerializeField] private NavigationController mainNavigationController = default;
	[SerializeField] private LevelController[] levels = default;

	private void Start() {
		base.Awake();

		cameraButton.SetAction(() => {
			CameraController.Instance.ChangeOrto();
		});

		List<NodeController> nodeControllers = new();
		for (int i = 0; i < levels.Length; i++) {
			levels[i].Init(OnEnterMap);
			nodeControllers.Add(levels[i].GetNodeController());
		}
		mainNavigationController.SetPoints(nodeControllers);

		UIController.Instance.ShowView<UIMapView>();

		OnEnterMap();
	}

	private void OnEnterMap() {
		CameraController.Instance.SetTapAction(() => {
			mainNavigationController.Stop();
			for (int i = 0; i < levels.Length; i++) {
				if (levels[i].TouchInputRaycast()) {
					CameraController.Instance.SetTapAction(null);
					levels[i].OnEnter();
					break;
				}
			}
		});
	}
}
