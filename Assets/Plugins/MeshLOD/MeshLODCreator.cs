#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using static MeshLODConfig;

[ExecuteInEditMode]
public class MeshLODCreator : MonoBehaviour {

	[SerializeField] private Transform[] m_childs = null;
	[SerializeField] private ConfigWithScriptableObject[] m_configsWSO = null;

	[Space(20f)]
	[SerializeField] private UnityEngine.Object m_folder = null;
	[SerializeField] private string m_name = null;

	private void Awake() {
		if (m_childs == null) {
			m_childs = new Transform[] { transform };
		}
		if (m_configsWSO == null) {
			m_configsWSO = new ConfigWithScriptableObject[] {
				new ConfigWithScriptableObject {
					configs = GetDefaultsConfigs()
				}
			};
		}
	}

	private void Update() {
		for (int i = 0; i < m_configsWSO.Length; i++) {
			if (m_configsWSO[i].scriptableObjectConfig != null) {
				m_configsWSO[i].configs = m_configsWSO[i].scriptableObjectConfig.configs;
			}
		}
	}

	[ContextMenu("Create")]
	public LODGroup[] Create() {
		LODGroup[] lodGroups = new LODGroup[m_childs.Length];
		for (int i = 0; i < lodGroups.Length; i++) {
			lodGroups[i] = new GameObject("lodObj").gameObject.AddComponent<LODGroup>();
			LOD[] lods = new LOD[m_configsWSO[i].configs.Length];

			for (int j = 0; j < m_configsWSO[i].configs.Length; j++) {
				Renderer rend = MeshUtils.OptimizeAndCombine(m_childs[i], m_configsWSO[i].configs[j].quality * 100f).GetComponent<MeshRenderer>();
				rend.transform.parent = lodGroups[i].transform;
				lods[j] = new LOD(m_configsWSO[i].configs[j].screenRelativeTransitionHeight, new Renderer[] { rend });
			}

			lodGroups[i].SetLODs(lods);
		}

		return lodGroups;
	}

	public static LODGroup Create(GameObject obj, Config[] configs) {
		LODGroup lodGroup = new GameObject("lodObj").gameObject.AddComponent<LODGroup>();
		LOD[] lods = new LOD[configs.Length];
		for (int i = 0; i < lods.Length; i++) {
			Renderer rend = MeshUtils.OptimizeAndCombine(obj.transform, configs[i].quality * 100f).GetComponent<MeshRenderer>();
			rend.transform.parent = lodGroup.transform;
			lods[i] = new LOD(configs[i].screenRelativeTransitionHeight, new Renderer[] { rend });
		}
		lodGroup.SetLODs(lods);
		return lodGroup;
	}

	[ContextMenu("Create and Save")]
	private void CreateAndSave() {
		if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m_folder, out string guid, out long _)) {
			string folderPath = AssetDatabase.GUIDToAssetPath(guid);

			LODGroup lodGroup = Create()[0];
			LOD[] lods = lodGroup.GetLODs();

			for (int i = 0; i < lods.Length; i++) {
				Renderer[] rends = lods[i].renderers;
				for (int k = 0; k < rends.Length; k++) {
					MeshFilter meshFilter = rends[k].GetComponent<MeshFilter>();
					string meshPath = $"{folderPath}/mesh{i}{k}.mesh";
					AssetDatabase.CreateAsset(meshFilter.sharedMesh, meshPath);
					meshFilter.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh));
				}
			}

			GameObject obj = lodGroup.gameObject;
			PrefabUtility.SaveAsPrefabAsset(lodGroup.gameObject, $"{folderPath}/{m_name}.prefab").GetComponent<LODGroup>();
			DestroyImmediate(obj);

			AssetDatabase.Refresh();
		}
	}

	[Serializable]
	public class ConfigWithScriptableObject {
		public MeshLODConfig scriptableObjectConfig;
		public Config[] configs;
	}
}
#endif
