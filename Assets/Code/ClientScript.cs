using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;

public class ClientScript : MonoBehaviour {

	[HideInInspector] public GameScript gameScript;

	private int positionUpdatesPerSecond = 15;
	private float currentUpdateCooldown = 0f;

	public float remainingSeconds = (8f)*(60f); // 5 MINUTES

	private ChatManager chatManager;

	private GameObject rankingBackground;
	private GameObject textTargeted;
	private GameObject textBig;
	private float textBigAlpha = 0f;
	public GameObject map;

	public LocalPlayerScript localPlayer;
	public string myCode;
	private Player myPlayer;
	public List<Player> listPlayers = new List<Player>();

	private static float slowSpeed = 1f/(1f);
	private static float hackingSpeed = 1f/(0.25f); // EL SEGUNDO NUMERO ES CUANTO TARDA EN TIEMPO REAL EN LLEGAR A 1
	private static float fastSpeed = 1f/(0.05f);

	private string winnerCode = "-1";

	public static float hackingTimerMax = 3f;

	// Use this for initialization
	void Awake () {
	
		chatManager = new ChatManager(GameObject.Find ("Canvas/ChatPanel/ScrollRect/AllChat"));

		rankingBackground = GameObject.Find ("Canvas/RankingBackground");
		rankingBackground.SetActive (false);

		textTargeted = GameObject.Find ("Canvas/TextTargeted");
		textTargeted.SetActive (false);

		textBig = GameObject.Find ("Canvas/TextBig");

		map = Instantiate (Resources.Load("Prefabs/Maps/Map_Portal") as GameObject);

		if (!Network.isServer) {
			map.transform.FindChild ("RespawnPoints").gameObject.SetActive (false);
		}

		localPlayer = Instantiate (Resources.Load("Prefabs/LocalPlayer") as GameObject).GetComponent<LocalPlayerScript>();

		// TE UNES COMO PLAYER
		myCode = Network.player.ToString ();
		myPlayer = new Player(myCode, localPlayer.visualAvatar);
		listPlayers.Add(myPlayer);

	}
	
	// Update is called once per frame
	void Update () {

		if (EventSystem.current.currentSelectedGameObject == chatManager.chatInputField) {
			chatManager.lastChatPannelInteraction = 0f;
		} else if (chatManager.lastChatPannelInteraction < chatManager.chatPannelInteractionThreshold) {
			chatManager.lastChatPannelInteraction += Time.deltaTime;
		}

		remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.deltaTime);

		checkIfActivateChat ();
		updateChat ();
		updateRanking ();
		updateCanvas ();
		updateMyInfoInOtherClients ();
		synchronizeOtherPlayers ();
		updateHacking ();

		if (remainingSeconds <= 0f) {
			localPlayer.gameEnded = true;

			if (winnerCode == "-1") {
				if (Network.isServer) { 
					listPlayers = listPlayers.OrderByDescending(o=>o.kills).ThenBy(o=>o.playerCode).ToList();
					GetComponent<NetworkView> ().RPC ("winnerRPC", RPCMode.All, listPlayers[0].playerCode);
				}
			} else if (winnerCode == myCode) {
				textBig.GetComponent<Text>().text = "<color=#44FF44>CONGRATULATIONS!</color> YOU WON";
			} else {
				textBig.GetComponent<Text>().text = "PLAYER <color=#FF4444>#"+winnerCode+"</color> HAS WON";
			}

			textBigAlpha = 1f;
		}

		textBig.GetComponent<CanvasRenderer> ().SetAlpha (textBigAlpha);

