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
			return list[Random(0, list.Count)];
		}
		return default;
	}

	public static T Random<T>(this T[] array) {
		if (array.Length > 0) {
			return array[Random(0, array.Length)];
		}
		return default;
	}

	public static float GetAngle360(Vector3 p0, Vector3 p1, Vector3 p2) {
		float angle = GetAngleSigned(p0, p1, p2);
		if (angle < 0) {
			return angle + 360;
		}
		return angle;
	}

	public static float GetAngleSigned(Vector3 p0, Vector3 p1, Vector3 p2) {
		Vector3 v0 = p0 - p1;
		Vector3 v1 = p2 - p1;
		return Mathf.Atan2(Vector3.Dot(p2, Vector3.Cross(v0, v1)), Vector3.Dot(v0, v1)) * Mathf.Rad2Deg;
	}

	public static bool Intersection2D(Vector3 v0, Vector3 d0, Vector3 v1, Vector3 d1, out Vector3 intersection) {
		v0 = new Vector3(v0.x, v0.z, 0f);
		v1 = new Vector3(v1.x, v1.z, 0f);
		d0 = new Vector3(d0.x, d0.z, 0f);
		d1 = new Vector3(d1.x, d1.z, 0f);
		intersection = GetIntersectionPointCoordinates(v0, v0 + d0 * 100f, v1, v1 + d1 * 100f, out bool found);
		return found;
	}

	private static Vector2 GetIntersectionPointCoordinates(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1, out bool found) {
		float tmp = (b1.x - b0.x) * (a1.y - a0.y) - (b1.y - b0.y) * (a1.x - a0.x);

		if (tmp == 0) {
			// No solution!
			found = false;
			return Vector2.zero;
		}

		float mu = ((a0.x - b0.x) * (a1.y - a0.y) - (a0.y - b0.y) * (a1.x - a0.x)) / tmp;

		found = true;

		return new Vector2(
			b0.x + (b1.x - b0.x) * mu,
			b0.y + (b1.y - b0.y) * mu
		);
	}

	public static bool TryGetIntersection(out Vector3 intersection, Vector3 a0, Vector3 a1, Vector3 b0, Vector3 b1) {
		bool isIntersecting = false;

		//3d -> 2d
		Vector2 p1 = new(a0.x, a0.z);
		Vector2 p2 = new(a1.x, a1.z);

		Vector2 p3 = new(b0.x, b0.z);
		Vector2 p4 = new(b1.x, b1.z);

		float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

		//Make sure the denominator is > 0, if so the lines are parallel
		if (denominator != 0) {
			float u_a = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
			float u_b = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

			//Is intersecting if u_a and u_b are between 0 and 1
			if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1) {
				isIntersecting = true;
			}
		}

		intersection = default;
		if (isIntersecting) {
			Vector3 dirA = (a1 - a0).normalized * 100f;
			Vector3 dirB = (b1 - b0).normalized * 100f;
			if (!Math3d.LineLineIntersection(out intersection, a0, dirA, b0, dirB)) {
				intersection = MidPoint(a0, b0);
			}
		}

		return isIntersecting;
	}
}
