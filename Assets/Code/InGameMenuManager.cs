using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class InGameMenuManager {

	public bool active = false;
	private string status = "activating";
	private GameObject physicalInGameMenu;
	private GameObject physicalOptionSource;
	private GameObject semicircleSource;
	private Option rootOption;
	private Option currentParentOption;
	private List<GameObject> physicalOptionsPool = new List<GameObject> ();
	public List<GameObject> semicirclesPool = new List<GameObject> ();
	private List<Option> ancientOptions = new List<Option> ();
	private List<Option> nonbornOptions = new List<Option> ();

	private static float separation = 7f;

	public int scrollValue = 0;
	public float scrollCooldown = 0f;

	// Use this for initialization
	public InGameMenuManager(GameObject auxPhysicalInGameMenu) {
		
		physicalInGameMenu = auxPhysicalInGameMenu;
		physicalOptionSource = physicalInGameMenu.transform.FindChild ("OptionSource").gameObject;
		physicalOptionSource.SetActive (false);
		semicircleSource = physicalInGameMenu.transform.FindChild ("SemicircleSource").gameObject;
		semicircleSource.SetActive (false);

		rootOption = new Option ("", Option.Action.None);
		Option graphicsOption = new Option ("Graphics", Option.Action.None);
		rootOption.AddChild (graphicsOption);
		Option audioOption = new Option ("Audio", Option.Action.None);
		rootOption.AddChild (audioOption);
		rootOption.AddChild (new Option ("Exit", Option.Action.Exit));

		Option fullScreenGraphicsOption = new Option ("Full Screen", Option.Action.None);
		graphicsOption.AddChild (fullScreenGraphicsOption);
		fullScreenGraphicsOption.AddChild (new Option ("On", Option.Action.FullScreenOn));
		fullScreenGraphicsOption.AddChild (new Option ("Off", Option.Action.FullScreenOff));

		Option resolutionScreenGraphicsOption = new Option ("Resolution", Option.Action.None);
		graphicsOption.AddChild (resolutionScreenGraphicsOption);
		for (int i = 0; i < Screen.resolutions.Length; i++) {
			resolutionScreenGraphicsOption.AddChild (new Option (Screen.resolutions[i].width + "x" + Screen.resolutions[i].height, Option.Action.ChangeResolution));
		}

		//moreVideoOption.AddChild(new Option ("The Game", Option.Action.None));

		audioOption.AddChild (new Option ("On", Option.Action.None));
		audioOption.AddChild (new Option ("Off", Option.Action.None));

	}
	// Update is called once per frame
	public void Update (float deltaTime) {

		if (!physicalInGameMenu.activeInHierarchy) {
			physicalInGameMenu.SetActive (true);
		}

		handleScroll ();

		if (status == "activating") {

			if (currentParentOption == null) {
				if (rootOption.assignedPhysicalOption == null) {
					rootOption.assignedPhysicalOption = GetPhysicalOption ();
				}
				Deepen (rootOption);
			}

			if (physicalInGameMenu.GetComponent<Image> ().color.a > 0f && Input.GetKeyDown (KeyCode.Escape) || Input.GetMouseButtonDown(1)) {
				Emerge ();
			}

			physicalInGameMenu.GetComponent<Image> ().color = Hacks.ColorLerpAlpha (physicalInGameMenu.GetComponent<Image> ().color, 0.5f, Time.deltaTime*5f);

			if (Input.GetKeyDown (KeyCode.DownArrow) || Input.GetKeyDown (KeyCode.S) || scrollValue == -1) {

				currentParentOption.selectedChild++;
				if (currentParentOption.selectedChild >= currentParentOption.children.Count) {
					currentParentOption.selectedChild = 0;
				}

			} else if (Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.W) || scrollValue == 1) {

				currentParentOption.selectedChild--;
				if (currentParentOption.selectedChild < 0) {
					currentParentOption.selectedChild = currentParentOption.children.Count - 1;
				}

			}

			if (Input.GetKeyDown (KeyCode.Return) || Input.GetMouseButtonDown(0)) {
				if (currentParentOption.children [currentParentOption.selectedChild].children.Count > 0) {
					
					Deepen(currentParentOption.children [currentParentOption.selectedChild]);

				} else if (currentParentOption.children [currentParentOption.selectedChild].action == Option.Action.Exit) {
					
					Network.Disconnect ();

				} else if (currentParentOption.children [currentParentOption.selectedChild].action == Option.Action.FullScreenOn) {
					
					PlayerPrefs.SetInt ("FullScreen", 1);
					GlobalData.fullScreen = true;
					Screen.SetResolution (GlobalData.screenWidth, GlobalData.screenHeight, GlobalData.fullScreen);

				} else if (currentParentOption.children [currentParentOption.selectedChild].action == Option.Action.FullScreenOff) {
					
					PlayerPrefs.SetInt ("FullScreen", 0);
					GlobalData.fullScreen = false;
					Screen.SetResolution (GlobalData.screenWidth, GlobalData.screenHeight, GlobalData.fullScreen);

				} else if (currentParentOption.children [currentParentOption.selectedChild].action == Option.Action.ChangeResolution) {

					GlobalData.screenWidth = Screen.resolutions [currentParentOption.selectedChild].width;
					GlobalData.screenHeight = Screen.resolutions [currentParentOption.selectedChild].height;

					PlayerPrefs.SetInt ("ScreenWidth", GlobalData.screenWidth);
					PlayerPrefs.SetInt ("ScreenHeight", GlobalData.screenHeight);

					Screen.SetResolution (GlobalData.screenWidth, GlobalData.screenHeight, GlobalData.fullScreen);

				}

			}

		} else if (status == "deactivating") {

			physicalInGameMenu.GetComponent<Image> ().color = Hacks.ColorLerpAlpha (physicalInGameMenu.GetComponent<Image> ().color, 0f, Time.deltaTime*20f);

			if (physicalInGameMenu.GetComponent<Image> ().color.a <= 0.05f && nonbornOptions.Count == 0) {
				physicalInGameMenu.GetComponent<Image> ().color = Hacks.ColorLerpAlpha (physicalInGameMenu.GetComponent<Image> ().color, 0f, 1f);
				physicalInGameMenu.SetActive (false);
				active = false;
				status = "activating";
			}

		}

		// HANDLE ANCESTORS
		List<Option> toRemove = new List<Option>();

		foreach (Option option in ancientOptions) {

			option.Update (deltaTime);

			int generation = 1;
			Option auxOption = currentParentOption.parentOption;

			while (auxOption != option) {
				auxOption = auxOption.parentOption;
				generation++;
			}

			option.circleAnchoredPosition = Vector2.Lerp (option.circleAnchoredPosition, new Vector2(-960f -240f*generation, 0f), Time.deltaTime*10f);
			option.assignedSemicircle.GetComponent<RectTransform> ().anchoredPosition = option.circleAnchoredPosition;

			option.circleLocalScale = Vector3.Lerp (option.circleLocalScale, new Vector3(1f, 1f, 1f), Time.deltaTime*10);
			option.assignedSemicircle.transform.localScale = option.circleLocalScale;

			if (option.assignedSemicircle.GetComponent<RectTransform> ().anchoredPosition.x < -1195f) {
				toRemove.Add (option);
			}

		}

		foreach (Option option in toRemove) {
			semicirclesPool.Add (option.assignedSemicircle);
			option.assignedSemicircle.SetActive (false);
			option.assignedSemicircle = null;

			physicalOptionsPool.Add (option.assignedPhysicalOption);
			option.assignedPhysicalOption.SetActive (false);
			option.assignedPhysicalOption = null;

			ancientOptions.Remove (option);
		}
		toRemove.Clear ();

		// HANDLE NONBORNS
		foreach (Option option in nonbornOptions) {

			option.Update (deltaTime);

			int generation = 1;
			Option auxOption = option;

			while (auxOption.parentOption != currentParentOption) {
				auxOption = auxOption.parentOption;
				generation++;
			}

			option.circleAnchoredPosition = Vector2.Lerp (option.circleAnchoredPosition, new Vector2(-960f +480f*generation, 0f), Time.deltaTime*15f);
			option.assignedSemicircle.GetComponent<RectTransform> ().anchoredPosition = option.circleAnchoredPosition;

			option.circleLocalScale = Vector3.Lerp (option.circleLocalScale, new Vector3(2f, 2f, 2f), Time.deltaTime*15f);
			option.assignedSemicircle.transform.localScale = option.circleLocalScale;

			option.circleColor = Hacks.ColorLerpAlpha (option.circleColor, -0.1f, Time.deltaTime*15f);
			option.assignedSemicircle.GetComponent<Image> ().color = option.circleColor;

			foreach (Option child in option.children) {
				child.assignedPhysicalOption.GetComponent<Text> ().color = Hacks.ColorLerpAlpha(child.color, option.circleColor.a, 1f);
			}

			if (option.circleColor.a <= 0f) {
				toRemove.Add (option);
			}

		}

		foreach (Option option in toRemove) {
			semicirclesPool.Add (option.assignedSemicircle);


			foreach (Option child in option.children) {
				if (child.assignedPhysicalOption != null) {
					child.assignedPhysicalOption.transform.SetParent (option.assignedSemicircle.transform.parent);
					child.assignedPhysicalOption.SetActive (false);
					physicalOptionsPool.Add (child.assignedPhysicalOption);
					child.assignedPhysicalOption = null;
				}
			}

			//option.parentOption.assignedPhysicalOption.transform.SetParent (option.parentOption.assignedSemicircle.transform);

			option.assignedSemicircle.SetActive (false);
			option.assignedSemicircle = null;
			nonbornOptions.Remove (option);
		}
		toRemove.Clear ();

		// REPOSITION CURRENTPARENT
		if (currentParentOption != null) {

			currentParentOption.Update (deltaTime);

			currentParentOption.circleAnchoredPosition = Vector2.Lerp (currentParentOption.circleAnchoredPosition, new Vector2(-960f, 0f), Time.deltaTime*10f);
			currentParentOption.assignedSemicircle.GetComponent<RectTransform> ().anchoredPosition = currentParentOption.circleAnchoredPosition;

			currentParentOption.circleLocalScale = Vector3.Lerp (currentParentOption.circleLocalScale, new Vector3(1f, 1f, 1f), Time.deltaTime*10f);
			currentParentOption.assignedSemicircle.transform.localScale = currentParentOption.circleLocalScale;

			currentParentOption.circleColor = Hacks.ColorLerpAlpha(currentParentOption.circleColor, 1f, Time.deltaTime*10f);
			currentParentOption.assignedSemicircle.GetComponent<Image> ().color = currentParentOption.circleColor;

			currentParentOption.assignedPhysicalOption.GetComponent<Text> ().text = currentParentOption.text;

			Vector3 targetScale = physicalOptionSource.transform.localScale*0.5f;
			Color targetColor = new Color (1f, 1f, 1f, 1f);

			currentParentOption.color = Hacks.ColorLerpAlpha (currentParentOption.color, currentParentOption.circleColor.a, Time.deltaTime * 5f);
			currentParentOption.localScale = Vector3.Lerp (currentParentOption.localScale, targetScale, Time.deltaTime*10f);
			currentParentOption.angle = Mathf.Lerp (currentParentOption.angle, 0f, Time.deltaTime * 10f);
			//currentParentOption.distance = Mathf.Lerp (currentParentOption.distance, 470f, Time.deltaTime * 20f);

			currentParentOption.localEulerAngles = new Vector3 (0f, 0f, currentParentOption.angle);

			currentParentOption.assignedPhysicalOption.transform.localScale = currentParentOption.localScale;
			currentParentOption.assignedPhysicalOption.transform.localEulerAngles = currentParentOption.localEulerAngles;
			currentParentOption.assignedPhysicalOption.GetComponent<Text> ().color = currentParentOption.color;

			Vector2 right;

			if (currentParentOption.parentOption != null && currentParentOption.parentOption.assignedSemicircle != null) {
				Transform auxParent = currentParentOption.assignedPhysicalOption.transform.parent;
				currentParentOption.assignedPhysicalOption.transform.SetParent (currentParentOption.parentOption.assignedSemicircle.transform);

				currentParentOption.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f - currentParentOption.parentOption.assignedSemicircle.GetComponent<RectTransform> ().sizeDelta.x / 2f, 0f);
				right = new Vector2 (currentParentOption.assignedPhysicalOption.GetComponent<RectTransform> ().right.x, currentParentOption.assignedPhysicalOption.GetComponent<RectTransform> ().right.y);
				currentParentOption.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition = currentParentOption.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition + right * (currentParentOption.distance + currentParentOption.assignedPhysicalOption.GetComponent<RectTransform> ().sizeDelta.x / 2f * currentParentOption.assignedPhysicalOption.transform.localScale.x);
			
				currentParentOption.assignedPhysicalOption.transform.SetParent (auxParent);
			} 
			 else {
				currentParentOption.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f - currentParentOption.assignedSemicircle.GetComponent<RectTransform> ().sizeDelta.x / 2f, 0f) + new Vector2(1f, 0f) * (230f + currentParentOption.assignedPhysicalOption.GetComponent<RectTransform> ().sizeDelta.x / 2f * currentParentOption.assignedPhysicalOption.transform.localScale.x);
			}



			currentParentOption.position = currentParentOption.assignedPhysicalOption.transform.position;

			// REPOSITION CURRENT CHILDREN
			foreach (Option option in currentParentOption.children) {

				option.Update (deltaTime);

				int index = currentParentOption.children.IndexOf (option);
				int relativePosition = index - currentParentOption.selectedChild;

				option.assignedPhysicalOption.GetComponent<Text> ().text = option.text;

				targetScale = physicalOptionSource.transform.localScale;
				targetColor = new Color (1f, 1f, 1f, currentParentOption.circleColor.a);

				if (currentParentOption.selectedChild != index) {
					targetScale *= 0.5f;
				}

				option.color = Hacks.ColorLerpAlpha (option.color, currentParentOption.circleColor.a, Time.deltaTime * 5f);
				option.localScale = Vector3.Lerp (option.localScale, targetScale, Time.deltaTime*10f);
				option.angle = Mathf.Lerp (option.angle, relativePosition * -separation, Time.deltaTime * 10f);
				//option.distance = Mathf.Lerp (option.distance, 470f, Time.deltaTime * 20f);

				option.localEulerAngles = new Vector3 (0f, 0f, option.angle);

				option.assignedPhysicalOption.transform.localScale = option.localScale;
				option.assignedPhysicalOption.transform.localEulerAngles = option.localEulerAngles;
				option.assignedPhysicalOption.GetComponent<Text> ().color = option.color;

				option.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f - currentParentOption.assignedSemicircle.GetComponent<RectTransform>().sizeDelta.x/2f, 0f);
				right = new Vector2 (option.assignedPhysicalOption.GetComponent<RectTransform> ().right.x, option.assignedPhysicalOption.GetComponent<RectTransform> ().right.y);
				option.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition = option.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition + right*(option.distance + option.assignedPhysicalOption.GetComponent<RectTransform> ().sizeDelta.x/2f * option.assignedPhysicalOption.transform.localScale.x);

				option.position = option.assignedPhysicalOption.transform.position;

			}

		}
	
	}

	void handleScroll() {

		if (Input.mouseScrollDelta.y == 0f) {
			scrollCooldown = Mathf.Max (0f, scrollCooldown - Time.deltaTime);
		} else {
			scrollCooldown = 0.1f;
		}

		if (scrollCooldown <= 0f) {
			scrollValue = 0;
		}

		if (scrollValue == 0) {
			if (Input.mouseScrollDelta.y > 0.01f) {
				scrollValue = 1;
			} else if (Input.mouseScrollDelta.y < -0.01f) {
				scrollValue = -1;
			}
		} else {
			scrollValue = 2;
		}

	}

	void Deepen(Option newRoot) {

		// STORE UNUSED
		if (currentParentOption != null) {

			currentParentOption.assignedPhysicalOption.transform.SetParent (currentParentOption.assignedSemicircle.transform);
			
			//physicalOptionsPool.Add (currentParentOption.assignedPhysicalOption);
			//currentParentOption.assignedPhysicalOption.SetActive (false);
			//currentParentOption.assignedPhysicalOption = null;

			foreach (Option option in currentParentOption.children) {

				if (option != newRoot) {
					physicalOptionsPool.Add (option.assignedPhysicalOption);
					option.assignedPhysicalOption.SetActive (false);
					option.assignedPhysicalOption = null;
				}

			}

		}

		// CHANGE CURRENTPARENT
		if (currentParentOption != null) {
			ancientOptions.Add(currentParentOption);
		}
		currentParentOption = newRoot;
		nonbornOptions.Remove (currentParentOption);

		// ASSIGN UNUSED
		if (currentParentOption.assignedSemicircle == null) {
			currentParentOption.assignedSemicircle = GetSemicircle ();
		}
			
		currentParentOption.assignedPhysicalOption.transform.SetParent (currentParentOption.assignedSemicircle.transform);

		foreach (Option option in currentParentOption.children) {

			if (option.assignedPhysicalOption == null) {
				option.assignedPhysicalOption = GetPhysicalOption ();
				option.assignedPhysicalOption.transform.SetParent (currentParentOption.assignedSemicircle.transform);
			}

		}
			
		//currentParentOption.assignedSemicircle.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (-480f, 0f);
		//currentParentOption.assignedSemicircle.transform.localScale = new Vector3(2f, 2f, 2f);

		currentParentOption.circleColor = new Color (1f, 1f, 1f, 0f);
		currentParentOption.assignedSemicircle.GetComponent<Image> ().color = currentParentOption.circleColor;

	}

	void Emerge() {

		if (currentParentOption == rootOption) {
			status = "deactivating";
		}

		// CHANGE CURRENTPARENT
		nonbornOptions.Add(currentParentOption);
		currentParentOption = currentParentOption.parentOption;
		ancientOptions.Remove (currentParentOption);



		if (currentParentOption != null) {

			// ASSIGN UNUSED
			if (currentParentOption.assignedSemicircle == null) {
				currentParentOption.assignedSemicircle = GetSemicircle ();
			}

			if (currentParentOption.assignedPhysicalOption == null) {
				currentParentOption.assignedPhysicalOption = GetPhysicalOption ();
			}
			currentParentOption.assignedPhysicalOption.transform.SetParent (currentParentOption.assignedSemicircle.transform);

			foreach (Option option in currentParentOption.children) {

				if (option.assignedPhysicalOption == null) {
					option.assignedPhysicalOption = GetPhysicalOption ();
				}
				option.assignedPhysicalOption.transform.SetParent (currentParentOption.assignedSemicircle.transform);

			}
		}

	}

	GameObject GetPhysicalOption() {

		GameObject physicalOption;

		if (physicalOptionsPool.Count > 0) {
			physicalOption = physicalOptionsPool [0];
			physicalOptionsPool.RemoveAt (0);
			physicalOption.SetActive (true);
		} else {
			physicalOption = MonoBehaviour.Instantiate (physicalOptionSource);
			physicalOption.transform.SetParent (physicalInGameMenu.transform);
			physicalOption.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f, 0f);
			physicalOption.transform.localScale = physicalOptionSource.transform.localScale;
			physicalOption.SetActive (true);
		}

		return physicalOption;

	}

	GameObject GetSemicircle() {

		GameObject semicircle;

		if (semicirclesPool.Count > 0) {
			semicircle = semicirclesPool [0];
			semicirclesPool.RemoveAt (0);
			semicircle.SetActive (true);
		} else {
			semicircle = MonoBehaviour.Instantiate (semicircleSource);
			semicircle.transform.SetParent (physicalInGameMenu.transform);
			semicircle.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f, 0f);
			semicircle.transform.localScale = semicircleSource.transform.localScale;
			semicircle.SetActive (true);
		}

		return semicircle;

	}

	public class Option {

		public Option parentOption;
		public string text = "";
		public Action action;
		public List<Option> children = new List<Option> ();
		public int selectedChild = 0;

		public GameObject assignedPhysicalOption = null;
		public float angle = 0f;
		public Vector3 localEulerAngles = new Vector3(0f, 0f, 0f);
		public Vector3 localScale = new Vector3 (0.3f, 0.3f, 0.3f);
		public Vector3 position = new Vector3(0f, 0f, 0f);
		public Color color = new Color (1f, 1f, 1f, 0f);
		public float distance = 470f;

		public GameObject assignedSemicircle = null;
		public Color circleColor = new Color (1f, 1f, 1f, 0f);
		public Vector2 circleAnchoredPosition = new Vector2(-480f, 0f);
		public Vector3 circleLocalScale = new Vector3(2f, 2f, 2f);

		private static Color chosenColor = new Color (230f/255f, 230f/255f, 90f/255f);
		private static Color selectedColor = new Color (1f, 1f, 1f);
		private static Color unselectedColor = new Color (0.65f, 0.65f, 0.65f);

		public Option (string auxText, Action auxAction) {

			text = auxText;
			action = auxAction;

		}

		public void Update(float deltaTime) {

			color = new Color (unselectedColor.r, unselectedColor.g, unselectedColor.b, color.a);
			if (parentOption != null && parentOption.children [parentOption.selectedChild] == this) {
				color = new Color (selectedColor.r, selectedColor.g, selectedColor.b, color.a);
			}

			if (action == Action.FullScreenOn) {
				
				if (GlobalData.fullScreen) { color = new Color (chosenColor.r, chosenColor.g, chosenColor.b, color.a); }

			} else if (action == Action.FullScreenOff) {
				
				if (!GlobalData.fullScreen) { color = new Color (chosenColor.r, chosenColor.g, chosenColor.b, color.a); }

			} else if (action == Action.ChangeResolution) {

				string[] res = text.Split('x');
				if (res [0] == "" + GlobalData.screenWidth && res [1] == "" + GlobalData.screenHeight) {
					color = color = new Color (chosenColor.r, chosenColor.g, chosenColor.b, color.a);
				}

			}



		}

		public void AddChild(Option o) {
			o.angle = -separation * children.Count;
			children.Add (o);
			o.parentOption = this;
			if (children.IndexOf (o) != selectedChild) {
				o.localScale = new Vector3 (0.15f, 0.15f, 0.15f);
				o.color = new Color (0.75f, 0.75f, 0.75f, 0f);
			}
		}

		public enum Action
		{
			None,
			FullScreenOn,
			FullScreenOff,
			ChangeResolution,
			Exit
		};

	}

}
