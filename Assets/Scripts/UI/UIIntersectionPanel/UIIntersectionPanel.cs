using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIIntersectionPanel : UIItem {

	[SerializeField] private Button backButton = default;
	[SerializeField] private UIIntersection uiIntersection = default;

	[SerializeField] private float minVisibleDistance = default;

	private readonly List<UIIntersection> uiIntersections = new();
	private List<Node> intersections = new();

	public void Init(Action onBackButton) {
		backButton.onClick.AddListener(() => {
			onBackButton?.Invoke();
		});
	}

	public void Show(Action onComplete, List<Node> intersections) {
		gameObject.SetActive(true);
		RectTransform.DOAnchorPosY(0f, 0.25f).OnComplete(() => {
			onComplete?.Invoke();
		});

		this.intersections = intersections;
		CreateSemaphoreButtons();
		Update();
	}

	public void Hide() {
		RectTransform.DOAnchorPosY(100f, 0.25f).OnComplete(() => {
			gameObject.SetActive(false);
		});
		ClearSemaphoreButtons();
	}

	private void CreateSemaphoreButtons() {
		for (int i = 0; i < intersections.Count; i++) {
			UIIntersection item = Instantiate(uiIntersection, MainCanvas.transform);
			item.gameObject.SetActive(true);
			item.Init(intersections[i]);
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
			bool visible = Vector3.Distance(intersections[i].transform.position, Camera.main.transform.position) <= minVisibleDistance;

			if (visible != uiIntersections[i].gameObject.activeSelf) {
				uiIntersections[i].gameObject.SetActive(visible);
			}

			if (visible) {
				uiIntersections[i].UpdateUI();
			}
		}
	}
}
