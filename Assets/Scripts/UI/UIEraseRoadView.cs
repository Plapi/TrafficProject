using System;
using UnityEngine;
using DG.Tweening;

public class UIEraseRoadView : UIView<UIEraseRoadView.Data> {

	public override void OnBack() {
		UIController.Instance.HideCurrentView(() => {
			DataValue.onBack?.Invoke();
		});
	}

	public void Init(Action onBackButton) {
		backButton.SetAction(() => {
			onBackButton?.Invoke();
		});
	}

	public override void ShowAnim(Action onComplete = null) {
		RectTransform.SetAnchorY(100f);
		RectTransform.DOAnchorPosY(0f, TRANS_TIME).OnComplete(() => {
			onComplete?.Invoke();
		});
	}

	public override void HideAnim(Action onComplete = null) {
		RectTransform.DOAnchorPosY(100f, TRANS_TIME).OnComplete(() => {
			onComplete?.Invoke();
		});
	}

	public class Data : IUIViewData {
		public Action onBack;
	}
}
