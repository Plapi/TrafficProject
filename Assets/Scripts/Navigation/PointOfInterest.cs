using System;
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
	[SerializeField] private int carsTarget = default;
	[SerializeField] private float spawnTime = default;

	private float spawnTimeProgress;

	public Action OnCarEnterListener;

	public Node HeadNode => headNode;
	public Node OtherNode => otherNode;

	public int CarsProgress { get; private set; }
	public int CarsTarget => carsTarget;
	[HideInInspector] public int CarsCountStartedWithThisDestination;

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

	public void InitProgressSpawnTime() {
		spawnTimeProgress = spawnTime;
	}

	public void UpdateSpawnTime() {
		spawnTimeProgress -= Time.deltaTime;
	}

	public bool CanSpawnCar() {
		return spawnTimeProgress <= 0f;
	}

	public void OnCarEnter() {
		CarsProgress++;
		OnCarEnterListener?.Invoke();
	}

	public void ResetCarsProgress() {
		CarsProgress = 0;
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
		Vector3 dir = (otherNode.transform.position - headNode.transform.position).normalized;
		destinationMark.transform.SetPositionAndRotation(pos - dir * 5f, Quaternion.LookRotation(dir));
		destinationMark.SetName(string.IsNullOrEmpty(locationName) ? "Todo" : locationName);
	}
#endif
}

