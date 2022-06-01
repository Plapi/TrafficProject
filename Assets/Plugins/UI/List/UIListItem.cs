using UnityEngine;

public abstract class UIListItem : UIItem {
	public abstract void Init(IData data);
	public interface IData { }
}
