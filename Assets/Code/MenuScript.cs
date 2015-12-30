using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {

	private string menuMode = "main";

	private GameObject menuMain;
	private GameObject join;
	private GameObject create;
	private GameObject exit;
	private float initialMenuMainTextScale;

	private GameObject menuJoin;
	private GameObject canvasJoin;
	public GameObject canvasJoinContent;
	private GameObject menuJoinBack;

	private GameObject menuCreate;
	private GameObject menuCreateGo;
	private GameObject menuCreateBack;

	private List<Match> currentMatches = new List<Match> ();

	private GameObject test;

	void OnServerInitialized()
	{
		Debug.Log("Server Initializied");
	}

	// Use this for initialization
	void Start () {

		menuMain = GameObject.Find ("MenuMain");
		join = GameObject.Find ("Join");
		create = GameObject.Find ("Create");
		exit = GameObject.Find ("Exit");
		initialMenuMainTextScale = join.transform.localScale.x;

		menuJoinBack = GameObject.Find ("MenuJoin/Back");
		menuJoin = GameObject.Find ("MenuJoin");
		menuJoin.SetActive (false);
		canvasJoin = GameObject.Find ("Canvas/CanvasJoin");
		canvasJoinContent = GameObject.Find ("Canvas/CanvasJoin/Panel/ScrollRect/Content");
		canvasJoin.SetActive (false);

		menuCreateGo = GameObject.Find ("MenuCreate/Details/Go");
		menuCreateBack = GameObject.Find ("MenuCreate/Back");
		menuCreate = GameObject.Find ("MenuCreate");
		menuCreate.SetActive (false);
	
	}
	
	// Update is called once per frame
	void Update () {

		if (menuMode == "main") {

			checkMenuMainOptions ();

		} else if (menuMode == "join") {

			checkMenuJoinOptions();

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

				flushMatches();

				int num = Random.Range(10, 101);

				for (int i = 1; i <= num; i++) {
					
					Match m = new Match (this, "Partida "+i);
					currentMatches.Add (m);
					m.adjustPosition();
					
				}
				
				float size = (currentMatches.Count -17)*24f;
				if (size < 0f) { size = 0f; }
				canvasJoinContent.GetComponent<RectTransform> ().sizeDelta = new Vector2 (0f, size);
				canvasJoinContent.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f, -canvasJoinContent.GetComponent<RectTransform> ().sizeDelta.y/2f);

			}
			else if (Hacks.isOver(create)) {
				menuMode = "create";
				menuCreate.SetActive(true);
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
				NetworkManager.StartServer("Test");
				Application.LoadLevel("Game");
			}
			else if (Hacks.isOver(menuCreateBack)) {
				menuMode = "main";
				menuMain.SetActive(true);
				menuCreate.SetActive(false);
			}
		}
		
	}

	private void checkMenuJoinOptions() {
		
		// CHECK FOR CLICK
		if (Input.GetMouseButtonDown (0)) {
			if (false) {
				//NetworkManager.StartServer("Test");
				Application.LoadLevel("Game");
			}
			else if (Hacks.isOver(menuJoinBack)) {
				menuMode = "main";
				menuMain.SetActive(true);
				canvasJoin.SetActive(false);
				menuJoin.SetActive(false);
			}
		}
		
	}

	private void flushMatches() {

		while (currentMatches.Count > 0) {

			Destroy(currentMatches[0].root);
			currentMatches[0] = null;
			currentMatches.RemoveAt(0);

		}

	}

	private class Match {

		private MenuScript owner;
		public GameObject root;
		public string matchName;

		public Match(MenuScript auxOwner, string auxMatchName) {

			owner = auxOwner;
			matchName = auxMatchName;

			root = Instantiate (Resources.Load("Prefabs/CanvasJoinMatch") as GameObject);
			root.gameObject.transform.parent = owner.canvasJoinContent.transform;
			root.GetComponent<Text>().text = matchName;
			root.transform.localScale = new Vector3 (1f, 1f, 1f);


		}

		public void adjustPosition() {

			int auxPosition = owner.currentMatches.IndexOf(this);
			root.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f, -490f -auxPosition*24f);
			root.GetComponent<RectTransform> ().sizeDelta = new Vector2 (720f, 964f);

		}

	}



}
