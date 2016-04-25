using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMarker : MonoBehaviour {

	[HideInInspector] public ClientScript.Player player;

	public static PlayerMarker Traverse(GameObject g) {

		if (g.GetComponent<PlayerMarker> () != null) {
			return g.GetComponent<PlayerMarker> ();
		} else if (g.transform.parent != null) {
			return Traverse (g.transform.parent.gameObject);
		} else {
			return null;
		}

	}

}


