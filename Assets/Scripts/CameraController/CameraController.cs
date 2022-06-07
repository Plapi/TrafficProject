using System;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviourSingleton<CameraController> {

	[SerializeField] private Camera cam = default;

	[SerializeField] private float minOrtSize = default;
	[SerializeField] private float maxOrtSize = default;

	[SerializeField] private Vector2 cameraMinBounds = default;
	[SerializeField] private Vector2 cameraMaxBounds = default;

	private Plane plane;
	private GTouch firstTouch;
	private bool allowMoving = true;
	private bool isOverUI;
	private Action tapAction;

#if UNITY_EDITOR || UNITY_STANDALONE
	private Vector3? prevMousePosition;
#endif

	public int TouchesCount => GTouch.TouchesCount;

	public void SetMoveEnable(bool enable) {
		allowMoving = enable;
	}

	public void SetTapAction(Action action) {
		tapAction = action;
	}

	public void ChangeOrto() {
		cam.orthographic = !cam.orthographic;

		float prevAngle = cam.transform.localEulerAngles.x;
		cam.transform.SetLocalAngleX(90f);
		Vector3 moveTo;
		if (cam.orthographic) {
			moveTo = cam.transform.position + cam.transform.up * 30f;
			moveTo.y = 50f;
		} else {
			moveTo = cam.transform.position - cam.transform.up * 30f;
			moveTo.y = 30f;
		}
		cam.transform.SetLocalAngleX(prevAngle);

		cam.transform.DOLocalMove(moveTo, 0.25f);
		cam.transform.DOLocalRotate(new Vector3(cam.orthographic ? 90f : 45f, cam.transform.localEulerAngles.y, cam.transform.localEulerAngles.z), 0.25f);
	}

	public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
		return Quaternion.Euler(angles) * (point - pivot) + pivot;
	}

	public void Update() {
#if UNITY_EDITOR || UNITY_STANDALONE
		bool fixCameraPos = false;

		if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) {
			if (prevMousePosition != null) {
				Vector3 rotate = (Vector3)prevMousePosition - Input.mousePosition;
				Vector3 pos1 = PlanePosition(new Vector2(Screen.width / 2f, Screen.height / 2f));
				cam.transform.RotateAround(pos1, Vector3.up, -rotate.x / 5f);
			}
			prevMousePosition = Input.mousePosition;
			fixCameraPos = true;
		}

		if (Input.GetMouseButtonUp(1) || Input.GetMouseButton(2)) {
			prevMousePosition = null;
		}

		float scrollDelta = Input.mouseScrollDelta.y;
		if (Mathf.Abs(scrollDelta) > Mathf.Epsilon) {
			if (cam.orthographic) {
				cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scrollDelta, minOrtSize, maxOrtSize);
			} else {
				Vector3 pos1 = PlanePosition(Input.mousePosition);
				cam.transform.position += (pos1 - cam.transform.position).normalized * scrollDelta;
				fixCameraPos = true;
			}
		}
		if (fixCameraPos) {
			FixCameraPos();
		}
