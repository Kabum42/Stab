using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SubstanceTerrain))]
public class ObjectBuilderEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		SubstanceTerrain terrainScript = (SubstanceTerrain)target;
		if (GUILayout.Button("Update Terrain Splats"))
		{
			terrainScript.UpdateSplats();
		}
	}
}