using UnityEngine;

public class NodeMeshLines : MonoBehaviour {

	private NodeMeshLineExpander[] firstLines = new NodeMeshLineExpander[0];
	private NodeMeshLineExpander[] secondLines = new NodeMeshLineExpander[0];
	private NodeMeshLineExpander[] thirdLines = new NodeMeshLineExpander[0];

	public void UpdateMesh(Line[] lines) {
		if (firstLines.Length != lines.Length) {
			for (int i = 0; i < firstLines.Length; i++) {
				Destroy(firstLines[i].gameObject);
				Destroy(secondLines[i].gameObject);
				Destroy(thirdLines[i].gameObject);
			}
			firstLines = new NodeMeshLineExpander[lines.Length];
			secondLines = new NodeMeshLineExpander[lines.Length];
			thirdLines = new NodeMeshLineExpander[lines.Length];
			for (int i = 0; i < firstLines.Length; i++) {
				firstLines[i] = NewExpander($"first{i}");
				secondLines[i] = NewExpander($"second{i}");
				thirdLines[i] = NewExpander($"third{i}");
				firstLines[i].SetColor(ColorPalette.Get(ColorId.RoadSideMark));
				secondLines[i].SetColor(ColorPalette.Get(ColorId.Road));
				thirdLines[i].SetColor(ColorPalette.Get(ColorId.RoadSideMark));
			}
		}
		for (int i = 0; i < lines.Length; i++) {
			Vector3[] edgePoints = firstLines[i].UpdateMesh(lines[i].points);
			edgePoints = secondLines[i].UpdateMesh(edgePoints);
			thirdLines[i].UpdateMesh(edgePoints, true);
		}
	}

	private NodeMeshLineExpander NewExpander(string name) {
		NodeMeshLineExpander expander = new GameObject(name).AddComponent<NodeMeshLineExpander>();
		expander.transform.parent = transform;
		expander.transform.localPosition = Vector3.zero;
		return expander;
	}

	public class Line {
		public Vector3[] points;
	}
}
