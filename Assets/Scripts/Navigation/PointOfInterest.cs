using UnityEngine;

public class PointOfInterest : MonoBehaviour {

	[SerializeField] private Node headNode = default;
	[SerializeField] private Node otherNode = default;
	[SerializeField] private NodeRestrictedArea restrictedArea = default;

	public Node HeadNode => headNode;
	public Node OtherNode => otherNode;

	public bool IntersectRestrictedArea(Node node0, Node node1) {
		return restrictedArea.IntersectConnexion(node0, node1);
	}

	public NavigationPoint StartNavigationPoint {
		get {
			if (Config.Instance.RightDriving) {
				return headNode.GetNavigationRightPoints()[0];
			}
			return headNode.GetNavigationLeftPoints()[0];
		}
	}

	public NavigationPoint EndNavigationPoint {
		get {
			if (Config.Instance.RightDriving) {
				return headNode.GetNavigationLeftPoints()[0];
			}
			return headNode.GetNavigationRightPoints()[0];
		}
	}
}

