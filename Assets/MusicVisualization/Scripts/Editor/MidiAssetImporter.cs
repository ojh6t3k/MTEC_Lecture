using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class MidiAssetImporter : AssetPostprocessor
{
	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
	{
		foreach(string asset in importedAssets)
		{
			string extension = Path.GetExtension(asset);
			if(extension.Equals(".mid") == true)
			{
				// Create Midi Asset
				MidiAsset createdAsset = ScriptableObject.CreateInstance<MidiAsset>();

				string newFileName = Path.ChangeExtension(asset, ".asset");
				// Load Midi data
				createdAsset.FileLoad(asset);

				AssetDatabase.CreateAsset(createdAsset, newFileName);
				AssetDatabase.SaveAssets();
			}
		}
	}
}
