using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class PointOfInterest : MonoBehaviour {

	[SerializeField] private string locationName = default;
	[SerializeField] private Node headNode = default;
	[SerializeField] private Node otherNode = default;
	[SerializeField] private NodeRestrictedArea restrictedArea = default;

	[SerializeField] private DestinationMark destinationMark = default;

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

#if UNITY_EDITOR
	private void Update() {
		if (Application.isPlaying) {
			return;
		}
		if (destinationMark == null) {
			destinationMark = Instantiate(Resources.Load<DestinationMark>("DestinationMark"), transform);
			destinationMark.name = "DestinationMark";
		}

		Utils.PerpendicularPoints(otherNode.transform.position, headNode.transform.position, out Vector3 pos, out _, 2.75f);
		destinationMark.transform.position = pos;

		Vector3 dir = (otherNode.transform.position - headNode.transform.position).normalized;
		destinationMark.transform.rotation = Quaternion.LookRotation(dir);

		destinationMark.SetName(string.IsNullOrEmpty(locationName) ? "Todo" : locationName);
	}
#endif
}

