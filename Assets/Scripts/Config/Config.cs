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
		roadQuadrupleWidth = RoadWidth * 4f;
	}

	public float RoadWidth;
	public float RoadHalfWidth => roadHalfWidth;
	public float RoadDoubleWidth => roadDoubleWidth;
	public float RoadQuadrupleWidth => roadQuadrupleWidth;
	public float RoadCurveDist;
	public float RoadHeight;
	public float RoadsMinAngle;
	public float RoadTextureMap;

	public bool RightDriving;

	public int DefaultSemaphoreTimer;
	public int MinSemaphoreTimer;
	public int MaxSemaphoreTimer;

	public Material RoadMaterial;
	public Material RoadWrongMaterial;
	public Material RoadSideMarkMaterial;
	public Material RoadBase;

	private float roadHalfWidth;
	private float roadDoubleWidth;
	private float roadQuadrupleWidth;

	public Color LevelBorderColor;
}
