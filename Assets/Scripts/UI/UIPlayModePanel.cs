using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIPlayModePanel : UIItem {

	[SerializeField] private Button backButton = default;
	[SerializeField] private Button normalSpeedButton = default;
	[SerializeField] private Button fastSpeedButton = default;

	public void Init(Action onBackButton, Action onNormalSpeedButton, Action onFastSpeedButton) {
		backButton.onClick.AddListener(() => {
			onBackButton?.Invoke();
		});
		normalSpeedButton.onClick.AddListener(() => {
			onNormalSpeedButton?.Invoke();
		});
		fastSpeedButton.onClick.AddListener(() => {
			onFastSpeedButton?.Invoke();
		});
	}

	public void Show(Action onComplete) {
		gameObject.SetActive(true);
		RectTransform.DOAnchorPosY(0f, 0.25f).OnComplete(() => {
			onComplete?.Invoke();
		});
	}

	public void Hide() {
		RectTransform.DOAnchorPosY(100f, 0.25f).OnComplete(() => {
			gameObject.SetActive(false);
		});
	}
}
