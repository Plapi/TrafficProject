using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UILevelView : UIView<UILevelView.Data> {

	[SerializeField] private Button roadButton = default;
	[SerializeField] private Button demolishButton = default;
	[SerializeField] private Button intersectionButton = default;
	[SerializeField] private Button playButton = default;

	public override void OnBack() {
		UIController.Instance.HideCurrentView(DataValue.onBack);
	}

	public override void OnInit() {
		roadButton.SetAction(() => {
			DataValue.onRoadButton?.Invoke();
		});
		demolishButton.SetAction(() => {
			DataValue.onDemolishButton?.Invoke();
		});
		intersectionButton.SetAction(() => {
			DataValue.onIntersectionButton?.Invoke();
		});
		playButton.SetAction(() => {
			DataValue.onPlayButton?.Invoke();
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
		public Action onRoadButton;
		public Action onDemolishButton;
		public Action onIntersectionButton;
		public Action onPlayButton;
	}
}
