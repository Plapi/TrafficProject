using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISemaphoreTimer : UIItem {

	[SerializeField] private TextMeshProUGUI timerText = default;
	[SerializeField] private Button decreaseButton = default;
	[SerializeField] private Button increaseButton = default;

	public void Init(SemaphoreData semaphoreData, int timerIndex) {

		UpdateTimer(semaphoreData, timerIndex);

		decreaseButton.SetAction(() => {
			semaphoreData.timers[timerIndex]--;
			UpdateTimer(semaphoreData, timerIndex);
		});
		increaseButton.SetAction(() => {
			semaphoreData.timers[timerIndex]++;
			UpdateTimer(semaphoreData, timerIndex);
		});
	}

	private void UpdateTimer(SemaphoreData semaphoreData, int timerIndex) {
		semaphoreData.timers[timerIndex] = Mathf.Clamp(semaphoreData.timers[timerIndex], Config.Instance.MinSemaphoreTimer, Config.Instance.MaxSemaphoreTimer);
		timerText.text = semaphoreData.timers[timerIndex].ToString();
	}
}
