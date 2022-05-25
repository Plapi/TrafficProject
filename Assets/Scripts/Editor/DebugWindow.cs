using System;
using System.Reflection;
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

		if (GUILayout.Button("Delete Nodes Data")) {
			NodeController.DeleteData();
		}

		if (GUILayout.Button("Test")) {

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
