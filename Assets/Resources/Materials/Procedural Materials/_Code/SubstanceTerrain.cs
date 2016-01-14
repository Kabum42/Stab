using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class SubstanceTerrain : MonoBehaviour
{
	private Terrain terrain;
	private TerrainData terrainData;
	public ProceduralMaterial[] substances = new ProceduralMaterial[0];
	
	private const string MAIN_TEXTURE = "_MainTex";
	private const string NORMAL_TEXTURE = "_BumpMap";
	
	private void Awake()
	{
		UpdateSplats();
	}
	
	public void UpdateSplats()
	{
		if (terrainData == null)
		{
			terrain = gameObject.GetComponent<Terrain>();
			terrainData = terrain.terrainData;
		}
		SplatPrototype[] splatmaps = terrainData.splatPrototypes;
		int count = Mathf.Min(substances.Length, splatmaps.Length);
		for (int i = 0; i < count; i++)
		{
			ProceduralMaterial substance = substances[i];
			if (substance == null)
			{
				continue;
			}
			SplatPrototype splatmap = splatmaps[i];
			
			if (!substance.isReadable)
			{
				substance.isReadable = true;
				substance.RebuildTexturesImmediately();
			}
			
			ProceduralTexture baseProceduralMap = (ProceduralTexture)substance.GetTexture(MAIN_TEXTURE);
			ProceduralTexture normalProceduralMap = (ProceduralTexture)substance.GetTexture(NORMAL_TEXTURE);
			
			float smoothness = substance.GetFloat("_Glossiness");
			float metallic = 0.0f;
			Color specColor = Color.white;
			if (substance.HasProperty("_Metallic"))
			{
				metallic = substance.GetFloat("_Metallic");
			}
			if (substance.HasProperty("_SpecColor"))
			{
				specColor = substance.GetColor("_SpecColor");
			}
			
			Texture2D baseMap = new Texture2D(baseProceduralMap.width, baseProceduralMap.height);
			baseMap.SetPixels32(baseProceduralMap.GetPixels32(0, 0, baseProceduralMap.width, baseProceduralMap.height));
			baseMap.Apply();
			Texture2D normalMap = new Texture2D(normalProceduralMap.width, normalProceduralMap.height);
			normalMap.SetPixels32(normalProceduralMap.GetPixels32(0, 0, normalProceduralMap.width, normalProceduralMap.height));
			normalMap.Apply();
			
			splatmap.texture = baseMap;
			splatmap.normalMap = normalMap;
			splatmap.tileOffset = substance.GetTextureOffset(MAIN_TEXTURE);
			splatmap.tileSize = substance.GetTextureScale(MAIN_TEXTURE);
			
			splatmap.specular = specColor;
			splatmap.smoothness = smoothness;
			splatmap.metallic = metallic;
			
			splatmaps[i] = splatmap;
		}
		
		terrainData.splatPrototypes = splatmaps;
		terrain.terrainData = terrainData;
		terrain.Flush();
	}
}