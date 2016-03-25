﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ServerScript : NetworkBehaviour {

	[HideInInspector] public GameScript gameScript;
	[HideInInspector] public List<RespawnLocation> listRespawnLocations = new List<RespawnLocation> ();
	private float oldestTimeUsed;

	// Use this for initialization
	void Start () {
	
		createRespawnPoints(gameScript.clientScript.map.transform.FindChild ("Scenario/RespawnPoints").gameObject);

	}
	
	// Update is called once per frame
	void Update () {

			checkForKillings ();
			checkIfSendRankingData(); 

	}

	void createRespawnPoints(GameObject respawnPoints) {

		float now = Time.realtimeSinceStartup;
		oldestTimeUsed = now;

		foreach (Transform child in respawnPoints.transform)
		{
			RespawnLocation rL = new RespawnLocation (child.position, child.eulerAngles);
			rL.lastTimeUsed = now;
			listRespawnLocations.Add (rL);
		}

		Destroy (respawnPoints);

	}

	void checkForKillings() {

		for (int i = 0; i < gameScript.clientScript.listPlayers.Count; i++) {

			if (gameScript.clientScript.listPlayers[i].attacking > 0f) {
				
				List<GameObject> currentTriggering = gameScript.clientScript.listPlayers[i].visualAvatar.GetComponent<PlayerMarker> ().ownAttacker.listTriggering;

				for (int j = 0; j < currentTriggering.Count; j++) {

					ClientScript.Player currentPlayer = currentTriggering[j].GetComponent<PlayerMarker>().player;

					if (currentPlayer.immune == 0f) {

						gameScript.clientScript.listPlayers[i].kills++;
						respawn (currentPlayer.playerCode);
						currentPlayer.immune = 5f;
						sendRankingData ();

					}

				}
			}

		}


	}

	void respawn(string playerCode) {

		GetComponent<NetworkView> ().RPC ("updateRankingRPC", RPCMode.Others, playerCode, listRespawnLocations [0].position, listRespawnLocations [0].eulerAngles);

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

	// CLASSES
	public class RespawnLocation {

		public Vector3 position;
		public Vector3 eulerAngles;
		public float lastTimeUsed;

		public RespawnLocation(Vector3 position, Vector3 eulerAngles) {

			this.position = position;
			this.eulerAngles = eulerAngles;

		}

	}

}
