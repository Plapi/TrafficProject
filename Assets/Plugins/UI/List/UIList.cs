using System;
using System.Collections.Generic;
using UnityEngine;

public class UIList : UIItem {

	[SerializeField] private UIItem m_element = null;
	[SerializeField] private RectTransform m_content = null;
	[SerializeField] private Axis m_axis = Axis.HORIZONTAL;

	[SerializeField] private float m_space = 0f;
	[SerializeField] private bool m_updateSize = true;
	[SerializeField] private bool m_setItemPos = true;

	private List<UIItem> m_elements = new List<UIItem>();

	private float m_start;
	private float m_size;

	protected virtual void Awake() {
		if (m_content == null) {
			m_content = RectTransform;
		}

		m_element.gameObject.SetActive(false);
		if (m_axis == Axis.HORIZONTAL) {
			m_start = m_element.RectTransform.anchoredPosition.x;
			m_size = m_element.Size.x;
		} else {
			m_start = -m_element.RectTransform.anchoredPosition.y;
			m_size = m_element.Size.y;
		}
	}

	public void ScrollToItem(UIItem item) {
		if (m_axis == Axis.HORIZONTAL) {
			m_content.SetAPX(item.RectTransform.anchoredPosition.x - m_start);
		} else {
			m_content.SetAPY(-item.RectTransform.anchoredPosition.y - m_start);
		}
	}

	public void ForeachElement<T>(Action<T> action) where T : UIItem {
		m_elements.ForEach(element => action(element as T));
	}

	public T[] UpdateElements<T>(int elementsCount, Action<int, T> action = null) where T : UIItem {
		int max = Mathf.Max(m_elements.Count, elementsCount);
		T[] elements = new T[elementsCount];

		float current = m_start;

		for (int i = 0; i < max; i++) {
			if (i < elementsCount) {
				if (i > 0) {
					current += m_space;
				}

				elements[i] = GetElement<T>(i);
				elements[i].gameObject.SetActive(true);

				if (m_setItemPos) {
					if (m_axis == Axis.HORIZONTAL) {
						m_elements[i].RectTransform.SetAPX(current);
					} else {
						m_elements[i].RectTransform.SetAPY(-current);
					}
				}

				current += m_size;

				action?.Invoke(i, elements[i]);
			} else {
				m_elements[i].gameObject.SetActive(false);
			}
		}

		current += m_start;

		if (m_updateSize) {
			if (m_axis == Axis.HORIZONTAL) {
				Size = new Vector2(current, Size.y);
			} else {
				Size = new Vector2(Size.x, current);
			}
		}

		return elements;
	}

	private T GetElement<T>(int index) where T : UIItem {
		if (index >= m_elements.Count) {
			UIItem element = Instantiate(m_element);
			element.transform.SetParent(m_content, false);
			m_elements.Add(element);
		}
		return m_elements[index] as T;
	}

	private enum Axis {
		HORIZONTAL,
		VERTICAL
	}
}
