using System;
using System.Collections.Generic;
using UnityEngine;

public class NavigationAgent : MonoBehaviour {

	[SerializeField] private float maxSpeed = default;
	[SerializeField] private float acceleration = default;
	[SerializeField] private float deceleration = default;
	[SerializeField] private float rotationSpeed = default;

	[SerializeField] private float frontSize = default;
	[SerializeField] private float backSize = default;
	[SerializeField] private float frontDistance = default;
	[SerializeField] private float halfWidth = default;

	private List<NavigationPoint> navPoints;
	private List<Vector3> points;
	private List<int> navPointsIndexes;

	public float currentSpeed;
	private int currentIndex;
	private Action onComplete;

	public NavigationPoint CurrentNavPoint => currentNavPoint;
	public NavigationPoint NextNavPoint => nextNavPoint;

	private NavigationPoint currentNavPoint;
	private NavigationPoint nextNavPoint;

	public Vector3 FrontPoint => frontPoint;
	public Vector3 FrontLeftPoint => frontLeftPoint;
	public Vector3 FrontRightPoint => frontRightPoint;

	public Vector3 BackLeftPoint => backLeftPoint;
	public Vector3 BackRightPoint => backRightPoint;

	public Vector3 FrontForwardPoint => frontForwardPoint;
	public Vector3 FrontForwardLeftPoint => frontForwardLeftPoint;
	public Vector3 FrontForwardRightPoint => frontForwardRightPoint;

	private Vector3 frontPoint;
	private Vector3 frontLeftPoint;
	private Vector3 frontRightPoint;

	private Vector3 backPoint;
	private Vector3 backLeftPoint;
	private Vector3 backRightPoint;

	private Vector3 frontForwardPoint;
	private Vector3 frontForwardLeftPoint;
	private Vector3 frontForwardRightPoint;

	public NavigationAgent BlockedByOtherAgent;

	public void Go(List<NavigationPoint> points, Action onComplete) {
		navPoints = points;
		this.onComplete = onComplete;

		currentNavPoint = points[0];
		nextNavPoint = points[1];
		nextNavPoint.AddAgent(this);

		transform.position = points[0].Position;
		transform.LookAt(points[1].Position);
		currentSpeed = 0f;
		currentIndex = 0;

		CalculatePoints();

		enabled = true;
	}

	public void Destroy() {
		currentNavPoint?.RemoveAgent(this);
		nextNavPoint?.RemoveAgent(this);
		Destroy(gameObject);
	}

	public bool ContainsPoint(Vector3 p) {
		if (Utils.PointInTriangle(p, backLeftPoint, frontForwardLeftPoint, frontForwardRightPoint)) {
			return true;
		}
		if (Utils.PointInTriangle(p, backLeftPoint, frontForwardRightPoint, backRightPoint)) {
			return true;
		}
		return false;
	}

	private void Update() {
		if (BlockedByOtherAgent == null) {
			currentSpeed += acceleration;
		} else {
			currentSpeed -= deceleration;
		}
		currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

		float distance = currentSpeed * Time.deltaTime;
		float distanceToNextPoint = Vector3.Distance(transform.position, points[currentIndex + 1]);
		while (distance > distanceToNextPoint) {
			distance -= distanceToNextPoint;
			currentIndex++;

			if (currentIndex == points.Count - 1) {
				enabled = false;
				onComplete?.Invoke();
				return;
			}

			transform.position = points[currentIndex];
			distanceToNextPoint = Vector3.Distance(transform.position, points[currentIndex + 1]);
		}

		Vector3 direction = (points[currentIndex + 1] - transform.position).normalized;
		transform.position += direction * distance;
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * currentSpeed * rotationSpeed);

		NavigationPoint cnv = navPoints[navPointsIndexes[currentIndex]];
		int nextIndex = navPointsIndexes[currentIndex + 1] == navPointsIndexes[currentIndex] ?
			(navPointsIndexes[currentIndex + 1] + 1) : navPointsIndexes[currentIndex + 1];
		NavigationPoint nnp = navPoints[nextIndex];
		if (cnv != currentNavPoint || nnp != nextNavPoint) {
			cnv.RemoveAgent(this);
			nnp.RemoveAgent(this);

			currentNavPoint = cnv;
			nextNavPoint = nnp;

			currentNavPoint.AddAgent(this);
			nextNavPoint.AddAgent(this);
		}
		Debug.DrawLine(transform.position, currentNavPoint.Position, Color.red);
		Debug.DrawLine(transform.position, nextNavPoint.Position, Color.green);

		CalculateBoundPoints();
	}

	private void CalculatePoints() {
		points = new() { navPoints[0].Position };
		navPointsIndexes = new() { 0 };
		for (int i = 0; i < navPoints.Count - 1; i++) {
			if (navPoints[i].TryGetCurvePoints(navPoints[i + 1], out Vector3[] curvePoints)) {
				points.AddRange(curvePoints);
				for (int j = 0; j < curvePoints.Length; j++) {
					navPointsIndexes.Add(i);
				}
			}
			points.Add(navPoints[i + 1].Position);
			navPointsIndexes.Add(i + 1);
		}
	}

	private void CalculateBoundPoints() {
		frontPoint = transform.position + transform.forward * frontSize;
		Utils.PerpendicularPoints(frontPoint, transform.position, out frontRightPoint, out frontLeftPoint, halfWidth);

		backPoint = transform.position - transform.forward * backSize;
		Utils.PerpendicularPoints(backPoint, transform.position, out backLeftPoint, out backRightPoint, halfWidth);

		frontForwardPoint = transform.position + transform.forward * (frontDistance + frontSize);
		Utils.PerpendicularPoints(frontForwardPoint, transform.position, out frontForwardRightPoint, out frontForwardLeftPoint, halfWidth);
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected() {
		if (!Application.isPlaying) {
			CalculateBoundPoints();
		}

		Gizmos.color = Color.green;
		Gizmos.DrawLine(frontLeftPoint, frontRightPoint);
		Gizmos.DrawLine(frontLeftPoint, frontForwardLeftPoint);
		Gizmos.DrawLine(frontRightPoint, frontForwardRightPoint);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(backLeftPoint, backRightPoint);
		Gizmos.DrawLine(backLeftPoint, frontLeftPoint);
		Gizmos.DrawLine(backRightPoint, frontRightPoint);

		Gizmos.color = Color.blue;
		Gizmos.DrawLine(frontForwardLeftPoint, frontForwardRightPoint);
	}
#endif
}
