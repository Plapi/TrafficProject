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

	public float RoadWidth;
	public float RoadHalfWidth => RoadWidth / 2f;
	public float RoadCurveDist;
	public float RoadHeight;
	public float RoadsMinAngle;
}
