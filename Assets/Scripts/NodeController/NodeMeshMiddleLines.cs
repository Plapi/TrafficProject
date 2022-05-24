using System.Collections.Generic;
using UnityEngine;

public class NodeMeshMiddleLines : MonoBehaviour {

	private const float DEFAULT_EMPTY_SPACE = 2;
	private const float FIRST_EMPTY_SPACE = DEFAULT_EMPTY_SPACE / 2f;
	private const float CUBE_LENGTH = 2;
	private const float CUBE_HALF_LENGTH = CUBE_LENGTH / 2f;

	private readonly List<GameObject> middleLines = new();

	private GameObject longLine;

	public void UpdateMiddleLines(Vector3[] points, bool hasLongLine) {

		float currentDist = 0f;
		float emptySpace = FIRST_EMPTY_SPACE;
		bool addEmpty = true;
		int cubeIndex = 0;

		for (int i = 0; i < points.Length - 1; i++) {
			currentDist += Vector3.Distance(points[i], points[i + 1]);
			Vector3 dir = (points[i] - points[i + 1]).normalized;

			while (addEmpty && currentDist > emptySpace || !addEmpty && currentDist > CUBE_LENGTH) {
				if (addEmpty) {
					currentDist -= emptySpace;
					emptySpace = DEFAULT_EMPTY_SPACE;
				} else {
					currentDist -= CUBE_HALF_LENGTH;
					GameObject line = GetMiddleLine(cubeIndex);
					line.transform.localPosition = points[i + 1] + dir * currentDist;
					line.transform.SetLocalY(-0.05f);
					line.transform.rotation = Quaternion.LookRotation(dir);
					currentDist -= CUBE_HALF_LENGTH;
					cubeIndex++;
				}
				addEmpty = !addEmpty;
			}
		}

		if (hasLongLine) {
			if (longLine == null) {
				longLine = new GameObject("LongLine");
				longLine.transform.parent = transform;

				GameObject child0 = GameObject.CreatePrimitive(PrimitiveType.Cube);
				if (child0.TryGetComponent(out BoxCollider boxCollider)) {
					Destroy(boxCollider);
				}
				child0.transform.parent = longLine.transform;
				child0.transform.localScale = new Vector3(Config.Instance.RoadHalfWidth + 0.25f, 0.2f, 0.5f);
				Vector3 dir = Config.Instance.RightDriving ? Vector3.right : Vector3.left;
				child0.transform.localPosition = dir * (Config.Instance.RoadHalfWidth - 0.25f) / 2f;

				GameObject child1 = InstantiateMiddleLine();
				child1.transform.parent = longLine.transform;
				child1.transform.localPosition = new Vector3(0f, 0f, -1.25f);
			}
			longLine.transform.localPosition = points[^1];
			longLine.transform.SetLocalY(-0.05f);
			longLine.transform.LookAt(transform.position);
		} else {
			if (longLine != null) {
				Destroy(longLine);
			}
		}

		DeactivateUnusedMidleLines(cubeIndex);
	}

	private GameObject GetMiddleLine(int index) {
		for (int i = middleLines.Count; i <= index; i++) {
			GameObject line = InstantiateMiddleLine();
			line.transform.parent = transform;
			line.SetActive(false);
			middleLines.Add(line);
		}
		middleLines[index].SetActive(true);
		return middleLines[index];
	}

	private GameObject InstantiateMiddleLine() {
		return Instantiate(Resources.Load<GameObject>("RoadMiddleLine"));
	}

	private void DeactivateUnusedMidleLines(int from) {
		for (int i = from; i < middleLines.Count; i++) {
			middleLines[i].SetActive(false);
		}
	}
}
