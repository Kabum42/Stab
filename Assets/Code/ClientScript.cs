using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;

public class ClientScript : MonoBehaviour {

	[HideInInspector] public ServerScript serverScript;

	private NetworkView networkView;

	private int netUpdatesPerSecond = 15;
	private float currentUpdateCooldown = 0f;

	[HideInInspector] public float remainingSeconds = (8f)*(60f); // 8 MINUTES
	[HideInInspector] public bool lockedRemainingSeconds = false;

	private GameObject rankingBackground;
	private GameObject textBig;
	private float textBigAlpha = 0f;
	[HideInInspector] public GameObject map;

	[HideInInspector] public LocalPlayerScript localPlayer;
	public Player myPlayer;
	public List<Player> listPlayers = new List<Player>();
	private List<GameObject> visualAvatarsPool = new List<GameObject> ();

	private static float slowSpeed = 1f/(1f);
	private static float hackingSpeed = 1f/(0.25f); // EL SEGUNDO NUMERO ES CUANTO TARDA EN TIEMPO REAL EN LLEGAR A 1
	private static float fastSpeed = 1f/(0.05f);

	public bool gameEnded = false;
	private Player winnerPlayer;

	public static float hackingTimerMax = 5f;

	private Camera auxCamera;

	public static float hackKillDistance = 3f;
	public static float interceptKillDistance = 6f;
	public static float immuneTimeAtRespawn = 3f;

	private bool justRespawned = false;

	private bool disconnecting = false;

	// Use this for initialization
	void Awake () {

		GlobalData.clientScript = this;
		networkView = GetComponent<NetworkView> ();

		map = this.gameObject;

		localPlayer = Instantiate (Resources.Load("Prefabs/LocalPlayer") as GameObject).GetComponent<LocalPlayerScript>();

		// SERVER RELATED
		if (Network.isServer) {
			// YOU JOIN AS PLAYER
			myPlayer = new Player(0, localPlayer.visualAvatar);
			listPlayers.Add(myPlayer);
			myPlayer.networkPlayer = Network.player;
			localPlayer.visualAvatar.GetComponent<PlayerMarker>().player = myPlayer;

			serverScript = gameObject.AddComponent<ServerScript> ();
			serverScript.initialize (this);
			lockedRemainingSeconds = true;
		} else {
			Destroy(map.transform.FindChild ("RespawnPoints").gameObject);
			myPlayer = new Player(-1, localPlayer.visualAvatar);
			listPlayers.Add(myPlayer);
			localPlayer.visualAvatar.GetComponent<PlayerMarker>().player = myPlayer;
		}



		GameObject auxCameraHolder = new GameObject ();
		auxCamera = auxCameraHolder.AddComponent<Camera> ();
		auxCamera.fieldOfView = 70f;
		auxCameraHolder.name = "AuxiliarCamera";
		auxCameraHolder.SetActive (false);

		rankingBackground = localPlayer.rankingBackground;

		textBig = localPlayer.canvas.transform.FindChild("TextBig").gameObject;

		networkView.RPC("sayHiRPC", RPCMode.Server);

	}
	
	// Update is called once per frame
	void Update () {

		if (!lockedRemainingSeconds) {
			remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.deltaTime);
		}
		textBigAlpha = (Mathf.Max(0f, textBigAlpha - Time.deltaTime * (1f/5f)));
			
		updateRanking ();
		if (!myPlayer.dead) {
			updateMyInfoInOtherClients ();
		}
		synchronizeOtherPlayers ();
		updateHacking ();

		if (remainingSeconds <= 0f) {
			if (!gameEnded) {
				if (Network.isServer) {
					gameEnded = true;
					sortList ();
					networkView.RPC ("winnerRPC", RPCMode.All, listPlayers[0].ID);
				}
			} else if (winnerPlayer.networkPlayer == Network.player) {
				textBig.GetComponent<Text>().text = "<color=#44FF44>CONGRATULATIONS!</color> YOU WON";
				textBigAlpha = 1f;
			} else {
				textBig.GetComponent<Text>().text = "<color=#FF4444>#"+winnerPlayer.name+"</color> HAS WON";
				textBigAlpha = 1f;
			}
		}

		textBig.GetComponent<CanvasRenderer> ().SetAlpha (textBigAlpha);

