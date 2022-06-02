using UnityEngine;
using UnityEngine.UI;

public class UIIntersection : UIItem {

	[SerializeField] private Button semaphoreSwitchButton = default;
	[SerializeField] private Button prioritySwitchButton = default;
	[SerializeField] private UISemaphoreTimer semaphoreTimer = default;

	private Node intersection;
	private Button[] priorityButtons;
	private UISemaphoreTimer[] semaphoreTimers;
	private NavigationPoint[] inputPoints;
	private SemaphoreData semaphoreData;

	public void Init(Node intersection) {
		this.intersection = intersection;
		semaphoreData = intersection.GetSemaphoreData();

		inputPoints = Config.Instance.RightDriving ? intersection.GetNavigationRightPoints() : intersection.GetNavigationLeftPoints();
		priorityButtons = new Button[inputPoints.Length];
		for (int i = 0; i < priorityButtons.Length; i++) {
			priorityButtons[i] = Instantiate(prioritySwitchButton, transform);
			UpdatePriorityButton(inputPoints, i);
			int index = i;
			priorityButtons[i].SetAction(() => {
				intersection.UpdateGiveWay(index, !inputPoints[index].GivesWay);
				UpdatePriorityButton(inputPoints, index);
			});
		}
		semaphoreTimers = new UISemaphoreTimer[inputPoints.Length];
		for (int i = 0; i < semaphoreTimers.Length; i++) {
			semaphoreTimers[i] = Instantiate(semaphoreTimer, transform);
			semaphoreTimers[i].Init(semaphoreData, i);
		}

		UpdateSwitchSemaphore();
		semaphoreSwitchButton.SetAction(() => {
			semaphoreData.isOn = !semaphoreData.isOn;
			intersection.UpdateSemaphores();
			intersection.UpdateGiveWaysObjects();
			UpdateSwitchSemaphore();
		});
	}

	public void UpdateUI() {
		semaphoreSwitchButton.GetComponent<RectTransform>().anchoredPosition = Utils.WorldPositionToUI(intersection.transform.position, MainCanvas);
		semaphoreSwitchButton.SetText($"S\n{(semaphoreData.isOn ? "On" : "Off")}");

		if (!semaphoreData.isOn) {
			for (int i = 0; i < priorityButtons.Length; i++) {
				priorityButtons[i].GetComponent<RectTransform>().anchoredPosition = Utils.WorldPositionToUI(inputPoints[i].Position, MainCanvas);
			}
		} else {
			for (int i = 0; i < semaphoreTimers.Length; i++) {
				semaphoreTimers[i].RectTransform.anchoredPosition = Utils.WorldPositionToUI(inputPoints[i].Position, MainCanvas);
			}
		}
	}

	private void UpdateSwitchSemaphore() {
		for (int i = 0; i < priorityButtons.Length; i++) {
			priorityButtons[i].gameObject.SetActive(!semaphoreData.isOn);
		}
		for (int i = 0; i < semaphoreTimers.Length; i++) {
			semaphoreTimers[i].gameObject.SetActive(semaphoreData.isOn);
		}
	}

	private void UpdatePriorityButton(NavigationPoint[] inputPoints, int index) {
		priorityButtons[index].SetText($"P\n{(inputPoints[index].GivesWay ? "On" : "Off")}");
	}
}
