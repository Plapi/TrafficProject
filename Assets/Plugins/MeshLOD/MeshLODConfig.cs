using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MeshLODConfig", menuName = "ScriptableObjects/MeshLODConfig", order = 1)]
public class MeshLODConfig : ScriptableObject {

	public Config[] configs = GetDefaultsConfigs();

	public static Config[] GetDefaultsConfigs() {
		return new Config[4] {
			new Config {
				quality = 1f,
				screenRelativeTransitionHeight = 0.5f
			},
			new Config {
				quality = 0.75f,
				screenRelativeTransitionHeight = 0.25f
			},
			new Config {
				quality = 0.5f,
				screenRelativeTransitionHeight = 0.15f
			},
			new Config {
				quality = 0.25f,
				screenRelativeTransitionHeight = 0.05f
			}
		};
	}

	[Serializable]
	public class Config {
		[Range(0f, 1f)] public float quality;
		[Range(0f, 1f)] public float screenRelativeTransitionHeight;
	}
}
