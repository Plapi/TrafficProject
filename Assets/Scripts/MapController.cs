using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviourSingleton<MapController> {

	private const string DATA_KEY = "MAP_DATA_";

	[SerializeField] private Button cameraButton = default;
	[SerializeField] private UIPointOfInterestPanel pointOfInterestPanel = default;
	[SerializeField] private UIConfirmRoadPanel confirmRoadPanel = default;
	[SerializeField] private RectTransform uiLevelLockedObject = default;

	[SerializeField] private NavigationController mainNavigationController = default;
	[SerializeField] private LevelController[] levels = default;

	[SerializeField] private int mapIndex = default;

	public UIPointOfInterestPanel PointOfInterestPanel => pointOfInterestPanel;
	public UIConfirmRoadPanel ConfirmRoadPanel => confirmRoadPanel;
	public RectTransform UILevelLockedObject => uiLevelLockedObject;

	private readonly List<NodeController> nodeControllers = new();
	private readonly List<PointOfInterest> pointOfInterests = new();

	private bool mainNavIsActive;
	private float spawnTime;

	private LevelData[] levelsData;

	private void Start() {
		base.Awake();

		cameraButton.SetAction(() => {
			CameraController.Instance.ChangeOrto();
		});

		SetLevelsData();

		for (int i = 0; i < levels.Length; i++) {
			int levelIndex = i;
			levels[i].Init(OnEnter, (bool complete, int time) => {
				OnLevelComplete(levelIndex, complete, time);
			}, levelsData[i].isLocked);
			nodeControllers.Add(levels[i].GetNodeController());
			pointOfInterests.AddRange(levels[i].GetNodeController().GetPointOfInterests());
		}

		UIController.Instance.ShowView<UIMapView>();

		OnEnter();
	}

	private void OnEnter() {
		StartNavigation();
		CameraController.Instance.SetTapAction(() => {
			for (int i = 0; i < levels.Length; i++) {
				if (!levelsData[i].isLocked && levels[i].TouchInputRaycast()) {
					CameraController.Instance.SetTapAction(null);
					StopNavigation();
					levels[i].OnEnter();
					break;
				}
			}
		});
	}

	private void OnLevelComplete(int levelIndex, bool complete, int time) {
		UIController.Instance.InitAndShowView<UILevelCompleteView>(new UILevelCompleteView.Data {
			complete = complete,
			time = time,
			onContinue = () => {
				UIController.Instance.HideCurrentView(() => {
					if (complete) {
						levels[levelIndex].OnExit();
					}
				});
				if (complete) {
					if (levelIndex + 1 < levels.Length) {
						UnlockLevel(levelIndex + 1);
						levels[levelIndex + 1].UnlockAnim();
					} else {
						Debug.LogError("Map complete!");
					}
				}
			}
		}, () => {
			UIController.Instance.PopInstantView<UIPlayModeView>();
			if (complete) {
				UIController.Instance.PopInstantView<UILevelView>();
			}
		});
		cameraButton.gameObject.SetActive(false);
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

		for (int i = 0; i < levels.Length; i++) {
			if (levelsData[i].isLocked) {
				levels[i].UpdateUILockedObject();
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

	private void SetLevelsData() {
		string key = $"{DATA_KEY}{mapIndex}";
		if (PlayerPrefs.HasKey(key)) {
			levelsData = Utils.Deserialize<LevelData[]>(PlayerPrefs.GetString(key));
		} else {
			levelsData = new LevelData[levels.Length];
			for (int i = 0; i < levelsData.Length; i++) {
				levelsData[i] = new() {
					isLocked = true
				};
			}
			levelsData[0].isLocked = false;
		}
	}

	private void UnlockLevel(int levelIndex) {
		levelsData[levelIndex].isLocked = false;
		string key = $"{DATA_KEY}{mapIndex}";
		PlayerPrefs.SetString(key, Utils.Serialize(levelsData));
		PlayerPrefs.Save();
	}

	public void DeleteLevelsData() {
		string key = $"{DATA_KEY}{mapIndex}";
		PlayerPrefs.DeleteKey(key);
		PlayerPrefs.Save();
	}

	private class LevelData {
		public bool isLocked;
	}
}
