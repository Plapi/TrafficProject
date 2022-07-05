using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UIPlayModeView : UIView<UIPlayModeView.Data> {

	private const float TOTAL_TIME = 60f;

	[SerializeField] private Button normalSpeedButton = default;
	[SerializeField] private Button fastSpeedButton = default;

	[SerializeField] private TextMeshProUGUI timerText = default;
	[SerializeField] private Image timerFill = default;

	private bool allowUpdate;
	private float remainingTime;
	public int RemainingTimeInt { get; private set; }

	public override void OnBack() {
		allowUpdate = false;
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

	private void Update() {
		if (!allowUpdate) {
			return;
		}

		remainingTime -= Time.deltaTime;
		RemainingTimeInt = Mathf.CeilToInt(remainingTime);

		if (RemainingTimeInt < 0) {
			RemainingTimeInt = 0;
		}
		timerText.text = new TimeSpan(0, 0, RemainingTimeInt).ToString(@"m\:ss");
		timerFill.fillAmount = remainingTime / TOTAL_TIME;

		if (RemainingTimeInt == 0) {
			allowUpdate = false;
			DataValue.onTimePassed();
		}
	}

	public override void ShowAnim(Action onComplete = null) {
		RectTransform.SetAnchorY(100f);
		RectTransform.DOAnchorPosY(0f, 0.25f).OnComplete(() => {
			onComplete?.Invoke();
			allowUpdate = true;
		});

		remainingTime = TOTAL_TIME;
		Update();
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
		public Action onTimePassed;
	}
}
