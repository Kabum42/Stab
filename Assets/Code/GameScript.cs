﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class GameScript : MonoBehaviour {

	private ChatManager chatManager;
	private GameObject chatInputField;
	private GameObject chatPanel;
	private bool lastTimeChatInputFocused = false;
	private float lastChatPannelInteraction = 0f;
	private float chatPannelInteractionThreshold = 8f;

	public List<RankingPlayer> allRankingPlayers = new List<RankingPlayer>();
	private GameObject rankingBackground;
	private float currentRankingCooldown = 0f;

	private GameObject map;
	private GameObject localPlayer;
	private List<OtherPlayer> listOtherPlayers = new List<OtherPlayer>();

	private int positionUpdatesPerSecond = 15;
	private float currentUpdateCooldown = 0f;

	private float remainingSeconds = 600f;

	private GameObject textTargeted;

	// Use this for initialization
	void Start () {

		GlobalData.Start ();

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		// AQUI HAY QUE PASARLE AL CHATMANAGER EL GAMEOBJECT QUE ES EL CONTENT DONDE IRAN LOS MENSAJES
		chatManager = new ChatManager(GameObject.Find ("Canvas/ChatPanel/ScrollRect/AllChat"));

		chatInputField = GameObject.Find ("Canvas/ChatPanel/InputField");
		chatPanel = GameObject.Find ("Canvas/ChatPanel");

		rankingBackground = GameObject.Find ("Canvas/RankingBackground");
		rankingBackground.SetActive (false);

		textTargeted = GameObject.Find ("Canvas/TextTargeted");
		textTargeted.SetActive (false);

		map = Instantiate (Resources.Load("Prefabs/Maps/Map_Portal") as GameObject);

		localPlayer = Instantiate (Resources.Load("Prefabs/LocalPlayer") as GameObject);
        localPlayer.GetComponent<LocalPlayerScript>().gameScript = this;

		if (Network.isServer) {
			// SI TU ERES EL SERVER, TE AGREGAS A TI MISMO COMO UN RANKINGPLAYER
			RankingPlayer rp = new RankingPlayer (Network.player.ToString ());
			rp.ping = 0;
			allRankingPlayers.Add (rp);
		}
	
	}
	
	// Update is called once per frame
	void Update () {

		if (EventSystem.current.currentSelectedGameObject == chatInputField) {
			lastChatPannelInteraction = 0f;
		} else if (lastChatPannelInteraction < chatPannelInteractionThreshold) {
			lastChatPannelInteraction += Time.deltaTime;
		}

		remainingSeconds -= Time.deltaTime;
		if (remainingSeconds < 0f) { remainingSeconds = 0f; }

		checkIfActivateChat ();
		updateChat ();

		if (Network.isServer) { 
			checkForKillings ();
			checkIfSendRankingData(); 
		}

		checkRanking ();
		updateMyInfoInOtherClients ();
		synchronizeOtherPlayers ();

	}

	void FixedUpdate() {

		if (localPlayer.GetComponent<LocalPlayerScript>().crossHairTargeted == null) {
			textTargeted.SetActive(false);
		}
		else {
			textTargeted.SetActive(true);
			for (int j = 0; j < listOtherPlayers.Count; j++) {
				if (listOtherPlayers[j].visualAvatar == localPlayer.GetComponent<LocalPlayerScript>().crossHairTargeted) {
					textTargeted.GetComponent<Text>().text = "<Player "+listOtherPlayers[j].playerCode+">";
				}
			}
		}

	}

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

	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log("Clean up after player " + player);
		Network.RemoveRPCs(player);
		//Network.DestroyPlayerObjects(player);
		GetComponent<NetworkView>().RPC("removePlayerRPC", RPCMode.All, player.ToString());
	}

	[RPC]
	void sendRemainingSecondsRPC(string playerCode, float auxRemainingSeconds)
	{
		if (playerCode == Network.player.ToString()) {
			remainingSeconds = auxRemainingSeconds;
		}
	}

	[RPC]
	void addChatMessageRPC(string playerCode, string text)
	{
		string owner = "Player " + playerCode;
		if (playerCode == Network.player.ToString()) { owner = "You"; }

		addChatMessage (new ChatMessage (owner, text));
	}

	[RPC]
	void removePlayerRPC(string playerCode) {

		for (int i = 0; i < listOtherPlayers.Count; i++) {
			
			if (listOtherPlayers[i].playerCode == playerCode) {

				Destroy(listOtherPlayers[i].visualAvatar);
				listOtherPlayers.RemoveAt(i);

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

	[RPC]
	void updatePlayerRPC(string playerCode, Vector3 position, Vector3 rotation, string currentAnimation, string currentMode, bool sprintActive, float attacking)
	{
		bool foundPlayer = false;

		for (int i = 0; i < listOtherPlayers.Count; i++) {

			if (listOtherPlayers[i].playerCode == playerCode) {
				listOtherPlayers[i].targetPosition = position;
				listOtherPlayers[i].targetRotation = rotation;
				listOtherPlayers[i].SmartCrossfade(currentAnimation);
				listOtherPlayers[i].currentMode = currentMode;
				listOtherPlayers[i].sprintActive = sprintActive;
				listOtherPlayers[i].attacking = attacking;
				foundPlayer = true;
				break;
			}

		}

		if (!foundPlayer) {

			OtherPlayer aux = new OtherPlayer(playerCode);
			listOtherPlayers.Add(aux);
			aux.visualAvatar.transform.position = position;
			aux.visualAvatar.transform.eulerAngles = rotation;
			aux.targetPosition = position;
			aux.targetRotation = rotation;
			aux.SmartCrossfade(currentAnimation);
			aux.currentMode = currentMode;

			if (Network.isServer) {
				// SE ACABA DE UNIR UN JUGADOR, ASI QUE LES DECIMOS A TODOS LA NUEVA SITUACION DEL RANKING
				RankingPlayer rp = new RankingPlayer(playerCode);
				rp.ping = Network.GetAveragePing(NetworkPlayerByCode(playerCode));
				allRankingPlayers.Add(rp);
				allRankingPlayers.Sort (CompareListByKills);

				serverSendRankingData();

				GetComponent<NetworkView>().RPC("sendRemainingSecondsRPC", RPCMode.Others, playerCode, remainingSeconds);

			}

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

	[RPC]
	void clearRankingRPC()
	{
		allRankingPlayers = new List<RankingPlayer> ();
	}

	[RPC]
	void addRankingRPC(string playerCode, int kills, int ping) 
	{
		RankingPlayer rp = new RankingPlayer (playerCode);
		rp.kills = kills;
		rp.ping = ping;
		allRankingPlayers.Add (rp);
	}

	NetworkPlayer NetworkPlayerByCode(string playerCode) {

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

	void checkIfActivateChat() {

		if (Input.GetKeyDown (KeyCode.Return) && localPlayer.GetComponent<LocalPlayerScript> ().receiveInput) {

			if (!chatPanel.activeInHierarchy) {
				chatPanel.SetActive(true);
				lastChatPannelInteraction = 0f;
			}

			localPlayer.GetComponent<LocalPlayerScript> ().receiveInput = false;
			
			EventSystem.current.SetSelectedGameObject(chatInputField, null);
			chatInputField.GetComponent<InputField> ().OnPointerClick(new PointerEventData(EventSystem.current));

		}

	}

	void updateChat() {

		if (Input.GetKeyDown(KeyCode.Return)
			&& lastTimeChatInputFocused) {

			if (chatInputField.GetComponent<InputField> ().text != "") {

				string info = chatInputField.GetComponent<InputField> ().text;
				GetComponent<NetworkView>().RPC("addChatMessageRPC", RPCMode.All, Network.player.ToString(), info);
				chatInputField.GetComponent<InputField> ().text = "";

			}

			EventSystem.current.SetSelectedGameObject(null);
			localPlayer.GetComponent<LocalPlayerScript> ().receiveInput = true;

		}

		lastTimeChatInputFocused = chatInputField.GetComponent<InputField> ().isFocused;

	}

	void addChatMessage(ChatMessage chatMessage) {

		chatManager.Add(chatMessage);
		chatManager.Update ();
		lastChatPannelInteraction = 0f;

	}

	void checkForKillings() {

		bool ownPlayerDead = false;
		List<OtherPlayer> otherPlayersDead = new List<OtherPlayer> ();

		// AQUI FALTA COMPROBAR SI HAN MATADO AL JUGADOR DEL SERVER
		if (localPlayer.GetComponent<LocalPlayerScript> ().attacking > 0f) {

			List<GameObject> currentTriggering = localPlayer.GetComponent<LocalPlayerScript> ().visualAvatar.GetComponent<PlayerMarker> ().ownAttacker.listTriggering;

			for (int j = 0; j < currentTriggering.Count; j++) {

				OtherPlayer currentOtherPlayer = currentTriggering[j].GetComponent<PlayerMarker>().otherPlayer;

				if (currentOtherPlayer.immune == 0f) {

					RankingPlayerByCode(Network.player.ToString()).kills++;
					currentOtherPlayer.immune = 5f;
					otherPlayersDead.Add (currentOtherPlayer);

				}

			}

		}


		for (int i = 0; i < listOtherPlayers.Count; i++) {

			if (listOtherPlayers [i].attacking > 0f) {

				List<GameObject> currentTriggering = listOtherPlayers [i].visualAvatar.GetComponent<PlayerMarker> ().ownAttacker.listTriggering;

				for (int j = 0; j < currentTriggering.Count; j++) {

					OtherPlayer currentOtherPlayer = currentTriggering[j].GetComponent<PlayerMarker>().otherPlayer;

					if (currentOtherPlayer.immune == 0f) {

						RankingPlayerByCode(listOtherPlayers [i].playerCode).kills++;
						currentOtherPlayer.immune = 5f;
						otherPlayersDead.Add (currentOtherPlayer);

					}

				}
			}

		}
			

	}

	void checkIfSendRankingData() {

		// SE VUELVE A COMPROBAR POR SI ACASO
		if (Network.isServer) {

			if (currentRankingCooldown >= 1f) {
				serverSendRankingData ();
			} else {
				currentRankingCooldown += Time.deltaTime;
			}

		}

	}

	void checkRanking() {

		if (Input.GetKey (KeyCode.Tab)) {

			updateRankingText();

			if (!rankingBackground.activeInHierarchy) {
				rankingBackground.SetActive (true);
				chatPanel.SetActive(false);
			}

		} else {

			if (rankingBackground.activeInHierarchy) {
				rankingBackground.SetActive (false);
			}

			showChatPanel();

		}

	}

	void updateRankingText() {

		string auxPlayers = "Player\n\n";
		string auxKills = "Kills\n\n";
		string auxPings = "Ping\n\n";
		int totalSeconds = (int) Mathf.Floor(remainingSeconds);
		int seconds = totalSeconds % 60;
		int minutes = totalSeconds / 60;
		string auxTime = minutes.ToString("00") + ":" + seconds.ToString("00");

		
		for (int i = 0; i < allRankingPlayers.Count; i++) {
			if (Network.player.ToString() == allRankingPlayers[i].playerCode) {
				auxPlayers += "<color=#D7D520>" + "Player"+allRankingPlayers[i].playerCode + "</color>";
			}
			else {
				auxPlayers += "Player"+allRankingPlayers[i].playerCode;
			}
			auxKills += "<color=#FF8C8CFF>"+ allRankingPlayers[i].kills + "</color>";
			auxPings += allRankingPlayers[i].ping +"";
			if (i != allRankingPlayers.Count -1) { 
				auxPlayers += "\n"; 
				auxKills += "\n"; 
				auxPings += "\n";
			}
		}

		rankingBackground.transform.FindChild ("TextPlayers").GetComponent<Text> ().text = auxPlayers;
		rankingBackground.transform.FindChild ("TextKills").GetComponent<Text> ().text = auxKills;
		rankingBackground.transform.FindChild ("TextPings").GetComponent<Text> ().text = auxPings;
		rankingBackground.transform.FindChild("TimeBackground/TextTime").GetComponent<Text> ().text = auxTime;

	}

	void showChatPanel() {

		if (lastChatPannelInteraction >= chatPannelInteractionThreshold) {
			if (chatPanel.activeInHierarchy) {
				chatPanel.SetActive (false);
			}
		} else {
			if (!chatPanel.activeInHierarchy) {
				chatPanel.SetActive (true);
			}
		}

	}

	void updateMyInfoInOtherClients() {


		if (currentUpdateCooldown >= 1f / (float)positionUpdatesPerSecond) {
			
			currentUpdateCooldown = 0f;

			if (localPlayer != null && GetComponent<NetworkView> () != null) {
				
				GameObject localVisualAvatar = localPlayer.GetComponent<LocalPlayerScript> ().visualAvatar;
				string currentMode = localPlayer.GetComponent<LocalPlayerScript>().currentMode;
				bool sprintActive = (localPlayer.GetComponent<LocalPlayerScript>().sprintActive > 0f);
				GetComponent<NetworkView>().RPC("updatePlayerRPC", RPCMode.Others, Network.player.ToString(), localVisualAvatar.transform.position, localVisualAvatar.transform.eulerAngles, localPlayer.GetComponent<LocalPlayerScript> ().lastAnimationOrder, currentMode, sprintActive, localPlayer.GetComponent<LocalPlayerScript> ().attacking);

			}
			
			
		} else {

			currentUpdateCooldown += Time.deltaTime;

		}

	}

	void synchronizeOtherPlayers () {

		for (int i = 0; i < listOtherPlayers.Count; i++) {

			listOtherPlayers[i].visualAvatar.transform.position = Hacks.LerpVector3(listOtherPlayers[i].visualAvatar.transform.position, listOtherPlayers[i].targetPosition, Time.deltaTime*10f);
			listOtherPlayers[i].visualAvatar.transform.eulerAngles = Hacks.LerpVector3Angle(listOtherPlayers[i].visualAvatar.transform.eulerAngles, listOtherPlayers[i].targetRotation, Time.deltaTime*10f);
			listOtherPlayers [i].attacking = Mathf.Max (0f, listOtherPlayers [i].attacking - Time.deltaTime);
			listOtherPlayers [i].immune = Mathf.Max (0f, listOtherPlayers [i].immune - Time.deltaTime);

			if (listOtherPlayers[i].currentMode == "regular") {

				Color c = Color.Lerp(listOtherPlayers[i].visualMaterial.GetColor("_Color"), new Color(1f, 1f-listOtherPlayers [i].attacking, 1f-listOtherPlayers [i].attacking, 1f), Time.fixedDeltaTime*5f);
				//Color c = Color.Lerp(listOtherPlayers[i].visualMaterial.GetColor("_Color"), new Color(1f, 1f, 1f, 1f), Time.fixedDeltaTime*5f);
				listOtherPlayers[i].visualMaterial.SetColor("_Color", c);

			} else if (listOtherPlayers[i].currentMode == "stealth") {

				Color c = Color.Lerp(listOtherPlayers[i].visualMaterial.GetColor("_Color"), new Color(1f, 1f-listOtherPlayers [i].attacking, 1f-listOtherPlayers [i].attacking, 0.4f), Time.fixedDeltaTime*5f);
				//Color c = Color.Lerp(listOtherPlayers[i].visualMaterial.GetColor("_Color"), new Color(1f, 1f, 1f, 0.4f), Time.fixedDeltaTime*5f);
				listOtherPlayers[i].visualMaterial.SetColor("_Color", c);

			}

			if (listOtherPlayers[i].sprintActive && !listOtherPlayers[i].sprintTrail.Emit) {
				listOtherPlayers[i].sprintTrail.Emit = true;
			}
			else if (!listOtherPlayers[i].sprintActive && listOtherPlayers[i].sprintTrail.Emit) {
				listOtherPlayers[i].sprintTrail.Emit = false;
			}
				
		}

	}

	/*
    public void requestAttack(Vector3 lookingAt)
    {
        if (Network.isServer)
        {
            requestedAttack(Network.player.ToString(), lookingAt);
        }
        else
        {
            GetComponent<NetworkView>().RPC("requestedAttackRPC", RPCMode.Server, Network.player.ToString(), lookingAt);
        }
    }
	*/

	public class OtherPlayer {

		public string playerCode;
		public GameObject visualAvatar;
		public string lastAnimationOrder = "Idle01";
		public Material visualMaterial;
		public string currentMode = "regular";
		public bool sprintActive = false;
		public MeleeWeaponTrail sprintTrail;
		public float attacking = 0f;
		public float immune = 0f;

		public Vector3 targetPosition;
		public Vector3 targetRotation;

		public OtherPlayer(string auxPlayerCode) {

			playerCode = auxPlayerCode;
			visualAvatar = Instantiate (Resources.Load("Prefabs/Subject") as GameObject);
			visualAvatar.name = "VisualAvatar "+playerCode;
			visualAvatar.GetComponent<PlayerMarker>().otherPlayer = this;
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

	public class RankingPlayer {

		public string playerCode;
		public int kills;
		public int ping;

		public RankingPlayer(string auxPlayerCode) {

			playerCode = auxPlayerCode;
			kills = 0;
			ping = 0;

		}

	}

	private static int CompareListByKills(RankingPlayer rp1, RankingPlayer rp2)
	{
		return rp1.kills.CompareTo(rp2.kills); 
	}

}


