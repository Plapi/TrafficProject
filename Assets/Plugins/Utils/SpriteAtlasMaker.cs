#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEditor;
using UnityEditor.U2D;

public class SpriteAtlasMaker : MonoBehaviour {

	[SerializeField] private SpriteAtlas m_atlas = null;

	[ContextMenu("Update Atlas")]
	private void UpdateAtlas() {

		List<Sprite> allUsedSprites = new List<Sprite>();

		Utils.IterateAllChilds(transform, child => {
			if (child.TryGetComponent(out Image image)) {
				if (EditorUtility.IsPersistent(image.sprite) && !allUsedSprites.Contains(image.sprite)) {
					allUsedSprites.Add(image.sprite);
				}
			}
		});

		Debug.LogError($"Count:{allUsedSprites.Count}");

		SpriteAtlasExtensions.Add(m_atlas, allUsedSprites.ToArray());
	}
}
#endif
