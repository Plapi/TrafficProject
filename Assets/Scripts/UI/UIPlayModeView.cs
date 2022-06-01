using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIPlayModeView : UIView<UIPlayModeView.Data> {

	[SerializeField] private Button normalSpeedButton = default;
	[SerializeField] private Button fastSpeedButton = default;

	public override void OnBack() {
		UIController.Instance.HideCurrentView(() => {
			DataValue.onBack?.Invoke();
		});
	}

	public override void OnInit() {
		base.OnInit();
		normalSpeedButton.SetAction(() => {
			DataValue.onNormalSpeedButton?.Invoke();
		});
		fastSpeedButton.SetAction(() => {
			DataValue.onFastSpeedButton?.Invoke();
		});
	}

	public override void ShowAnim(Action onComplete = null) {
		RectTransform.SetAnchorY(100f);
		RectTransform.DOAnchorPosY(0f, 0.25f).OnComplete(() => {
			onComplete?.Invoke();
		});
	}

	public override void HideAnim(Action onComplete = null) {
		RectTransform.DOAnchorPosY(100f, 0.25f).OnComplete(() => {
			onComplete?.Invoke();
		});
	}

	public class Data : IUIViewData {
		public Action onBack;
		public Action onNormalSpeedButton;
		public Action onFastSpeedButton;
	}
}
