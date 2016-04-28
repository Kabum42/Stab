using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ServerScript : MonoBehaviour {

	[HideInInspector] public ClientScript clientScript;
	[HideInInspector] public List<RespawnLocation> listRespawnLocations = new List<RespawnLocation> ();
	private float oldestTimeUsed;

	public float currentRankingCooldown = 0f;
	public float currentHackDataCooldown = 0f;
	public static int hackDataUpdatesPerSecond = 15;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

		checkForSuicides ();
		checkIfSendDataToClients(); 

	}

	public void initialize(ClientScript auxClientScript) {

		clientScript = auxClientScript;

		GameObject respawnPoints = clientScript.map.transform.FindChild ("RespawnPoints").gameObject;
		createRespawnPoints (respawnPoints);

		respawn (clientScript.myCode);

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

	void checkForSuicides() {

		foreach (ClientScript.Player player in clientScript.listPlayers) {

			// THIS CHECKS IF SOMEONE IS FALLING INTO THE ETERNAL VOID OF THE BUGSPHERE
			if (player.targetPosition.y < -100f) {
				respawn (player.playerCode);
			}

		}
			
	}

	public void respawn(int playerCode) {

        List<int> possibleRespawns = new List<int>();
        float cooldown = 5f;
        bool cooldownFulfilled = (Time.realtimeSinceStartup - oldestTimeUsed) >= cooldown;

        for (int i = 0; i < listRespawnLocations.Count; i++)
        {
            if (cooldownFulfilled)
            {
                // SOMEONE FULFILLS THE COOLDOWN
                if ((Time.realtimeSinceStartup - listRespawnLocations[i].lastTimeUsed) >= cooldown)
                {
                    // THIS ONE FULLFILS THE COOLDOWN
                    possibleRespawns.Add(i);
                }
            }
            else
            {
                // NO ONE FULFILLS THE COOLDOWN, EVERYONE IS EQUALLY VALID
                possibleRespawns.Add(i);
            }
        }

        int chosenRespawn = possibleRespawns[UnityEngine.Random.Range(0, possibleRespawns.Count)];
        listRespawnLocations[chosenRespawn].lastTimeUsed = Time.realtimeSinceStartup;

		GetComponent<NetworkView> ().RPC ("respawnRPC", RPCMode.All, playerCode, listRespawnLocations [chosenRespawn].position, listRespawnLocations [chosenRespawn].eulerAngles);

        // UPDATE NEW OLDEST_TIME
        oldestTimeUsed = listRespawnLocations[0].lastTimeUsed;

        for (int i = 0; i < listRespawnLocations.Count; i++)
        {
            oldestTimeUsed = Mathf.Min(oldestTimeUsed, listRespawnLocations[i].lastTimeUsed);
        }

	}

	void checkIfSendDataToClients() {

		if (currentRankingCooldown >= 1f) {
			sendRankingData ();
		} else {
			currentRankingCooldown += Time.deltaTime;
		}


		if (currentHackDataCooldown >= 1f / (float)hackDataUpdatesPerSecond) {
			sendHackData ();
		} else {
			currentHackDataCooldown += Time.deltaTime;
		}

	}

	public void sendRankingData() {

		currentRankingCooldown = 0f;

		clientScript.sortList ();

		foreach (ClientScript.Player player in clientScript.listPlayers) {

			player.ping = Network.GetAveragePing(clientScript.NetworkPlayerByCode(player.playerCode));
			if (player.ping < 0) { player.ping = 0; }
			GetComponent<NetworkView>().RPC("updateRankingRPC", RPCMode.Others, player.playerCode, player.kills, player.ping);

		}

	}

	void sendHackData() {

		currentHackDataCooldown = 0f;

		foreach (ClientScript.Player player in clientScript.listPlayers) {

			GetComponent<NetworkView>().RPC("updateHackDataRPC", RPCMode.Others, player.playerCode, player.hackingPlayerCode, player.justHacked);
			player.justHacked = false;

		}

	}


	public void hackAttack (int playerCode) {

		if (!clientScript.localPlayer.gameEnded) {

			ClientScript.Player attackerPlayer = clientScript.PlayerByCode (playerCode);
			ClientScript.Player victimPlayer = clientScript.firstLookingPlayer(attackerPlayer);

			if (victimPlayer != null) {
				// THERE'S A VICTIM
				float distance = Vector3.Distance(attackerPlayer.targetPosition, victimPlayer.targetPosition);

				if (attackerPlayer.hackingPlayerCode == victimPlayer.playerCode && distance <= ClientScript.hackKillDistance) {
					// IF THE DISTANCE IS SMALL AND WAS ALREADY HACKED, DIES
					if (victimPlayer.immune <= 0f) {

						attackerPlayer.kills++;
						attackerPlayer.hackingPlayerCode = -1;
						sendHackData ();
						GetComponent<NetworkView> ().RPC ("killRPC", RPCMode.All, attackerPlayer.playerCode, victimPlayer.playerCode);

						respawn (victimPlayer.playerCode);
						victimPlayer.immune = 5f;

						sendRankingData ();

					}

				} else {
					// IF THE DISTANCE IS BIG OR ISN'T HACKED, THEN IT'S HACKED
					attackerPlayer.hackingPlayerCode = victimPlayer.playerCode;
					attackerPlayer.hackingTimer = ClientScript.hackingTimerMax;
					attackerPlayer.justHacked = true;
				}
			}

		}

	}

	public void interceptAttack (int playerCode) {

		if (!clientScript.localPlayer.gameEnded) {

			ClientScript.Player attackerPlayer = clientScript.PlayerByCode (playerCode);

			List<ClientScript.Player> playersCrosshair = clientScript.insideBigCrosshair (attackerPlayer, ClientScript.interceptKillDistance);

			bool deaths = false;

			foreach (ClientScript.Player player in playersCrosshair) {

				if (player.hackingPlayerCode == attackerPlayer.playerCode) {
					// YOU'RE DEAD, MADAFACKA
					deaths = true;

					attackerPlayer.kills++;
					GetComponent<NetworkView> ().RPC ("killRPC", RPCMode.All, attackerPlayer.playerCode, player.playerCode);

					respawn (player.playerCode);
					player.immune = 5f;

				}

			}

			if (deaths) {
				sendHackData ();
				sendRankingData ();
			}

		}

	}

	// NETWORK RELATED
	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log("Clean up after player " + player);
		Network.RemoveRPCs(player);
		GetComponent<NetworkView>().RPC("removePlayerRPC", RPCMode.All, Int32.Parse(player.ToString()));
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