#endif

		GTouch[] touches = GTouch.GetTouches();
		if (touches[0] == null) {
			return;
		}

		plane.SetNormalAndPosition(transform.up, transform.position);

		if (allowMoving) {
			if (touches[0].Phase == TouchPhase.Began || touches[0].Phase == TouchPhase.Moved) {
				isOverUI = Utils.IsOverUI();
				if (touches[0].Phase == TouchPhase.Began) {
					firstTouch = touches[0];
				} else if (!isOverUI) {
					cam.transform.Translate(PlanePositionDelta(touches[0]), Space.World);
				}
			} else if (touches[0].Phase == TouchPhase.Ended && touches[1] == null) {
				if (tapAction != null && !Utils.IsOverUI() && IsTap(touches[0])) {
					tapAction();
				}
			}

			if (isOverUI) {
				return;
			}
		}

		if (touches[1] != null) {
			Vector3 pos1 = PlanePosition(touches[0].Position);
			Vector3 pos2 = PlanePosition(touches[1].Position);
			Vector3 pos1b = PlanePosition(touches[0].Position - touches[0].DeltaPosition);
			Vector3 pos2b = PlanePosition(touches[1].Position - touches[1].DeltaPosition);

			if (cam.orthographic) {
				float dist0 = Vector3.Distance(touches[0].Position, touches[1].Position);
				float dist1 = Vector3.Distance(touches[0].Position - touches[0].DeltaPosition, touches[1].Position - touches[1].DeltaPosition);
				float zoom = dist0 - dist1;
				cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - zoom / 20f, minOrtSize, maxOrtSize);
			} else {
				float zoom = Vector3.Distance(pos1, pos2) / Vector3.Distance(pos1b, pos2b);
				if (zoom == 0 || zoom > 10) {
					return;
				}
				cam.transform.position = Vector3.LerpUnclamped(pos1, cam.transform.position, 1 / zoom);
			}

			if (pos2b != pos2) {
				cam.transform.RotateAround(pos1, plane.normal, Vector3.SignedAngle(pos2 - pos1, pos2b - pos1b, plane.normal));
			}
		}
		FixCameraPos();
	}

	private void FixCameraPos() {
		cam.transform.SetY(Mathf.Clamp(cam.transform.position.y, 5f, 200f));
		cam.transform.SetX(Mathf.Clamp(cam.transform.position.x, cameraMinBounds.x, cameraMaxBounds.x));
		cam.transform.SetZ(Mathf.Clamp(cam.transform.position.z, cameraMinBounds.y, cameraMaxBounds.y));
	}

	private bool IsTap(GTouch lastTouch) {
		if (firstTouch != null) {
			return lastTouch.Time - firstTouch.Time < 0.2f && Vector2.Distance(lastTouch.Position, firstTouch.Position) < 10;
		}
		return false;
	}

	private Vector3 PlanePositionDelta(GTouch touch) {
		if (touch.Phase != TouchPhase.Moved) {
			return Vector3.zero;
		}

		var rayBefore = cam.ScreenPointToRay(touch.Position - touch.DeltaPosition);
		var rayNow = cam.ScreenPointToRay(touch.Position);
		if (plane.Raycast(rayBefore, out var enterBefore) && plane.Raycast(rayNow, out var enterNow)) {
			return rayBefore.GetPoint(enterBefore) - rayNow.GetPoint(enterNow);
		}

		return Vector3.zero;
	}

	private Vector3 PlanePosition(Vector2 screenPos) {
		var rayNow = cam.ScreenPointToRay(screenPos);
		if (plane.Raycast(rayNow, out var enterNow)) {
			return rayNow.GetPoint(enterNow);
		}
		return Vector3.zero;
	}

	private class GTouch {

		public Vector2 Position { get; private set; }
		public Vector2 DeltaPosition { get; private set; }
		public TouchPhase Phase { get; private set; }
		public float Time { get; private set; }

		private static readonly GTouch[] touches = new GTouch[2];

		public static int TouchesCount {
			get {
				if (touches[0] != null) {
					if (touches[1] != null) {
						return 2;
					}
					return 1;
				}
				return 0;
			}
		}

#if UNITY_EDITOR || UNITY_STANDALONE
		private static Vector3 prevMousePos;
#endif

		public static GTouch[] GetTouches() {
#if UNITY_EDITOR || UNITY_STANDALONE
			return GetTouchesInStandalone();
#else
			return GetTouchesOnMobile();
#endif
		}

#if UNITY_EDITOR || UNITY_STANDALONE
		private static GTouch[] GetTouchesInStandalone() {
			if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)) {
				touches[0] = new GTouch {
					Position = Input.mousePosition,
					Time = UnityEngine.Time.time
				};

				if (Input.GetMouseButtonDown(0)) {
					prevMousePos = Input.mousePosition;
					touches[0].Phase = TouchPhase.Began;
				} else {
					touches[0].DeltaPosition = Input.mousePosition - prevMousePos;
					prevMousePos = Input.mousePosition;
					if (Input.GetMouseButtonUp(0)) {
						touches[0].Phase = TouchPhase.Ended;
					} else {
						touches[0].Phase = touches[0].DeltaPosition.magnitude > 0f ? TouchPhase.Moved : TouchPhase.Stationary;
					}
				}

				if (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl)) {
					touches[1] = new GTouch {
						Position = touches[0].Position - Vector2.one * 150f,
						DeltaPosition = touches[0].DeltaPosition,
						Phase = touches[0].Phase
					};
				} else {
					touches[1] = null;
				}
			} else {
				touches[0] = null;
				touches[1] = null;
			}
			return touches;
		}
#endif

		private static GTouch[] GetTouchesOnMobile() {
			for (int i = 0; i < touches.Length; i++) {
				if (Input.touchCount > i) {
					Touch touch = Input.GetTouch(i);
					touches[i] = new GTouch {
						Position = touch.position,
						DeltaPosition = touch.deltaPosition,
						Phase = touch.phase,
						Time = UnityEngine.Time.time
					};
				} else {
					touches[i] = null;
				}
			}
			return touches;
		}
	}
}
