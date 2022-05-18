using UnityEngine;

public class Config : ScriptableObject {

	private static Config instance;
	public static Config Instance {
		get {
			if (instance == null) {
				instance = Resources.Load<Config>("Config");
			}
			return instance;
		}
	}

	public float RoadWidth = 2.5f;
	public float RoadHalfWidth => RoadWidth / 2f;
	public float RoadCurveDist = 5f;
	public float RoadHeight = 0.1f;
}
