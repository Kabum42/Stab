using UnityEngine;
using System.Collections;

public class LookAtCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
		this.transform.LookAt (Camera.main.gameObject.transform);
	}

}
