using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using JsonFx.Json;

public class DebugWindow : EditorWindow {

	private const string APP_NAME = "TP";

	private static Vector2 ScrollPos {
		get => new(0f, EditorPrefs.GetFloat(APP_NAME + "_EDITOR_WINDOW_SCROLL_POS_Y", 0f));
		set => EditorPrefs.SetFloat(APP_NAME + "_EDITOR_WINDOW_SCROLL_POS_Y", value.y);
	}

	[MenuItem("Window/Debug Window")]
	private static void Init() {
		((DebugWindow)GetWindow(typeof(DebugWindow))).Show();
	}

	private void OnGUI() {
		EditorGUILayout.BeginVertical();
		if (!Application.isPlaying) {
			SceneNavigator.Display();
		}

		ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);

		Time.timeScale = EditorGUILayout.Slider("Time Scale", Time.timeScale, 0f, 1f);

		if (GUILayout.Button("Take Screenshot")) {
			EditorCoroutine.Start(TakeScreenshotIEnumerator());
		}

		if (GUILayout.Button("Delete Nodes Data")) {
			LevelController[] levels = FindObjectsOfType<LevelController>();
			for (int i = 0; i < levels.Length; i++) {
				levels[i].DeleteData();
			}
		}

		if (GUILayout.Button("Delete Levels Data")) {
			FindObjectOfType<MapController>().DeleteLevelsData();
		}

		if (GUILayout.Button("Create Level Borders")) {
			LevelController[] levels = FindObjectsOfType<LevelController>();
			for (int i = 0; i < levels.Length; i++) {
				levels[i].CreateLevelBorders();
			}
		}

