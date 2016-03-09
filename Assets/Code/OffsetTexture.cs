using UnityEngine;
using System.Collections;

public class OffsetTexture : MonoBehaviour {

    private Material mat;
    public Vector2 speedOffset;

	// Use this for initialization
	void Start () {
        mat = this.GetComponent<SkinnedMeshRenderer>().material;
	}
	
	// Update is called once per frame
	void Update () {

        Vector2 newOffset = new Vector2(mat.mainTextureOffset.x + (speedOffset.x * Time.deltaTime), mat.mainTextureOffset.y + (speedOffset.y * Time.deltaTime));
        mat.mainTextureOffset = newOffset;
	
	}
}
