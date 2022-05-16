using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour {

	private const float WIDTH = 2.5f;
	private const float HALF_WIDTH = WIDTH / 2f;
	private const float HEIGHT = 0.1f;

	[SerializeField] private BoxCollider map = default;
	[SerializeField] private NodeConnexions nodeConnexions = default;

	private readonly List<Node> nodes = new();
	private readonly Node[] lastNodes = new Node[3];

	private Node currentNode;

	private void Update() {

		if (!GetRaycastPoint(out Vector3 point)) {
			return;
		}

		if (currentNode != null) {
			currentNode.transform.position = point;
			UpdateLastNodes();
		}

		if (Input.GetMouseButtonUp(0)) {
			Node newNode = NewNode(point);

			if (currentNode == null) {
				lastNodes[0] = currentNode = newNode;
				nodes.Add(currentNode);
				lastNodes[1] = newNode = NewNode(point);
			} else {
				nodes.Add(currentNode);
				if (lastNodes[2] != null) {
					for (int i = 0; i < lastNodes.Length - 1; i++) {
						lastNodes[i] = lastNodes[i + 1];
					}
				}
				lastNodes[2] = newNode;
			}

			nodeConnexions.AddConnexion(currentNode, newNode);

			currentNode = newNode;
			UpdateLastNodes();
			return;
		}

		if (currentNode != null && Input.GetMouseButtonUp(1)) {
			nodeConnexions.RemoveConnexion(nodes[^1], currentNode);
			Destroy(currentNode.gameObject);
			if (!nodeConnexions.HasConnexions(nodes[^1])) {
				Destroy(nodes[^1].gameObject);
				nodes.RemoveAt(nodes.Count - 1);
			} else {
				Update1LastNode();
			}
			for (int i = 0; i < lastNodes.Length; i++) {
				lastNodes[i] = null;
			}
		}
	}

	private void UpdateLastNodes() {
		if (lastNodes[2] == null) {
			Update2LastNodes();
		} else {
			Update3LastNodes();
		}
	}

	private void Update1LastNode() {
		Vector3 dir = lastNodes[0].transform.position - lastNodes[1].transform.position;
		Vector3 left = Vector3.Cross(dir, Vector3.up).normalized;
		lastNodes[1].LeftPoint.position = lastNodes[1].transform.position + left * HALF_WIDTH;
		lastNodes[1].RightPoint.position = lastNodes[1].transform.position - left * HALF_WIDTH;
		nodeConnexions.UpdateConnexionsMesh(lastNodes[0], lastNodes[1]);
	}

	private void Update2LastNodes() {
		Vector3 dir = lastNodes[0].transform.position - lastNodes[1].transform.position;
		Vector3 left = Vector3.Cross(dir, Vector3.up).normalized;

		lastNodes[0].LeftPoint.position = lastNodes[0].transform.position + left * HALF_WIDTH;
		lastNodes[0].RightPoint.position = lastNodes[0].transform.position - left * HALF_WIDTH;

		lastNodes[1].LeftPoint.position = lastNodes[1].transform.position + left * HALF_WIDTH;
		lastNodes[1].RightPoint.position = lastNodes[1].transform.position - left * HALF_WIDTH;

		nodeConnexions.UpdateConnexionsMesh(lastNodes[0], lastNodes[1]);
	}

	private void Update3LastNodes() {
		Vector3 left01 = Vector3.Cross(lastNodes[0].transform.position - lastNodes[1].transform.position, Vector3.up).normalized;
		Vector3 dir01Left = (lastNodes[1].transform.position + left01 * HALF_WIDTH - lastNodes[0].LeftPoint.position).normalized;
		Vector3 dir01Right = (lastNodes[1].transform.position - left01 * HALF_WIDTH - lastNodes[0].RightPoint.position).normalized;

		Vector3 left21 = Vector3.Cross(lastNodes[1].transform.position - lastNodes[2].transform.position, Vector3.up).normalized;
		lastNodes[2].LeftPoint.position = lastNodes[2].transform.position + left21 * HALF_WIDTH;
		lastNodes[2].RightPoint.position = lastNodes[2].transform.position - left21 * HALF_WIDTH;

		Vector3 dir21Left = (lastNodes[1].transform.position + left21 * HALF_WIDTH - lastNodes[2].LeftPoint.position).normalized;
		Vector3 dir21Right = (lastNodes[1].transform.position - left21 * HALF_WIDTH - lastNodes[2].RightPoint.position).normalized;

		if (Math3d.LineLineIntersection(out Vector3 intersectionLeft, lastNodes[0].LeftPoint.position, dir01Left, lastNodes[2].LeftPoint.position, dir21Left)) {
			lastNodes[1].LeftPoint.position = intersectionLeft;
		}

		if (Math3d.LineLineIntersection(out Vector3 intersectionRight, lastNodes[0].RightPoint.position, dir01Right, lastNodes[2].RightPoint.position, dir21Right)) {
			lastNodes[1].RightPoint.position = intersectionRight;
		}

		nodeConnexions.UpdateConnexionsMesh(lastNodes[0], lastNodes[1]);
		nodeConnexions.UpdateConnexionsMesh(lastNodes[1], lastNodes[2]);
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
