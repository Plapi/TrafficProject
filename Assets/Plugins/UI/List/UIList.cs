using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIList : UIItem {

	[SerializeField] protected ScrollRect scrollRect = default;
	[SerializeField] private TemplateWithType[] templates = default;

	[SerializeField] private float topOffset = default;
	[SerializeField] private float bottomOffset = default;
	[SerializeField] private float space = default;
	[SerializeField] private bool alwaysIsScrollable = false;

	private List<UIListItem.IData> dataList;
	private ItemYPosition[] itemYPositions;

	private Dictionary<int, UIListItem> usedItems = new Dictionary<int, UIListItem>();
	private Dictionary<string, Stack<UIListItem>> cachedItems = new Dictionary<string, Stack<UIListItem>>();

	public Action<UIListItem, UIListItem.IData> OnItemSetup;

	private void Awake() {
		scrollRect.onValueChanged.AddListener(value => {
			UpdateList();
		});
		for (int i = 0; i < templates.Length; i++) {
			templates[i].Template.gameObject.SetActive(false);
			cachedItems.Add(templates[i].Type, new Stack<UIListItem>());
		}
	}

	public void Init(UIListItem.IData[] datas) {
		Init(new List<UIListItem.IData>(datas));
	}

	public virtual void Init(List<UIListItem.IData> list) {
		dataList = list;

		SetContentHeight();

		// disable scrolling if not needed (viewport size >= content size)
		scrollRect.enabled = alwaysIsScrollable || scrollRect.content.rect.height > scrollRect.viewport.rect.height;

		ClearAllUsedItems();
		UpdateList();
	}

	public void AddItemData(UIListItem.IData itemData) {
		dataList.Add(itemData);
		Init(dataList);
	}

	public void UpdateItemData(UIListItem.IData itemData) {
		int index = dataList.IndexOf(itemData);
		if (index != -1 && usedItems.TryGetValue(index, out UIListItem item)) {
			item.Init(dataList[index]);
			OnItemSetup?.Invoke(item, dataList[index]);
		}
	}

	public void RemoveItemData(UIListItem.IData itemData) {
		if (dataList.Remove(itemData)) {
			Init(dataList);
		}
	}

	protected virtual void UpdateList() {
		if (dataList == null) {
			return;
		}

		// find and cache items outside viewport from 0 to first visible index
		int firstVisibleItemIndex = 0;
		float topScroll = scrollRect.content.anchoredPosition.y;
		for(int i = 0; i < itemYPositions.Length; i++) {
			if (itemYPositions[i].Max < -topScroll) {
				firstVisibleItemIndex = i;
				break;
			}
			if (usedItems.ContainsKey(i)) {
				CacheItem(usedItems[i]);
				usedItems.Remove(i);
			}
		}

		// find and cache items outside viewport to last visible index
		int lastVisibleIndex = itemYPositions.Length - 1;
		float bottomScroll = scrollRect.content.anchoredPosition.y + scrollRect.viewport.rect.height;
		for (int i = lastVisibleIndex; i >= firstVisibleItemIndex; i--) {
			if (itemYPositions[i].Min >= -bottomScroll) {
				lastVisibleIndex = i;
				break;
			}
			if (usedItems.ContainsKey(i)) {
				CacheItem(usedItems[i]);
				usedItems.Remove(i);
			}
		}

		// add items inside viewport
		for (int i = firstVisibleItemIndex; i <= lastVisibleIndex; i++) {
			if (!usedItems.ContainsKey(i)) {
				usedItems.Add(i, GetItem(dataList[i]));
				usedItems[i].AnchoredPosY = itemYPositions[i].Min;
				usedItems[i].Init(dataList[i]);
				OnItemSetup?.Invoke(usedItems[i], dataList[i]);
			}
		}
	}

	private void SetContentHeight() {
		itemYPositions = new ItemYPosition[dataList.Count];
		float height = topOffset;
		for (int i = 0; i < itemYPositions.Length; i++) {
			itemYPositions[i] = new ItemYPosition {
				Min = -height,
				Max = -(height += GetTemplate(dataList[i].GetType().ToString()).Template.Height)
			};
			height += space;
		}
		height += bottomOffset;
		scrollRect.content.SetAnchorY(0f);
		scrollRect.content.SetSizeY(height);
	}

	private UIListItem GetItem(UIListItem.IData data) {
		string type = data.GetType().ToString();
		Stack<UIListItem> stack = cachedItems[type];
		UIListItem item = stack.Count > 0 ? stack.Pop() : Instantiate(GetTemplate(type).Template, scrollRect.content);
		item.gameObject.SetActive(true);
		return item;
	}

	private void CacheItem(UIListItem item) {
		item.gameObject.SetActive(false);
		cachedItems[GetTemplate(item).Type].Push(item);
	}

	private void ClearAllUsedItems() {
		foreach (var item in usedItems) {
			CacheItem(item.Value);
		}
		usedItems.Clear();
	}

	private TemplateWithType GetTemplate(UIListItem item) {
		for (int i = 0; i < templates.Length; i++) {
			if (templates[i].Template.GetType() == item.GetType()) {
				return templates[i];
			}
		}
		return null;
	}

	private TemplateWithType GetTemplate(string type) {
		for (int i = 0; i < templates.Length; i++) {
			if (templates[i].Type == type) {
				return templates[i];
			}
		}
		Debug.LogError($"Type not found {type}");
		return null;
	}

	[Serializable]
	private class TemplateWithType {
		public UIListItem Template;
		public string Type;
	}

	private class ItemYPosition {
		public float Max;
		public float Min;
	}
}
