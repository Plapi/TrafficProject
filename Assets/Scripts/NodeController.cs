using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour {

	private const float HEIGHT = 0.1f;

	[SerializeField] private BoxCollider map = default;

	private readonly List<Node> nodes = new();
	private readonly Node[] lastNodes = new Node[3];

	private void Update() {

		if (!GetRaycastPoint(out Vector3 point)) {
			return;
		}

		if (lastNodes[0] != null) {
			lastNodes[0].transform.position = point;

			if (Input.GetMouseButtonDown(1)) {
				lastNodes[0].RemoveConnexion(lastNodes[1]);
				Destroy(lastNodes[0].gameObject);
				if (lastNodes[1].ConnexionsCount == 0) {
					Destroy(lastNodes[1].gameObject);
				} else {
					lastNodes[1].UpdateMesh();
				}
				for (int i = 0; i < lastNodes.Length; i++) {
					lastNodes[i] = null;
				}
				return;
			}

			lastNodes[0].UpdateMesh();
			lastNodes[1].UpdateMesh();
		}

		if (Input.GetMouseButtonDown(0)) {
			if (lastNodes[0] == null) {
				lastNodes[1] = NewNode(point);
			} else {
				for (int i = lastNodes.Length - 1; i > 0; i--) {
					lastNodes[i] = lastNodes[i - 1];
				}
			}
			lastNodes[0] = NewNode(point);
			lastNodes[0].AddConnexion(lastNodes[1]);
			return;
		}
	}

	private Node NewNode(Vector3 point) {
		Node node = new GameObject($"node{nodes.Count}").AddComponent<Node>();
		node.transform.parent = transform;
		node.transform.position = point;
		nodes.Add(node);
		return node;
	}

	private bool GetRaycastPoint(out Vector3 point) {
		point = default;
		if (map.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, float.MaxValue)) {
			point = hit.point;
			point = new Vector3(Mathf.RoundToInt(point.x), point.y + HEIGHT, Mathf.RoundToInt(point.z));
			return true;
		}
		return false;
	}
}
