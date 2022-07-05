using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UILevelCompleteView : UIView<UILevelCompleteView.Data> {

	[SerializeField] private Image background = default;
	[SerializeField] private RectTransform window = default;
	[SerializeField] private TextMeshProUGUI title = default;
	[SerializeField] private TextMeshProUGUI timer = default;
	[SerializeField] private GameObject completeTrueContainer = default;
	[SerializeField] private GameObject completeFalseContainer = default;

	public override void OnBack() {
		DataValue.onContinue?.Invoke();
	}

	public override void OnInit() {
		title.text = $"Level {(DataValue.complete ? "Complete!" : "Fail")}";
		completeTrueContainer.SetActive(DataValue.complete);
		completeFalseContainer.SetActive(!DataValue.complete);
		if (DataValue.complete) {
			timer.text = new TimeSpan(0, 0, DataValue.time).ToString(@"m\:ss");
		}
	}

	public override void ShowAnim(Action onComplete = null) {
		window.SetAnchorY(window.sizeDelta.y / 2f + RectTransform.rect.size.y / 2f);
		window.DOAnchorPos3DY(0f, 0.25f);

		background.SetAlpha(0f);
		background.DOFade(0.55f, 0.25f).OnComplete(() => onComplete?.Invoke());
	}

	public override void HideAnim(Action onComplete = null) {
		window.DOAnchorPos3DY(-window.sizeDelta.y / 2f - RectTransform.rect.size.y / 2f, 0.25f);
		background.DOFade(0f, 0.25f).OnComplete(() => onComplete?.Invoke());
	}

	public class Data : IUIViewData {
		public bool complete;
		public int time;
		public Action onContinue;
	}
}
