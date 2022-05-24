using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectableUIElement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler {

	private const float TAP_TIME = 0.2f;

	private Action m_onBeginDrag;
	private Action m_onDrag;
	private Action m_onEndDrag;
	private Action m_onTap;

	private float? m_beginDragTime;
	private Vector2 m_lastMousePosition;

	public void Init(Action onBeginDrag, Action onDrag, Action onEndDrag, Action onTap) {
		m_onBeginDrag = onBeginDrag;
		m_onDrag = onDrag;
		m_onEndDrag = onEndDrag;
		m_onTap = onTap;
	}

	public void OnBeginDrag(PointerEventData eventData) {
		m_lastMousePosition = eventData.position;
		m_onBeginDrag?.Invoke();
		m_beginDragTime = Time.time;
	}

	public void OnDrag(PointerEventData eventData) {
		Vector2 currentMousePosition = eventData.position;
		Vector2 diff = currentMousePosition - m_lastMousePosition;
		RectTransform rect = GetComponent<RectTransform>();

		Vector3 newPosition = rect.position + new Vector3(diff.x, diff.y, transform.position.z);
		Vector3 oldPos = rect.position;
		rect.position = newPosition;
		if (!IsRectTransformInsideSreen(rect)) {
			rect.position = oldPos;
		}
		m_lastMousePosition = currentMousePosition;

		m_onDrag?.Invoke();
	}

	public void OnEndDrag(PointerEventData eventData) {
		m_onEndDrag?.Invoke();
	}

	private bool IsRectTransformInsideSreen(RectTransform rectTransform) {
		bool isInside = false;
		Vector3[] corners = new Vector3[4];
		rectTransform.GetWorldCorners(corners);
		int visibleCorners = 0;
		Rect rect = new Rect(0, 0, Screen.width, Screen.height);
		foreach (Vector3 corner in corners) {
			if (rect.Contains(corner)) {
				visibleCorners++;
			}
		}
		if (visibleCorners == 4) {
			isInside = true;
		}
		return isInside;
	}

	public void OnPointerClick(PointerEventData eventData) {
		if (m_beginDragTime == null || Time.time - m_beginDragTime <= TAP_TIME) {
			m_onTap?.Invoke();
		}
		m_beginDragTime = null;
	}
}