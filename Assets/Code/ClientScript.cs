using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Networking;

public class ClientScript : NetworkBehaviour {

	[HideInInspector] public GameScript gameScript;

	public float currentRankingCooldown = 0f;

	private int positionUpdatesPerSecond = 15;
	private float currentUpdateCooldown = 0f;

	public float remainingSeconds = 600f;

	private ChatManager chatManager;

	private GameObject rankingBackground;
	private GameObject textTargeted;
	public GameObject map;

	public LocalPlayerScript localPlayer;
	private string myCode;
	public List<Player> listPlayers = new List<Player>();

	// Use this for initialization
	void Awake () {
	
		chatManager = new ChatManager(GameObject.Find ("Canvas/ChatPanel/ScrollRect/AllChat"));

		rankingBackground = GameObject.Find ("Canvas/RankingBackground");
		rankingBackground.SetActive (false);

		textTargeted = GameObject.Find ("Canvas/TextTargeted");
		textTargeted.SetActive (false);

		map = Instantiate (Resources.Load("Prefabs/Maps/Map_Portal") as GameObject);

		localPlayer = Instantiate (Resources.Load("Prefabs/LocalPlayer") as GameObject).GetComponent<LocalPlayerScript>();
		localPlayer.gameScript = gameScript;

		// TE UNES COMO PLAYER
		myCode = Network.player.ToString ();
		Player aux = new Player(myCode);
		Destroy (aux.visualAvatar); 
		aux.visualAvatar = localPlayer.visualAvatar;
		Destroy (aux.visualMaterial);
		aux.visualMaterial = aux.visualAvatar.transform.FindChild("Mesh").GetComponent<SkinnedMeshRenderer>().material;
		listPlayers.Add(aux);

	}
	
	// Update is called once per frame
	void Update () {

		currentRankingCooldown += Time.deltaTime;

		PlayerByCode (myCode).attacking = localPlayer.attacking;

		if (EventSystem.current.currentSelectedGameObject == chatManager.chatInputField) {
			chatManager.lastChatPannelInteraction = 0f;
		} else if (chatManager.lastChatPannelInteraction < chatManager.chatPannelInteractionThreshold) {
			chatManager.lastChatPannelInteraction += Time.deltaTime;
		}

		remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.deltaTime);

		checkIfActivateChat ();
		updateChat ();
		updateRanking ();
		updateMyInfoInOtherClients ();
		synchronizeOtherPlayers ();

		if (localPlayer.crossHairTargeted == null) {
			textTargeted.SetActive(false);
		}
		else {
			textTargeted.SetActive(true);
			for (int j = 0; j < listPlayers.Count; j++) {
				if (listPlayers[j].visualAvatar == localPlayer.GetComponent<LocalPlayerScript>().crossHairTargeted) {
					textTargeted.GetComponent<Text>().text = "<Player "+listPlayers[j].playerCode+">";
				}
			}
		}

