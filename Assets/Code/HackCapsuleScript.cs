using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HackCapsuleScript : MonoBehaviour {

	/*
	public List<GameObject> listTriggering = new List<GameObject>();

	void Update() {

		List<GameObject> toErase = new List<GameObject> ();

		for (int i = 0; i < listTriggering.Count; i++) {
			if (listTriggering[i] == null) {
				// OBJECT NO LONGER EXISTS
				toErase.Add(listTriggering[i]);
			}
		}

		for (int i = 0; i < toErase.Count; i++) {
			listTriggering.Remove (toErase [i]);
		}

	}

	void OnTriggerEnter(Collider other) {

		if (GlobalData.clientScript != null && !listTriggering.Contains (other.gameObject)) {
			ClientScript.Player player = getPlayerFromGameObject (other.gameObject);
			if (player == null) {
				listTriggering.Add (other.gameObject);
			}
			else if (player != null && player != GlobalData.clientScript.myPlayer) {
				listTriggering.Add(other.gameObject);
			}
		}

	}

	void OnTriggerExit(Collider other) {

		if (GlobalData.clientScript != null) {
			if (listTriggering.Contains (other.gameObject)) {
				listTriggering.Remove (other.gameObject);
			}
		}

	}

	ClientScript.Player getPlayerFromGameObject (GameObject g) {

		PlayerMarker pM = PlayerMarker.Traverse (g);
		if (pM != null) {
			return pM.player;
		}
		return null;

	}

	public ClientScript.Player firstLookingPlayer() {

		if (listTriggering.Count > 0) {
			
			listTriggering = listTriggering.OrderBy(o=>Vector3.Distance(GlobalData.clientScript.localPlayer.personalCamera.transform.position, o.transform.position)).ToList();
			for (int i = 0; i < listTriggering.Count; i++) {
				ClientScript.Player auxPlayer = getPlayerFromGameObject(listTriggering[i]);
				if (auxPlayer != null) {
					if (!auxPlayer.dead) {
						return auxPlayer;
					}
				} else {
					return null;
				}
			}

		}
		return null;

	}
	*/

}