using UnityEngine;
using TMPro;

public class DestinationMark : MonoBehaviour {

	[SerializeField] private TextMeshPro text = default;

	public void SetName(string name) {
		text.text = name;
	}
}
