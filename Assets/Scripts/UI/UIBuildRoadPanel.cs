using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIBuildRoadPanel : UIItem {

	[SerializeField] private Button backButton = default;

	public void Init(Action onBackButton) {
		backButton.onClick.AddListener(() => {
			onBackButton?.Invoke();
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
