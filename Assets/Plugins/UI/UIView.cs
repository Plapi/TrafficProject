using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public abstract class UIView<T> : UIViewBase where T : IUIViewData {

	[SerializeField] protected Button backButton = default;

	public T DataValue { get; private set; }

	protected void Awake() {
		if (backButton != null) {
			backButton.SetAction(OnBack);
		}
	}

	public virtual void OnInit() { }
	public override void Init(IUIViewData data) {
		DataValue = (T)data;
		OnInit();
	}

	public virtual void OnBack() {
		UIController.Instance.HideCurrentView();
	}
}

public abstract class UIViewBase : UIItem {
	protected const float TRANS_TIME = 0.3f;
	protected const Ease TRANS_EASE = Ease.OutExpo;

	public abstract void Init(IUIViewData data);

	public virtual void ShowAnim(Action onComplete = null) {
		transform.SetAsLastSibling();
		AnchoredPosX = Width;
		RectTransform.DOAnchorPosX(0f, TRANS_TIME)
			.SetEase(TRANS_EASE)
			.OnComplete(() => onComplete?.Invoke());
	}

	public virtual void HideAnim(Action onComplete = null) {
		RectTransform.DOAnchorPosX(Width, TRANS_TIME)
			.SetEase(TRANS_EASE)
			.OnComplete(() => {
				onComplete?.Invoke();
			});
	}
}

public interface IUIViewData { }