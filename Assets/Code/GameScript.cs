using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameScript : MonoBehaviour {

	private ChatManager chatManager;
	private GameObject chatInputField;
	private GameObject chatPanel;
	private bool lastTimeChatInputFocused = false;
	private float lastChatPannelInteraction = 0f;
	private float chatPannelInteractionThreshold = 8f;

	public List<RankingPlayer> allRankingPlayers = new List<RankingPlayer>();
	private GameObject rankingBackground;
	private bool rankingClearedSinceLastCheck = false;

	private GameObject map;
	private GameObject localPlayer;
	private List<OtherPlayer> listOtherPlayers = new List<OtherPlayer>();

	private int positionUpdatesPerSecond = 15;
	private float currentUpdateCooldown = 0f;



	// Use this for initialization
	void Start () {

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		// AQUI HAY QUE PASARLE AL CHATMANAGER EL GAMEOBJECT QUE ES EL CONTENT DONDE IRAN LOS MENSAJES
		chatManager = new ChatManager(GameObject.Find ("Canvas/ChatPanel/ScrollRect/AllChat"));

		chatInputField = GameObject.Find ("Canvas/ChatPanel/InputField");
		chatPanel = GameObject.Find ("Canvas/ChatPanel");

		rankingBackground = GameObject.Find ("Canvas/RankingBackground");
		rankingBackground.SetActive (false);

		map = Instantiate (Resources.Load("Prefabs/Maps/Map_Test") as GameObject);

		localPlayer = Instantiate (Resources.Load("Prefabs/LocalPlayer") as GameObject);

		if (Network.isServer) {
			// SI TU ERES EL SERVER, TE AGREGAS A TI MISMO COMO UN RANKINGPLAYER
			allRankingPlayers.Add(new RankingPlayer(Network.player.ToString()));
			rankingClearedSinceLastCheck = true;
		}
	
	}
	
	// Update is called once per frame
	void Update () {

		if (EventSystem.current.currentSelectedGameObject == chatInputField) {
			lastChatPannelInteraction = 0f;
		} else if (lastChatPannelInteraction < chatPannelInteractionThreshold) {
			lastChatPannelInteraction += Time.deltaTime;
		}

		checkIfActivateChat ();
		updateChat ();
		checkRanking ();
		updateMyPositionInOtherClients ();
		synchronizeOtherPlayers ();

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
	void updatePlayerRPC(string playerCode, Vector3 position, Vector3 rotation, string currentAnimation)
	{
		bool foundPlayer = false;

		for (int i = 0; i < listOtherPlayers.Count; i++) {

			if (listOtherPlayers[i].playerCode == playerCode) {
				listOtherPlayers[i].targetPosition = position;
				listOtherPlayers[i].targetRotation = rotation;
				listOtherPlayers[i].SmartCrossfade(currentAnimation);
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

			if (Network.isServer) {
				// SE ACABA DE UNIR UN JUGADOR, ASI QUE LES DECIMOS A TODOS LA NUEVA SITUACION DEL RANKING
				allRankingPlayers.Add(new RankingPlayer(playerCode));
				allRankingPlayers.Sort (CompareListByKills);

				serverSendRankingData();

			}

		}

	}

	void serverSendRankingData() {

		GetComponent<NetworkView>().RPC("clearRankingRPC", RPCMode.Others);
		
		for (int i = 0; i < allRankingPlayers.Count; i++) {
			GetComponent<NetworkView>().RPC("addRankingRPC", RPCMode.Others, allRankingPlayers[i].playerCode, allRankingPlayers[i].kills);
		}

		GetComponent<NetworkView>().RPC("endedRankingRPC", RPCMode.Others);

		rankingClearedSinceLastCheck = true;
		
	}

	[RPC]
	void clearRankingRPC()
	{
		allRankingPlayers = new List<RankingPlayer> ();
	}

	[RPC]
	void addRankingRPC(string playerCode, int kills) 
	{
		RankingPlayer rp = new RankingPlayer (playerCode);
		rp.kills = kills;
		allRankingPlayers.Add (rp);
	}

	[RPC]
	void endedRankingRPC()
	{
		rankingClearedSinceLastCheck = true;
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

	void checkRanking() {

		if (Input.GetKey (KeyCode.Tab)) {

			if (rankingClearedSinceLastCheck) {
				rankingClearedSinceLastCheck = false;
				updateRankingText();
			}

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

		string auxPlayers = "PLAYERS\n\n";
		string auxKills = "KILLS\n\n";

		
		for (int i = 0; i < allRankingPlayers.Count; i++) {
			auxPlayers += "Player"+allRankingPlayers[i].playerCode;
			auxKills += "<color=#FF8C8CFF>"+ allRankingPlayers[i].kills + "</color>";
			if (i != allRankingPlayers.Count -1) { 
				auxPlayers += "\n"; 
				auxKills += "\n"; 
			}
		}

		rankingBackground.transform.FindChild ("TextPlayers").GetComponent<Text> ().text = auxPlayers;
		rankingBackground.transform.FindChild ("TextKills").GetComponent<Text> ().text = auxKills;

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

	void updateMyPositionInOtherClients() {


		if (currentUpdateCooldown >= 1f / (float)positionUpdatesPerSecond) {
			
			currentUpdateCooldown = 0f;

			if (localPlayer != null && GetComponent<NetworkView> () != null) {
				
				GameObject localVisualAvatar = localPlayer.GetComponent<LocalPlayerScript> ().visualAvatar;
				GetComponent<NetworkView>().RPC("updatePlayerRPC", RPCMode.Others, Network.player.ToString(), localVisualAvatar.transform.position, localVisualAvatar.transform.eulerAngles, localPlayer.GetComponent<LocalPlayerScript> ().lastAnimationOrder);
				
			}
			
			
		} else {

			currentUpdateCooldown += Time.deltaTime;

		}

	}

	void synchronizeOtherPlayers () {

		for (int i = 0; i < listOtherPlayers.Count; i++) {

			listOtherPlayers[i].visualAvatar.transform.position = Hacks.LerpVector3(listOtherPlayers[i].visualAvatar.transform.position, listOtherPlayers[i].targetPosition, Time.deltaTime*10f);
			listOtherPlayers[i].visualAvatar.transform.eulerAngles = Hacks.LerpVector3Angle(listOtherPlayers[i].visualAvatar.transform.eulerAngles, listOtherPlayers[i].targetRotation, Time.deltaTime*10f);
			
		}

	}

	public class OtherPlayer {

		public string playerCode;
		public GameObject visualAvatar;
		public string lastAnimationOrder = "Idle01";

		public Vector3 targetPosition;
		public Vector3 targetRotation;

		public OtherPlayer(string auxPlayerCode) {

			playerCode = auxPlayerCode;
			visualAvatar = Instantiate (Resources.Load("Prefabs/ToonSoldier") as GameObject);
			visualAvatar.name = "VisualAvatar "+playerCode;

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

		public RankingPlayer(string auxPlayerCode) {

			playerCode = auxPlayerCode;
			kills = 0;

		}

	}

	private static int CompareListByKills(RankingPlayer rp1, RankingPlayer rp2)
	{
		return rp1.kills.CompareTo(rp2.kills); 
	}

}


