using UnityEngine;

public class Config : ScriptableObject {

	private static Config instance;
	public static Config Instance {
		get {
			if (instance == null) {
				instance = Resources.Load<Config>("Config");
				instance.Init();
			}
			return instance;
		}
	}

	private void Init() {
		roadHalfWidth = RoadWidth / 2f;
		roadDoubleWidth = RoadWidth * 2f;
	}

	public float RoadWidth;
	public float RoadHalfWidth => roadHalfWidth;
	public float RoadDoubleWidth => roadDoubleWidth;
	public float RoadCurveDist;
	public float RoadHeight;
	public float RoadsMinAngle;
	public float RoadTextureMap;

	public bool RightDriving;

	public Material RoadMaterial;
	public Material RoadLineMaterial;

	private float roadHalfWidth;
	private float roadDoubleWidth;
}
