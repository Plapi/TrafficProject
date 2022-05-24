using UnityEngine;
using System.Collections;

public class MatrixBlender : MonoBehaviour {

	[SerializeField] private Camera cam = default;

	public static Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float time) {
		Matrix4x4 ret = new();
		for (int i = 0; i < 16; i++) {
			ret[i] = Mathf.Lerp(from[i], to[i], time);
		}
		return ret;
	}

	private IEnumerator LerpFromTo(Matrix4x4 src, Matrix4x4 dest, float duration) {
		float startTime = Time.time;
		while (Time.time - startTime < duration) {
			cam.projectionMatrix = MatrixLerp(src, dest, (Time.time - startTime) / duration);
			yield return 1;
		}
		cam.projectionMatrix = dest;
	}

	public Coroutine BlendToMatrix(Matrix4x4 targetMatrix, float duration) {
		StopAllCoroutines();
		return StartCoroutine(LerpFromTo(cam.projectionMatrix, targetMatrix, duration));
	}
}