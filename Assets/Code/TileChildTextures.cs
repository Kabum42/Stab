using UnityEngine;
using System.Collections;

public class TileChildTextures : MonoBehaviour {

	// Use this for initialization
	void Start () {

		foreach (Transform child in transform) {

			child.GetComponent<Renderer> ().material.mainTextureScale = new Vector2 (child.localScale.x*child.GetComponent<Renderer> ().material.mainTextureScale.x, child.localScale.y*child.GetComponent<Renderer> ().material.mainTextureScale.y);

		}
	
	}

}
