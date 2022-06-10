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
	private readonly List<PointOfInterest> pointOfInterests = new();

	private bool mainNavIsActive;
	private float spawnTime;

	private void Start() {
		base.Awake();

		cameraButton.SetAction(() => {
			CameraController.Instance.ChangeOrto();
		});

		for (int i = 0; i < levels.Length; i++) {
			levels[i].Init(OnEnter);
			nodeControllers.Add(levels[i].GetNodeController());
			pointOfInterests.AddRange(levels[i].GetNodeController().GetPointOfInterests());
		}

		UIController.Instance.ShowView<UIMapView>();

		OnEnter();
	}

	private void OnEnter() {
		StartNavigation();
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
		spawnTime = 0f;
		mainNavIsActive = true;
		mainNavigationController.SetPoints(nodeControllers, out _);
		nodeControllers.ForEach(n => {
			n.StartIntersectionsWithSemaphore();
		});
	}

	private void StopNavigation() {
		mainNavIsActive = false;
		mainNavigationController.Stop();
		nodeControllers.ForEach(n => {
			n.StopIntersectionsWithSemaphores();
		});
	}

	private void Update() {
		if (mainNavIsActive) {
			spawnTime -= Time.deltaTime;
			if (spawnTime < 0f) {
				spawnTime = Utils.Random(2f, 5f);
				TrySpawnAgent();
			}
		}
	}

	private void TrySpawnAgent() {
		for (int i = 0; i < pointOfInterests.Count; i++) {
			PointOfInterest from = pointOfInterests[i];
			for (int j = i + 1; j < pointOfInterests.Count; j++) {
				PointOfInterest to = pointOfInterests[j];
				if (mainNavigationController.HasPath(from, to)) {
					mainNavigationController.TravelAgent(from.name, to.name);
					pointOfInterests.Remove(from);
					pointOfInterests.Remove(to);
					pointOfInterests.Add(to);
					pointOfInterests.Add(from);
					return;
				}
			}
		}
	}
}
