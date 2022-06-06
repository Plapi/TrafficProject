using UnityEngine;

public class LinkedNode : Node {
	[SerializeField] private Node linkNode = default;

	public Node LinkNode => linkNode;

	protected override void OnDrawGizmos() {
		base.OnDrawGizmos();
		if (!drawGizmos|| linkNode == null) {
			return;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, linkNode.transform.position);
	}
}