		if (Input.GetKeyDown (KeyCode.Escape)) {
			Network.Disconnect ();
		}
	
	}

	void updateMyInfoInOtherClients() {


		if (currentUpdateCooldown >= 1f / (float)positionUpdatesPerSecond) {

			currentUpdateCooldown = 0f;

			if (localPlayer != null && GetComponent<NetworkView> () != null) {

				GameObject localVisualAvatar = localPlayer.GetComponent<LocalPlayerScript> ().visualAvatar;
				string currentMode = localPlayer.GetComponent<LocalPlayerScript>().currentMode;
				bool sprintActive = (localPlayer.GetComponent<LocalPlayerScript>().sprintActive > 0f);

				Cmd_updatePlayerRPC(Network.player.ToString(), localVisualAvatar.transform.position, localVisualAvatar.transform.eulerAngles, localPlayer.GetComponent<LocalPlayerScript> ().lastAnimationOrder, currentMode, sprintActive, localPlayer.GetComponent<LocalPlayerScript> ().attacking);

			}


		} else {

			currentUpdateCooldown += Time.deltaTime;

		}

	}

	void synchronizeOtherPlayers () {

		for (int i = 0; i < listPlayers.Count; i++) {

			if (listPlayers [i].playerCode != myCode) {

				listPlayers[i].visualAvatar.transform.position = Hacks.LerpVector3(listPlayers[i].visualAvatar.transform.position, listPlayers[i].targetPosition, Time.deltaTime*10f);
				listPlayers[i].visualAvatar.transform.eulerAngles = Hacks.LerpVector3Angle(listPlayers[i].visualAvatar.transform.eulerAngles, listPlayers[i].targetRotation, Time.deltaTime*10f);
				listPlayers [i].attacking = Mathf.Max (0f, listPlayers [i].attacking - Time.deltaTime);
				listPlayers [i].immune = Mathf.Max (0f, listPlayers [i].immune - Time.deltaTime);

				if (listPlayers[i].currentMode == "regular") {

					Color c = Color.Lerp(listPlayers[i].visualMaterial.GetColor("_Color"), new Color(1f, 1f-listPlayers [i].attacking, 1f-listPlayers [i].attacking, 1f), Time.fixedDeltaTime*5f);
					//Color c = Color.Lerp(listOtherPlayers[i].visualMaterial.GetColor("_Color"), new Color(1f, 1f, 1f, 1f), Time.fixedDeltaTime*5f);
					listPlayers[i].visualMaterial.SetColor("_Color", c);

				} else if (listPlayers[i].currentMode == "stealth") {

					Color c = Color.Lerp(listPlayers[i].visualMaterial.GetColor("_Color"), new Color(1f, 1f-listPlayers [i].attacking, 1f-listPlayers [i].attacking, 0.4f), Time.fixedDeltaTime*5f);
					//Color c = Color.Lerp(listOtherPlayers[i].visualMaterial.GetColor("_Color"), new Color(1f, 1f, 1f, 0.4f), Time.fixedDeltaTime*5f);
					listPlayers[i].visualMaterial.SetColor("_Color", c);

				}

				if (listPlayers[i].sprintActive && !listPlayers[i].sprintTrail.Emit) {
					listPlayers[i].sprintTrail.Emit = true;
				}
				else if (!listPlayers[i].sprintActive && listPlayers[i].sprintTrail.Emit) {
					listPlayers[i].sprintTrail.Emit = false;
				}

			}

		}

	}

	void updateRanking() {

		if (Input.GetKey (KeyCode.Tab)) {

			string auxPlayers = "Player\n\n";
			string auxKills = "Kills\n\n";
			string auxPings = "Ping\n\n";
			int totalSeconds = (int) Mathf.Floor(remainingSeconds);
			int seconds = totalSeconds % 60;
			int minutes = totalSeconds / 60;
			string auxTime = minutes.ToString("00") + ":" + seconds.ToString("00");


			for (int i = 0; i < listPlayers.Count; i++) {
				if (Network.player.ToString() == listPlayers[i].playerCode) {
					auxPlayers += "<color=#D7D520>" + "Player"+listPlayers[i].playerCode + "</color>";
				}
				else {
					auxPlayers += "Player"+listPlayers[i].playerCode;
				}
				auxKills += "<color=#FF8C8CFF>"+ listPlayers[i].kills + "</color>";
				auxPings += listPlayers[i].ping +"";
				if (i != listPlayers.Count -1) { 
					auxPlayers += "\n"; 
					auxKills += "\n"; 
					auxPings += "\n";
				}
			}

			rankingBackground.transform.FindChild ("TextPlayers").GetComponent<Text> ().text = auxPlayers;
			rankingBackground.transform.FindChild ("TextKills").GetComponent<Text> ().text = auxKills;
			rankingBackground.transform.FindChild ("TextPings").GetComponent<Text> ().text = auxPings;
			rankingBackground.transform.FindChild("TimeBackground/TextTime").GetComponent<Text> ().text = auxTime;

			rankingBackground.SetActive (true);
			chatManager.chatPanel.SetActive(false);

		} else {
			
			rankingBackground.SetActive (false);

			// SHOW CHAT PANEL
			if (chatManager.lastChatPannelInteraction >= chatManager.chatPannelInteractionThreshold) {
					chatManager.chatPanel.SetActive (false);
			} else {
					chatManager.chatPanel.SetActive (true);
			}

		}

	}

	void checkIfActivateChat() {

		if (Input.GetKeyDown (KeyCode.Return) && localPlayer.GetComponent<LocalPlayerScript> ().receiveInput) {

			chatManager.chatPanel.SetActive(true);
			chatManager.lastChatPannelInteraction = 0f;

			localPlayer.GetComponent<LocalPlayerScript> ().receiveInput = false;

			EventSystem.current.SetSelectedGameObject(chatManager.chatInputField, null);
			chatManager.chatInputField.GetComponent<InputField> ().OnPointerClick(new PointerEventData(EventSystem.current));

		}

	}

	void updateChat() {

		if (Input.GetKeyDown(KeyCode.Return)
			&& chatManager.lastTimeChatInputFocused) {

			if (chatManager.chatInputField.GetComponent<InputField> ().text != "") {

				string info = chatManager.chatInputField.GetComponent<InputField> ().text;
				GetComponent<NetworkView>().RPC("addChatMessageRPC", RPCMode.All, Network.player.ToString(), info);
				chatManager.chatInputField.GetComponent<InputField> ().text = "";

			}

			EventSystem.current.SetSelectedGameObject(null);
			localPlayer.GetComponent<LocalPlayerScript> ().receiveInput = true;

		}

		chatManager.lastTimeChatInputFocused = chatManager.chatInputField.GetComponent<InputField> ().isFocused;

	}

	public NetworkPlayer NetworkPlayerByCode(string playerCode) {

		if (Network.player.ToString () == playerCode) {
			return Network.player;
		}

		for (int i = 0; i < Network.connections.Length; i++) {
			if (Network.connections[i].ToString() == playerCode) {
				return Network.connections[i];
			}
		}

		return Network.connections[0];

	}

	public Player PlayerByCode(string playerCode) {

		for (int i = 0; i < listPlayers.Count; i++) {
			if (listPlayers[i].playerCode == playerCode) {
				return listPlayers [i];
			}
		}

		return null;

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

		Application.LoadLevel ("Menu");
	}

	// CLIENT RPCs
	[Command]
	void Cmd_updatePlayerRPC(string playerCode, Vector3 position, Vector3 rotation, string currentAnimation, string currentMode, bool sprintActive, float attacking)
	{
		bool foundPlayer = false;

		for (int i = 0; i < listPlayers.Count; i++) {

			if (listPlayers[i].playerCode == playerCode) {
				listPlayers[i].targetPosition = position;
				listPlayers[i].targetRotation = rotation;
				listPlayers[i].SmartCrossfade(currentAnimation);
				listPlayers[i].currentMode = currentMode;
				listPlayers[i].sprintActive = sprintActive;
				listPlayers[i].attacking = attacking;
				foundPlayer = true;
				break;
			}

		}

		if (!foundPlayer) {

			Player aux = new Player(playerCode);
			listPlayers.Add(aux);
			aux.visualAvatar.transform.position = position;
			aux.visualAvatar.transform.eulerAngles = rotation;
			aux.targetPosition = position;
			aux.targetRotation = rotation;
			aux.SmartCrossfade(currentAnimation);
			aux.currentMode = currentMode;

			if (Network.isServer) {
				// SE ACABA DE UNIR UN JUGADOR, ASI QUE LES DECIMOS A TODOS LA NUEVA SITUACION DEL RANKING
				gameScript.serverScript.sendRankingData();
				GetComponent<NetworkView>().RPC("sendRemainingSecondsRPC", RPCMode.Others, playerCode, remainingSeconds);

			}

		}

		Debug.Log ("JEJE");

	}

	[RPC]
	void addChatMessageRPC(string playerCode, string text)
	{
		string owner = "Player " + playerCode;
		if (playerCode == Network.player.ToString()) { owner = "You"; }

		ChatMessage message = new ChatMessage (owner, text);
		chatManager.Add (message);
		chatManager.Write ();
		chatManager.lastChatPannelInteraction = 0f;
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

	}

	[RPC]
	void updateRankingRPC(string playerCode, int kills, int ping)
	{
		Player player = PlayerByCode (playerCode);
		player.kills = kills;
		player.ping = ping;
	}

	[RPC]
	void respawnRPC(string playerCode, Vector3 position, Vector3 eulerAngles)
	{
		if (Network.player.ToString () == playerCode) {
			localPlayer.transform.position = position;
			localPlayer.transform.eulerAngles = eulerAngles;
		}
	}

	// CLASSES
	public class Player {

		public string playerCode;
		public GameObject visualAvatar;
		public string lastAnimationOrder = "Idle01";
		public Material visualMaterial;
		public string currentMode = "regular";
		public bool sprintActive = false;
		public MeleeWeaponTrail sprintTrail;
		public float attacking = 0f;
		public float immune = 0f;
		public int kills = 0;
		public int ping = 0;

		public Vector3 targetPosition;
		public Vector3 targetRotation;

		public Player(string auxPlayerCode) {

			playerCode = auxPlayerCode;
			visualAvatar = Instantiate (Resources.Load("Prefabs/Subject") as GameObject);
			visualAvatar.name = "VisualAvatar "+playerCode;
			visualAvatar.GetComponent<PlayerMarker>().player = this;
			visualMaterial = visualAvatar.transform.FindChild("Mesh").GetComponent<SkinnedMeshRenderer>().material;
			sprintTrail = visualAvatar.transform.FindChild ("Mesh/Trail").gameObject.GetComponent<MeleeWeaponTrail>();
			sprintTrail.Emit = false;

			targetPosition = visualAvatar.transform.position;
			targetRotation = visualAvatar.transform.eulerAngles;

		}

		public void SmartCrossfade(string animation) {

			Animator animator = visualAvatar.GetComponent<Animator> ();

			if (lastAnimationOrder != animation && !animator.GetCurrentAnimatorStateInfo(0).IsName(animation)) {
				animator.CrossFade(animation, GlobalData.crossfadeAnimation);
				lastAnimationOrder = animation;
			}

		}

	}

}
