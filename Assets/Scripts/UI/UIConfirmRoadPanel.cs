using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIConfirmRoadPanel : UIItem {

	[SerializeField] private Button declineButton = default;
	[SerializeField] private Button acceptButton = default;

	public void Init(Action onDeclineButton, Action onAcceptButton) {
		declineButton.SetAction(() => {
			onDeclineButton?.Invoke();
		});
		acceptButton.SetAction(() => {
			onAcceptButton?.Invoke();
		});
	}

	public void SetAcceptButtonInteractable(bool interactable) {
		acceptButton.interactable = interactable;
	}

	public void Show() {
		gameObject.SetActive(true);
		transform.DOKill();
		transform.localScale = Vector3.one * 0.01f;
		transform.DOScale(Vector3.one, 0.25f);
	}

	public void Hide() {
		transform.DOKill();
		transform.DOScale(Vector3.one * 0.01f, 0.25f).OnComplete(() => {
			gameObject.SetActive(false);
		});
	}

	public void UpdateAnchorPos(Transform tr) {
		RectTransform.anchoredPosition = Utils.WorldPositionToUI(tr.position, MainCanvas);
	}
}
