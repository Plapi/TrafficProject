using UnityEngine;

public class Test : MonoBehaviour {

	[SerializeField] private Transform L1_start = default;
	[SerializeField] private Transform L1_end = default;
	[SerializeField] private Transform L2_start = default;
	[SerializeField] private Transform L2_end = default;

	private void OnDrawGizmos() {
		Gizmos.color = IsIntersectingAlternative() ? Color.red : Color.green;
		Gizmos.DrawLine(L1_start.position, L1_end.position);
		Gizmos.DrawLine(L2_start.position, L2_end.position);
	}

	private bool IsIntersectingAlternative() {
		bool isIntersecting = false;

		//3d -> 2d
		Vector2 p1 = new Vector2(L1_start.position.x, L1_start.position.z);
		Vector2 p2 = new Vector2(L1_end.position.x, L1_end.position.z);

		Vector2 p3 = new Vector2(L2_start.position.x, L2_start.position.z);
		Vector2 p4 = new Vector2(L2_end.position.x, L2_end.position.z);

		float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

		//Make sure the denominator is > 0, if so the lines are parallel
		if (denominator != 0) {
			float u_a = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
			float u_b = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

			//Is intersecting if u_a and u_b are between 0 and 1
			if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1) {
				isIntersecting = true;
			}
		}

		return isIntersecting;
	}
}