		if (Input.GetKeyDown (KeyCode.Escape)) {
			Network.Disconnect ();
		}
	
	}

	void LateUpdate() {

		for (int i = 0; i < listPlayers.Count; i++) {
			if (listPlayers [i] != myPlayer) {

				GameObject head = listPlayers [i].visualAvatar.transform.FindChild ("Armature/Pelvis/Spine/Chest/Neck/Head").gameObject;

				float target = -listPlayers [i].currentCameraEulerX;

				head.transform.eulerAngles = new Vector3 (head.transform.eulerAngles.x, head.transform.eulerAngles.y, target);

			}
		}

	}

	public void setGameScript(GameScript auxGameScript) {

		gameScript = auxGameScript;
		localPlayer.gameScript = auxGameScript;

	}

	void updateCanvas() {

		Player auxPlayer = firstLookingPlayer(myPlayer);

		if (auxPlayer == null) {
			localPlayer.crosshairHackDot.GetComponent<Image>().color = new Color(1f, 1f, 1f);
			textTargeted.SetActive(false);
		}
		else {
			localPlayer.crosshairHackDot.GetComponent<Image>().color = new Color(1f, 0f, 0f);
			textTargeted.SetActive(true);
			textTargeted.GetComponent<Text>().text = "<Player "+auxPlayer.playerCode+">";
		}

		textBigAlpha = (Mathf.Max(0f, textBigAlpha - Time.deltaTime * (1f/5f)));

	}

	void updateHacking() {

		for (int i = 0; i < listPlayers.Count; i++) {

			if (listPlayers [i].hackingTimer > 0f) {

				listPlayers [i].hackingTimer = Mathf.Max (0f, listPlayers [i].hackingTimer - Time.deltaTime);

				if (listPlayers [i] == myPlayer) {
					float aux = 1f - listPlayers [i].hackingTimer/hackingTimerMax;
					localPlayer.crosshairHackTimer.SetActive (true);
					localPlayer.crosshairHackTimer.GetComponent<Image>().material.SetFloat("_Cutoff", aux);
				}

			}

			if (listPlayers [i].hackingTimer <= 0f || listPlayers [i].hackingPlayerCode == "-1") {
				
				listPlayers[i].hackingPlayerCode = "-1";
				listPlayers [i].hackingTimer = 0f;

				if (listPlayers [i] == myPlayer) {
					localPlayer.crosshairHackTimer.SetActive (false);
				}

			}
				
		}

	}


	public Player firstLookingPlayer(Player p1) {

		float lookingDistance = 10f;

		RaycastHit[] hits;
		hits = Physics.RaycastAll (p1.visualAvatar.transform.position + LocalPlayerScript.centerOfCamera, p1.cameraForward, lookingDistance);
		Array.Sort (hits, delegate(RaycastHit r1, RaycastHit r2) { return r1.distance.CompareTo(r2.distance); });

		for (int i = 0; i < hits.Length; i++) {
			if (hits[i].collider.gameObject.tag != "LocalPlayer" && hits[i].collider.gameObject.GetComponent<AttackMarker>() == null && hits[i].collider.gameObject != p1.visualAvatar) {

				if (hits [i].collider.gameObject.GetComponent<PlayerMarker> () != null) {
					// DOESN'T MATTER WHO, IT COLLIDED WITH A PLAYER
					Player auxPlayer = hits[i].collider.gameObject.GetComponent<PlayerMarker> ().player;
					if (!(auxPlayer.hackingPlayerCode == myCode)) {
						// YOU CAN SEE IT
						return auxPlayer;
					}

				}
				else {
					return null;
				}

			}
		}

		return null;

	}
		

	void updateMyInfoInOtherClients() {


		if (currentUpdateCooldown >= 1f / (float)positionUpdatesPerSecond) {

			currentUpdateCooldown = 0f;

			if (localPlayer != null && GetComponent<NetworkView> () != null) {

				GameObject localVisualAvatar = localPlayer.GetComponent<LocalPlayerScript> ().visualAvatar;
				//bool sprintActive = (localPlayer.GetComponent<LocalPlayerScript>().sprintActive > 0f);
				/* HACK SI AL FINAL NO SE USA EL SPRINT, QUITAR ESA INFO, QUE NO SE MANDE HACK */
				bool sprintActive = false;
				//GetComponent<NetworkView>().RPC("updatePlayerRPC", RPCMode.Others, myCode, localVisualAvatar.transform.position, localVisualAvatar.transform.eulerAngles, localPlayer.GetComponent<LocalPlayerScript>().personalCamera.transform.forward, localPlayer.GetComponent<LocalPlayerScript>().personalCamera.transform.eulerAngles.x, localPlayer.GetComponent<LocalPlayerScript> ().lastAnimationOrder, sprintActive, myPlayer.hackingPlayerCode, myPlayer.amountCurrentHacking, myPlayer.lastTargetCode);
				GetComponent<NetworkView>().RPC("updatePlayerRPC", RPCMode.Others, myCode, localVisualAvatar.transform.position, localVisualAvatar.transform.eulerAngles, localPlayer.GetComponent<LocalPlayerScript>().personalCamera.transform.forward, localPlayer.GetComponent<LocalPlayerScript>().personalCamera.transform.eulerAngles.x, localPlayer.GetComponent<LocalPlayerScript> ().lastAnimationOrder, sprintActive);

			}


		} else {

			currentUpdateCooldown += Time.deltaTime;

		}

		myPlayer.cameraForward = localPlayer.personalCamera.transform.forward;
		myPlayer.targetPosition = myPlayer.visualAvatar.transform.position;
		myPlayer.targetRotation = myPlayer.visualAvatar.transform.eulerAngles;

	}

	void synchronizeOtherPlayers () {

		for (int i = 0; i < listPlayers.Count; i++) {

			if (listPlayers [i].playerCode != myCode) {

				listPlayers [i].visualAvatar.transform.position = Vector3.Lerp (listPlayers [i].visualAvatar.transform.position, listPlayers [i].targetPosition, Time.deltaTime * 10f);
				listPlayers [i].visualAvatar.transform.eulerAngles = Hacks.LerpVector3Angle (listPlayers [i].visualAvatar.transform.eulerAngles, listPlayers [i].targetRotation, Time.deltaTime * 10f);
				listPlayers [i].immune = Mathf.Max (0f, listPlayers [i].immune - Time.deltaTime);
				listPlayers [i].currentCameraEulerX = Mathf.LerpAngle (listPlayers [i].currentCameraEulerX, listPlayers [i].targetCameraEulerX, Time.deltaTime * 10f);

				float r = 1f;
				float g = 1f;
				float b = 1f;
				float a = 1f;

				if (myPlayer.hackingPlayerCode == listPlayers [i].playerCode) {
					g = 0f;
					b = 0f;
				}
				if (listPlayers [i].hackingPlayerCode == myCode) {
					a = 0f;
				}

				Color targetColor = new Color (r, g, b, a);

				Color c = Color.Lerp (listPlayers [i].visualMaterial.GetColor ("_Color"), targetColor, Time.fixedDeltaTime * 5f);

				listPlayers [i].visualMaterial.SetColor ("_Color", c);
				listPlayers [i].visualMaterial.SetFloat ("_Cutoff", 1f - c.a);

				if (listPlayers [i].sprintActive && !listPlayers [i].sprintTrail.Emit) {
					listPlayers [i].sprintTrail.Emit = true;
				} else if (!listPlayers [i].sprintActive && listPlayers [i].sprintTrail.Emit) {
					listPlayers [i].sprintTrail.Emit = false;
				}

			} else {
				// ES MI PLAYER
				listPlayers [i].immune = Mathf.Max (0f, listPlayers [i].immune - Time.deltaTime);
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

			listPlayers = listPlayers.OrderByDescending(o=>o.kills).ThenBy(o=>o.playerCode).ToList();

			for (int i = 0; i < listPlayers.Count; i++) {
				if (listPlayers[i].playerCode == myCode) {
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
			textBig.SetActive (false);

		} else {
			
			rankingBackground.SetActive (false);
			textBig.SetActive (true);

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

		if (localPlayer.GetComponent<LocalPlayerScript> ().receiveInput == false &&
			EventSystem.current.currentSelectedGameObject != chatManager.chatInputField) {
			// ESTO ES PARA EVITAR BUGS EN LOS QUE DEJAS DE TENER FOCUSEADO EL JUEGO
			EventSystem.current.SetSelectedGameObject(chatManager.chatInputField);
			StartCoroutine(CaretToEnd());
		}
			
		chatManager.lastTimeChatInputFocused = chatManager.chatInputField.GetComponent<InputField> ().isFocused;

	}

	private IEnumerator CaretToEnd() {
		// Doing a WateForSeconds(0f) forces to be executed next frame
		yield return new WaitForSeconds(0f);
		chatManager.chatInputField.GetComponent<InputField> ().MoveTextEnd (true);
	}

	/*
	private IEnumerator TargetDetected() {

		int aux = UnityEngine.Random.Range (1, 6);
		AudioSource audio = Hacks.GetAudioSource ("Sound/Effects/BeepHeart/BeepHeart_"+aux.ToString("00"));
		audio.volume = 0.25f;
		audio.pitch = UnityEngine.Random.Range (0.80f, 0.85f);
		audio.Play ();

		yield return new WaitForSeconds(0.15f);

		aux = UnityEngine.Random.Range (1, 6);
		audio = Hacks.GetAudioSource ("Sound/Effects/BeepHeart/BeepHeart_"+aux.ToString("00"));
		audio.volume = 0.25f;
		audio.pitch = UnityEngine.Random.Range (0.90f, 0.95f);
		audio.Play ();

	}
	*/

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
	[RPC]
	void updatePlayerRPC(string playerCode, Vector3 position, Vector3 rotation, Vector3 cameraForward, float cameraEulerX, string currentAnimation, bool sprintActive)
	{
		bool foundPlayer = false;

		for (int i = 0; i < listPlayers.Count; i++) {

			if (listPlayers[i].playerCode == playerCode) {
				listPlayers[i].targetPosition = position;
				listPlayers[i].targetRotation = rotation;
				listPlayers[i].cameraForward = cameraForward;
				listPlayers[i].targetCameraEulerX = cameraEulerX;
				listPlayers[i].SmartCrossfade(currentAnimation);
				listPlayers[i].sprintActive = sprintActive;
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

			if (Network.isServer) {
				// SE ACABA DE UNIR UN JUGADOR, ASI QUE LES DECIMOS A TODOS LA NUEVA SITUACION DEL RANKING
				gameScript.serverScript.sendRankingData();
				// LE DECIMOS CUANTOS SEGUNDOS DE PARTIDA QUEDAN
				GetComponent<NetworkView>().RPC("sendRemainingSecondsRPC", RPCMode.Others, playerCode, remainingSeconds);
				// Y LE ASIGNAMOS UN SITIO DONDE APARECER
				gameScript.serverScript.respawn(playerCode);

			}

			chatManager.Add(new ChatMessage("System", "Player "+aux.playerCode+" has joined the game."));
			chatManager.Write ();
			chatManager.lastChatPannelInteraction = 0f;

		}

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
		if (playerCode == myCode) {
			remainingSeconds = auxRemainingSeconds;
		}
	}

	[RPC]
	void removePlayerRPC(string playerCode) {

		for (int i = 0; i < gameScript.clientScript.listPlayers.Count; i++) {

			if (gameScript.clientScript.listPlayers[i].playerCode == playerCode) {

				Destroy(gameScript.clientScript.listPlayers[i].visualAvatar);
				gameScript.clientScript.listPlayers.RemoveAt(i);

				chatManager.Add(new ChatMessage("System", "Player "+playerCode+" has left the game."));
				chatManager.Write ();
				chatManager.lastChatPannelInteraction = 0f;

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
	void hackAttackRPC(string playerCode) {
		gameScript.serverScript.hackAttack (playerCode);
	}

	[RPC]
	void interceptAttackRPC(string playerCode) {
		gameScript.serverScript.interceptAttack (playerCode);
	}

	[RPC]
	void updateHackDataRPC(string playerCode, string hackedPlayerCode, bool justHacked)
	{
		Player player = PlayerByCode (playerCode);
		Player hackedPlayer = PlayerByCode (hackedPlayerCode);

		if (hackedPlayer != null && justHacked) {
			player.hackingTimer = hackingTimerMax;
		}

		player.hackingPlayerCode = hackedPlayerCode;

	}

	[RPC]
	void killRPC(string assassinCode, string victimCode)
	{
		if (assassinCode == myCode) {
			// YOU SLAYED VICTIMCODE
			textBigAlpha = 1f;
			textBig.GetComponent<Text>().text = "<color=#77FF77>YOU KILLED</color> PLAYER <color=#FF4444>#"+victimCode+"</color>";
			localPlayer.nextCooldownFree = true;
		} else if (victimCode == myCode) {
			// SLAYED BY ASSASSINCODE
			textBigAlpha = 1f;
			textBig.GetComponent<Text>().text = "<color=#FF7777>KILLED</color> BY PLAYER <color=#FF4444>#"+assassinCode+"</color>";
		}
	}

	[RPC]
	void respawnRPC(string playerCode, Vector3 position, Vector3 eulerAngles)
	{
		if (playerCode == myCode) {
			localPlayer.GetComponent<Rigidbody> ().velocity = new Vector3 (0f, 0f, 0f);
			localPlayer.transform.position = position;
			localPlayer.transform.eulerAngles = eulerAngles;
		} else {
			Player auxPlayer = PlayerByCode (playerCode);
			auxPlayer.visualAvatar.transform.position = position;
			auxPlayer.visualAvatar.transform.eulerAngles = eulerAngles;
		}
	}

	[RPC]
	void winnerRPC(string playerCode)
	{
		winnerCode = playerCode;
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
		public float immune = 0f;
		public int kills = 0;
		public int ping = 0;
		public string hackingPlayerCode = "-1";
		public float hackingTimer = 0f;
		public bool justHacked = false;

		public Vector3 targetPosition;
		public Vector3 targetRotation;
		public Vector3 cameraForward;
		public float targetCameraEulerX;
		public float currentCameraEulerX;

		public Player(string auxPlayerCode) {

			visualAvatar = Instantiate (Resources.Load("Prefabs/BOT") as GameObject);
			Initialize(auxPlayerCode, visualAvatar);

		}

		public Player(string auxPlayerCode, GameObject visualAvatar) {

			Initialize(auxPlayerCode, visualAvatar);

		}

		public void Initialize (string auxPlayerCode, GameObject visualAvatar) {

			playerCode = auxPlayerCode;
			this.visualAvatar = visualAvatar;
			visualAvatar.name = "VisualAvatar "+playerCode;
			visualAvatar.GetComponent<PlayerMarker>().player = this;
			visualMaterial = visualAvatar.transform.FindChild("Mesh").GetComponent<SkinnedMeshRenderer>().material;
			sprintTrail = visualAvatar.transform.FindChild ("Mesh/Trail").gameObject.GetComponent<MeleeWeaponTrail>();
			sprintTrail.Emit = false;

			targetPosition = visualAvatar.transform.position;
			targetRotation = visualAvatar.transform.eulerAngles;
			cameraForward = Vector3.forward;

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
