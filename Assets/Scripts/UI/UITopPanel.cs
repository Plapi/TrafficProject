using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UITopPanel : UIItem {

	[SerializeField] private Button roadButton = default;
	[SerializeField] private Button demolishButton = default;
	[SerializeField] private Button intersectionButton = default;
	[SerializeField] private Button playButton = default;

	public void Init(Action onRoadButton, Action onDemolishButton, Action onIntersectionButton, Action onPlayButton) {
		roadButton.onClick.AddListener(() => {
			onRoadButton?.Invoke();
		});
		demolishButton.onClick.AddListener(() => {
			onDemolishButton?.Invoke();
		});
		intersectionButton.onClick.AddListener(() => {
			onIntersectionButton?.Invoke();
		});
		playButton.onClick.AddListener(() => {
			onPlayButton?.Invoke();
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
