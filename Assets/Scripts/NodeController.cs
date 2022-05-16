using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour {

	private const float WIDTH = 2.5f;
	private const float HALF_WIDTH = WIDTH / 2f;
	private const float HEIGHT = 0.1f;

	[SerializeField] private BoxCollider map = default;
	[SerializeField] private NodeConnexions nodeConnexions = default;

	private readonly List<Node> nodes = new();

	private Node currentNode;

	private void Update() {

		if (!GetRaycastPoint(out Vector3 point)) {
			return;
		}

		if (currentNode != null) {
			currentNode.transform.position = point;

			Vector3 dir = nodes[^1].transform.position - currentNode.transform.position;
			Vector3 left = Vector3.Cross(dir, Vector3.up).normalized;
			nodes[^1].LeftPoint.position = nodes[^1].transform.position + left * HALF_WIDTH;
			nodes[^1].RightPoint.position = nodes[^1].transform.position - left * HALF_WIDTH;

			currentNode.LeftPoint.position = currentNode.transform.position + left * HALF_WIDTH;
			currentNode.RightPoint.position = currentNode.transform.position - left * HALF_WIDTH;

			nodeConnexions.UpdateConnexionsMesh(nodes[^1], currentNode);
		}

		if (Input.GetMouseButtonUp(0)) {
			Node newNode = NewNode(point);

			if (currentNode == null) {
				currentNode = newNode;
				nodes.Add(currentNode);
				newNode = NewNode(point);
			} else {
				nodes.Add(currentNode);
			}
			
			nodeConnexions.AddConnexion(currentNode, newNode);

			currentNode = newNode;
			return;
		}

		if (currentNode != null && Input.GetMouseButtonUp(1)) {
			nodeConnexions.RemoveConnexion(nodes[^1], currentNode);
			Destroy(currentNode.gameObject);
			if (!nodeConnexions.HasConnexions(nodes[^1])) {
				Destroy(nodes[^1].gameObject);
				nodes.RemoveAt(nodes.Count - 1);
			}
		}
	}

	private Node NewNode(Vector3 point) {
		Node node = new GameObject($"node{nodes.Count}").AddComponent<Node>();
		node.transform.parent = transform;
		node.transform.position = point;
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
