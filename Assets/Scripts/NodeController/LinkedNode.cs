using UnityEngine;

public class LinkedNode : Node {
	[SerializeField] private Node linkNode = default;

	public Node LinkNode => linkNode;
}
