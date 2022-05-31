using UnityEngine;

public class UIItem : MonoBehaviour {

	private RectTransform rectTransform;
	private Vector3[] worldCorners = new Vector3[4];

	public RectTransform RectTransform {
		get {
			if (rectTransform == null) {
				rectTransform = GetComponent<RectTransform>();
			}
			return rectTransform;
		}
	}

	public Vector2 Size {
		get => RectTransform.sizeDelta;
		set => RectTransform.sizeDelta = value;
	}

	public float Width {
		get => Size.x;
		set => Size = new Vector2(value, Size.y);
	}

	public float Height {
		get => Size.y;
		set => Size = new Vector2(Size.x, value);
	}

	public float GlobalWidth => RectTransform.rect.width;
	public float globalHeight => RectTransform.rect.height;

	public Vector2 Pos {
		get => RectTransform.anchoredPosition;
		set => RectTransform.anchoredPosition = value;
	}

	public float Right {
		get {
			RectTransform.GetWorldCorners(worldCorners);
			return worldCorners[2].x;
		}
	}

	public float Left {
		get {
			RectTransform.GetWorldCorners(worldCorners);
			return worldCorners[1].x;
		}
	}

	private static RectTransform mainCanvas;
	public static RectTransform MainCanvas {
		get {
			if (mainCanvas == null) {
				mainCanvas = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
			}
			return mainCanvas;
		}
	}
}