		if (GUILayout.Button("Test")) {
			UIController.Instance.InitAndShowView<UILevelCompleteView>(new UILevelCompleteView.Data {
				onContinue = () => {
					UIController.Instance.HideCurrentView();
				}
			});
		}

		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();
	}

	private void CreateRoadTexture() {
		const int textureSize = 256;

		int startX, endX, startY, endY;

		Texture2D texture = new(textureSize, textureSize);
		Color roadColor = ColorPalette.Get(ColorId.Road);
		for (int x = 0; x < texture.width; x++) {
			for (int y = 0; y < texture.height; y++) {
				texture.SetPixel(x, y, roadColor);
			}

			startY = (int)(0.05f * textureSize);
			endY = (int)(0.1f * textureSize);
			for (int y = startY; y <= endY; y++) {
				texture.SetPixel(x, y, Color.white);
			}

			startY = (int)(0.9f * textureSize);
			endY = (int)(0.95f * textureSize);
			for (int y = startY; y <= endY; y++) {
				texture.SetPixel(x, y, Color.white);
			}
		}

		startX = (int)(0.25f * textureSize);
		endX = (int)(0.75f * textureSize);

		startY = (int)(0.475f * textureSize);
		endY = (int)(0.525f * textureSize);

		for (int x = startX; x < endX; x++) {
			for (int y = startY; y < endY; y++) {
				texture.SetPixel(x, y, Color.white);
			}
		}

		texture.Apply();

		File.WriteAllBytes(Application.dataPath + "/Resources/Road.png", texture.EncodeToPNG());
		AssetDatabase.Refresh();
	}

	private void CropTexture(string inPath, string outPath, int startX, int startY) {
		Material mat = (Material)AssetDatabase.LoadAssetAtPath(inPath, typeof(Material));

		Texture2D texture = DuplicateTexture((Texture2D)mat.mainTexture);
		Texture2D cropTexture = new(124, 124);

		for (int x = 0; x < cropTexture.width; x++) {
			for (int y = 0; y < cropTexture.height; y++) {
				cropTexture.SetPixel(x, y, texture.GetPixel(x + startX, y + startY));
			}
		}

		cropTexture.Apply();

		File.WriteAllBytes(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + outPath, cropTexture.EncodeToPNG());
	}

	private Texture2D DuplicateTexture(Texture2D source) {
		byte[] bytes = source.GetRawTextureData();
		Texture2D readableText = new(source.width, source.height, source.format, false);
		readableText.LoadRawTextureData(bytes);
		readableText.Apply();
		return readableText;
	}

	private static IEnumerator TakeScreenshotIEnumerator() {
		string screenCaptureName = "ScreenCapture " + DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss") + ".png";

		ScreenCapture.CaptureScreenshot(screenCaptureName);
		while (!File.Exists(Application.dataPath.Replace("Assets", screenCaptureName))) {
			yield return null;
		}

		string screenshotPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + screenCaptureName;

		File.WriteAllBytes(screenshotPath, File.ReadAllBytes(Application.dataPath.Replace("Assets", screenCaptureName)));
		File.Delete(Application.dataPath.Replace("Assets", screenCaptureName));

		System.Diagnostics.Process m_process = new System.Diagnostics.Process {
			StartInfo = new System.Diagnostics.ProcessStartInfo(screenshotPath)
		};

		m_process.Start();
	}

	private void TakeTransparentTexture() {
		string screenCaptureName = "ScreenCapture " + DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss") + ".png";
		const int width = 240;
		const int height = 240;

		RenderTexture prevRenderTexture = RenderTexture.active;

		Camera.main.targetTexture = RenderTexture.GetTemporary(width, height, 8);
		RenderTexture.active = Camera.main.targetTexture;
		Camera.main.Render();

		Texture2D texture = new(width, height);
		texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		texture.Apply();

		string screenshotPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + screenCaptureName;

		File.WriteAllBytes(screenshotPath, texture.EncodeToPNG());

		RenderTexture.active = prevRenderTexture;
		Camera.main.targetTexture = null;

		System.Diagnostics.Process m_process = new() {
			StartInfo = new System.Diagnostics.ProcessStartInfo(screenshotPath)
		};

		m_process.Start();
	}

	[MenuItem("IL Editor Utils/Take Screenshot %k")]
	private static void TakeScreenshot() {
		EditorCoroutine.Start(TakeScreenshotIEnumerator());
	}

	[MenuItem("IL Editor Utils/Reload Current Scene Or Prefab %t")]
	public static void ReloadCurrentSceneOrPrefab() {
		if (!Application.isPlaying) {
			PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			if (prefabStage != null) {
				AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(prefabStage.assetPath));
			} else {
				CheckSaveScene(() => {
					SaveLastGameObjectSelected(Selection.activeGameObject);
					EditorSceneManager.OpenScene(SceneManager.GetActiveScene().path);
					SelectLastGameObjectSelected();
				});
			}
		}
	}

	private static void SaveLastGameObjectSelected(GameObject _selectedGameObject) {
		EditorPrefs.SetString("TP_LAST_GAMEOBJECT_SELECTED_NAME", _selectedGameObject != null ? _selectedGameObject.name : null);
	}

	private static void SelectLastGameObjectSelected() {
		Selection.activeGameObject = GameObject.Find(EditorPrefs.GetString("TP_LAST_GAMEOBJECT_SELECTED_NAME"));
	}

	private static void CheckSaveScene(Action onComplete) {
		Scene scene = EditorSceneManager.GetActiveScene();
		EditorPrefs.SetString("LAST_SCENE", scene.path);
		if (scene.isDirty) {
			if (EditorUtility.DisplayDialog("Save Scene", "Do you want to save " + scene.name + " before playing?", "Yes", "No")) {
				EditorSceneManager.SaveScene(scene, "", false);
				onComplete();
			} else {
				onComplete();
			}
		} else {
			onComplete();
		}
	}

	public static class SceneNavigator {

		private const string EDITOR_NAVIGATOR_SCENES = APP_NAME + "_EDITOR_NAVIGATOR_SCENES";
		private const int MAX_NAVIGATOR_SCENES = 5;

		private static string[] NavigatorScenes {
			get => JsonReader.Deserialize<string[]>(EditorPrefs.GetString(EDITOR_NAVIGATOR_SCENES, JsonWriter.Serialize(new string[] { })));
			set => EditorPrefs.SetString(EDITOR_NAVIGATOR_SCENES, JsonWriter.Serialize(value));
		}

		private static void TryToAddScene(string scene) {
			List<string> scenes = new(NavigatorScenes);
			if (!scenes.Contains(scene)) {
				scenes.Insert(0, scene);
				while (scenes.Count > MAX_NAVIGATOR_SCENES) {
					scenes.RemoveAt(scenes.Count - 1);
				}
				NavigatorScenes = scenes.ToArray();
			}
		}

		public static void Reset() {
			NavigatorScenes = new string[0];
		}

		public static void Display() {
			Scene scene = SceneManager.GetActiveScene();
			if (string.IsNullOrEmpty(scene.path)) {
				return;
			}
			TryToAddScene(scene.path);

			List<string> scenes = new(NavigatorScenes);
			string[] sceneNames = new string[scenes.Count];
			int selectedScene = 0;

			for (int i = 0; i < scenes.Count; i++) {
				SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenes[i]);
				if (sceneAsset != null) {
					sceneNames[i] = sceneAsset.name;
				}
				if (scene.path == scenes[i]) {
					selectedScene = i;
				}
			}

			int newSelectedScene = GUILayout.SelectionGrid(selectedScene, sceneNames, scenes.Count);
			if (newSelectedScene != selectedScene) {
				if (Event.current.button == 0) {
					CheckSaveScene(() => {
						EditorSceneManager.OpenScene(scenes[newSelectedScene]);
					});
				} else {
					scenes.RemoveAt(newSelectedScene);
					NavigatorScenes = scenes.ToArray();
				}
			}
		}

		private static void CheckSaveScene(Action onComplete) {
			Scene scene = SceneManager.GetActiveScene();
			if (scene.isDirty) {
				if (EditorUtility.DisplayDialog("Save Scene", "Do you want to save " + scene.name + "?", "Yes", "No")) {
					EditorSceneManager.SaveScene(scene, "", false);
					onComplete();
				} else {
					onComplete();
				}
			} else {
				onComplete();
			}
		}
	}
}
