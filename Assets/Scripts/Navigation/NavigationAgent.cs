using System;
using System.Collections.Generic;
using UnityEngine;

public class NavigationAgent : MonoBehaviour {

	[SerializeField] private float maxSpeed = default;
	[SerializeField] private float acceleration = default;
	[SerializeField] private float rotationSpeed = default;
	[SerializeField] private float frontSize = default;
	[SerializeField] private float backSize = default;
	[SerializeField] private float keepDistance = default;

	private List<NavigationPoint> navPoints;
	private List<Vector3> points;

	public float currentSpeed;
	private int currentIndex;
	private Action onComplete;

	public void Go(List<NavigationPoint> points, Action onComplete) {
		navPoints = points;
		this.onComplete = onComplete;

		transform.position = points[0].Position;
		transform.LookAt(points[1].Position);
		currentSpeed = 0f;
		currentIndex = 0;

		CalculatePoints();

		enabled = true;
	}

	private void Update() {
		currentSpeed += acceleration;
		currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

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
	}

	private void CalculatePoints() {
		points = new() {
			navPoints[0].Position
		};
		for (int i = 0; i < navPoints.Count - 1; i++) {
			if (navPoints[i].TryGetCurvePoints(navPoints[i + 1], out Vector3[] curvePoints)) {
				points.AddRange(curvePoints);
			}
			points.Add(navPoints[i + 1].Position);
		}
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position, transform.position + transform.forward * frontSize);
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, transform.position + transform.forward * -backSize);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position + transform.forward * frontSize, transform.position + transform.forward * (frontSize + keepDistance));
	}
}
