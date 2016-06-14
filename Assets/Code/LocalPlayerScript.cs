using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class LocalPlayerScript : MonoBehaviour {

	public GameObject personalCamera;
	private GameObject firstPersonCamera;
	public HackCapsuleScript hackCapsule;
	//private float cameraDistance = 3.2f;
    private float cameraDistance = 0f;
	private float allPlayerRotationX = 0f;
	private float cameraValueX = 0f;
	private float cameraValueY = -17f;
	public static Vector3 centerOfCamera = new Vector3 (0f, 1.55f, 0f);
	//public static Vector3 centerOfCamera = new Vector3 (0.4f, 1.4f, 0f);
    //public static Vector3 centerOfCamera = new Vector3(0f, 1.4f, 0f);
	private Vector3 lastPositionCursor;

	private static float baseSpeed = 5f;
	private float turboSpeed = baseSpeed*(1.5f); // 70% ES LO QUE AUMENTA LA VELOCIDAD EL SPRINT DEL PICARO EN EL WOW
	private float characterSpeed = baseSpeed;

	public Animation lastAnimationOrder = Animation.Idle;

	public GameObject visualAvatar;
	public GameObject materialCarrier;
	private GameObject pelvis;

	//public bool receiveInput = true;
	public InputMode inputMode = InputMode.Playing;

	private float notMoving = 0f;

	public float blinkResource = 3f;
	[HideInInspector] public static float blinkDistance = 3.5f;
	public bool blinking = false;
	public Vector3 blinkStart;
	public Vector3 blinkEnd;

	public bool dead = false;

	[HideInInspector] public GameObject canvas;

	[HideInInspector] public GameObject rankingBackground;
	[HideInInspector] public GameObject chatPanel;
	[HideInInspector] public GameObject crosshairHack;
	[HideInInspector] public GameObject crosshairHackDot;
	[HideInInspector] public GameObject crosshairHackTriclip;
	[HideInInspector] public GameObject crosshairHackTimer;
	[HideInInspector] public GameObject crosshairHackParentHack;
	[HideInInspector] public GameObject crosshairHackParentIntercept;
	[HideInInspector] public List<GameObject> crosshairHackCharges = new List<GameObject>();
	[HideInInspector] public List<GameObject> crosshairHackChargesFull = new List<GameObject>();
	[HideInInspector] public List<GameObject> crosshairHackInterceptCharges = new List<GameObject>();
	[HideInInspector] public List<GameObject> crosshairHackInterceptChargesFull = new List<GameObject>();
	private GameObject sourceBlinkCharge;
	[HideInInspector] public List<GameObject> crosshairHackBlinkPool = new List<GameObject>();
	[HideInInspector] public List<GameObject> crosshairHackBlinkCurrent = new List<GameObject>();
	[HideInInspector] public List<GameObject> crosshairHackBlinkDisappearing = new List<GameObject>();
	[HideInInspector] public GameObject crosshairHackSkull;
	[HideInInspector] public GameObject inGameMenu;
	[HideInInspector] public GameObject fade;

	[HideInInspector] public InGameMenuManager inGameMenuManager;

	private int nextHackCharge = 1;
	public float hackResource = 3f;

	private int nextInterceptCharge = 1;
    public float interceptResource = 3f;

	public GameObject textTargeted;
	public GameObject distanceText;

	//public AnimationCurve attackCameraDistance;

    private Vector3 attackOldPosition;
    private Vector3 attackTargetPosition;

    private float crosshairHackTriclipOldZ = 0f;
    private float crosshairHackTriclipTargetZ = 0f;
    private Vector3 crosshairHackTriclipOriginalScale;

	private float crosshairHackSmallOldZ = 0f;
	private float crosshairHackSmallTargetZ = 0f;

	private float crosshairHackBigOldZ = 0f;
	private float hackBigAux = 3f;
    private float crosshairHackBigTargetZ = 0f;

	private GameObject firstPersonObjects;
	private GameObject armRight;

    public GameObject alertHacked;

	private bool lastTimeGrounded = true;
	private static float footStepCooldownMax = 0.35f;
	private float footStepCooldown = footStepCooldownMax/2f;

	private Material[] materials;

	[HideInInspector] public bool firstRespawn = true;

	[HideInInspector] public ChatManager chatManager;

	void Awake () {

		GlobalData.Start ();

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.SetCursor (null, Vector2.zero, CursorMode.Auto);

		personalCamera = this.gameObject.transform.FindChild ("PersonalCamera").gameObject;
		personalCamera.transform.localPosition = centerOfCamera;
		firstPersonCamera = this.gameObject.transform.FindChild ("PersonalCamera/FirstPersonCamera").gameObject;
		hackCapsule = this.gameObject.transform.FindChild ("PersonalCamera/HackCapsule").gameObject.GetComponent<HackCapsuleScript>();
		visualAvatar = Instantiate (Resources.Load("Prefabs/BOT") as GameObject);
		visualAvatar.transform.parent = this.gameObject.transform;
		visualAvatar.transform.localPosition = new Vector3 (0, 0, 0);
		visualAvatar.name = "VisualAvatar";
		Hacks.SetLayerRecursively (this.gameObject, LayerMask.NameToLayer ("Ignore Raycast"));
		materialCarrier = visualAvatar.transform.FindChild ("Mesh").gameObject;
		pelvis = visualAvatar.transform.FindChild ("Armature/Pelvis").gameObject;
		//materialCarrier.layer = LayerMask.NameToLayer ("DontRender");
		// THIS IS TO HIDE THE MAIN CHARACTER BUT STILL RENDER IT SO ALL ANIMATION AND PHYSICS UPDATES TAKE IT INTO ACCOUNT
		materials = materialCarrier.GetComponent<SkinnedMeshRenderer>().materials;
		for (int i = 0; i < materials.Length; i++) {
			materials[i].SetFloat ("_Cutoff", 1f);
		}

		canvas = Instantiate (Resources.Load("Prefabs/Canvas") as GameObject);
		canvas.name = "Canvas";
		Instantiate (Resources.Load("Prefabs/EventSystem") as GameObject).name = "EventSystem";

		inGameMenu = canvas.transform.FindChild ("InGameMenu").gameObject;
		inGameMenu.SetActive (false);
		inGameMenuManager = new InGameMenuManager (this, inGameMenu);

		crosshairHack = canvas.transform.FindChild ("CrosshairHack").gameObject;

        crosshairHackDot = crosshairHack.transform.FindChild("Dot").gameObject;
        crosshairHackTriclip = crosshairHack.transform.FindChild("Triclip").gameObject;
		crosshairHackTriclipOriginalScale = crosshairHackTriclip.GetComponent<RectTransform>().localScale;
		crosshairHackTimer = crosshairHack.transform.FindChild("Timer").gameObject;
		crosshairHackTimer.transform.FindChild("Full").gameObject.GetComponent<Image>().material.SetFloat("_Cutoff", 1f);
		crosshairHackTimer.SetActive (false);
		crosshairHackParentHack = crosshairHack.transform.FindChild("HackParent").gameObject;
        crosshairHackParentIntercept = crosshairHack.transform.FindChild("InterceptParent").gameObject;


		GameObject sourceHackCharge = crosshairHack.transform.FindChild("HackCharge").gameObject;
		int num_hack_charges = 3;
		for (int i = 0; i < num_hack_charges; i++)
		{
			GameObject newCharge = Instantiate(sourceHackCharge);
			newCharge.transform.SetParent(crosshairHackParentHack.transform);
			newCharge.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
			float aux = (float)i / (float)num_hack_charges;
			newCharge.GetComponent<RectTransform>().eulerAngles = new Vector3(0f, 0f, (aux * 360f) + 60f);
			Vector2 upVector2 = new Vector2(newCharge.GetComponent<RectTransform>().up.x, newCharge.GetComponent<RectTransform>().up.y);
			newCharge.GetComponent<RectTransform>().anchoredPosition = upVector2 * 82f;
			newCharge.name = "Charge_" + (i + 1);
			newCharge.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
			crosshairHackCharges.Add(newCharge);
			crosshairHackChargesFull.Add(newCharge.transform.FindChild("Full").gameObject);
		}
		Destroy(sourceHackCharge);


		GameObject sourceInterceptCharge = crosshairHack.transform.FindChild("InterceptCharge").gameObject;
		int num_intercept_charges = 3;
        for (int i = 0; i < num_intercept_charges; i++)
        {
            GameObject newCharge = Instantiate(sourceInterceptCharge);
            newCharge.transform.SetParent(crosshairHackParentIntercept.transform);
            newCharge.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            float aux = (float)i / (float)num_intercept_charges;
            newCharge.GetComponent<RectTransform>().eulerAngles = new Vector3(0f, 0f, (aux * 360f) + 60f);
            Vector2 upVector2 = new Vector2(newCharge.GetComponent<RectTransform>().up.x, newCharge.GetComponent<RectTransform>().up.y);
            newCharge.GetComponent<RectTransform>().anchoredPosition = upVector2 * 345f;
            newCharge.name = "Charge_" + (i + 1);
            newCharge.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            crosshairHackInterceptCharges.Add(newCharge);
            crosshairHackInterceptChargesFull.Add(newCharge.transform.FindChild("Full").gameObject);
        }
        Destroy(sourceInterceptCharge);


		sourceBlinkCharge = crosshairHack.transform.FindChild("BlinkCharge").gameObject;
		int num_blink_charges = 3;
		for (int i = 0; i < num_blink_charges; i++)
		{
			GameObject newCharge = Instantiate(sourceBlinkCharge);
			newCharge.transform.SetParent(crosshairHack.transform);
			crosshairHackBlinkCurrent.Add (newCharge);

			float positionY = 280f - 40f * i;
			float relative = positionY / (280f);

			newCharge.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -positionY);
			newCharge.GetComponent<RectTransform>().localScale = new Vector3(relative, relative, relative);
		}
		sourceBlinkCharge.SetActive (false);

		//crosshairHackBlinkCurrent

		crosshairHackSkull = crosshairHack.transform.FindChild("Skull").gameObject;
		crosshairHackSkull.SetActive (false);

		rankingBackground = canvas.transform.FindChild ("RankingBackground").gameObject;
		rankingBackground.SetActive (false);

		chatPanel = canvas.transform.FindChild ("ChatPanel").gameObject;
		chatPanel.SetActive (false);

		alertHacked = canvas.transform.FindChild ("Alert").gameObject;
        alertHacked.GetComponent<Image>().material.SetFloat("_Cutoff", 1f);

		distanceText = canvas.transform.FindChild ("DistanceText").gameObject;
		distanceText.SetActive (false);

		textTargeted = canvas.transform.FindChild ("TextTargeted").gameObject;
		textTargeted.SetActive (false);

		fade = canvas.transform.FindChild ("Fade").gameObject;
		if (GlobalData.clientScript != null) {
			fade.GetComponent<Image> ().color = Hacks.ColorLerpAlpha (fade.GetComponent<Image> ().color, GlobalData.fadeAlphaTarget, 1f);
			GlobalData.fadeAlphaTarget = 0f;
		} else {
			GlobalData.fadeAlphaTarget = 0f;
			fade.GetComponent<Image> ().color = Hacks.ColorLerpAlpha (fade.GetComponent<Image> ().color, GlobalData.fadeAlphaTarget, 1f);
		}


		lastPositionCursor = Input.mousePosition;

		GameObject sun = GameObject.Find ("Sun");
		if (sun != null) {
			SunShafts s = personalCamera.GetComponent<SunShafts> ();
			s.sunTransform = sun.transform;
		} else {
			Destroy(personalCamera.GetComponent<SunShafts>());
		}

		firstPersonObjects = this.transform.FindChild ("PersonalCamera/FirstPersonCamera/FirstPersonObjects").gameObject;
		armRight = this.transform.FindChild ("PersonalCamera/FirstPersonCamera/FirstPersonObjects/Arm_2").gameObject;

		chatManager = new ChatManager(chatPanel);

	}

	void OnApplicationFocus(bool focusStatus) {
		if (focusStatus && !Application.isEditor) {
			StartCoroutine(Hacks.LockCursor(-1f));
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (GlobalData.clientScript != null) {
			dead = GlobalData.clientScript.myPlayer.dead;
		}

		handleDeadCamera ();

		fillResources ();
		updateUI ();
		alertMockUp();

		handleInput ();
		handleCameraChanges ();
	
	}

	void FixedUpdate() {

		if (inputMode == InputMode.Playing /* !inGameMenuManager.active && !dead */) {
			handleMovementInput ();
		}

	}

	void handleInput() {

		if (inputMode == InputMode.Playing) {

			if (!dead) {

				handleHack ();
				handleIntercept ();
				handleRegularInput ();

			}

		} else if (inputMode == InputMode.Menu) {

			inGameMenuManager.Update (Time.deltaTime);

		} else if (inputMode == InputMode.Chat) {
			
			updateChat ();

		}

		if (GlobalData.clientScript != null) {
			checkIfActivateChat ();
		}
		chatManager.lastTimeChatInputFocused = chatManager.chatInputField.GetComponent<InputField> ().isFocused;

	}

	void checkIfActivateChat() {

		if (EventSystem.current.currentSelectedGameObject == chatManager.chatInputField) {
			chatManager.lastChatPannelInteraction = 0f;
		} else if (chatManager.lastChatPannelInteraction < chatManager.chatPannelInteractionThreshold) {
			chatManager.lastChatPannelInteraction += Time.deltaTime;
		}
			

		if (Input.GetKeyDown (KeyCode.Return) && inputMode == LocalPlayerScript.InputMode.Playing && !chatManager.lastTimeChatInputFocused) {

			chatManager.chatPanel.SetActive(true);
			chatManager.lastChatPannelInteraction = 0f;

			inputMode = LocalPlayerScript.InputMode.Chat;

			EventSystem.current.SetSelectedGameObject(chatManager.chatInputField, null);
			chatManager.chatInputField.GetComponent<InputField> ().OnPointerClick(new PointerEventData(EventSystem.current));

		}

		if (Input.GetKeyDown (KeyCode.Escape) && inputMode == LocalPlayerScript.InputMode.Chat) {

			chatManager.chatInputField.GetComponent<InputField> ().text = "";
			EventSystem.current.SetSelectedGameObject(null);
			GetComponent<LocalPlayerScript> ().inputMode = LocalPlayerScript.InputMode.Playing;

		}

	}

	void updateChat() {

		if (Input.GetKeyDown(KeyCode.Return) && chatManager.lastTimeChatInputFocused) {

			if (chatManager.chatInputField.GetComponent<InputField> ().text != "") {

				string info = chatManager.chatInputField.GetComponent<InputField> ().text;
				GlobalData.clientScript.writeInChat (info);

				chatManager.chatInputField.GetComponent<InputField> ().text = "";

			}

			EventSystem.current.SetSelectedGameObject(null);
			inputMode = InputMode.Playing;

		}

		if (inputMode != LocalPlayerScript.InputMode.Playing && EventSystem.current.currentSelectedGameObject != chatManager.chatInputField) {
			// ESTO ES PARA EVITAR BUGS EN LOS QUE DEJAS DE TENER FOCUSEADO EL JUEGO
			EventSystem.current.SetSelectedGameObject(chatManager.chatInputField);
			StartCoroutine(CaretToEnd());
		}

		// THIS IS TO ADJUST THE HEIGHT
		chatManager.Update ();

	}

	private IEnumerator CaretToEnd() {
		// Doing a WateForSeconds(0f) forces to be executed next frame
		yield return new WaitForSeconds(0f);
		chatManager.chatInputField.GetComponent<InputField> ().MoveTextEnd (true);
	}

	void handleDeadCamera() {

		if (GlobalData.clientScript != null && GlobalData.clientScript.myPlayer.dead) {

			cameraDistance = Mathf.Lerp (cameraDistance, 3f, Time.deltaTime * 5f);

			if (materials [0].GetFloat ("_Cutoff") == 1f) {
				visualAvatar.transform.SetParent (null);
				for (int i = 0; i < materials.Length; i++) {
					materials[i].SetFloat ("_Cutoff", 0f);
				}
			}

			this.transform.position = pelvis.transform.position;

		}

	}

	public void respawn() {
		
		cameraDistance = 0f;
		visualAvatar.transform.SetParent (this.transform);
		visualAvatar.transform.localPosition = Vector3.zero;
		visualAvatar.transform.localEulerAngles = Vector3.zero;
		for (int i = 0; i < materials.Length; i++) {
			materials[i].SetFloat ("_Cutoff", 1f);
		}

		if (firstRespawn) {
			firstRespawn = false;
		}

	}

	void fillResources() {

		// HACK_RESOURCE
		if (hackResource < 3f)
		{
			hackResource += Time.deltaTime;
			if (hackResource >= 3f) { hackResource = 3f; }
		}

		// INTERCEPT
		if (interceptResource < 3f)
		{
			interceptResource += Time.deltaTime*(1f/2f);
			if (interceptResource >= 3f) { interceptResource = 3f; }
		}

		// BLINK
		blinkResource = Mathf.Min (3f, blinkResource + Time.deltaTime*(1f/4f));

	}

	void updateUI() {

		characterSpeed = baseSpeed;
		crosshairHackTriclip.SetActive (true);
		crosshairHackSkull.SetActive (false);

		if (!firstRespawn) {
			fade.GetComponent<Image> ().color = Hacks.ColorLerpAlpha (fade.GetComponent<Image> ().color, GlobalData.fadeAlphaTarget, Time.deltaTime * 15f);
		}

		if (GlobalData.clientScript != null) {

			ClientScript.Player crosshairPlayer = playerOnCrosshair();
			crosshairHackDot.GetComponent<Image>().color = new Color(1f, 1f, 1f);
			textTargeted.SetActive(false);
			ClientScript.Player hackedPlayer = null;
			float distanceToHacked = float.MaxValue;
			distanceText.SetActive (false);

			if (GlobalData.clientScript.myPlayer.hackingNetworkPlayer != GlobalData.clientScript.myPlayer.networkPlayer) {
				// I'M HACKING SOMEONE
				hackedPlayer = GlobalData.clientScript.PlayerByNetworkPlayer(GlobalData.clientScript.myPlayer.hackingNetworkPlayer);
				distanceToHacked = Vector3.Distance(personalCamera.transform.position, hackedPlayer.cameraMockup.transform.position);
			}
				
			List<ClientScript.Player> playersInside = GlobalData.clientScript.insideBigCrosshair (GlobalData.clientScript.myPlayer, float.MaxValue, "bigCrosshair", false);

			if (playersInside.Contains (hackedPlayer)) {
				characterSpeed = turboSpeed;
				if (distanceToHacked <= ClientScript.hackKillDistance) {
					crosshairHackTriclip.SetActive (false);
					crosshairHackSkull.SetActive (true);
					distanceText.SetActive (true);
					distanceText.GetComponent<Text> ().text = "KILL";
					crosshairHackDot.GetComponent<Image>().color = new Color(1f, 0f, 0f);
					textTargeted.SetActive(true);
					textTargeted.GetComponent<Text>().text = "<Player "+hackedPlayer.networkPlayer.ToString()+">";
				}
			}

			if (crosshairPlayer != null && !textTargeted.activeInHierarchy) {
				// SOMEONE ON THE CROSSHAIR && TEXTTARGETED IS NOT ACTIVE
				crosshairHackDot.GetComponent<Image>().color = new Color(1f, 0f, 0f);
				textTargeted.SetActive(true);
				textTargeted.GetComponent<Text>().text = "<Player "+crosshairPlayer.networkPlayer.ToString()+">";
			}

			if (hackedPlayer != null) {
				if (distanceToHacked > ClientScript.hackKillDistance) {
					distanceText.SetActive (true);
					float number = (Vector3.Distance (hackedPlayer.cameraMockup.transform.position, personalCamera.transform.position) - ClientScript.hackKillDistance) * 10f;
					string numberText = number.ToString ("0");
					distanceText.GetComponent<Text> ().text = numberText;
				} else if (!distanceText.activeInHierarchy) {
					distanceText.SetActive (true);
					distanceText.GetComponent<Text> ().text = "READY";
				}
			}

		}

		// HACK_
		updateUIHack();

		// INTERCEPT
		updateUIIntercept();

		// BLINK
		updateUIBlink();

	}

	void updateUIHack() {
		// TODAS VACIAS
		for (int i = 0; i < crosshairHackCharges.Count; i++)
		{
			crosshairHackChargesFull[i].GetComponent<Image> ().enabled = false;
		}

		float auxResource = hackResource;
		int auxNext = nextHackCharge;

		// SE LLENAN LAS QUE TOCAN
		while (auxResource >= 1f)
		{
			crosshairHackChargesFull[auxNext].GetComponent<Image> ().enabled = true;
			auxResource -= 1f;
			auxNext++;
			if (auxNext >= crosshairHackInterceptCharges.Count) { auxNext = 0; }
		}

		if (crosshairHackSmallOldZ > (crosshairHackSmallTargetZ +5f)) {
			crosshairHackParentHack.transform.localScale = Vector3.Lerp (crosshairHackParentHack.transform.localScale, new Vector3 (1.1f, 1.1f, 1.1f), Time.deltaTime * 20f);
		} else {
			crosshairHackParentHack.transform.localScale = Vector3.Lerp (crosshairHackParentHack.transform.localScale, new Vector3 (1f, 1f, 1f), Time.deltaTime * 20f);
		}

		crosshairHackSmallOldZ = Mathf.Lerp (crosshairHackSmallOldZ, crosshairHackSmallTargetZ, Time.deltaTime * 5f);
		crosshairHackParentHack.GetComponent<RectTransform>().eulerAngles = new Vector3(0f, 0f, crosshairHackSmallOldZ);
		crosshairHackTriclip.GetComponent<RectTransform> ().eulerAngles = crosshairHackParentHack.GetComponent<RectTransform> ().eulerAngles;

		if (hackResource >= 1f) {
			crosshairHackTriclip.GetComponent<RectTransform> ().localScale = Vector3.Lerp(crosshairHackTriclip.GetComponent<RectTransform> ().localScale, new Vector3 (0.5f, 0.5f, 0.5f), Time.deltaTime*10f);
			crosshairHackTriclip.GetComponent<Image> ().color = Color.Lerp (crosshairHackTriclip.GetComponent<Image> ().color, new Color (1f, 1f, 1f, 1f), Time.deltaTime * 10f);
		} else {
			crosshairHackTriclip.GetComponent<RectTransform> ().localScale = Vector3.Lerp(crosshairHackTriclip.GetComponent<RectTransform> ().localScale, new Vector3 (0.35f, 0.35f, 0.35f), Time.deltaTime*10f);
			crosshairHackTriclip.GetComponent<Image> ().color = Color.Lerp (crosshairHackTriclip.GetComponent<Image> ().color, new Color (1f, 1f, 1f, 0.5f), Time.deltaTime * 10f);
		}
	}

	void updateUIIntercept() {
		// TODAS VACIAS
		for (int i = 0; i < crosshairHackInterceptCharges.Count; i++)
		{
			crosshairHackInterceptChargesFull [i].GetComponent<Image> ().enabled = false;
		}

		float auxResource = interceptResource;
		int auxNext = nextInterceptCharge;

		// SE LLENAN LAS QUE TOCAN
		while (auxResource >= 1f)
		{
			crosshairHackInterceptChargesFull[auxNext].GetComponent<Image> ().enabled = true;
			auxResource -= 1f;
			auxNext++;
			if (auxNext >= crosshairHackInterceptCharges.Count) { auxNext = 0; }
		}

		float timePerPhase = (0.5f/3f);

		if (hackBigAux < 3f) {

			if (hackBigAux < 1f) {

				hackBigAux += Time.deltaTime * (1f / timePerPhase);
				float scale = Mathf.Lerp(crosshairHackParentIntercept.transform.localScale.x, 0.98f, Time.deltaTime*10f);
				crosshairHackParentIntercept.transform.localScale = new Vector3 (scale, scale, scale);

			} else if (hackBigAux < 2f) {

				crosshairHackBigOldZ -= Time.deltaTime * (360f / 3f) * (1f / timePerPhase);
				if (crosshairHackBigOldZ <= crosshairHackBigTargetZ) {
					crosshairHackBigOldZ = crosshairHackBigTargetZ;
					hackBigAux = 2f;
				}
				crosshairHackParentIntercept.GetComponent<RectTransform> ().eulerAngles = new Vector3 (0f, 0f, crosshairHackBigOldZ);

			} else {

				hackBigAux += Time.deltaTime * (1f / timePerPhase);
				float scale = Mathf.Lerp(crosshairHackParentIntercept.transform.localScale.x, 1f, Time.deltaTime*10f);
				crosshairHackParentIntercept.transform.localScale = new Vector3 (scale, scale, scale);

			}

		}

	}

	void updateUIBlink() {

		List <GameObject> auxList = new List<GameObject> ();

		foreach (GameObject g in crosshairHackBlinkDisappearing) {
			GameObject full = g.transform.FindChild ("Full").gameObject;
			full.GetComponent<Image> ().color = new Color (full.GetComponent<Image> ().color.r, full.GetComponent<Image> ().color.g, full.GetComponent<Image> ().color.b, full.GetComponent<Image> ().color.a - Time.deltaTime*7.5f);
			g.transform.localScale = Vector3.Lerp(g.transform.localScale, new Vector3(1.5f, 1.5f, 1.5f), Time.deltaTime*10f);
			if (full.GetComponent<Image> ().color.a <= 0f) {
				auxList.Add (g);
			}
		}

		foreach (GameObject g in auxList) {
			crosshairHackBlinkDisappearing.Remove (g);
			g.SetActive (false);
			crosshairHackBlinkPool.Add (g);
		}

		int num_blink_charges = (int) Mathf.Floor (blinkResource);
		for (int i = 0; i < num_blink_charges; i++)
		{

			if (crosshairHackBlinkCurrent.Count < (i + 1)) {
				addBlinkCharge ();
				crosshairHackBlinkCurrent [i].transform.FindChild ("Full").gameObject.GetComponent<Image> ().color = Hacks.ColorLerpAlpha (crosshairHackBlinkCurrent [i].transform.FindChild ("Full").gameObject.GetComponent<Image> ().color, 0f, 1f);
				crosshairHackBlinkCurrent [i].GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f, -(280f - 40f * (i + 1)));
			}

			GameObject charge = crosshairHackBlinkCurrent [i];

			float targetPositionY = 280f - 40f * i;
			charge.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, Mathf.Lerp(charge.GetComponent<RectTransform>().anchoredPosition.y, -targetPositionY, Time.deltaTime*10f));

			float relative = -charge.GetComponent<RectTransform>().anchoredPosition.y / (280f);
			charge.GetComponent<RectTransform>().localScale = new Vector3(relative, relative, relative);

			charge.transform.FindChild ("Full").gameObject.GetComponent<Image> ().color = Hacks.ColorLerpAlpha (charge.transform.FindChild ("Full").gameObject.GetComponent<Image> ().color, 0.5f, Time.deltaTime*10f);

			charge.SetActive (true);
		}

	}

	void addBlinkCharge() {

		GameObject charge;

		if (crosshairHackBlinkPool.Count > 0) {
			charge = crosshairHackBlinkPool [0];
			crosshairHackBlinkPool.RemoveAt (0);
		} else {
			charge = Instantiate(sourceBlinkCharge);
			charge.transform.SetParent(crosshairHack.transform);
		}
			
		crosshairHackBlinkCurrent.Add (charge);

	}

    void alertMockUp()
    {

        float cutoff = alertHacked.GetComponent<Image>().material.GetFloat("_Cutoff");

        float min = 0.1f;

        if (cutoff < 1f && cutoff > min) {
            cutoff -= Time.deltaTime*2f;
            if (cutoff <= min) { cutoff = 1f; }
            alertHacked.GetComponent<Image>().material.SetFloat("_Cutoff", cutoff);
        }

    }

    void handleIntercept()
    {

        // RIGHT CLICK
        if (Input.GetMouseButtonDown(1) && interceptResource >= 1f)
        {
			
            interceptResource -= 1f;
            nextInterceptCharge++;
            if (nextInterceptCharge >= crosshairHackInterceptCharges.Count) { nextInterceptCharge = 0; }
            crosshairHackBigTargetZ -= 120f;

            if (crosshairHackBigTargetZ < 0f)
            {
                crosshairHackBigTargetZ += 360f;
				crosshairHackBigOldZ += 360f;
            }


			if (crosshairHackBigOldZ < crosshairHackBigTargetZ) {
				crosshairHackBigOldZ += 360f;
			}

			if (hackBigAux > 1f && hackBigAux < 2f) {
				hackBigAux = 1f;
			} else if (hackBigAux > 2f) {
				hackBigAux = 0f;
			}


			if (GlobalData.clientScript != null) {

				List<ClientScript.Player> playersInside = GlobalData.clientScript.insideBigCrosshair (GlobalData.clientScript.myPlayer, ClientScript.interceptKillDistance, "bigCrosshair", true);

				foreach (ClientScript.Player player in playersInside) {
					if (player.hackingNetworkPlayer == GlobalData.clientScript.myPlayer.networkPlayer) {
						if (Network.isServer) {
							GlobalData.clientScript.serverScript.interceptAttack (GlobalData.clientScript.myPlayer.networkPlayer, player.networkPlayer);
						} else {
							GlobalData.clientScript.GetComponent<NetworkView>().RPC("interceptAttackRPC", RPCMode.Server, player.networkPlayer);
						}
					}
				}


			}

        }

    }

	void handleHack() {

		// LEFT CLICK
		if (Input.GetMouseButtonDown (0) && hackResource >= 1f) {

			hackResource -= 1f;
			nextHackCharge++;
			if (nextHackCharge >= crosshairHackCharges.Count) { nextHackCharge = 0; }
			crosshairHackSmallTargetZ -= 120f;

			if (crosshairHackSmallTargetZ < 0f)
			{
				crosshairHackSmallTargetZ += 360f;
				crosshairHackSmallOldZ += 360f;
			}


			if (crosshairHackSmallOldZ < crosshairHackSmallTargetZ) {
				crosshairHackSmallOldZ += 360f;
			}

			if (GlobalData.clientScript != null) {

				bool usedHack = false;
				ClientScript.Player hackedPlayer = GlobalData.clientScript.PlayerByNetworkPlayer (GlobalData.clientScript.myPlayer.hackingNetworkPlayer);

				if (hackedPlayer != null) {
					// TRIES TO KILL HIM
					List<ClientScript.Player> playersInside = GlobalData.clientScript.insideBigCrosshair (GlobalData.clientScript.myPlayer, ClientScript.hackKillDistance, "bigCrosshair", false);

					if (playersInside.Contains (hackedPlayer)) {
						usedHack = true;
						if (Network.isServer) {
							GlobalData.clientScript.serverScript.hackAttack (GlobalData.clientScript.myPlayer.networkPlayer, hackedPlayer.networkPlayer, true);
						} else {
							GlobalData.clientScript.GetComponent<NetworkView>().RPC("hackAttackKillRPC", RPCMode.Server, hackedPlayer.networkPlayer);
						}
					}
						
				} 

				if (!usedHack) {
					// TRIES HACKING SOMEONE
					ClientScript.Player crosshairPlayer = playerOnCrosshair ();

					if (crosshairPlayer != null) {
						if (Network.isServer) {
							GlobalData.clientScript.serverScript.hackAttack (GlobalData.clientScript.myPlayer.networkPlayer, crosshairPlayer.networkPlayer, false);
						} else {
							GlobalData.clientScript.GetComponent<NetworkView>().RPC("hackAttackRPC", RPCMode.Server, crosshairPlayer.networkPlayer);
						}
					}
				}

			}

		}

	}

	public ClientScript.Player playerOnCrosshair() {

		if (GlobalData.clientScript != null) {

			ClientScript.Player auxPlayer = null;
			ClientScript.Player hackedPlayer = GlobalData.clientScript.PlayerByNetworkPlayer (GlobalData.clientScript.myPlayer.hackingNetworkPlayer);

			if (hackedPlayer != null) {
				List<ClientScript.Player> playersInside = GlobalData.clientScript.insideBigCrosshair (GlobalData.clientScript.myPlayer, ClientScript.hackKillDistance, "bigCrosshair", false);
				if (playersInside.Contains (hackedPlayer)) {
					return hackedPlayer;
				}
			}

			auxPlayer = hackCapsule.firstLookingPlayer();
			return auxPlayer;
		}
		return null;

	}

	public Vector3Nullable firstLookingNonPlayer(float distance) {

		return firstNonPlayer(personalCamera.transform.forward, distance);

	}

	public Vector3Nullable firstNonPlayer(Vector3 direction, float distance) {

		direction.Normalize ();

		Vector3Nullable vector3Nullable = new Vector3Nullable ();

		RaycastHit[] hits;
		if (distance >= 0f) {
			hits = Physics.RaycastAll (this.transform.position + LocalPlayerScript.centerOfCamera, direction, distance);
		} else {
			hits = Physics.RaycastAll (this.transform.position + LocalPlayerScript.centerOfCamera, direction);
		}
		Array.Sort (hits, delegate(RaycastHit r1, RaycastHit r2) { return r1.distance.CompareTo(r2.distance); });

		for (int i = 0; i < hits.Length; i++) {

			if (hits[i].collider.gameObject.tag != "LocalPlayer" && !(visualAvatar.GetComponent<RagdollScript>().IsArticulation(hits[i].collider.gameObject))) {

				PlayerMarker pM = PlayerMarker.Traverse (hits [i].collider.gameObject);

				if (pM == null) {
					// DOESN'T MATTER WHO, IT COLLIDED WITH A NON-PLAYER
					vector3Nullable.isNull = false;
					vector3Nullable.vector3 = hits[i].point;
					return vector3Nullable;
				} else {
					return vector3Nullable;
				}
			}
		}

		return vector3Nullable;

	}

	/*
	void adjustFirstPersonObjects() {

		float difference = 0f;

		if (personalCamera.transform.eulerAngles.x > 180f) {
			// CUANTO MAS LEJOS DE 360, MAS ALTO MIRAS
			difference = -Mathf.Abs(360f-personalCamera.transform.eulerAngles.x)/90f;
		} else {
			// CUANTO MAS LEJOS DE 0, MAS BAJO MIRAS
			difference = Mathf.Abs(0f-personalCamera.transform.eulerAngles.x)/90f;
		}

		float maxAmount = 0.15f;
		firstPersonObjects.transform.localPosition = new Vector3 (0f, difference*maxAmount, 0f);

	}
	*/

	void handleRegularInput() {

		if (inputMode == InputMode.Playing) {

			if (Input.GetKeyDown (KeyCode.Escape)) {
				inputMode = InputMode.Menu;
			}

			if (Input.GetKeyDown(KeyCode.LeftShift) && blinkResource >= 1f && !blinking) {

				blinking = true;
				
				blinkResource -= 1f;

				GameObject firstBlinkCharge = crosshairHackBlinkCurrent [0];
				crosshairHackBlinkDisappearing.Add (firstBlinkCharge);
				crosshairHackBlinkCurrent.Remove (firstBlinkCharge);

				blinkStart = this.transform.GetComponent<Rigidbody> ().position;

				float colliderOffset = this.transform.GetComponent<CapsuleCollider> ().radius;

				Vector3 direction = Vector3.zero;

				if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.DownArrow)) {
					direction += personalCamera.transform.forward;
				}
				if (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow)) {
					direction += -personalCamera.transform.forward;
				}
				if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) {
					direction += personalCamera.transform.right;
				}
				if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) {
					direction += -personalCamera.transform.right;
				}

				if (direction == Vector3.zero) {
					direction = personalCamera.transform.forward;
				}

				Vector3Nullable blockingPoint = firstNonPlayer (direction, blinkDistance + colliderOffset);

				if (blockingPoint.isNull == true) {
					blinkEnd = blinkStart + direction * blinkDistance;
				} else {
					float distanceToBlock = Vector3.Distance (blockingPoint.vector3, visualAvatar.transform.position + LocalPlayerScript.centerOfCamera);
					blinkEnd = blinkStart + direction * (distanceToBlock - colliderOffset);
				}

				//AudioSource audio = Hacks.GetAudioSource ("Sound/Effects/blink");
				//audio.pitch = 1f;
				//audio.volume = 1f;
				//audio.Play ();

			}

			if (Input.GetKeyDown(KeyCode.Space) && IsGrounded()) {

				this.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(this.gameObject.GetComponent<Rigidbody>().velocity.x, 6f, this.gameObject.GetComponent<Rigidbody>().velocity.z);

			}

		}

		if (blinking) {

			this.transform.GetComponent<Rigidbody> ().velocity = new Vector3 (0f, 0f, 0f);

			Vector3 previousPosition = this.transform.GetComponent<Rigidbody> ().position;
			Vector3 blinkDirection = blinkEnd - blinkStart;
			blinkDirection.Normalize ();
			Vector3 currentPosition = previousPosition + blinkDirection * Time.deltaTime * 80f;

			if (Vector3.Distance (blinkStart, currentPosition) >= Vector3.Distance (blinkStart, blinkEnd)) {
				// BLINKING HAS ENDED
				blinking = false;
				currentPosition = previousPosition;
			}

			this.transform.GetComponent<Rigidbody> ().MovePosition (currentPosition);
			personalCamera.GetComponent<Camera> ().fieldOfView = Mathf.Lerp (personalCamera.GetComponent<Camera> ().fieldOfView, 80f, Time.deltaTime * 10f);

		} else {
			personalCamera.GetComponent<Camera> ().fieldOfView = Mathf.Lerp (personalCamera.GetComponent<Camera> ().fieldOfView, 70f, Time.deltaTime * 10f);
		}

	}

	void handleMovementInput() {

		Vector2 movement = new Vector3 (0, 0);

		if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow)) {
			movement = new Vector3(movement.x, 1f);
		} 
		else if (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow)) {
			movement = new Vector3(movement.x, -1f);
		}
		if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) {
			movement = new Vector3(movement.x +1f, movement.y);
		}
		if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) {
			movement = new Vector3(movement.x -1f, movement.y);
		}

		bool falling = false;
		if (!IsGrounded ()) {
			falling = true;
			SmartCrossfade(visualAvatar.GetComponent<Animator>(), Animation.Fall);
		}


		if (movement.x == 0 && movement.y == 0) {
			// NO INPUT
			if (!falling) {
				SmartCrossfade(visualAvatar.GetComponent<Animator>(), Animation.Idle);
			}


			float aux = this.gameObject.GetComponent<Rigidbody>().velocity.y;

			if (notMoving <= 0f) {

				notMoving += Time.deltaTime;
				// ESTO ES PARA QUE AL SUBIR COLINAS Y PARAR NO HAGA UN BOUNCE HACIA ARRIBA RARO, ASI LE PERMITE REAJUSTARSE PERO DE FORMA SUAVE
				if (aux > 0.3f && IsGrounded()) { aux = 0.3f; }

			}

			this.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0f, aux, 0f);


		} else {

			notMoving = 0f;

			movement.Normalize();

			if (!falling) {
				if (Mathf.Abs (movement.x) >= Mathf.Abs(movement.y)) {
					if (movement.x > 0) { SmartCrossfade(visualAvatar.GetComponent<Animator>(), Animation.Move_L); }
					else { SmartCrossfade(visualAvatar.GetComponent<Animator>(), Animation.Move_R); }
				}
				else {
					if (movement.y > 0) { SmartCrossfade(visualAvatar.GetComponent<Animator>(), Animation.Move_F); }
					else { SmartCrossfade(visualAvatar.GetComponent<Animator>(), Animation.Move_B); }
				}
			}

			this.gameObject.GetComponent<Rigidbody>().MovePosition(this.gameObject.GetComponent<Rigidbody>().position + (this.gameObject.transform.forward*movement.y -this.gameObject.transform.right*movement.x)*characterSpeed*Time.fixedDeltaTime);

			if (IsGrounded ()) {
				footStepCooldown -= Time.deltaTime *(characterSpeed/baseSpeed);
				if (footStepCooldown <= 0f) {
					footStepCooldown = footStepCooldownMax*UnityEngine.Random.Range(0.95f, 1.05f);
					FootStep ();
				}
			}

		}

		if (!lastTimeGrounded && IsGrounded ()) {
			// JUST LANDED
			float amount = Mathf.Min(Mathf.Abs(this.gameObject.GetComponent<Rigidbody>().velocity.y)/Mathf.Abs(Physics.gravity.y)*0.1f, 0.15f);
			personalCamera.transform.localPosition = personalCamera.transform.localPosition - personalCamera.transform.up*amount;
			AudioSource audio = Hacks.GetAudioSource ("Sound/Effects/JumpLand");
			audio.volume = 0.7f * (amount/0.15f);
			audio.pitch = UnityEngine.Random.Range (0.85f, 1.15f);
			audio.Play ();
		}

		lastTimeGrounded = IsGrounded ();

	}

	void FootStep() {

		int amountFootSteps = 4;
		int aux = UnityEngine.Random.Range(1, amountFootSteps +1);
		AudioSource audio = Hacks.GetAudioSource ("Sound/Effects/Footsteps/Footstep_"+aux.ToString("00"));
		audio.volume = 0.3f;
		audio.pitch = UnityEngine.Random.Range (0.8f, 1f);
		audio.Play ();

	}

	bool IsGrounded()  {
		float distToGround = (float)this.gameObject.GetComponent<CapsuleCollider>().bounds.extents.y;
		RaycastHit[] hits;
		hits = Physics.RaycastAll (this.gameObject.transform.position + this.gameObject.GetComponent<CapsuleCollider>().center, -Vector3.up, distToGround + 0.3f);
		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].collider.gameObject.GetComponent<PlayerMarker> () == null) {
				return true;
			}
		}
		return false;
	}

	Vector3 IsGroundedVector3() {
		float distToGround = (float)this.gameObject.GetComponent<CapsuleCollider>().bounds.extents.y;
		RaycastHit hit;
		if (Physics.Raycast(this.gameObject.transform.position + this.gameObject.GetComponent<CapsuleCollider>().center, -Vector3.up, out hit, distToGround + 5f)) {
			return hit.normal;
		}
		return Vector3.up;
	}

	void SmartCrossfade(Animator animator, Animation animation) {

		if (lastAnimationOrder != animation && !animator.GetCurrentAnimatorStateInfo(0).IsName(AnimationString[(int)animation])) {
			animator.CrossFadeInFixedTime(AnimationString[(int)animation], GlobalData.crossfadeAnimation);
			lastAnimationOrder = animation;
		}

	}

	void handleCameraChanges() {

		// 0.75f IS A CONSTANT TO MAKE SENSITIVITY AS CLOSE TO CS:GO ONE AS POSSIBLE, MORE STANDARD FOR PLAYERS
		if (inputMode == InputMode.Playing) {
			cameraValueY += (Input.GetAxisRaw("Mouse Y"))*GlobalData.mouseSensitivity*0.75f;
		}


		//cameraValueY = Mathf.Clamp (cameraValueY, -60f, 60f);
		cameraValueY = Mathf.Clamp (cameraValueY, -90f, 90f);

		float compoundValueX = cameraValueX;
		float compoundValueY = cameraValueY;

		personalCamera.transform.localEulerAngles = new Vector3 (-compoundValueY, compoundValueX, personalCamera.transform.localEulerAngles.z);

		personalCamera.transform.localPosition = centerOfCamera;

		RaycastHit hit;
		Vector3 direction = -personalCamera.transform.forward;
		if (Physics.Raycast (personalCamera.transform.position, direction, out hit, cameraDistance)) {
			personalCamera.transform.position = hit.point;
		}
		else {
			personalCamera.transform.position = personalCamera.transform.position +(direction*cameraDistance);
		}


		float changeX = 0f;
		if (inputMode == InputMode.Playing) {
			changeX = (Input.GetAxisRaw("Mouse X"))*GlobalData.mouseSensitivity*0.75f; 
		}

		this.gameObject.transform.localEulerAngles = new Vector3 (this.gameObject.transform.localEulerAngles.x, this.gameObject.transform.localEulerAngles.y +changeX, this.gameObject.transform.localEulerAngles.z);

		lastPositionCursor = Input.mousePosition;

	}

    public static void RotateHead(GameObject visualAvatar, float currentCameraEulerX)
    {

		if (visualAvatar.GetComponent<Animator> ().enabled) {

			while (currentCameraEulerX < 0) {
				currentCameraEulerX += 360f;
			}

			GameObject neck = visualAvatar.transform.FindChild("Armature/Pelvis/Spine/Chest/Neck").gameObject;
			GameObject head = visualAvatar.transform.FindChild("Armature/Pelvis/Spine/Chest/Neck/Head").gameObject;

			float targetNeck = 0f;
			float targetHead = 0f;
			float minAngle = 25f;

			if (currentCameraEulerX > 180)
			{
				currentCameraEulerX = Mathf.Max (270f + minAngle, currentCameraEulerX);
				targetNeck = (-currentCameraEulerX +360f)*0.25f;
				targetHead = (-currentCameraEulerX +360f)*0.75f;
			}
			else
			{
				currentCameraEulerX = Mathf.Min (90f - minAngle, currentCameraEulerX);
				targetNeck = (-currentCameraEulerX)*0.75f;
				targetHead = (-currentCameraEulerX)*0.25f;
			}

			neck.transform.localEulerAngles = new Vector3 (0f, neck.transform.localEulerAngles.y, neck.transform.localEulerAngles.z);
			neck.transform.RotateAround (head.transform.position, head.transform.forward, targetNeck);
			head.transform.RotateAround(head.transform.position, head.transform.forward, targetHead);

		}

    }

	public enum InputMode
	{
		Playing,
		Menu,
		Chat
	};

	public enum Animation
	{
		Idle,
		Fall,
		Move_F,
		Move_B,
		Move_R,
		Move_L
	};

	public static string[] AnimationString = new string[] 
	{ 
		"Idle",
		"Fall",
		"Move_F",
		"Move_B",
		"Move_R",
		"Move_L"
	};

	public class Vector3Nullable {

		public bool isNull = true;
		public Vector3 vector3 = Vector3.zero;

		public Vector3Nullable() {

		}

	}

}
