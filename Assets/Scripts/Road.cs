using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Road : MonoBehaviour {

	private const float WIDTH = 2.5f;
	private const float HALF_WIDTH = WIDTH / 2f;
	private const float HEIGHT = 0.1f;

	private Mesh mesh;

	private Vector3[] vertices = new Vector3[2] {
		new Vector3(-HALF_WIDTH, HEIGHT, 0),
		new Vector3(HALF_WIDTH, HEIGHT, 0)
	};

	public void Init(Vector3 firstPoint, Material material) {
		transform.position = firstPoint;

		mesh = new();
		vertices = new Vector3[2] {
			new Vector3(-HALF_WIDTH, HEIGHT, 0),
			new Vector3(HALF_WIDTH, HEIGHT, 0)
		};
		AddPoint();

		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = material;
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;
	}

	public void AddPoint() {
		Vector3[] newVertices = new Vector3[vertices.Length + 2];
		for (int i = 0; i < vertices.Length; i++) {
			newVertices[i] = vertices[i];
		}
		newVertices[^2] = new Vector3(-HALF_WIDTH, HEIGHT, 0);
		newVertices[^1] = new Vector3(HALF_WIDTH, HEIGHT, 0);

		int[] triangles = new int[newVertices.Length / 2 * 3];
		for (int i = 0; i < triangles.Length; i += 6) {
			triangles[i] = i;
			triangles[i + 1] = i + 2;
			triangles[i + 2] = i + 1;
			triangles[i + 3] = i + 2;
			triangles[i + 4] = i + 3;
			triangles[i + 5] = i + 1;
		}

		Vector3[] normals = new Vector3[newVertices.Length];
		for (int i = 0; i < normals.Length; i++) {
			normals[i] = Vector3.up;
		}

		vertices = newVertices;
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
	}

	public void RemovePoint() {

	}

	public void UpdateLastPoint(Vector3 point) {
		float dist = Mathf.Max(1f, Vector3.Distance(transform.position, point));
		vertices[^2].z = dist;
		vertices[^1].z = dist;
		mesh.vertices = vertices;
	}

	/*public void SetSecondPoint(Vector3 point) {
		//globalPoints[1] = point;
		transform.LookAt(point);

		float dist = Mathf.Max(1f, Vector3.Distance(transform.position, point));
		vertices[2].z = dist;
		vertices[3].z = dist;
		mesh.vertices = vertices;
	}*/

	public void MoveCar(Transform car) {

		/*Vector3 firstPoint = globalPoints[0] + Vector3.up * HEIGHT;
		Vector3 secondPoint = globalPoints[1] + Vector3.up * HEIGHT;

		car.gameObject.SetActive(true);
		car.transform.position = firstPoint;
		car.transform.LookAt(secondPoint);
		car.DOMove(secondPoint, vertices[2].z / 10f).SetEase(Ease.Linear).OnComplete(() => {
			MoveCar(car);
		});*/
	}
}
