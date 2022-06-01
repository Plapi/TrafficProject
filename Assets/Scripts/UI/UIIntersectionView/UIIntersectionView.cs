using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIIntersectionView : UIView<UIIntersectionView.Data> {

	[SerializeField] private UIIntersection uiIntersection = default;

	[SerializeField] private float minVisibleDistance = default;

	private readonly List<UIIntersection> uiIntersections = new();

	public override void OnInit() {
		base.OnInit();
		CreateSemaphoreButtons();
		Update();
	}

	public override void OnBack() {
		UIController.Instance.HideCurrentView(() => {
			DataValue.onBack?.Invoke();
		});
		ClearSemaphoreButtons();
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

	private void CreateSemaphoreButtons() {
		for (int i = 0; i < DataValue.intersections.Count; i++) {
			UIIntersection item = Instantiate(uiIntersection, MainCanvas.transform);
			item.gameObject.SetActive(true);
			item.Init(DataValue.intersections[i]);
			uiIntersections.Add(item);
		}
	}

	private void ClearSemaphoreButtons() {
		for (int i = 0; i < uiIntersections.Count; i++) {
			Destroy(uiIntersections[i].gameObject);
		}
		uiIntersections.Clear();
	}

	private void Update() {
		for (int i = 0; i < uiIntersections.Count; i++) {
			bool visible = Vector3.Distance(DataValue.intersections[i].transform.position, Camera.main.transform.position) <= minVisibleDistance;

			if (visible != uiIntersections[i].gameObject.activeSelf) {
				uiIntersections[i].gameObject.SetActive(visible);
			}

			if (visible) {
				uiIntersections[i].UpdateUI();
			}
		}
	}

	public class Data : IUIViewData {
		public Action onBack;
		public List<Node> intersections;
	}
}
