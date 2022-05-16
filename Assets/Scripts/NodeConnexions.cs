using System.Collections.Generic;
using UnityEngine;

public class NodeConnexions : MonoBehaviour {

	private readonly Dictionary<string, Connexion> connexions = new();

	public void AddConnexion(Node from, Node to) {
		connexions.Add(GetConnexionId(from, to), new(from, to, transform));
	}

	public void RemoveConnexion(Node from, Node to) {
		string id = GetConnexionId(from, to);
		connexions[id].Destroy();
		connexions.Remove(id);
	}

	public bool HasConnexions(Node node) {
		foreach (var c in connexions) {
			if (c.Value.from == node || c.Value.to == node) {
				return true;
			}
		}
		return false;
	}

	public void UpdateConnexionsMesh(Node from, Node to) {
		connexions[GetConnexionId(from, to)].UpdateMesh();
	}

	private string GetConnexionId(Node from, Node to) {
		return $"{from.name}_{to.name}";
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		foreach (var c in connexions) {
			Gizmos.DrawLine(c.Value.from.transform.position, c.Value.to.transform.position);
		}
	}

	private class Connexion {

		public Node from;
		public Node to;

		private readonly MeshRenderer renderer;
		private readonly MeshFilter filter;
		private readonly Vector3[] vertices = new Vector3[4];

		public Connexion(Node from, Node to, Transform parent) {
			this.from = from;
			this.to = to;

			renderer = new GameObject($"mesh_{from.name}_{to.name}").AddComponent<MeshRenderer>();
			renderer.transform.parent = parent;
			renderer.material = Resources.Load<Material>("Materials/Road");

			filter = renderer.gameObject.AddComponent<MeshFilter>();
			filter.mesh = new() {
				vertices = vertices,
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

		public void UpdateMesh() {
			vertices[0] = from.RightPoint.position;
			vertices[1] = from.LeftPoint.position;
			vertices[2] = to.RightPoint.position;
			vertices[3] = to.LeftPoint.position;
			filter.mesh.vertices = vertices;
		}

		public void Destroy() {
			Object.Destroy(renderer.gameObject);
		}
	}
}
