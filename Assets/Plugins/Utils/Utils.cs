using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using JsonFx.Json;

public static class Utils {
	public static void SetX(this Transform transform, float x) {
		transform.position = new Vector3(x, transform.position.y, transform.position.z);
	}
	public static void SetY(this Transform transform, float y) {
		transform.position = new Vector3(transform.position.x, y, transform.position.z);
	}
	public static void SetZ(this Transform transform, float z) {
		transform.position = new Vector3(transform.position.x, transform.position.y, z);
	}

	public static void SetXY(this Transform transform, float x, float y) {
		transform.position = new Vector3(x, y, transform.position.z);
	}
	public static void SetXZ(this Transform transform, float x, float z) {
		transform.position = new Vector3(x, transform.position.y, z);
	}
	public static void SetYZ(this Transform transform, float y, float z) {
		transform.position = new Vector3(transform.position.z, y, z);
	}
	public static void SetXYZ(this Transform transform, float x, float y, float z) {
		transform.position = new Vector3(x, y, z);
	}

	public static void SetLocalX(this GameObject obj, float x) {
		obj.transform.SetLocalX(x);
	}
	public static void SetLocalX(this Transform transform, float x) {
		transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
	}
	public static void SetLocalY(this GameObject obj, float y) {
		obj.transform.SetLocalY(y);
	}
	public static void SetLocalY(this Transform transform, float y) {
		transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
	}
	public static void SetLocalZ(this GameObject obj, float z) {
		obj.transform.SetLocalZ(z);
	}
	public static void SetLocalZ(this Transform transform, float z) {
		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
	}

	public static void SetLocalXY(this Transform transform, float x, float y) {
		transform.localPosition = new Vector3(x, y, transform.localPosition.z);
	}
	public static void SetLocalXZ(this Transform transform, float x, float z) {
		transform.localPosition = new Vector3(x, transform.localPosition.y, z);
	}
	public static void SetLocalYZ(this Transform transform, float y, float z) {
		transform.localPosition = new Vector3(transform.localPosition.z, y, z);
	}
	public static void SetLocalXYZ(this Transform transform, float x, float y, float z) {
		transform.localPosition = new Vector3(x, y, z);
	}

