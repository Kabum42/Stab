using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;

public class ClientScript : MonoBehaviour {

	[HideInInspector] public GameScript gameScript;

	public float currentRankingCooldown = 0f;

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
		localPlayer.gameScript = gameScript;

		// TE UNES COMO PLAYER
		myCode = Network.player.ToString ();
		myPlayer = new Player(myCode, localPlayer.visualAvatar);
		listPlayers.Add(myPlayer);

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
		updateCanvas ();
		updateMyInfoInOtherClients ();
		synchronizeOtherPlayers ();
		updateHacking ();

		if (remainingSeconds <= 0f) {
			localPlayer.gameEnded = true;
			localPlayer.attacking = 0f;

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

	void updateCanvas() {

		if (myPlayer.hackingPlayerCode == "-1") {
			localPlayer.crossHair.GetComponent<Image>().color = new Color(1f, 1f, 1f);
			textTargeted.SetActive(false);
		}
		else {
			localPlayer.crossHair.GetComponent<Image>().color = new Color(1f, 0f, 0f);
			textTargeted.SetActive(true);
			textTargeted.GetComponent<Text>().text = "<Player "+myPlayer.hackingPlayerCode+">";
		}

		textBigAlpha = (Mathf.Max(0f, textBigAlpha - Time.deltaTime * (1f/5f)));

	}

	void updateHacking() {

		int playersHackingYou = 0;

		// THIS IS WHO YOU'RE TRYING TO _HACK
		Player auxPlayer = firstLookingPlayer(myPlayer);
		if (auxPlayer != null) {
			
			myPlayer.hackingPlayerCode = auxPlayer.playerCode; 

		} 
		else { 
			
			myPlayer.hackingPlayerCode = "-1";

		}



		for (int i = 0; i < listPlayers.Count; i++) {
			if (listPlayers [i] != myPlayer) {

				// YOU TRY TO _HACK EVERYBODY TO UPDATE THEIR VARIABLES
				float aux = tryHacking (myPlayer, listPlayers [i], ref listPlayers [i].isBeingHacked);
				listPlayers [i].isBeingHacked = aux;
				if (listPlayers [i].playerCode == myPlayer.hackingPlayerCode) {
					myPlayer.amountCurrentHacking = aux;
				}

				// THIS IS TO UPDATE YOUR VARIABLES
				if (listPlayers [i].hackingPlayerCode == myCode) {
					listPlayers [i].hackingMyPlayer = listPlayers [i].amountCurrentHacking;
					playersHackingYou++;
				} else if (listPlayers [i].lastTargetCode == myCode) {
					listPlayers [i].hackingMyPlayer = Mathf.Max(0f, listPlayers [i].hackingMyPlayer - Time.deltaTime * slowSpeed);
				} else {
					listPlayers [i].hackingMyPlayer = Mathf.Max(0f, listPlayers [i].hackingMyPlayer - Time.deltaTime * fastSpeed);
				}

			}
				
		}


		if (playersHackingYou >= 3) {
			// OVERLOAD
		}

	}

	float tryHacking(Player p1, Player p2, ref float hackingVariable) {

		if ((p1.hackingPlayerCode == p2.playerCode)) {
			// HACKER IS LOOKING AT PREY
			if (p2.hackingPlayerCode == p1.playerCode) {
				// PREY IS LOOKING AT HACKER TOO, HACKER INTERCEPTED
				return Mathf.Max(0f, hackingVariable - Time.deltaTime * fastSpeed);
			} else {
				// NOT INTERCEPTED, HACKER IS HACKING PREY
				return Mathf.Min(1.5f, hackingVariable + Time.deltaTime * hackingSpeed);
			}

		} else {
			// HACKER IS NOT LOOKING AT PREY
			if (p1.lastTargetCode == p2.playerCode) {
				// PREY WAS LAST HACKER'S TARGET, SLOW UNHACKING
				return Mathf.Max(0f, hackingVariable - Time.deltaTime * slowSpeed);
			} else {
				// PREY WASN'T LAST HACKER'S TARGET, FAST UNHACKING
				return Mathf.Max(0f, hackingVariable - Time.deltaTime * fastSpeed);
			}
		}

	}

	Player firstLookingPlayer(Player p1) {

		float lookingDistance = 10f;

		RaycastHit[] hits;
		hits = Physics.RaycastAll (p1.visualAvatar.transform.position + LocalPlayerScript.centerOfCamera, p1.cameraForward, lookingDistance);
		Array.Sort (hits, delegate(RaycastHit r1, RaycastHit r2) { return r1.distance.CompareTo(r2.distance); });

		for (int i = 0; i < hits.Length; i++) {
			if (hits[i].collider.gameObject.tag != "LocalPlayer" && hits[i].collider.gameObject.GetComponent<AttackMarker>() == null && hits[i].collider.gameObject != p1.visualAvatar) {

				if (hits [i].collider.gameObject.GetComponent<PlayerMarker> () != null) {
					// DOESN'T MATTER WHO, IT COLLIDED WITH A PLAYER
					Player auxPlayer = hits[i].collider.gameObject.GetComponent<PlayerMarker> ().player;
					if (auxPlayer.hackingMyPlayer < 1f) {
						// YOU CAN SEE IT
						if (p1.lastTargetCode != auxPlayer.playerCode) {
							//StartCoroutine(TargetDetected());
						}
						p1.lastTargetCode = auxPlayer.playerCode;
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
				bool sprintActive = (localPlayer.GetComponent<LocalPlayerScript>().sprintActive > 0f);
				GetComponent<NetworkView>().RPC("updatePlayerRPC", RPCMode.Others, myCode, localVisualAvatar.transform.position, localVisualAvatar.transform.eulerAngles, localPlayer.GetComponent<LocalPlayerScript>().personalCamera.transform.forward, localPlayer.GetComponent<LocalPlayerScript> ().lastAnimationOrder, sprintActive, myPlayer.hackingPlayerCode, myPlayer.amountCurrentHacking, myPlayer.lastTargetCode, localPlayer.GetComponent<LocalPlayerScript> ().attacking);

			}


		} else {

			currentUpdateCooldown += Time.deltaTime;

		}

		myPlayer.cameraForward = localPlayer.personalCamera.transform.forward;

	}

	void synchronizeOtherPlayers () {

		for (int i = 0; i < listPlayers.Count; i++) {

			if (listPlayers [i].playerCode != myCode) {

				listPlayers [i].visualAvatar.transform.position = Vector3.Lerp (listPlayers [i].visualAvatar.transform.position, listPlayers [i].targetPosition, Time.deltaTime * 10f);
				listPlayers [i].visualAvatar.transform.eulerAngles = Vector3.Lerp (listPlayers [i].visualAvatar.transform.eulerAngles, listPlayers [i].targetRotation, Time.deltaTime * 10f);
				listPlayers [i].attacking = Mathf.Max (0f, listPlayers [i].attacking - Time.deltaTime);
				listPlayers [i].immune = Mathf.Max (0f, listPlayers [i].immune - Time.deltaTime);

				Color c = Color.Lerp (listPlayers [i].visualMaterial.GetColor ("_Color"), new Color (1f, 1f -listPlayers[i].isBeingHacked, 1f -listPlayers[i].isBeingHacked, 1f -listPlayers[i].hackingMyPlayer), Time.fixedDeltaTime * 5f);
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
	void updatePlayerRPC(string playerCode, Vector3 position, Vector3 rotation, Vector3 cameraForward, string currentAnimation, bool sprintActive, string hackingPlayerCode, float amountCurrentHacking, string lastTargetCode, float attacking)
	{
		bool foundPlayer = false;

		for (int i = 0; i < listPlayers.Count; i++) {

			if (listPlayers[i].playerCode == playerCode) {
				listPlayers[i].targetPosition = position;
				listPlayers[i].targetRotation = rotation;
				listPlayers[i].cameraForward = cameraForward;
				listPlayers[i].SmartCrossfade(currentAnimation);
				listPlayers[i].sprintActive = sprintActive;
				listPlayers[i].hackingPlayerCode = hackingPlayerCode;
				listPlayers [i].amountCurrentHacking = amountCurrentHacking;
				listPlayers [i].lastTargetCode = lastTargetCode;
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

			if (Network.isServer) {
				// SE ACABA DE UNIR UN JUGADOR, ASI QUE LES DECIMOS A TODOS LA NUEVA SITUACION DEL RANKING
				gameScript.serverScript.sendRankingData();
				// LE DECIMOS CUANTOS SEGUNDOS DE PARTIDA QUEDAN
				GetComponent<NetworkView>().RPC("sendRemainingSecondsRPC", RPCMode.Others, playerCode, remainingSeconds);
				// Y LE ASIGNAMOS UN SITIO DONDE APARECER
				gameScript.serverScript.respawn(playerCode);

			}

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
		public float attacking = 0f;
		public float immune = 0f;
		public int kills = 0;
		public int ping = 0;
		public string lastTargetCode = "-1";
		public string hackingPlayerCode = "-1";
		public float amountCurrentHacking = 0f;
		public float hackingMyPlayer = 0f;
		public float isBeingHacked = 0f;

		public Vector3 targetPosition;
		public Vector3 targetRotation;
		public Vector3 cameraForward;

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
				animator.CrossFade(animation, GlobalData.crossfadeAnimation);
				lastAnimationOrder = animation;
			}

		}

	}

}
