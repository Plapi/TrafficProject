using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UIPointOfInterestPanel : UIItem {

	[SerializeField] private RectTransform carPivot = default;
	[SerializeField] private TextMeshProUGUI text = default;
	[SerializeField] private Image circleFill = default;

	private PointOfInterest pointOfInterest;

	private Tween fillTween;

	public void Init(PointOfInterest pointOfInterest) {
		this.pointOfInterest = pointOfInterest;
		this.pointOfInterest.OnCarEnterListener = OnCarEnterPointOfInterest;
		UpdateUI(false);
	}

	public void ShowAnim() {
		UpdatePosition();
		gameObject.SetActive(true);
		transform.localScale = Vector3.one * 0.01f;
		transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutExpo);
		this.Delay(0.15f, SpinCarPivot);
	}

	public void HideAnim(Action onComplete) {
		transform.DOScale(Vector3.one * 0.01f, 0.25f).SetEase(Ease.InExpo).OnComplete(() => {
			gameObject.SetActive(false);
			onComplete?.Invoke();
		});
		pointOfInterest.OnCarEnterListener = null;
	}

	public void UpdatePosition() {
		RectTransform.anchoredPosition = Utils.WorldPositionToUI(pointOfInterest.transform.position, MainCanvas);
	}

	private void OnCarEnterPointOfInterest() {
		UpdateUI(true);
		SpinCarPivot();
	}

	public void UpdateUI(bool animCircleFill) {
		text.text = Mathf.Max(0, pointOfInterest.CarsTarget - pointOfInterest.CarsProgress).ToString();

		float fillProgress = (float)pointOfInterest.CarsProgress / pointOfInterest.CarsTarget;
		if (animCircleFill) {
			if (fillTween != null) {
				fillTween.Kill(false);
			}
			fillTween = this.ValueTo(circleFill.fillAmount, fillProgress, 0.75f, Ease.OutExpo, value => {
				circleFill.fillAmount = value;
			}, () => {
				fillTween = null;
			});
		} else {
			circleFill.fillAmount = fillProgress;
		}

		transform.DOPunchScale(Vector3.one * 0.2f, 0.25f);
	}

	private void SpinCarPivot() {
		carPivot.DORotate(new Vector3(0f, 0f, 360f), 0.75f, RotateMode.LocalAxisAdd).SetEase(Ease.OutBack);
	}

	/*public float punchScale = 0.1f;
	public float duration = 0.25f;
	public int vibrato = 10;
	public float elasticity = 1;

	private void Update() {
		if (Input.GetKeyDown(KeyCode.A)) {
			transform.DOPunchScale(Vector3.one * punchScale, 0.25f);
		}
	}*/
}
