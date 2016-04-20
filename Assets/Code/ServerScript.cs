using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerScript : MonoBehaviour {

	[HideInInspector] public GameScript gameScript;
	[HideInInspector] public List<RespawnLocation> listRespawnLocations = new List<RespawnLocation> ();
	private float oldestTimeUsed;

	public float currentRankingCooldown = 0f;
	public float currentHackDataCooldown = 0f;
	public static int hackDataUpdatesPerSecond = 15;

	// Use this for initialization
	void Start () {
	
		createRespawnPoints(gameScript.clientScript.map.transform.FindChild ("RespawnPoints").gameObject);
		respawn (gameScript.clientScript.myCode);

	}
	
	// Update is called once per frame
	void Update () {

		checkForSuicides ();
		checkIfSendDataToClients(); 

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

		for (int i = 0; i < gameScript.clientScript.listPlayers.Count; i++) {

			// THIS CHECKS IF SOMEONE IS FALLING INTO THE ETERNAL VOID OF THE BUGSPHERE
			if (gameScript.clientScript.listPlayers [i].targetPosition.y < -100f) {
				respawn (gameScript.clientScript.listPlayers [i].playerCode);
			}

		}


	}

	public void respawn(string playerCode) {

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

        int chosenRespawn = possibleRespawns[Random.Range(0, possibleRespawns.Count)];
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

		gameScript.clientScript.listPlayers.Sort (CompareListByKills);

		for (int i = 0; i < gameScript.clientScript.listPlayers.Count; i++) {
			gameScript.clientScript.listPlayers[i].ping = Network.GetAveragePing(gameScript.clientScript.NetworkPlayerByCode(gameScript.clientScript.listPlayers[i].playerCode));
			if (gameScript.clientScript.listPlayers[i].ping < 0) { gameScript.clientScript.listPlayers[i].ping = 0; }
			GetComponent<NetworkView>().RPC("updateRankingRPC", RPCMode.Others, gameScript.clientScript.listPlayers[i].playerCode, gameScript.clientScript.listPlayers[i].kills, gameScript.clientScript.listPlayers[i].ping);
		}

	}

	void sendHackData() {

		currentHackDataCooldown = 0f;

		for (int i = 0; i < gameScript.clientScript.listPlayers.Count; i++) {
			GetComponent<NetworkView>().RPC("updateHackDataRPC", RPCMode.Others, gameScript.clientScript.listPlayers[i].playerCode, gameScript.clientScript.listPlayers[i].hackingPlayerCode, gameScript.clientScript.listPlayers[i].justHacked);
			gameScript.clientScript.listPlayers [i].justHacked = false;
		}

	}


	public void hackAttack (string playerCode) {

		if (!gameScript.clientScript.localPlayer.gameEnded) {

			ClientScript.Player attackerPlayer = gameScript.clientScript.PlayerByCode (playerCode);
			ClientScript.Player victimPlayer = gameScript.clientScript.firstLookingPlayer(attackerPlayer);

			if (victimPlayer != null) {
				// THERE'S A VICTIM
				float distance = Vector3.Distance(attackerPlayer.targetPosition, victimPlayer.targetPosition);

				if (attackerPlayer.hackingPlayerCode == victimPlayer.playerCode && distance <= 2f) {
					// IF THE DISTANCE IS SMALL AND WAS ALREADY HACKED, DIES
					if (victimPlayer.immune <= 0f) {

						attackerPlayer.kills++;
						GetComponent<NetworkView> ().RPC ("killRPC", RPCMode.All,attackerPlayer.playerCode, victimPlayer.playerCode);
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

	public void interceptAttack (string playerCode) {



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
