using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using JsonFx.Json;
using TMPro;
using Poly2Tri;

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

	public static void SetSizeX(this RectTransform rectTransform, float x) {
		rectTransform.sizeDelta = new Vector2(x, rectTransform.sizeDelta.y);
	}
	public static void SetSizeY(this RectTransform rectTransform, float y) {
		rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, y);
	}

	public static void SetAnchorX(this RectTransform rectTransform, float x) {
		rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
	}
	public static void SetAnchorY(this RectTransform rectTransform, float y) {
		rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, y);
	}

	public static void SetOffsetMinX(this RectTransform rectTransform, float x) {
		rectTransform.offsetMin = new Vector2(x, rectTransform.offsetMin.y);
	}
	public static void SetOffsetMinY(this RectTransform rectTransform, float y) {
		rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, y);
	}

	public static void SetOffsetMaxX(this RectTransform rectTransform, float x) {
		rectTransform.offsetMax = new Vector2(x, rectTransform.offsetMax.y);
	}
	public static void SetOffsetMaxY(this RectTransform rectTransform, float y) {
		rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, y);
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

	public static bool TryGetChild(this Transform transform, int index, out Transform child) {
		child = null;
		if (transform.childCount > index) {
			child = transform.GetChild(index);
			return true;
		}
		return false;
	}

	public static bool TryGetChild(this Transform transform, string name, out Transform child) {
		child = null;
		foreach (Transform t in transform) {
			if (t.name == name) {
				child = t;
				return true;
			}
		}
		return false;
	}

	public static string ToHex(this Color color) {
		return "#" + ColorUtility.ToHtmlStringRGBA(color);
	}

	public static Vector3 GetBottomLeft(this Bounds bounds) {
		return bounds.min;
	}

	public static Vector3 GetTopLeft(this Bounds bounds) {
		return new Vector3(bounds.min.x, 0f, bounds.max.z);
	}

	public static Vector3 GetTopRight(this Bounds bounds) {
		return bounds.max;
	}

	public static Vector3 GetBottomRight(this Bounds bounds) {
		return new Vector3(bounds.max.x, 0f, bounds.min.z);
	}

	public static bool Contains(this Bounds bounds, Bounds otherBounds) {
		return bounds.Contains(otherBounds.GetBottomLeft()) &&
			bounds.Contains(otherBounds.GetTopLeft()) &&
			bounds.Contains(otherBounds.GetTopRight()) &&
			bounds.Contains(otherBounds.GetBottomRight());
	}

	public static void SetAction(this Button button, Action action) {
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() => action?.Invoke());
	}

	public static void SetText(this Button button, string text) {
		if (button.transform.TryGetChild(0, out Transform child)) {
			if (child.TryGetComponent(out Text unityText)) {
				unityText.text = text;
			} else if (child.TryGetComponent(out TextMeshProUGUI textMeshProUI)) {
				textMeshProUI.text = text;
			} else {
				Debug.LogError("Text component not found");
			}
		} else {
			Debug.LogError("Child not found");
		}
	}

	public static void SetAlpha(this Image image, float alpha) {
		image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
	}

	public static Texture2D ToTexture2D(this Texture texture) {
		Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);

		RenderTexture currentRT = RenderTexture.active;

		RenderTexture renderTexture = new RenderTexture(texture.width, texture.height, 32);
		Graphics.Blit(texture, renderTexture);

		RenderTexture.active = renderTexture;
		texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		texture2D.Apply();

		RenderTexture.active = currentRT;

		return texture2D;
	}

	public static Texture2D Rotate(this Texture2D texture, bool clockwise) {
		Color32[] original = texture.GetPixels32();
		Color32[] rotated = new Color32[original.Length];
		int w = texture.width;
		int h = texture.height;
		int iRotated, iOriginal;
		for (int j = 0; j < h; ++j) {
			for (int i = 0; i < w; ++i) {
				iRotated = (i + 1) * h - j - 1;
				iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
				rotated[iRotated] = original[iOriginal];
			}
		}
		Texture2D rotatedTexture = new Texture2D(h, w);
		rotatedTexture.SetPixels32(rotated);
		rotatedTexture.Apply();
		return rotatedTexture;
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

	public static void Delay(this MonoBehaviour behaviour, float delay, Action onComplete) {
		if (behaviour != null) {
			behaviour.StartCoroutine(Delay(delay, onComplete));
		}
	}

	private static IEnumerator Delay(float delayTime, Action onComplete) {
		if (delayTime <= 0) {
			yield return null;
		} else {
			yield return new WaitForSeconds(delayTime);
		}
		onComplete?.Invoke();
	}

	public static void EndOfFrame(this MonoBehaviour behaviour, Action onComplete) {
		if (behaviour != null) {
			behaviour.StartCoroutine(EndOfFrame(onComplete));
		}
	}

	private static IEnumerator EndOfFrame(Action onComplete) {
		yield return new WaitForEndOfFrame();
		onComplete?.Invoke();
	}

	public static void WaitForFrames(this MonoBehaviour behaviour, int frames, Action onComplete) {
		behaviour.StartCoroutine(WaitForFrames(frames, onComplete));
	}

	public static IEnumerator WaitForFrames(int frames, Action onComplete) {
		while (frames > 0) {
			frames--;
			yield return null;
		}
		onComplete?.Invoke();
	}

	public static void WaitUntil(this MonoBehaviour behaviour, Func<bool> predicate, Action onComplete) {
		behaviour.StartCoroutine(WaitUntil(predicate, onComplete));
	}

	public static IEnumerator WaitUntil(Func<bool> predicate, Action onComplete) {
		yield return new WaitUntil(predicate);
		onComplete?.Invoke();
	}

	public static Coroutine LoopAction(this MonoBehaviour behaviour, Action action, float time = 1f) {
		if (behaviour != null) {
			return behaviour.StartCoroutine(LoopAction(action, time));
		}
		return null;
	}

	private static IEnumerator LoopAction(Action action, float time) {
		WaitForSeconds waitForSeconds = new WaitForSeconds(time);
		while (true) {
			action?.Invoke();
			yield return waitForSeconds;
		}
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
		return Vector3.SignedAngle((p0 - p1).normalized, (p2 - p1).normalized, Vector3.up);
		//Vector3 v0 = p0 - p1;
		//Vector3 v1 = p2 - p1;
		//return Mathf.Atan2(Vector3.Dot(p2, Vector3.Cross(v0, v1)), Vector3.Dot(v0, v1)) * Mathf.Rad2Deg;
	}

	public static Vector3 Intersection(Vector3 v0, Vector3 d0, Vector3 v1, Vector3 d1) {
		if (Math3d.LineLineIntersection(out Vector3 intersection, v0, d0, v1, d1)) {
			return intersection;
		}
		return MidPoint(v0, v1);
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

	public static bool LineIntersectOtherLine(Vector3 a0, Vector3 a1, Vector3 b0, Vector3 b1) {
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
				return true;
			}
		}

		return false;
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

	public static bool PolyContainsPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p) {
		Point2D[] polyPoints = new Point2D[4] {
			new(p0.x, p0.z),
			new(p1.x, p1.z),
			new(p2.x, p2.z),
			new(p3.x, p3.z)
		};
		return PolygonUtil.PointInPolygon2D(polyPoints, new(p.x, p.z));
	}

	public static bool PolyContainsAnyPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3[] points) {
		Point2D[] polyPoints = new Point2D[4] {
			new(p0.x, p0.z),
			new(p1.x, p1.z),
			new(p2.x, p2.z),
			new(p3.x, p3.z)
		};
		return PolyContainsAnyPoint(polyPoints, points);
	}

	public static bool PointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c) {
		return PointInTriangle(new Vector2(p.x, p.z), new Vector2(a.x, a.z), new Vector2(b.x, b.z), new Vector2(c.x, c.z));
	}

	private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c) {
		Vector2 d, e;
		double w1, w2;
		d = b - a;
		e = c - a;

		if (Mathf.Approximately(e.y, 0)) {
			e.y = 0.0001f;
		}

		w1 = (e.x * (a.y - p.y) + e.y * (p.x - a.x)) / (d.x * e.y - d.y * e.x);
		w2 = (p.y - a.y - w1 * d.y) / e.y;
		return (w1 >= 0f) && (w2 >= 0.0) && ((w1 + w2) <= 1.0);
	}

	public static bool PolyContainsAnyPoint(Point2D[] polyPoints, Vector3[] points) {
		Point2D[] points2D = new Point2D[points.Length];
		for (int i = 0; i < points2D.Length; i++) {
			points2D[i] = new(points[i].x, points[i].z);
		}
		for (int i = 0; i < points2D.Length; i++) {
			if (PolygonUtil.PointInPolygon2D(polyPoints, points2D[i])) {
				return true;
			}
		}
		return false;
	}

	public static Vector3 GetClosestPointOnLine(Vector3 p, Vector3 a, Vector3 b) {
		return a + Vector3.Project(p - a, b - a);
	}
}

public class JSONVector3 {
	public float x;
	public float y;
	public float z;

	public Vector3 ToVector3() {
		return new Vector3(x, y, z);
	}

	public static JSONVector3 FromVector3(Vector3 v3) {
		return new JSONVector3 {
			x = v3.x,
			y = v3.y,
			z = v3.z
		};
	}
}

#if UNITY_EDITOR
public static class DrawArrow {
	public static void Draw(Vector3 pos0, Vector3 pos1, Color color) {
		ForGizmo(pos0, (pos1 - pos0).normalized * Vector3.Distance(pos0, pos1), color, 1f);
	}

	public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
		Gizmos.DrawRay(pos, direction);

		Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
		Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
		Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
		Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
	}

	public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
		Gizmos.color = color;
		Gizmos.DrawRay(pos, direction);

		Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
		Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
		Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
		Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
	}

	public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
		Debug.DrawRay(pos, direction);

		Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
		Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
		Debug.DrawRay(pos + direction, right * arrowHeadLength);
		Debug.DrawRay(pos + direction, left * arrowHeadLength);
	}
	public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
		Debug.DrawRay(pos, direction, color);

		Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
		Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
		Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
		Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
	}
}
#endif