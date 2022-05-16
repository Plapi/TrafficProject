using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {

	private const float WIDTH = 2.5f;
	private const float HALF_WIDTH = WIDTH / 2f;

	private readonly List<Node> connexions = new();
	private Vector3[] meshVertices = new Vector3[0];

	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;

	public int ConnexionsCount => connexions.Count;

	private void Awake() {
		meshRenderer = new GameObject("mesh").AddComponent<MeshRenderer>();
		meshRenderer.transform.parent = transform;
		meshRenderer.transform.localPosition = Vector3.zero;
		meshRenderer.material = Resources.Load<Material>("Materials/Road");

		meshFilter = meshRenderer.gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = new() {
			vertices = new Vector3[4],
			triangles = new int[6] {
				0, 2, 1,
				2, 3, 1
			},
			normals = new Vector3[4] {
				Vector3.up,
				Vector3.up,
				Vector3.up,
				Vector3.up
			}
		};
	}

	public void AddConnexion(Node node) {
		connexions.Add(node);
		node.connexions.Add(this);
	}

	public void RemoveConnexion(Node node) {
		connexions.Remove(node);
		node.connexions.Remove(this);
	}

	public void UpdateMesh() {
		if (connexions.Count == 1) {
			if (meshVertices.Length != 5) {
				meshVertices = new Vector3[5];
				meshVertices[0] = Vector3.zero;
			}
			PerpPointToNode(connexions[0], out meshVertices[1], out meshVertices[2]);
			PerpMidPointToNode(connexions[0], out meshVertices[3], out meshVertices[4]);
		} else if (connexions.Count == 2) {
			if (meshVertices.Length != 7) {
				meshVertices = new Vector3[7];
				meshVertices[0] = Vector3.zero;
			}

			PerpMidPointToNode(connexions[0], out meshVertices[1], out meshVertices[2]);
			PerpMidPointToNode(connexions[1], out meshVertices[3], out meshVertices[4]);

			Vector3 bis = Utils.BisectVector(connexions[0].transform.position, transform.position, connexions[1].transform.position).normalized;
			meshVertices[5] = bis * HALF_WIDTH;
			meshVertices[6] = -bis * HALF_WIDTH;
		}

		//meshFilter.mesh.vertices = meshVertices;
	}

	private void PerpPointToNode(Node node, out Vector3 left, out Vector3 right) {
		left = right = default;
		Vector3 dir = node.transform.position - transform.position;
		Vector3 cross = Vector3.Cross(dir, Vector3.up).normalized;
		meshVertices[0] = cross * HALF_WIDTH;
		meshVertices[1] = -cross * HALF_WIDTH;
	}

	private void PerpMidPointToNode(Node node, out Vector3 left, out Vector3 right) {
		left = right = default;
		Vector3 dir = node.transform.position - transform.position;
		Vector3 cross = Vector3.Cross(dir, Vector3.up).normalized;
		Vector3 midPoint = transform.InverseTransformPoint(Utils.MidPoint(transform.position, node.transform.position));
		meshVertices[2] = midPoint + cross * HALF_WIDTH;
		meshVertices[3] = midPoint - cross * HALF_WIDTH;
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawCube(transform.position, Vector3.one * 0.25f);

		connexions.ForEach(cn => {
			Gizmos.DrawLine(transform.position, cn.transform.position);
		});

		Gizmos.color = Color.green;
		for (int i = 0; i < meshVertices.Length; i++) {
			Gizmos.DrawCube(transform.position + meshVertices[i], Vector3.one * 0.15f);
		}
	}
}
