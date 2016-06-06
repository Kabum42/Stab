using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HackCapsuleScript : MonoBehaviour {


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
				//listTriggering.Add (other.gameObject);
			}
			else {
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
				// CHECKING PLAYER LIMBS BY DISTANCE
				ClientScript.Player auxPlayer = getPlayerFromGameObject(listTriggering[i]);

				Vector3 originRaycast = GlobalData.clientScript.localPlayer.personalCamera.transform.position;
				Vector3 endRaycast = listTriggering [i].transform.position;

				Vector3 directionRaycast = (endRaycast - originRaycast).normalized;
				RaycastHit hitInfo;
				float maxDistance = Vector3.Distance(originRaycast, endRaycast);
				Physics.Raycast(originRaycast, directionRaycast, out hitInfo, maxDistance);

				if (hitInfo.collider != null && hitInfo.collider.gameObject != null) {
					// IT COLLIDED WITH SOMETHING
					ClientScript.Player auxPlayer2 = getPlayerFromGameObject(hitInfo.collider.gameObject);

					if (auxPlayer == auxPlayer2) {
						if (!auxPlayer.dead && auxPlayer.hackingNetworkPlayer != GlobalData.clientScript.myPlayer.networkPlayer) {
							return auxPlayer;
						}
					} else {
						//return null;
					}

				}

			}

		}
		return null;

	}


}