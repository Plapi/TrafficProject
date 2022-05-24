using UnityEngine;

[ExecuteInEditMode]
public class UIProgressBar : UIItem {

	[SerializeField] [Range(0f, 1f)] private float m_progress = 0f;
	[SerializeField] private UIItem m_progressItem = null;
	[SerializeField] private Axis m_axis;

	public float progress => m_progress;

	public void UpdateProgress(float progress) {
		if (float.IsNaN(progress)) {
			progress = 0f;
		}
		if (m_progressItem != null) {
			if (m_axis == Axis.HORIZONTAL) {
				m_progressItem.Width = GlobalWidth * (m_progress = Mathf.Clamp01(progress));
			} else {
				m_progressItem.Height = globalHeight * (m_progress = Mathf.Clamp01(progress));
			}
		}
	}

	private enum Axis {
		HORIZONTAL,
		VERTICAL
	}

#if UNITY_EDITOR
	private void Update() {
		if (!Application.isPlaying) {
			UpdateProgress(m_progress);
		}
	}
#endif
}
