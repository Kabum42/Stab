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

		bool ownPlayerDead = false;
		List<ClientScript.Player> playersDead = new List<ClientScript.Player> ();

		for (int i = 0; i < gameScript.clientScript.listPlayers.Count; i++) {

			if (gameScript.clientScript.listPlayers[i].attacking > 0f) {

				List<GameObject> currentTriggering = gameScript.clientScript.listPlayers[i].visualAvatar.GetComponent<PlayerMarker> ().ownAttacker.listTriggering;

				for (int j = 0; j < currentTriggering.Count; j++) {

					ClientScript.Player currentPlayer = currentTriggering[j].GetComponent<PlayerMarker>().player;

					if (currentPlayer.immune == 0f) {

						RankingPlayerByCode(gameScript.clientScript.listPlayers[i].playerCode).kills++;
						currentPlayer.immune = 5f;
						//playersDead.Add (currentPlayer);

					}

				}
			}

		}


	}

	void checkIfSendRankingData() {

		// SE VUELVE A COMPROBAR POR SI ACASO

			if (currentRankingCooldown >= 1f) {
				serverSendRankingData ();
			} else {
				currentRankingCooldown += Time.deltaTime;
			}


	}

	void serverSendRankingData() {

		currentRankingCooldown = 0f;

		GetComponent<NetworkView>().RPC("clearRankingRPC", RPCMode.Others);

		for (int i = 0; i < allRankingPlayers.Count; i++) {
			allRankingPlayers[i].ping = Network.GetAveragePing(NetworkPlayerByCode(allRankingPlayers[i].playerCode));
			if (allRankingPlayers[i].ping == -1) { allRankingPlayers[i].ping = 0; }
			GetComponent<NetworkView>().RPC("addRankingRPC", RPCMode.Others, allRankingPlayers[i].playerCode, allRankingPlayers[i].kills, allRankingPlayers[i].ping);
		}

	}


	RankingPlayer RankingPlayerByCode(string playerCode)
	{

		for (int i = 0; i < allRankingPlayers.Count; i++)
		{
			if (allRankingPlayers[i].playerCode == playerCode)
			{
				return allRankingPlayers[i];
			}
		}

		return null;

	}

	// NETWORK RELATED
	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log("Clean up after player " + player);
		Network.RemoveRPCs(player);
		//Network.DestroyPlayerObjects(player);
		GetComponent<NetworkView>().RPC("removePlayerRPC", RPCMode.All, player.ToString());
	}

	// SERVER RPCs
	[RPC]
	void sendRemainingSecondsRPC(string playerCode, float auxRemainingSeconds)
	{
		if (playerCode == Network.player.ToString()) {
			gameScript.clientScript.remainingSeconds = auxRemainingSeconds;
		}
	}

	[RPC]
	void removePlayerRPC(string playerCode) {

		for (int i = 0; i < gameScript.clientScript.listPlayers.Count; i++) {

			if (gameScript.clientScript.listPlayers[i].playerCode == playerCode) {

				Destroy(gameScript.clientScript.listPlayers[i].visualAvatar);
				gameScript.clientScript.listPlayers.RemoveAt(i);

				break;
			}

		}

		for (int i = 0; i < allRankingPlayers.Count; i++) {

			if (allRankingPlayers[i].playerCode == playerCode) {

				allRankingPlayers.RemoveAt(i);
				serverSendRankingData();

			}

		}

	}

}
