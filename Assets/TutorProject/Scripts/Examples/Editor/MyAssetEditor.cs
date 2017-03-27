using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

[CustomEditor(typeof(MyAsset))]
public class MyAssetEditor : Editor
{
	void OnEnable()
	{
	}

	public override void OnInspectorGUI()
	{
		MyAsset myAsset = (MyAsset)target;

		myAsset.intVar = EditorGUILayout.IntField("IntVar", myAsset.intVar);
		myAsset.floatVar = EditorGUILayout.FloatField("FloatVar", myAsset.floatVar);

		if(GUILayout.Button("Apply") == true)
		{
			EditorUtility.SetDirty(target);
			AssetDatabase.SaveAssets();
		}

		if(GUILayout.Button("Revert") == true)
		{
			myAsset.Revert();
			EditorUtility.SetDirty(target);
			AssetDatabase.SaveAssets();
		}
	}

	[MenuItem("Assets/Create/MyAsset")]
	public static void CreateMyAsset()
	{
		MyAsset asset = CreateInstance<MyAsset>();

		string path = "Assets";
		foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
		{
			path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
			if(!string.IsNullOrEmpty(path) && File.Exists(path)) 
			{
				path = Path.GetDirectoryName(path);
				break;
			}
		}
		path = Path.Combine(path, "MyAsset.asset");
		Debug.Log(path);

		AssetDatabase.CreateAsset(asset, path);
		AssetDatabase.Refresh();
	}
}
