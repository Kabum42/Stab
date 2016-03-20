using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackMarker : MonoBehaviour {
	
	public PlayerMarker ownPlayer;
	public List<GameObject> listTriggering = new List<GameObject>();

	void OnTriggerEnter(Collider other) {

		if (other.gameObject != ownPlayer.gameObject && other.gameObject.GetComponent<PlayerMarker> () != null) {
			listTriggering.Add(other.gameObject);
		}

	}

	void OnTriggerExit(Collider other) {

		if (listTriggering.Contains (other.gameObject)) {
			listTriggering.Remove (other.gameObject);
		}

	}
}
