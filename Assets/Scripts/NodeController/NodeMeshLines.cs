using System.Collections.Generic;
using UnityEngine;

public class NodeMeshLines : MonoBehaviour {

	private NodeMeshLineExpander[] firstLines = new NodeMeshLineExpander[0];
	private NodeMeshLineExpander[] secondLines = new NodeMeshLineExpander[0];
	private NodeMeshLineExpander[] thirdLines = new NodeMeshLineExpander[0];

	private readonly List<NodeMeshMiddleLines> middleLines = new();

	public void UpdateExpanderLines(Line[] lines, bool oSide = false) {
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
			Vector3[] edgePoints = firstLines[i].UpdateMesh(lines[i].points, lines[i].value, oSide);
			edgePoints = secondLines[i].UpdateMesh(edgePoints, lines[i].value, oSide);
			thirdLines[i].UpdateMesh(edgePoints, lines[i].value, oSide, true);
		}
	}

	public void UpdateMiddleLines(List<Vector3[]> listPoints) {
		for (int i = 0; i < listPoints.Count; i++) {
			GetMiddleLine(i).UpdateMiddleLines(listPoints[i], true);
		}
		DestroyUnusedMiddleLines(listPoints.Count);
	}

	public void UpdateMiddleLines(Vector3[] points) {
		GetMiddleLine(0).UpdateMiddleLines(points, false);
		DestroyUnusedMiddleLines(1);
	}

	public NodeMeshMiddleLines GetMiddleLine(int index) {
		for (int i = middleLines.Count; i <= index; i++) {
			middleLines.Add(null);
		}
		if (middleLines[index] == null) {
			middleLines[index] = new GameObject($"middleLine{index}").AddComponent<NodeMeshMiddleLines>();
			middleLines[index].transform.parent = transform;
			middleLines[index].transform.localPosition = Vector3.zero;
		}
		return middleLines[index];
	}

	private void DestroyUnusedMiddleLines(int index) {
		for (int i = index; i < middleLines.Count; i++) {
			if (middleLines[i] != null) {
				Destroy(middleLines[i].gameObject);
			}
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
		public float value = 0.25f;
	}
}
