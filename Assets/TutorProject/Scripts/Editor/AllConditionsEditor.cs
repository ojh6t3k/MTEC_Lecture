using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AllConditions))]
public class TutorAllConditionsEditor : Editor
{
	private const string creationPath = "Assets/Resources/AllConditions.asset";

	[MenuItem("Assets/Create/AllConditions")]
	private static void CreateTutorAllConditionsAsset()
	{
		if(AllConditions.Instance)
			return;

		AllConditions instance = CreateInstance<AllConditions>();
		AssetDatabase.CreateAsset(instance, creationPath);
		AssetDatabase.Refresh();

		AllConditions.Instance = instance;

		instance.conditions = new Condition[0];
	}
}
