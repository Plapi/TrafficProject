using UnityEngine;

public class LinkedNode : Node {

	[SerializeField] private LinkedNode linkNode = default;
	[SerializeField] private DestinationMark destinationMark = default;
	[SerializeField] private float spawnTime = default;

	private float spawnTimeProgress;

	public LinkedNode LinkNode => linkNode;

	public void InitProgressSpawnTime() {
		spawnTimeProgress = spawnTime;
	}

	public void UpdateSpawnTime(int multiply = 1) {
		spawnTimeProgress -= Time.deltaTime * multiply;
	}

	public bool CanSpawnCar() {
		return spawnTimeProgress <= 0f;
	}

#if UNITY_EDITOR
	protected override void OnDrawGizmos() {
		base.OnDrawGizmos();
		if (!drawGizmos || linkNode == null) {
			return;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, linkNode.transform.position);
	}

	public void UpdateDestinationMark() {
		if (Application.isPlaying || linkNode == null) {
			return;
		}

		if (destinationMark == null) {
			destinationMark = Instantiate(Resources.Load<DestinationMark>("DestinationMark"), transform);
			destinationMark.name = "DestinationMark";
		}

		Utils.PerpendicularPoints(transform.position, linkNode.transform.position, out _, out Vector3 pos, 2.75f);
		Vector3 dir = (transform.position - linkNode.transform.position).normalized;
		destinationMark.transform.SetPositionAndRotation(pos - dir * 5f, Quaternion.LookRotation(dir));
		destinationMark.SetName("Exit");
	}
#endif
}
