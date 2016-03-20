using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerScript : MonoBehaviour {

	[HideInInspector] public GameScript gameScript;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

			checkForKillings ();
			checkIfSendRankingData(); 

	}

	void checkForKillings() {

		for (int i = 0; i < gameScript.clientScript.listPlayers.Count; i++) {

			if (gameScript.clientScript.listPlayers[i].attacking > 0f) {

				List<GameObject> currentTriggering = gameScript.clientScript.listPlayers[i].visualAvatar.GetComponent<PlayerMarker> ().ownAttacker.listTriggering;

				for (int j = 0; j < currentTriggering.Count; j++) {

					ClientScript.Player currentPlayer = currentTriggering[j].GetComponent<PlayerMarker>().player;

					if (currentPlayer.immune == 0f) {

						gameScript.clientScript.listPlayers[i].kills++;
						currentPlayer.immune = 5f;
						sendRankingData ();

					}

				}
			}

		}


	}

	void checkIfSendRankingData() {

		if (gameScript.clientScript.currentRankingCooldown >= 1f) {
				sendRankingData ();
			} else {
				gameScript.clientScript.currentRankingCooldown += Time.deltaTime;
			}

	}

	public void sendRankingData() {

		gameScript.clientScript.currentRankingCooldown = 0f;

		gameScript.clientScript.listPlayers.Sort (CompareListByKills);

		for (int i = 0; i < gameScript.clientScript.listPlayers.Count; i++) {
			gameScript.clientScript.listPlayers[i].ping = Network.GetAveragePing(gameScript.clientScript.NetworkPlayerByCode(gameScript.clientScript.listPlayers[i].playerCode));
			if (gameScript.clientScript.listPlayers[i].ping < 0) { gameScript.clientScript.listPlayers[i].ping = 0; }
			GetComponent<NetworkView>().RPC("updateRankingRPC", RPCMode.Others, gameScript.clientScript.listPlayers[i].playerCode, gameScript.clientScript.listPlayers[i].kills, gameScript.clientScript.listPlayers[i].ping);
		}

	}

	private static int CompareListByKills(ClientScript.Player p1, ClientScript.Player p2)
	{
		return p1.kills.CompareTo(p2.kills); 
	}

	// NETWORK RELATED
	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log("Clean up after player " + player);
		Network.RemoveRPCs(player);
		GetComponent<NetworkView>().RPC("removePlayerRPC", RPCMode.All, player.ToString());
	}

}
