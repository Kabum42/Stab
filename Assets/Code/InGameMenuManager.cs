using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class InGameMenuManager {

	public bool active = false;
	private string status = "activating";
	public GameObject physicalInGameMenu;
	private GameObject physicalOptionSource;
	private GameObject semicircle;
	public Option rootOption;
	private Option currentParentOption;
	public List<GameObject> physicalOptionsPool = new List<GameObject> ();

	private static float separation = 7f;

	// Use this for initialization
	public InGameMenuManager(GameObject auxPhysicalInGameMenu) {
		
		physicalInGameMenu = auxPhysicalInGameMenu;
		physicalOptionSource = physicalInGameMenu.transform.FindChild ("OptionSource").gameObject;
		physicalOptionSource.SetActive (false);
		semicircle = physicalInGameMenu.transform.FindChild ("Semicircle").gameObject;

		rootOption = new Option ("", Option.Action.None);
		currentParentOption = rootOption;
		Option videoOption = new Option ("Video", Option.Action.None);
		rootOption.AddChild (videoOption);
		Option audioOption = new Option ("Audio", Option.Action.None);
		rootOption.AddChild (audioOption);
		rootOption.AddChild (new Option ("Exit", Option.Action.Exit));

		videoOption.AddChild (new Option ("1920x1080", Option.Action.None));
		videoOption.AddChild (new Option ("1080x720", Option.Action.None));
		Option moreVideoOption = new Option ("More Options", Option.Action.None);
		videoOption.AddChild (moreVideoOption);

		moreVideoOption.AddChild(new Option ("The Game", Option.Action.None));

		audioOption.AddChild (new Option ("On", Option.Action.None));
		audioOption.AddChild (new Option ("Off", Option.Action.None));

	}
	// Update is called once per frame
	public void Update (float deltaTime) {

		if (!physicalInGameMenu.activeInHierarchy) {
			physicalInGameMenu.SetActive (true);
		}

		if (status == "activating") {

			if (physicalInGameMenu.GetComponent<Image> ().color.a > 0f && Input.GetKeyDown (KeyCode.Escape)) {
				if (currentParentOption == rootOption) {
					status = "deactivating";
				} else {
					Emerge ();
				}
			}

			physicalInGameMenu.GetComponent<Image> ().color = Hacks.ColorLerpAlpha (physicalInGameMenu.GetComponent<Image> ().color, 0.5f, Time.deltaTime*5f);

			if (Input.GetKeyDown (KeyCode.DownArrow) || Input.GetKeyDown (KeyCode.S)) {

				currentParentOption.selectedChild++;
				if (currentParentOption.selectedChild >= currentParentOption.children.Count) {
					currentParentOption.selectedChild = 0;
				}

			} else if (Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.W)) {

				currentParentOption.selectedChild--;
				if (currentParentOption.selectedChild < 0) {
					currentParentOption.selectedChild = currentParentOption.children.Count - 1;
				}

			}

			if (Input.GetKeyDown (KeyCode.Return)) {
				if (currentParentOption.children [currentParentOption.selectedChild].children.Count > 0) {
					Deepen(currentParentOption.children [currentParentOption.selectedChild]);
				} else if (currentParentOption.children [currentParentOption.selectedChild].action == Option.Action.Exit) {
					Network.Disconnect ();
				}
			}

		} else if (status == "deactivating") {

			physicalInGameMenu.GetComponent<Image> ().color = Hacks.ColorLerpAlpha (physicalInGameMenu.GetComponent<Image> ().color, 0f, Time.deltaTime*20f);

			if (physicalInGameMenu.GetComponent<Image> ().color.a <= 0.05f) {
				physicalInGameMenu.GetComponent<Image> ().color = Hacks.ColorLerpAlpha (physicalInGameMenu.GetComponent<Image> ().color, 0f, 1f);
				physicalInGameMenu.SetActive (false);
				active = false;
				status = "activating";
			}

		}

		if (currentParentOption.assignedPhysicalOption == null) {
			currentParentOption.assignedPhysicalOption = GetPhysicalOption ();
		}

		// REPOSITION CURRENTPARENT

		currentParentOption.assignedPhysicalOption.GetComponent<Text> ().text = currentParentOption.text;

		Vector3 targetScale = physicalOptionSource.transform.localScale*0.5f;
		Color targetColor = new Color (1f, 1f, 1f, 1f);

		currentParentOption.color = Color.Lerp (currentParentOption.color, targetColor, Time.deltaTime * 5f);
		currentParentOption.localScale = Vector3.Lerp (currentParentOption.localScale, targetScale, Time.deltaTime*10f);
		currentParentOption.angle = Mathf.Lerp (currentParentOption.angle, 0f, Time.deltaTime * 10f);
		currentParentOption.distance = Mathf.Lerp (currentParentOption.distance, 230f, Time.deltaTime * 20f);

		currentParentOption.localEulerAngles = new Vector3 (0f, 0f, currentParentOption.angle);

		currentParentOption.assignedPhysicalOption.transform.localScale = currentParentOption.localScale;
		currentParentOption.assignedPhysicalOption.transform.localEulerAngles = currentParentOption.localEulerAngles;
		currentParentOption.assignedPhysicalOption.GetComponent<Text> ().color = currentParentOption.color;

		currentParentOption.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (semicircle.GetComponent<RectTransform>().anchoredPosition.x - semicircle.GetComponent<RectTransform>().sizeDelta.x/2f, 0f);
		Vector2 right = new Vector2 (currentParentOption.assignedPhysicalOption.GetComponent<RectTransform> ().right.x, currentParentOption.assignedPhysicalOption.GetComponent<RectTransform> ().right.y);
		currentParentOption.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition = currentParentOption.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition + right*(currentParentOption.distance + currentParentOption.assignedPhysicalOption.GetComponent<RectTransform> ().sizeDelta.x/2f * currentParentOption.assignedPhysicalOption.transform.localScale.x);

		currentParentOption.position = currentParentOption.assignedPhysicalOption.transform.position;

		// REPOSITION CURRENT CHILDREN
		foreach (Option option in currentParentOption.children) {

			if (option.assignedPhysicalOption == null) {
				option.assignedPhysicalOption = GetPhysicalOption ();
			}

			int index = currentParentOption.children.IndexOf (option);
			int relativePosition = index - currentParentOption.selectedChild;

			option.assignedPhysicalOption.GetComponent<Text> ().text = option.text;

			targetScale = physicalOptionSource.transform.localScale;
			targetColor = new Color (1f, 1f, 1f, 1f);

			if (currentParentOption.selectedChild != index) {
				targetScale *= 0.5f;
				targetColor = new Color (0.75f, 0.75f, 0.75f, 1f);
			}

			option.color = Color.Lerp (option.color, targetColor, Time.deltaTime * 5f);
			option.localScale = Vector3.Lerp (option.localScale, targetScale, Time.deltaTime*10f);
			option.angle = Mathf.Lerp (option.angle, relativePosition * -separation, Time.deltaTime * 10f);
			option.distance = Mathf.Lerp (option.distance, 460f, Time.deltaTime * 20f);

			option.localEulerAngles = new Vector3 (0f, 0f, option.angle);

			option.assignedPhysicalOption.transform.localScale = option.localScale;
			option.assignedPhysicalOption.transform.localEulerAngles = option.localEulerAngles;
			option.assignedPhysicalOption.GetComponent<Text> ().color = option.color;

			option.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (semicircle.GetComponent<RectTransform>().anchoredPosition.x - semicircle.GetComponent<RectTransform>().sizeDelta.x/2f, 0f);
			right = new Vector2 (option.assignedPhysicalOption.GetComponent<RectTransform> ().right.x, option.assignedPhysicalOption.GetComponent<RectTransform> ().right.y);
			option.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition = option.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition + right*(option.distance + option.assignedPhysicalOption.GetComponent<RectTransform> ().sizeDelta.x/2f * option.assignedPhysicalOption.transform.localScale.x);

			option.position = option.assignedPhysicalOption.transform.position;

		}

	
	}

	void Deepen(Option newRoot) {

		physicalOptionsPool.Add (currentParentOption.assignedPhysicalOption);
		currentParentOption.assignedPhysicalOption.SetActive (false);
		currentParentOption.assignedPhysicalOption = null;

		foreach (Option option in currentParentOption.children) {

			if (option != newRoot) {
				physicalOptionsPool.Add (option.assignedPhysicalOption);
				option.assignedPhysicalOption.SetActive (false);
				option.assignedPhysicalOption = null;
			}

		}

		currentParentOption = newRoot;

	}

	void Emerge() {

		foreach (Option option in currentParentOption.children) {

			physicalOptionsPool.Add (option.assignedPhysicalOption);
			option.assignedPhysicalOption.SetActive (false);
			option.assignedPhysicalOption = null;

		}

		currentParentOption = currentParentOption.parentOption;

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
		public Color color = new Color (1f, 1f, 1f, 1f);
		public float distance = 460f;

		public Option (string auxText, Action auxAction) {

			text = auxText;
			action = auxAction;

		}

		public void AddChild(Option o) {
			o.angle = -separation * children.Count;
			children.Add (o);
			o.parentOption = this;
			if (children.IndexOf (o) != selectedChild) {
				o.localScale = new Vector3 (0.15f, 0.15f, 0.15f);
				o.color = new Color (0.75f, 0.75f, 0.75f, 1f);
			}
		}

		public enum Action
		{
			None,
			Exit
		};

	}

}