		if (disconnecting) {
			if (Mathf.Abs (localPlayer.fade.GetComponent<Image> ().color.a - GlobalData.fadeAlphaTarget) < 0.01f) {
				Application.LoadLevel ("Menu");
			}
		}
	
	}



	void LateUpdate() {

		foreach (Player player in listPlayers) {
			
			LocalPlayerScript.RotateHead (player.visualAvatar, player.currentCameraEulerX);

		}

	}

	void updateHacking() {

		foreach (Player player in listPlayers) {

			if (player.hackingTimer > 0f) {

				player.hackingTimer = Mathf.Max (0f, player.hackingTimer - Time.deltaTime);

				if (player == myPlayer) {
					float aux = 1f - player.hackingTimer/hackingTimerMax;
					localPlayer.crosshairHackTimer.SetActive (true);
					localPlayer.crosshairHackTimer.GetComponent<Image>().material.SetFloat("_Cutoff", aux);
				}

			}

			if (player.hackingTimer <= 0f || player.hackingNetworkPlayer == player.networkPlayer) {

				player.hackingNetworkPlayer = player.networkPlayer;
				player.hackingTimer = 0f;

				if (player == myPlayer) {
					localPlayer.crosshairHackTimer.SetActive (false);
				}

			}

		}

	}


	public Player firstLookingPlayer(Player p1) {

		//float lookingDistance = 10f;

		RaycastHit[] hits;
		hits = Physics.RaycastAll (p1.targetPosition + LocalPlayerScript.centerOfCamera, p1.cameraForward);
		Array.Sort (hits, delegate(RaycastHit r1, RaycastHit r2) { return r1.distance.CompareTo(r2.distance); });

		for (int i = 0; i < hits.Length; i++) {

			if (hits[i].collider.gameObject.tag != "LocalPlayer" && !(p1.visualAvatar.GetComponent<RagdollScript>().IsArticulation(hits[i].collider.gameObject))) {

				PlayerMarker pM = PlayerMarker.Traverse (hits [i].collider.gameObject);

				if (pM != null) {
					// DOESN'T MATTER WHO, IT COLLIDED WITH A PLAYER
					Player auxPlayer = pM.player;
					if (!(auxPlayer.hackingNetworkPlayer == p1.networkPlayer) && !auxPlayer.dead) {
						// YOU CAN SEE IT
						return auxPlayer;
					}

				} else {

					return null;

				}
			}
		}

		return null;

	}

	public bool blockingBetweenPlayers(Player p1, GameObject p2Object) {

		RaycastHit[] hits;
		Vector3 distance = (p2Object.transform.position - (p1.targetPosition +  LocalPlayerScript.centerOfCamera));
		Vector3 direction = distance.normalized;
		hits = Physics.RaycastAll (p1.targetPosition + LocalPlayerScript.centerOfCamera, direction, distance.magnitude);
		Array.Sort (hits, delegate(RaycastHit r1, RaycastHit r2) { return r1.distance.CompareTo(r2.distance); });

		Player p2 = PlayerMarker.Traverse (p2Object).player;

		for (int i = 0; i < hits.Length; i++) {

			if (hits[i].collider.gameObject.tag != "LocalPlayer" && !(p1.visualAvatar.GetComponent<RagdollScript>().IsArticulation(hits[i].collider.gameObject)) && !(p2.visualAvatar.GetComponent<RagdollScript>().IsArticulation(hits[i].collider.gameObject))) {

				PlayerMarker pM = PlayerMarker.Traverse (hits [i].collider.gameObject);

				if (pM == null) {
					return true;
				}
			}
		}

		return false;

	}

	public List<Player> insideBigCrosshair(Player p1, float distanceZ, string key, bool invisibleAllowed) {

		float minDistanceInScreen = 0f;
		if (key == "bigCrosshair") { minDistanceInScreen = 0.24f; }

		return insideBigCrosshair (p1, distanceZ, minDistanceInScreen, invisibleAllowed);

	}

	public List<Player> insideBigCrosshair(Player p1, float distanceZ, float minDistanceInScreen, bool invisibleAllowed) {

		List<Player> playersInside = new List<Player> ();

		foreach (Player player in listPlayers) {

			if (player != p1 && (invisibleAllowed || player.hackingNetworkPlayer != p1.networkPlayer) && !player.dead) {

				bool IsInside = false;

				auxCamera.gameObject.transform.position = p1.cameraMockup.transform.position;
				auxCamera.gameObject.transform.eulerAngles = p1.cameraMockup.transform.eulerAngles;

				for (int i = 0; i < player.vitalPoints.Length; i++) {

					Vector3 aux = auxCamera.WorldToScreenPoint (player.vitalPoints[i].transform.position);

					if (aux.z >= 0f) {
						// IF THE AUXILIAR POSITION IS IN FRONT OF THE CAMERA
						Vector2 auxRelative = new Vector2 (aux.x -Screen.width/2f, aux.y -Screen.height/2f);
						auxRelative = auxRelative / Screen.width;

						float distanceInScreen = Vector2.Distance (auxRelative, new Vector2 (0f, 0f));

						if (distanceInScreen <= minDistanceInScreen && !blockingBetweenPlayers(p1, player.vitalPoints[i])) {
							// IS INSIDE THE SCREEN DISTANCE AND NOTHING DIFFERENT FROM A PLAYER IS BLOCKING THE VIEW
							IsInside = true;
						}

					}

				}

				if (IsInside && Vector3.Distance(player.cameraMockup.transform.position, p1.cameraMockup.transform.position) < distanceZ) {
					playersInside.Add (player);
				}

			}
		}

		return playersInside;

	}

	public Player insideBigCrosshairExclusive(Player p1, float distanceZ, string key, bool invisibleAllowed) {

		float minDistanceInScreen = 0f;
		if (key == "bigCrosshair") { minDistanceInScreen = 0.23f; }
		if (key == "smallCrosshair") { minDistanceInScreen = 0.05f; }

		return insideBigCrosshairExclusive (p1, distanceZ, minDistanceInScreen, invisibleAllowed);

	}

	public Player insideBigCrosshairExclusive(Player p1, float distanceZ, float minDistanceInScreen, bool invisibleAllowed) {

		Player playerInside = null;
		float minDistanceZ = float.MaxValue;

		foreach (Player player in listPlayers) {

			if (player != p1 && (invisibleAllowed || player.hackingNetworkPlayer != p1.networkPlayer) && !player.dead) {

				bool IsInside = false;

				auxCamera.gameObject.transform.position = p1.cameraMockup.transform.position;
				auxCamera.gameObject.transform.eulerAngles = p1.cameraMockup.transform.eulerAngles;

				for (int i = 0; i < player.vitalPoints.Length; i++) {

					Vector3 aux = auxCamera.WorldToScreenPoint (player.vitalPoints[i].transform.position);

					if (aux.z >= 0f) {
						// IF THE AUXILIAR POSITION IS IN FRONT OF THE CAMERA

						Vector2 auxRelative = new Vector2 (aux.x -Screen.width/2f, aux.y -Screen.height/2f);
						auxRelative = auxRelative / Screen.width;

						float distanceInScreen = Vector2.Distance (auxRelative, new Vector2 (0f, 0f));

						if (distanceInScreen <= minDistanceInScreen && !blockingBetweenPlayers(p1, player.vitalPoints[i])) {
							// IS INSIDE THE SCREEN DISTANCE AND NOTHING DIFFERENT FROM A PLAYER IS BLOCKING THE VIEW
							IsInside = true;
						}

					}

				}

				float currentDistanceZ = Vector3.Distance (player.cameraMockup.transform.position, p1.cameraMockup.transform.position);

				if (IsInside && currentDistanceZ < distanceZ) {
					if (currentDistanceZ < minDistanceZ) {
						minDistanceZ = currentDistanceZ;
						playerInside = player;
					}
				}

			}
		}

		return playerInside;

	}
		

	void updateMyInfoInOtherClients() {


		if (currentUpdateCooldown >= 1f / (float)netUpdatesPerSecond) {

			currentUpdateCooldown = 0f;

			if (localPlayer != null && networkView != null && !localPlayer.firstRespawn) {

				if (justRespawned) {
					justRespawned = false;
					networkView.RPC ("updatePlayerInstantRPC", RPCMode.Others, myPlayer.ID, localPlayer.visualAvatar.transform.position);
				} else {
					networkView.RPC("updatePlayerRPC", RPCMode.Others, myPlayer.ID, localPlayer.visualAvatar.transform.position, localPlayer.visualAvatar.transform.eulerAngles.y, localPlayer.personalCamera.transform.forward, localPlayer.personalCamera.transform.eulerAngles.x, (int)localPlayer.lastAnimationOrder);
				}

			}


		} else {

			currentUpdateCooldown += Time.deltaTime;

		}

		myPlayer.currentCameraEulerX = localPlayer.personalCamera.transform.eulerAngles.x;
		myPlayer.cameraForward = localPlayer.personalCamera.transform.forward;
		myPlayer.targetPosition = myPlayer.visualAvatar.transform.position;
		myPlayer.targetAvatarEulerY = myPlayer.visualAvatar.transform.eulerAngles.y;
		myPlayer.cameraMockup.transform.LookAt (myPlayer.cameraMockup.transform.position + myPlayer.cameraForward);

	}

	void synchronizeOtherPlayers () {

		foreach (Player player in listPlayers) {

			if (player != myPlayer) {

				player.visualAvatar.transform.position = Vector3.Lerp (player.visualAvatar.transform.position, player.targetPosition, Time.deltaTime * 10f);
				player.visualAvatar.transform.eulerAngles = Hacks.LerpVector3Angle (player.visualAvatar.transform.eulerAngles, new Vector3(0f, player.targetAvatarEulerY, 0f), Time.deltaTime * 10f);
				player.immune = Mathf.Max (0f, player.immune - Time.deltaTime);
				player.currentCameraEulerX = Mathf.LerpAngle (player.currentCameraEulerX, player.targetCameraEulerX, Time.deltaTime * 10f);
				player.cameraMockup.transform.LookAt (player.cameraMockup.transform.position + player.cameraForward);

				float r = 1f;
				float g = 1f;
				float b = 1f;
				float a = 1f;

				if (myPlayer.hackingNetworkPlayer == player.networkPlayer) {
					g = 0f;
					b = 0f;
				}

				if (player.hackingNetworkPlayer == Network.player) {
					a = 0f;
				}


				Color targetColor = new Color (r, g, b, a);

				for (int i = 0; i < player.visualMaterials.Length; i++) {
					Color c = Color.Lerp (player.visualMaterials[i].GetColor ("_Color"), targetColor, Time.fixedDeltaTime * 5f);

					player.visualMaterials[i].SetColor ("_Color", c);
					player.visualMaterials[i].SetFloat ("_Cutoff", 1f - c.a);
				}



			} else {
				// ES MI PLAYER
				player.immune = Mathf.Max (0f, player.immune - Time.deltaTime);
			}

		}

	}

	public void sortList() {

		// FIRST CRITERION :  KILLS, FROM BIGGER TO SMALLER
		// SECOND CRITERION :  LASTKILLREMAININGSECONDS, FROM SMALLER TO BIGGER
		// THIRD CRITERION :  PLAYERCODE, JUST TO HAVE A UNIQUE ARBITRARY PARAMETER TO ORDER THEM IF THERE'S A DRAW
		listPlayers = listPlayers.OrderByDescending(o=>o.kills).ThenBy(o=>o.lastKillRemainingSeconds).ThenBy(o=>o.ID).ToList();

	}

	void updateRanking() {

		if (Input.GetKey (KeyCode.Tab) && (localPlayer.inputMode == LocalPlayerScript.InputMode.Playing)) {

			string auxPlayers = "Player\n\n";
			string auxKills = "Kills\n\n";
			string auxPings = "Ping\n\n";
			int totalSeconds = (int) Mathf.Floor(remainingSeconds);
			int seconds = totalSeconds % 60;
			int minutes = totalSeconds / 60;
			string auxTime = minutes.ToString("00") + ":" + seconds.ToString("00");

			sortList ();

			int i = 0;
			foreach (Player player in listPlayers) {

				if (player.networkPlayer == Network.player) {
					auxPlayers += "<color=#D7D520>" + player.name + "</color>";
				}
				else {
					auxPlayers += player.name;
				}
				auxKills += "<color=#FF8C8CFF>"+ player.kills + "</color>";
				auxPings += player.ping +"";
				if (i != listPlayers.Count -1) { 
					auxPlayers += "\n"; 
					auxKills += "\n"; 
					auxPings += "\n";
				}

				i++;
			}

			rankingBackground.transform.FindChild ("TextPlayers").GetComponent<Text> ().text = auxPlayers;
			rankingBackground.transform.FindChild ("TextKills").GetComponent<Text> ().text = auxKills;
			rankingBackground.transform.FindChild ("TextPings").GetComponent<Text> ().text = auxPings;
			rankingBackground.transform.FindChild("TimeBackground/TextTime").GetComponent<Text> ().text = auxTime;

			rankingBackground.SetActive (true);
			localPlayer.chatManager.chatPanel.SetActive(false);
			textBig.SetActive (false);

		} else {
			
			rankingBackground.SetActive (false);
			textBig.SetActive (true);

			// SHOW CHAT PANEL
			if (localPlayer.chatManager.lastChatPannelInteraction >= localPlayer.chatManager.chatPannelInteractionThreshold) {
				localPlayer.chatManager.chatPanel.SetActive (false);
			} else {
				localPlayer.chatManager.chatPanel.SetActive (true);
			}

		}

	}

	bool hasCommands(string text) {

		bool result = false;

		if (text.Substring (0, 1) == "/") {
			result = true;
			string[] stringArray = text.Split (' ');
			if (stringArray [0] == "/kick") {
				
				if (Network.isServer) {
					if (stringArray.Length > 1) {
						Player player = PlayerByName (stringArray[1]);
						if (player != null) {
							if (player == myPlayer) {
								localPlayer.chatManager.Add (new ChatMessage ("System", "You can't kick yourself."));
							} else {
								NetworkPlayer networkPlayer = NetworkPlayerByName (stringArray[1]);
								serverScript.bannedIPs.Add (networkPlayer.ipAddress);
								Network.CloseConnection(networkPlayer, true);
								localPlayer.chatManager.Add (new ChatMessage ("System", stringArray [1] + " was kicked."));
							}
						} else {
							localPlayer.chatManager.Add (new ChatMessage ("System", stringArray [1] + " was not found."));
						}
					} else {
						localPlayer.chatManager.Add (new ChatMessage ("System", "You must write a player's name after writing /kick"));
					}
				} else {
					localPlayer.chatManager.Add (new ChatMessage ("System", "Only the server can kick players."));
				}

			} else if (stringArray [0] == "/help") {
				
				localPlayer.chatManager.Add(new ChatMessage("System", "\"/kick PlayerName\" to kick a player."));

			} else {
				
				localPlayer.chatManager.Add(new ChatMessage("System", "Invalid command. Type /help for a list of commands."));

			}
		}

		return result;

	}

	public void writeInChat(string info) {

		if (!hasCommands (info)) {
			networkView.RPC("addChatMessageRPC", RPCMode.All, myPlayer.ID, info);
		}

	}

	public NetworkPlayer NetworkPlayerByName(string playerName) {

		Player auxPlayer = PlayerByName (playerName);

		if (auxPlayer != null) {
			return auxPlayer.networkPlayer;
		}

		return new NetworkPlayer();

	}

	public Player PlayerByName(string playerName) {

		foreach (Player player in listPlayers) {
			if (player.name == playerName) {
				return player;
			}
		}

		return null;

	}

	public Player PlayerByNetworkPlayer(NetworkPlayer nPlayer) {

		foreach (Player player in listPlayers) {
			if (player.networkPlayer == nPlayer) {
				return player;
			}
		}

		return null;

	}

	public Player PlayerByID(int ID) {

		foreach (Player player in listPlayers) {
			if (player.ID == ID) {
				return player;
			}
		}

		return null;

	}

	public bool NetworkPlayerIsServer(NetworkPlayer nPlayer) {

		if (Network.isServer) {
			if (nPlayer == Network.player) {
				return true;
			}
		} else {
			if (Network.connections [0] == nPlayer) {
				return true;
			}
		}

		return false;

	}

	private GameObject GetVisualAvatar() {

		GameObject visualAvatar;

		if (visualAvatarsPool.Count > 0) {
			visualAvatar = visualAvatarsPool [0];
			visualAvatarsPool.RemoveAt (0);
		} else {
			visualAvatar = Instantiate (Resources.Load ("Prefabs/BOT") as GameObject);
		}

		return visualAvatar;

	}

	// NETWORK RELATED
	void OnDisconnectedFromServer(NetworkDisconnection info) {
		if (Network.isServer)
			Debug.Log("Local server connection disconnected");
		else
			if (info == NetworkDisconnection.LostConnection)
				Debug.Log("Lost connection to the server");
			else
				Debug.Log("Successfully diconnected from the server");

		Disconnect ();
	}

	void OnFailedToConnect(NetworkConnectionError info) {

		Debug.Log ("As a client could not connect for some reason");

		Disconnect ();
	}

	void OnFailedToConnectToMasterServer(NetworkConnectionError info) {

		Debug.Log ("Could not connect to master server");

		Disconnect ();
	}

	void OnMasterServerEvent(MasterServerEvent msEvent) {
		if (msEvent == MasterServerEvent.RegistrationFailedGameName || msEvent == MasterServerEvent.RegistrationFailedGameType || msEvent == MasterServerEvent.RegistrationFailedNoServer) {
			Debug.Log ("Could not connect to master server");
			Disconnect();
		}
	}

	public void Disconnect() {
		GlobalData.fadeAlphaTarget = 1f;
		disconnecting = true;
	}

	// CLIENT RPCs
	[RPC]
	void sayHiRPC(NetworkMessageInfo info) {

		// ADD THE NEW PLAYER TO EVERYONE
		networkView.RPC ("addPlayerRPC", RPCMode.All, info.sender, int.Parse(info.sender.ToString()), true);

	}

	[RPC]
	void firstSpawnRPC(NetworkMessageInfo info) {

		Player player = PlayerByNetworkPlayer (info.sender);
		if (player != null) {
			// LE ASIGNAMOS UN SITIO DONDE APARECER
			serverScript.respawn(player.ID);
		}

	}

	[RPC]
	void addPlayerRPC(NetworkPlayer nPlayer, int ID, bool isNew, NetworkMessageInfo info) {

		NetworkPlayer sender = info.sender;
		//handle the fact that unity is a bit dumb and calls the local player "-1" instead of its real networkplayer!!
		if(sender.ToString() == "-1") { sender = Network.player; }

		if (NetworkPlayerIsServer (sender)) {

			if (nPlayer != Network.player) {
				Player newPlayer = new Player(ID);
				newPlayer.networkPlayer = nPlayer;
				listPlayers.Add(newPlayer);
				if (isNew) {
					localPlayer.chatManager.Add (new ChatMessage ("System", newPlayer.name + " has joined the game."));
				}
			} else {
				// YOU JOIN AS PLAYER
				myPlayer.Initialize(ID);
				myPlayer.networkPlayer = nPlayer;
				networkView.RPC("firstSpawnRPC", RPCMode.Server);
			}

			if (Network.isServer) {
				// LE DECIMOS CUANTOS SEGUNDOS DE PARTIDA QUEDAN
				networkView.RPC("sendRemainingSecondsRPC", nPlayer, remainingSeconds);
				// SEND EVERYONE TO THE NEW PLAYER
				foreach (Player player in listPlayers) {
					if (player.networkPlayer != nPlayer) {
						networkView.RPC("addPlayerRPC", nPlayer, player.networkPlayer, player.ID, false);
					}
				}
				// SE ACABA DE UNIR UN JUGADOR, ASI QUE LES DECIMOS A TODOS LA NUEVA SITUACION DEL RANKING
				serverScript.sendRankingData();
			}

		}

	}

	[RPC]
	void updatePlayerRPC(int ID, Vector3 position, float avatarEulerY, Vector3 cameraForward, float cameraEulerX, int currentAnimation, NetworkMessageInfo info)
	{

		Player player = PlayerByID (ID);

		if (player != null) {

			if (Network.isServer && player.networkPlayer != info.sender) {
				serverScript.bannedIPs.Add (info.sender.ipAddress);
				Network.CloseConnection(info.sender, true);
				return;
			}

			if (!player.dead) {
				player.targetPosition = position;
				player.targetAvatarEulerY = avatarEulerY;
				player.cameraForward = cameraForward;
				player.targetCameraEulerX = cameraEulerX;
				player.SmartCrossfade(LocalPlayerScript.AnimationString[currentAnimation]);
			}

		}

	}

	[RPC]
	void updatePlayerInstantRPC(int ID, Vector3 position, NetworkMessageInfo info)
	{

		Player player = PlayerByNetworkPlayer (info.sender);

		if (player != null) {

			if (Network.isServer && player.networkPlayer != info.sender) {
				serverScript.bannedIPs.Add (info.sender.ipAddress);
				Network.CloseConnection(info.sender, true);
				return;
			}

			player.visualAvatar.transform.position = position;
			player.targetPosition = position;

		}

	}

	[RPC]
	void addChatMessageRPC(int ID, string text, NetworkMessageInfo info)
	{
		NetworkPlayer sender = info.sender;
		//handle the fact that unity is a bit dumb and calls the local player "-1" instead of its real networkplayer!!
		if(sender.ToString() == "-1") { sender = Network.player; }

		Player player = PlayerByID (ID);

		if (player != null) {

			if (Network.isServer && player.networkPlayer != info.sender) {
				serverScript.bannedIPs.Add (info.sender.ipAddress);
				Network.CloseConnection(info.sender, true);
				return;
			}

			string owner = player.name;
			if (player == myPlayer) { owner = "You"; }

			localPlayer.chatManager.Add (new ChatMessage (owner, text));
		}
	}

	// SERVER RPCs
	[RPC]
	void sendRemainingSecondsRPC(float auxRemainingSeconds)
	{
		remainingSeconds = auxRemainingSeconds;
	}

	[RPC]
	void removePlayerRPC(int ID, NetworkMessageInfo info) {

		NetworkPlayer sender = info.sender;
		//handle the fact that unity is a bit dumb and calls the local player "-1" instead of its real networkplayer!!
		if(sender.ToString() == "-1") { sender = Network.player; }

		if (NetworkPlayerIsServer (sender)) {
			Player player = PlayerByID (ID);

			if (player != null) {
				player.visualAvatar.SetActive (false);
				visualAvatarsPool.Add (player.visualAvatar);

				listPlayers.Remove (player);

				localPlayer.chatManager.Add(new ChatMessage("System", "Player "+player.name+" has left the game."));
			}
		}

	}

	[RPC]
	void updateRankingRPC(int ID, int kills, int ping, NetworkMessageInfo info)
	{
		
		NetworkPlayer sender = info.sender;
		//handle the fact that unity is a bit dumb and calls the local player "-1" instead of its real networkplayer!!
		if(sender.ToString() == "-1") { sender = Network.player; }

		if (NetworkPlayerIsServer (sender)) {
			Player player = PlayerByID (ID);
			if (player != null) {
				player.kills = kills;
				player.ping = ping;
			}
		}

	}

	[RPC]
	void hackAttackRPC(NetworkPlayer victimNetworkPlayer, NetworkMessageInfo info) {
		serverScript.hackAttack (info.sender, victimNetworkPlayer, false);
	}

	[RPC]
	void hackAttackKillRPC(NetworkPlayer victimNetworkPlayer, NetworkMessageInfo info) {
		serverScript.hackAttack (info.sender,victimNetworkPlayer, true);
	}

	[RPC]
	void interceptAttackRPC(NetworkPlayer victimNetworkPlayer, NetworkMessageInfo info) {
		serverScript.interceptAttack (info.sender, victimNetworkPlayer);
	}

	[RPC]
	void updateHackDataRPC(int ID, NetworkPlayer hackedNetWorkPlayer, bool justHacked, NetworkMessageInfo info)
	{

		NetworkPlayer sender = info.sender;
		//handle the fact that unity is a bit dumb and calls the local player "-1" instead of its real networkplayer!!
		if(sender.ToString() == "-1") { sender = Network.player; }

		if (NetworkPlayerIsServer (sender)) {
			Player player = PlayerByID (ID);
			Player hackedPlayer = PlayerByNetworkPlayer (hackedNetWorkPlayer);

			if (player != null) {
				if (hackedPlayer != null) {
					player.immune = 0f;
					if (justHacked) {
						player.hackingTimer = hackingTimerMax;
						if (hackedNetWorkPlayer == Network.player) {
							// ALERT
							localPlayer.alertHacked.GetComponent<Image>().material.SetFloat("_Cutoff", 1f - Time.deltaTime);
						}
					}
				}

				player.hackingNetworkPlayer = hackedNetWorkPlayer;
			}

		}

	}

	[RPC]
	void killRPC(int assassinID, int victimID, NetworkMessageInfo info)
	{

		NetworkPlayer sender = info.sender;
		//handle the fact that unity is a bit dumb and calls the local player "-1" instead of its real networkplayer!!
		if(sender.ToString() == "-1") { sender = Network.player; }

		if (NetworkPlayerIsServer (sender)) {
			Player assassinPlayer = PlayerByID (assassinID);
			Player victimPlayer = PlayerByID (victimID);

			assassinPlayer.immune = 0f;

			if (assassinPlayer == myPlayer) {
				// YOU SLAYED VICTIMCODE
				textBigAlpha = 1f;
				textBig.GetComponent<Text>().text = "<color=#77FF77>YOU KILLED</color> <color=#FF4444>#"+victimPlayer.name+"</color>";
			} else if (victimPlayer == myPlayer) {
				// SLAYED BY ASSASSINCODE
				textBigAlpha = 1f;
				textBig.GetComponent<Text>().text = "<color=#FF7777>KILLED</color> BY <color=#FF4444>#"+assassinPlayer.name+"</color>";
			}
			assassinPlayer.lastKillRemainingSeconds = remainingSeconds;

			victimPlayer.dead = true;
			victimPlayer.visualAvatar.GetComponent<RagdollScript> ().Enable ();
			// HACKING ITSELF REPRESENTS HACKING NO ONE
			victimPlayer.hackingNetworkPlayer = victimPlayer.networkPlayer;

			if (victimPlayer != myPlayer)
			{
				for (int i = 0; i < victimPlayer.visualMaterials.Length; i++)
				{
					victimPlayer.visualMaterials[i].SetFloat("_Cutoff", 0f);
				}
			}

			Vector3 forceDirection = victimPlayer.cameraMockup.transform.position - assassinPlayer.cameraMockup.transform.position;
			forceDirection.Normalize ();
			victimPlayer.visualAvatar.GetComponent<RagdollScript> ().rootGameObject.GetComponent<Rigidbody> ().AddForce (forceDirection * 7000f);
		}

	}
		

	[RPC]
	void respawnRPC(int ID, Vector3 position, Vector3 eulerAngles, NetworkMessageInfo info)
	{
		
		NetworkPlayer sender = info.sender;
		//handle the fact that unity is a bit dumb and calls the local player "-1" instead of its real networkplayer!!
		if(sender.ToString() == "-1") { sender = Network.player; }

		if (NetworkPlayerIsServer (sender)) {

			Player player = PlayerByID (ID);

			if (player == myPlayer) {
				localPlayer.respawn ();
				localPlayer.GetComponent<Rigidbody> ().velocity = new Vector3 (0f, 0f, 0f);
				localPlayer.transform.position = position;
				localPlayer.transform.eulerAngles = eulerAngles;
				myPlayer.targetPosition = position;
				myPlayer.targetAvatarEulerY = eulerAngles.y;
				localPlayer.hackResource = 3f;
				localPlayer.interceptResource = 3f;
				localPlayer.blinkResource = 3f;
				justRespawned = true;
			} else if (player != null) {
				player.visualAvatar.transform.position = position;
				player.targetPosition = position;
				player.visualAvatar.transform.eulerAngles = eulerAngles;
				player.targetAvatarEulerY = eulerAngles.y;
			}

			if (player != null) {
				player.visualAvatar.GetComponent<RagdollScript> ().Disable ();
				player.immune = immuneTimeAtRespawn;
				player.dead = false;
			}
		}

	}

	[RPC]
	void winnerRPC(int ID, NetworkMessageInfo info)
	{

		NetworkPlayer sender = info.sender;
		//handle the fact that unity is a bit dumb and calls the local player "-1" instead of its real networkplayer!!
		if(sender.ToString() == "-1") { sender = Network.player; }

		if (NetworkPlayerIsServer (sender)) {
			winnerPlayer = PlayerByID(ID);
			gameEnded = true;
		}

	}

	// CLASSES
	public class Player {

		public string name;
		public int ID = -1;
		public NetworkPlayer networkPlayer;
		public GameObject visualAvatar;
		public GameObject cameraMockup;
		public GameObject[] vitalPoints;
		public string lastAnimationOrder = "Idle01";
		public Material[] visualMaterials;
		public bool dead = false;
		public float deadTime = 0f;
		public float immune = 0f;
		public int kills = 0;
		public float lastKillRemainingSeconds = float.MaxValue;
		public int ping = 0;
		public NetworkPlayer hackingNetworkPlayer;
		public float hackingTimer = 0f;
		public bool justHacked = false;

		public Vector3 targetPosition;
		public float targetAvatarEulerY;
		public Vector3 cameraForward;
		public float targetCameraEulerX;
		public float currentCameraEulerX;

		public Player(int ID) {

			visualAvatar = GlobalData.clientScript.GetVisualAvatar();
			Initialize(ID, visualAvatar);

		}

		public Player(int ID, GameObject visualAvatar) {

			Initialize(ID, visualAvatar);

		}

		public void Initialize (int ID) {
			
			Initialize (ID, visualAvatar);

		}

		public void Initialize (int ID, GameObject visualAvatar) {

			this.ID = ID;
			name = "Player" + ID;
			hackingNetworkPlayer = networkPlayer;
			this.visualAvatar = visualAvatar;
			visualAvatar.SetActive (true);
			visualAvatar.name = "VisualAvatar "+ ID;
			visualAvatar.GetComponent<PlayerMarker>().player = this;
			visualMaterials = visualAvatar.transform.FindChild("Mesh").GetComponent<SkinnedMeshRenderer>().materials;

			targetPosition = visualAvatar.transform.position;
			targetAvatarEulerY = visualAvatar.transform.eulerAngles.y;
			cameraForward = Vector3.forward;
			cameraMockup = visualAvatar.transform.FindChild ("CameraMockup").gameObject;
			cameraMockup.transform.localPosition = LocalPlayerScript.centerOfCamera;

			vitalPoints = new GameObject[3];
			vitalPoints [0] = visualAvatar.transform.FindChild ("Armature/Pelvis").gameObject;
			vitalPoints [1] = visualAvatar.transform.FindChild ("Armature/Pelvis/Spine/Chest").gameObject;
			vitalPoints [2] = visualAvatar.transform.FindChild ("Armature/Pelvis/Spine/Chest/Neck/Head").gameObject;

		}

		public void SmartCrossfade(string animation) {

			Animator animator = visualAvatar.GetComponent<Animator> ();

			if (lastAnimationOrder != animation && !animator.GetCurrentAnimatorStateInfo(0).IsName(animation)) {
                animator.CrossFadeInFixedTime(animation, GlobalData.crossfadeAnimation);
				lastAnimationOrder = animation;
			}

		}

	}

}