	public static void SetScaleX(this Transform transform, float x) {
		transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
	}
	public static void SetScaleY(this Transform transform, float y) {
		transform.localScale = new Vector3(transform.localScale.x, y, transform.localScale.z);
	}
	public static void SetScaleZ(this Transform transform, float z) {
		transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, z);
	}
	public static void SetScale(this Transform transform, float scale) {
		transform.localScale = Vector3.one * scale;
	}

	public static void SetLocalAngleX(this Transform transform, float x) {
		transform.localEulerAngles = new Vector3(x, transform.localEulerAngles.y, transform.localEulerAngles.z);
	}
	public static void SetLocalAngleY(this Transform transform, float y) {
		transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, y, transform.localEulerAngles.z);
	}
	public static void SetLocalAngleZ(this Transform transform, float z) {
		transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, z);
	}

	public static string ToHex(this Color color) {
		return "#" + ColorUtility.ToHtmlStringRGBA(color);
	}

	public static Vector3? GetHitPoint() {
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, 1 << 8)) {
			return hit.point;
		}
		return null;
	}

	public static bool TryGetHitPoint(float screenPX, float screenPY, out Vector3 hitPoint) {
		hitPoint = Vector3.zero;
		Vector3 screenPos = new Vector3(Screen.width * screenPX, Screen.height * screenPY, 0f);
		if (Physics.Raycast(Camera.main.ScreenPointToRay(screenPos), out RaycastHit hit, Mathf.Infinity, 1 << 8)) {
			hitPoint = hit.point;
			return true;
		}
		return false;
	}

	public static bool TryGetHitPoint(out Vector3 hitPoint) {
		hitPoint = Vector3.zero;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, 1 << 8)) {
			hitPoint = hit.point;
			return true;
		}
		return false;
	}

	public static bool TryGetPerpHitPoint(out Vector3 hitPoint) {
		hitPoint = Vector3.zero;
		if (Physics.Raycast(new Ray(Camera.main.transform.position, Vector3.down), out RaycastHit hit, Mathf.Infinity, 1 << 8)) {
			hitPoint = hit.point;
			return true;
		}
		return false;
	}

	public static bool TryGetHitCollider(int layer, out Collider collider) {
		collider = null;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, layer)) {
			collider = hit.collider;
			return true;
		}
		return false;
	}

	public static Vector2 WorldPositionToUI(Vector3 worldPos, RectTransform rectTransform) {
		Vector2 viewportPosition = Camera.main.WorldToViewportPoint(worldPos);
		return new Vector2((viewportPosition.x * rectTransform.sizeDelta.x) - (rectTransform.sizeDelta.x * 0.5f),
				(viewportPosition.y * rectTransform.sizeDelta.y) - (rectTransform.sizeDelta.y * 0.5f));
	}

	public static bool PointsAreEqual(Vector3 point0, Vector3 point1) {
		return Math.Abs(point0.x - point1.x) < 0.5f && Mathf.Abs(point0.z - point1.z) < 0.5f;
	}

	public static Vector3 FindNearestPointOnLine(Vector3 a, Vector3 b, Vector3 p) {
		Vector3 heading = b - a;
		float magnitudeMax = heading.magnitude;
		heading.Normalize();

		Vector3 lhs = p - a;
		float dotP = Vector3.Dot(lhs, heading);
		dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
		return a + heading * dotP;
	}

	public static int RoundToInt(float value) {
		return Mathf.RoundToInt(value);
	}

	public static void Swap<T>(IList<T> list, int i, int j) {
		T value = list[j];
		list[j] = list[i];
		list[i] = value;
	}

	private static readonly System.Random random = new System.Random();
	public static void Shuffle<T>(IList<T> list) {
		for (int i = list.Count - 1; i > 0; i--) {
			Swap(list, i, random.Next(i + 1));
		}
	}

	public static Vector3 MidPoint(Vector3 p0, Vector3 p1) {
		return Vector3.Lerp(p0, p1, 0.5f);
	}

	public static Vector3 BisectVector(Vector3 p0, Vector3 p1, Vector3 p2) {
		Vector3 dir0 = p0 - p1;
		Vector3 dir1 = p2 - p1;
		Vector3 dir2 = p2 - p0;
		Vector3 v3D = p0 + dir2 * dir0.magnitude / (dir0.magnitude + dir1.magnitude);
		Vector3 v3 = v3D - p1;
		return v3;
	}

	public static void PerpendicularPoints(Vector3 from, Vector3 to, out Vector3 p0, out Vector3 p1, float distance) {
		Vector3 cross = Vector3.Cross(to - from, Vector3.up).normalized;
		p0 = from + cross * distance;
		p1 = from - cross * distance;
	}

	public static void Delay(MonoBehaviour behaviour, float delayTime, Action onComplete) {
		if (behaviour != null) {
			behaviour.StartCoroutine(Delay(delayTime, onComplete));
		}
	}

	public static void WaitForFrames(MonoBehaviour behaviour, int frames, Action onComplete) {
		if (behaviour != null) {
			behaviour.StartCoroutine(WaitForFrames(frames, onComplete));
		}
	}

	public static IEnumerator Delay(float delayTime, Action onComplete) {
		if (delayTime <= 0) {
			yield return null;
		} else {
			yield return new WaitForSeconds(delayTime);
		}
		onComplete?.Invoke();
	}

	public static void WaitUntil(MonoBehaviour behaviour, Func<bool> predicate, Action onComplete) {
		if (behaviour != null) {
			behaviour.StartCoroutine(WaitUntil(predicate, onComplete));
		}
	}

	public static IEnumerator WaitUntil(Func<bool> predicate, Action onComplete) {
		yield return new WaitUntil(predicate);
		onComplete?.Invoke();
	}

	public static void EndOfFrame(MonoBehaviour behaviour, Action onComplete) {
		behaviour.StartCoroutine(EndOfFrame(onComplete));
	}

	public static IEnumerator EndOfFrame(Action onComplete) {
		yield return new WaitForEndOfFrame();
		onComplete?.Invoke();
	}

	public static IEnumerator WaitForFrames(int frames, Action onComplete) {
		while (frames > 0) {
			frames--;
			yield return Delay(0f, null);
		}
		onComplete?.Invoke();
	}

	public static bool IsOverUI() {
		if (EventSystem.current.IsPointerOverGameObject()) {
			return true;
		}
		if (Input.touchCount > 0) {
			return EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId);
		}
		return false;
	}

	public static void DebugMatrix<T>(T[][] matrix) {
		string s = "";
		for (int i = 0; i < matrix.Length; i++) {
			for (int j = 0; j < matrix[i].Length; j++) {
				s += matrix[i][j] + " ";
			}
			s += "\n";
		}
		Debug.LogError(s);
	}

	public static int Random(int min, int max) {
		return UnityEngine.Random.Range(min, max);
	}

	public static float Random(float min, float max) {
		return UnityEngine.Random.Range(min, max);
	}

	public static int GetIndexOf<T>(T[] array, T element) {
		for (int i = 0; i < array.Length; i++) {
			if (EqualityComparer<T>.Default.Equals(array[i], element)) {
				return i;
			}
		}
		return -1;
	}

	public static Tween SimpleTransition(float duration, Ease ease = Ease.Linear, Action<float> onUpdate = null, Action onComplete = null) {
		return DOTween.To(() => 0f, progress => onUpdate?.Invoke(progress), 1f, duration).SetEase(ease).OnComplete(() => onComplete?.Invoke());
	}

	public static Tween SimpleInverseTransition(float duration, Ease ease = Ease.Linear, Action<float> onUpdate = null, Action onComplete = null) {
		return DOTween.To(() => 1f, progress => onUpdate?.Invoke(progress), 0f, duration).SetEase(ease).OnComplete(() => onComplete?.Invoke());
	}

	public static string Serialize(object obj) {
		return JsonWriter.Serialize(obj);
	}

	public static T Deserialize<T>(string json) {
		return JsonReader.Deserialize<T>(json);
	}

	public static Vector2[] GetHexExpansion(int level, float size) {
		Vector2[] positions = new Vector2[level * 6 + 1];
		positions[0] = Vector2.zero;
		for (int i = 1; i < positions.Length; i++) {
			positions[i] = GetHexPos(i - 1, size);
		}
		return positions;
	}

	public static Vector2 GetHexPos(int i, float size) {
		float angle = 2 * Mathf.PI / 6 * i;
		return new Vector2(size * Mathf.Cos(angle), size * Mathf.Sin(angle));
	}

	public static string Format(int number) {
		return number.ToString("#,#");
	}

	public static string KiloFormat(int number) {
		if (number >= 100000000) {
			return (number / 1000000).ToString("#,0M");
		}
		if (number >= 10000000) {
			return (number / 1000000).ToString("0.#") + "M";
		}
		if (number >= 100000) {
			return (number / 1000).ToString("#,0K");
		}
		if (number >= 10000) {
			return (number / 1000).ToString("0.#") + "K";
		}
		return number.ToString("#,0");
	}

	public static void IterateAllChilds(Transform parent, Action<Transform> action) {
		action(parent);
		foreach (Transform child in parent) {
			IterateAllChilds(child, action);
		}
	}

	public static T Random<T>(this List<T> list) {
		if (list.Count > 0) {
			return list[Utils.Random(0, list.Count)];
		}
		return default;
	}

	public static T Random<T>(this T[] array) {
		if (array.Length > 0) {
			return array[Utils.Random(0, array.Length)];
		}
		return default;
	}

	public static float GetAngle(Vector3 p0, Vector3 p1, Vector3 p2) {
		Vector3 v0 = p0 - p1;
		Vector3 v1 = p2 - p1;
		return Mathf.Atan2(Vector3.Dot(p2, Vector3.Cross(v0, v1)), Vector3.Dot(v0, v1)) * Mathf.Rad2Deg;
	}
}
