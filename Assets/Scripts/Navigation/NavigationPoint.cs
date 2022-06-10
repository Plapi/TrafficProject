using System.Collections.Generic;
using UnityEngine;

public class NavigationPoint : BFSNode {

	private int index;
	private NavigationPoint[] nextNodes = new NavigationPoint[0];
	private readonly List<Vector3[]> curvePoints = new();

	public int Index { get => index; set => index = value; }
	public Vector3 Position;

	public bool GivesWay { get; private set; }
	public bool StopedbySemaphore { get; private set; }

	private readonly List<NavigationAgent> agents = new();

	public NavigationPoint(Vector3 position) {
		Position = position;
	}

	public BFSNode[] GetNextNodes() {
		return nextNodes;
	}

	public void ClearNextNodes() {
		nextNodes = new NavigationPoint[0];
	}

	public void AddNextNode(NavigationPoint nextPoint) {
		List<NavigationPoint> list = new(nextNodes);
		list.Add(nextPoint);
		nextNodes = list.ToArray();
		curvePoints.Add(null);
	}

	public void AddNextNodeWithCurvePoints(NavigationPoint nextPoint, Vector3 cPoint) {
		AddNextNode(nextPoint);
		curvePoints[^1] = new Vector3[4];
		for (int i = 1; i <= 4; i++) {
			curvePoints[^1][i - 1] = Bezier.GetPoint(Position, cPoint, nextPoint.Position, i * 0.2f);
		}
	}

	public bool TryGetCurvePoints(NavigationPoint node, out Vector3[] cPoints) {
		cPoints = null;
		for (int i = 0; i < nextNodes.Length; i++) {
			if (nextNodes[i] == node) {
				if (curvePoints[i] != null) {
					cPoints = curvePoints[i];
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public void AddAgent(NavigationAgent agent) {
		agents.Add(agent);
	}

	public void RemoveAgent(NavigationAgent agent) {
		agents.Remove(agent);
	}

	public List<NavigationAgent> GetAgents() {
		return agents;
	}

	public void UpdateGiveWay(bool giveWay) {
		GivesWay = giveWay;
	}

	public void UpdateStopedbySemaphore(bool stopedbySemaphore) {
		StopedbySemaphore = stopedbySemaphore;
	}
}
