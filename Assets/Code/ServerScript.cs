using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class ServerScript : MonoBehaviour {

	[HideInInspector] public ClientScript clientScript;
	[HideInInspector] public List<RespawnLocation> listRespawnLocations = new List<RespawnLocation> ();
	private float oldestTimeUsed;

	public float currentRankingCooldown = 0f;
	public float currentHackDataCooldown = 0f;
	public static int hackDataUpdatesPerSecond = 15;

	public List<string> bannedIPs = new List<string>();

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

		checkForRespawns ();
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

	void checkForRespawns() {

		foreach (ClientScript.Player player in clientScript.listPlayers) {

			// THIS CHECKS IF SOMEONE IS FALLING INTO THE ETERNAL VOID OF THE BUGSPHERE
			if (player.targetPosition.y < -100f) {
				respawn (player.playerCode);
			}

			// THIS CHECKS IF SOMEONE IS DEAD AS RAGDOLL FOR TOO MUCH AND MUST RESPAWN
			if (player.deadTime > 0f) {
				player.deadTime -= Time.deltaTime;
				if (player.deadTime <= 0f) {
					player.deadTime = 0f;
					respawn (player.playerCode);
				}
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

			if (player.justHacked && player.hackingPlayerCode == clientScript.myCode) {
				clientScript.localPlayer.alertHacked.GetComponent<Image>().material.SetFloat("_Cutoff", 1f - Time.deltaTime);
			}

			player.justHacked = false;

		}

	}


	public void hackAttack (int attackerCode, int victimCode, bool isKilling) {

		if (!clientScript.localPlayer.gameEnded) {

			ClientScript.Player attackerPlayer = clientScript.PlayerByCode (attackerCode);
			ClientScript.Player victimPlayer = clientScript.PlayerByCode (victimCode);

			if (!attackerPlayer.dead && !victimPlayer.dead && victimPlayer.hackingPlayerCode != attackerCode && victimPlayer.immune <= 0f) {
				// IT'S POSSIBLE
				if (isKilling) {
					// IF WAS ALREADY HACKED AND WITHIN KILL DISTANCE, DIES
					attackerPlayer.kills++;
					attackerPlayer.hackingPlayerCode = -1;
					GetComponent<NetworkView> ().RPC ("killRPC", RPCMode.All, attackerPlayer.playerCode, victimPlayer.playerCode);

					victimPlayer.deadTime = 3f;

					sendHackData ();
					sendRankingData ();

				} else {
					// IF WASN'T HACKED OR WITHIN KILL DISTANCE, THEN IT'S HACKED
					attackerPlayer.hackingPlayerCode = victimPlayer.playerCode;
					attackerPlayer.hackingTimer = ClientScript.hackingTimerMax;
					attackerPlayer.justHacked = true;
				}
			}

		}

	}

	public void interceptAttack (int attackerCode, int victimCode) {

		if (!clientScript.localPlayer.gameEnded) {

			ClientScript.Player attackerPlayer = clientScript.PlayerByCode (attackerCode);
			ClientScript.Player victimPlayer = clientScript.PlayerByCode (victimCode);

			if (!attackerPlayer.dead && !victimPlayer.dead && victimPlayer.hackingPlayerCode == attackerPlayer.playerCode) {
				// IT'S POSSIBLE

				attackerPlayer.kills++;
				GetComponent<NetworkView> ().RPC ("killRPC", RPCMode.All, attackerPlayer.playerCode, victimPlayer.playerCode);

				victimPlayer.deadTime = 3f;

				sendHackData ();
				sendRankingData ();

			}

		}

	}

	// NETWORK RELATED
	void OnPlayerConnected(NetworkPlayer player) {

		if (bannedIPs.Contains (player.ipAddress)) {
			Network.CloseConnection (player, true);
		} else {
			if (clientScript.lockedRemainingSeconds) {
				clientScript.lockedRemainingSeconds = false;
			}
		}

	}

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
