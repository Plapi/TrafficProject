using UnityEngine;

public class Node : MonoBehaviour {

	public Transform LeftPoint { get; private set; }
	public Transform RightPoint { get; private set; }

	private void Awake() {
		LeftPoint = new GameObject("left").transform;
		LeftPoint.transform.parent = transform;
		LeftPoint.transform.localPosition = Vector3.zero;
		RightPoint = new GameObject("right").transform;
		RightPoint.transform.parent = transform;
		RightPoint.transform.localPosition = Vector3.zero;
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawCube(transform.position, Vector3.one * 0.25f);
		Gizmos.color = Color.green;
		Gizmos.DrawCube(LeftPoint.position, Vector3.one * 0.15f);
		Gizmos.color = Color.blue;
		Gizmos.DrawCube(RightPoint.position, Vector3.one * 0.15f);
	}

	/*private readonly List<NodeConnexions> connectedNodes = new();

	public void AddConnectedNode(Node node) {
		connectedNodes.Add(new NodeConnexion {
			from = this,
			to = node
		});
	}

	public void RemoveConnectedNode(Node node) {
		for (int i = 0; i < connectedNodes.Count; i++) {
			if (connectedNodes[i].to == node) {
				connectedNodes[i].Destroy();
				connectedNodes.RemoveAt(i);
				i--;
			}
		}
	}

	public void UpdateConnectedNodeMeshes() {
		connectedNodes.ForEach(cn => {
			cn.Update();
		});
	}

	

	private class NodeConnexion {

		private const float WIDTH = 2.5f;
		private const float HALF_WIDTH = WIDTH / 2f;
		private const float HEIGHT = 0.1f;

		public Node from;
		public Node to;

		private MeshGroup leftMesh;
		private MeshGroup rightMesh;

		public void Update() {
			float length = Mathf.Max(1f, Vector3.Distance(from.transform.position, to.transform.position));

			if (leftMesh == null) {
				leftMesh = new("leftMesh", from.transform, new Vector3[4] {
					new Vector3(0f, HEIGHT, 0f),
					new Vector3(HALF_WIDTH, HEIGHT, 0f),
					new Vector3(0f, HEIGHT, 0f),
					new Vector3(HALF_WIDTH, HEIGHT, 0f)
				});
			}
			leftMesh.renderer.transform.LookAt(to.transform.position);
			leftMesh.vertices[2].z = length;
			leftMesh.vertices[3].z = length;
			leftMesh.mesh.vertices = leftMesh.vertices;

			if (rightMesh == null) {
				rightMesh = new("rightMesh", from.transform, new Vector3[4] {
					new Vector3(-HALF_WIDTH, HEIGHT, 0f),
					new Vector3(0f, HEIGHT, 0f),
					new Vector3(-HALF_WIDTH, HEIGHT, 0f),
					new Vector3(0f, HEIGHT, 0f)
				});
			}
			rightMesh.renderer.transform.LookAt(to.transform.position);
			rightMesh.vertices[2].z = length;
			rightMesh.vertices[3].z = length;
			rightMesh.mesh.vertices = rightMesh.vertices;
		}

		public void Destroy() {
			Object.Destroy(leftMesh.renderer.gameObject);
			Object.Destroy(rightMesh.renderer.gameObject);
		}

		private class MeshGroup {
			public MeshRenderer renderer;
			public Mesh mesh;
			public Vector3[] vertices;

			public MeshGroup(string name, Transform parent, Vector3[] vertices) {
				this.vertices = vertices;

				renderer = new GameObject(name).AddComponent<MeshRenderer>();
				renderer.transform.parent = parent;
				renderer.transform.localPosition = Vector3.zero;
				renderer.material = Resources.Load<Material>("Materials/Road");

				mesh = new() {
					vertices = this.vertices,
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

				MeshFilter meshFilter = renderer.gameObject.AddComponent<MeshFilter>();
				meshFilter.mesh = mesh;
			}
		}
	}*/
}
