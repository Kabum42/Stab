using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class MenuScript : MonoBehaviour {

	private string menuMode = "main";

	private GameObject canvas;

	private GameObject menuMain;
	private GameObject join;
	private GameObject create;
	private GameObject exit;
	private float initialMenuMainTextScale;

	private GameObject menuJoin;
	private GameObject menuJoinBack;
	private GameObject menuJoinRefresh;
	private GameObject menuJoinPlay;
	private GameObject canvasJoin;
	public GameObject canvasJoinContent;

	private GameObject menuCreate;
	private GameObject menuCreateGo;
	private GameObject menuCreateBack;
	private GameObject canvasCreate;

	private List<Match> currentMatches = new List<Match> ();
	private int selectedMatch = -1;

	private GameObject test;

	private List<PingMatch> unresolvedPingMatches = new List<PingMatch>();

	private AudioSource menuMusic;
	private VignetteAndChromaticAberration vignette;

	void OnServerInitialized()
	{
		Debug.Log("Server Initializied");
	}

	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.HostListReceived)
		{
			NetworkManager.hostList = MasterServer.PollHostList();

			flushMatches();

			for (int i = 0; i < NetworkManager.hostList.Length; i++) {
				
				Match m = new Match (this, ref NetworkManager.hostList[i], currentMatches.Count);
				currentMatches.Add (m);
				m.adjustPosition();
				
			}
			
			float size = (currentMatches.Count -17)*24f;
			if (size < 0f) { size = 0f; }
			canvasJoinContent.GetComponent<RectTransform> ().sizeDelta = new Vector2 (0f, size);
			canvasJoinContent.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f, -canvasJoinContent.GetComponent<RectTransform> ().sizeDelta.y/2f);
		}
	}

	void OnConnectedToServer()
	{
		Debug.Log("Server Joined");
		Application.LoadLevel ("Game");
	}

	// Use this for initialization
	void Start () {

		GlobalData.Start ();

		menuMusic = this.gameObject.AddComponent<AudioSource> ();
		menuMusic.clip = Resources.Load("Sound/Music/Cyberium") as AudioClip;
		menuMusic.loop = true;
		menuMusic.spatialBlend = 0f;
		menuMusic.volume = 0.25f;
		//menuMusic.Play ();

		//vignette = this.GetComponent<VignetteAndChromaticAberration> ();
		//vignette.intensity = 500f;

		canvas = GameObject.Find ("Canvas");

		menuMain = GameObject.Find ("MenuMain");
		join = GameObject.Find ("Join");
		create = GameObject.Find ("Create");
		exit = GameObject.Find ("Exit");
		initialMenuMainTextScale = join.transform.localScale.x;

		menuJoinBack = GameObject.Find ("MenuJoin/Back");
		menuJoinRefresh = GameObject.Find ("MenuJoin/Matches/Refresh");
		menuJoinPlay = GameObject.Find ("MenuJoin/Matches/Play");
		menuJoin = GameObject.Find ("MenuJoin");
		menuJoin.SetActive (false);
		canvasJoin = GameObject.Find ("Canvas/CanvasJoin");
		canvasJoinContent = GameObject.Find ("Canvas/CanvasJoin/Panel/ScrollRect/Content");
		canvasJoin.SetActive (false);

		menuCreateGo = GameObject.Find ("MenuCreate/Details/Go");
		menuCreateBack = GameObject.Find ("MenuCreate/Back");
		menuCreate = GameObject.Find ("MenuCreate");
		menuCreate.SetActive (false);
		canvasCreate = GameObject.Find ("Canvas/CanvasCreate");
		canvasCreate.SetActive (false);
	
	}
	
	// Update is called once per frame
	void Update () {

		//vignette.intensity = Mathf.Lerp (vignette.intensity, 7f, Time.deltaTime * 20f);

		if (menuMode == "main") {

			checkMenuMainOptions ();

		} else if (menuMode == "join") {

			checkMenuJoinOptions();
			checkMouseMatches();

		} else if (menuMode == "create") {

			checkMenuCreateOptions();

		}
	
	}

	private void checkMenuMainOptions() {

		updateMenuMainOption (join);
		updateMenuMainOption (create);
		updateMenuMainOption (exit);

		// CHECK FOR CLICK
		if (Input.GetMouseButtonDown (0)) {
			if (Hacks.isOver(join)) {
				menuMode = "join";
				menuJoin.SetActive(true);
				canvasJoin.SetActive(true);
				menuMain.SetActive(false);

				reloadMatches();
			}
			else if (Hacks.isOver(create)) {
				menuMode = "create";
				menuCreate.SetActive(true);
				canvasCreate.SetActive(true);
				menuMain.SetActive(false);
			}
			else if (Hacks.isOver(exit)) {
				Application.Quit();
			}
		}

	}

	private void updateMenuMainOption(GameObject g) {

		if (Hacks.isOver (g)) {

			float targetScale = Mathf.Lerp(g.transform.localScale.x, initialMenuMainTextScale*1.3f, Time.deltaTime*10f);
			g.transform.localScale = new Vector3(targetScale, targetScale, targetScale);
			g.GetComponent<TextMesh>().color = new Color(195f/255f, 20f/255f, 20f/255f);

		} else {

			float targetScale = Mathf.Lerp(g.transform.localScale.x, initialMenuMainTextScale, Time.deltaTime*10f);
			g.transform.localScale = new Vector3(targetScale, targetScale, targetScale);
			g.GetComponent<TextMesh>().color = new Color(1f, 1f, 1f);

		}

	}

	private void checkMenuCreateOptions() {
		
		// CHECK FOR CLICK
		if (Input.GetMouseButtonDown (0)) {
			if (Hacks.isOver(menuCreateGo)) {
				if (canvasCreate.transform.FindChild("NameField").GetComponent<InputField>().text != "") {
					string roomName = canvasCreate.transform.FindChild("NameField").GetComponent<InputField>().text;
					string password = canvasCreate.transform.FindChild("PasswordField").GetComponent<InputField>().text;
					if (password != "") {
						// WITH PASSWORD
						NetworkManager.StartServer(roomName, password);
					}
					else {
						// WITHOUT PASSWORD
						NetworkManager.StartServer(roomName);
					}
					Application.LoadLevel("Game");
				}
			}
			else if (Hacks.isOver(menuCreateBack)) {
				menuMode = "main";
				menuMain.SetActive(true);
				menuCreate.SetActive(false);
				canvasCreate.SetActive(false);
			}
		}
		
	}

	private void checkMenuJoinOptions() {

		for (int i = 0; i < unresolvedPingMatches.Count; i++) {
			unresolvedPingMatches[i].Check();
		}
		
		// CHECK FOR CLICK
		if (Input.GetMouseButtonDown (0)) {
			if (Hacks.isOver(menuJoinPlay) && selectedMatch != -1) {
				NetworkManager.JoinServer(NetworkManager.hostList[selectedMatch]);
			}
			else if (Hacks.isOver(menuJoinBack)) {
				menuMode = "main";
				menuMain.SetActive(true);
				canvasJoin.SetActive(false);
				menuJoin.SetActive(false);
			}
			else if (Hacks.isOver(menuJoinRefresh)) {
				reloadMatches();
			}
		}
		
	}

	private void checkMouseMatches() {

		for (int i = 0; i < currentMatches.Count; i++) {

			float scaleFactor = canvas.GetComponent<Canvas>().scaleFactor;
			
			float xCenter = currentMatches[i].root.GetComponent<RectTransform>().transform.TransformPoint(currentMatches[i].root.GetComponent<RectTransform>().rect.center).x;
			float xStart = xCenter - (currentMatches[i].root.GetComponent<RectTransform>().rect.width/2f)*scaleFactor;
			float xEnd = xCenter + (currentMatches[i].root.GetComponent<RectTransform>().rect.width/2f)*scaleFactor;
			
			float yCenter = currentMatches[i].root.GetComponent<RectTransform>().transform.TransformPoint(currentMatches[i].root.GetComponent<RectTransform>().rect.center).y;
			float yStart = yCenter + 481f*scaleFactor;
			float yEnd = yStart - 22f*scaleFactor;

			if (i == selectedMatch) {
				currentMatches[i].root.GetComponent<Text>().color = new Color (192f/255f, 20f/255f, 20f/255f);
			}
			else if (Input.mousePosition.x >= xStart && Input.mousePosition.x <= xEnd
			    && Input.mousePosition.y <= yStart && Input.mousePosition.y >= yEnd
			    && Input.mousePosition.y >= 105f*scaleFactor && Input.mousePosition.y <= 523f*scaleFactor) {

				currentMatches[i].root.GetComponent<Text>().color = new Color (192f/255f, 192f/255f, 0f);
				if (Input.GetMouseButtonDown(0)) {
					selectedMatch = i;
				}

			}
			else {
				currentMatches[i].root.GetComponent<Text>().color = new Color (1f, 1f, 1f);
			}
			
		}

	}

	private void reloadMatches() {

		NetworkManager.RefreshHostList ();

	}

	private void flushMatches() {

		selectedMatch = -1;

		while (currentMatches.Count > 0) {

			Destroy(currentMatches[0].root);
			currentMatches[0] = null;
			currentMatches.RemoveAt(0);

		}

		while (unresolvedPingMatches.Count > 0) {

			unresolvedPingMatches[0] = null;
			unresolvedPingMatches.RemoveAt(0);

		}

	}

	private class Match {

		public MenuScript owner;
		public GameObject root;
		public string matchName;
		public int position;
		public int players;
		public int pingTime = -1;

		public Match(MenuScript auxOwner, ref HostData hData, int auxPosition) {

			owner = auxOwner;
			matchName = hData.gameName;
			players = hData.connectedPlayers;
			position = auxPosition;

			owner.unresolvedPingMatches.Add(new PingMatch(this, hData.ip[0]));

			root = Instantiate (Resources.Load("Prefabs/CanvasJoinMatch") as GameObject);
			root.gameObject.transform.parent = owner.canvasJoinContent.transform;
			root.GetComponent<Text>().text = matchName + "  " + players +"/20";
			root.transform.localScale = new Vector3 (1f, 1f, 1f);


		}

		public void adjustPosition() {

			root.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f, -490f -position*24f);
			root.GetComponent<RectTransform> ().sizeDelta = new Vector2 (720f, 964f);

		}

		public void UpdatePingTime(int auxPingTime) {
			pingTime = auxPingTime;
			root.GetComponent<Text>().text = matchName + "  " + players +"/20" + "  "+pingTime+ "ms";
		}


	}

	private class PingMatch {

		public Match match;
		public Ping ping;

		public PingMatch(Match auxMatch, string auxIpAdress) {

			match = auxMatch;
			Debug.Log(auxIpAdress);
			ping = new Ping(auxIpAdress);

		}

		public void Check() {

			if (ping.isDone) {
				match.UpdatePingTime(ping.time);
				match.owner.unresolvedPingMatches.Remove(this);
			}

		}

	}



}
