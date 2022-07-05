using System;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviourSingleton<UIController> {

	[SerializeField] private UIViewBase[] views = default;

	private readonly Dictionary<Type, UIViewBase> dictViews = new();
	private readonly Stack<UIViewBase> openViews = new();

	protected override void Awake() {
		base.Awake();
		for (int i = 0; i < views.Length; i++) {
			dictViews.Add(views[i].GetType(), views[i]);
			views[i].gameObject.SetActive(false);
		}
	}

	public T GetView<T>() where T : UIViewBase {
		return (T)dictViews[typeof(T)];
	}

	public T ShowView<T>(Action onComplete = null) where T : UIViewBase {
		T view = GetView<T>();
		ShowView(view, onComplete);
		return view;
	}

	public T InitAndShowView<T>(IUIViewData data, Action onComplete = null) where T : UIViewBase {
		T view = GetView<T>();
		ShowView(view, onComplete);
		view.Init(data);
		return view;
	}

	public void ShowView(UIViewBase view, Action onComplete = null) {
		view.gameObject.SetActive(true);
		view.ShowAnim(() => {
			UIViewBase currentView = GetCurrentView();
			if (currentView != null) {
				currentView.gameObject.SetActive(false);
			}
			openViews.Push(view);
			onComplete?.Invoke();
		});
	}

	public void HideCurrentView(Action onComplete = null) {
		if (openViews.Count > 0) {
			UIViewBase view = openViews.Pop();
			view.HideAnim(() => {
				view.gameObject.SetActive(false);
				onComplete?.Invoke();
			});
			UIViewBase currentView = GetCurrentView();
			if (currentView != null) {
				currentView.gameObject.SetActive(true);
			}
		}
	}

	public void PopInstantView<T>() where T : UIViewBase {
		List<UIViewBase> list = new();
		while (openViews.Count > 0) {
			UIViewBase view = openViews.Pop();
			if (view is T) {
				break;
			}
			list.Add(view);
		}
		list.ForEach(item => {
			openViews.Push(item);
		});
	}

	public UIViewBase GetCurrentView() {
		if (openViews.Count > 0) {
			return openViews.Peek();
		}
		return null;
	}
}
