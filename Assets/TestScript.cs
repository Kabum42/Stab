using UnityEngine;
using System.Collections;

public class TestScript : MonoBehaviour {

	// Use this for initialization
	void Start () {

		this.GetComponent<Projector> ().material.color = new Color (0f, 0f, 0f, 0f);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
