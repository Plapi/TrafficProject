using UnityEngine;

public class ScriptableObjectSingleton<T> where T : ScriptableObject {
	private static T instance;
	public static T Instance {
		get {
			if (instance == null) {
				instance = Resources.Load<T>(typeof(T).ToString());
			}
			return instance;
		}
	}
}
