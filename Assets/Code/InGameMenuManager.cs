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

	// Use this for initialization
	public InGameMenuManager(GameObject auxPhysicalInGameMenu) {
		
		physicalInGameMenu = auxPhysicalInGameMenu;
		physicalOptionSource = physicalInGameMenu.transform.FindChild ("OptionSource").gameObject;
		physicalOptionSource.SetActive (false);
		semicircle = physicalInGameMenu.transform.FindChild ("Semicircle").gameObject;

		rootOption = new Option ("", Option.Action.None);
		currentParentOption = rootOption;
		rootOption.AddChild (new Option ("Video", Option.Action.None));
		rootOption.AddChild (new Option ("Audio", Option.Action.None));
		rootOption.AddChild (new Option ("Exit", Option.Action.Exit));

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
					currentParentOption = currentParentOption.parentOption;
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
				if (currentParentOption.children [currentParentOption.selectedChild].action == Option.Action.Exit) {
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

		foreach (Option option in currentParentOption.children) {

			if (option.assignedPhysicalOption == null) {
				option.assignedPhysicalOption = GetPhysicalOption ();
			}

			int index = currentParentOption.children.IndexOf (option);
			int relativePosition = index - currentParentOption.selectedChild;

			option.assignedPhysicalOption.GetComponent<Text> ().text = option.text;

			Vector3 targetScale = physicalOptionSource.transform.localScale;
			Color targetColor = new Color (1f, 1f, 1f, 1f);

			if (currentParentOption.selectedChild != index) {
				targetScale *= 0.5f;
				targetColor = new Color (0.75f, 0.75f, 0.75f, 1f);
			}

			option.assignedPhysicalOption.GetComponent<Text> ().color = Color.Lerp (option.assignedPhysicalOption.GetComponent<Text> ().color, targetColor, Time.deltaTime * 5f);
			option.assignedPhysicalOption.transform.localScale = Vector3.Lerp (option.assignedPhysicalOption.transform.localScale, targetScale, Time.deltaTime*10f);

			option.angle = Mathf.Lerp (option.angle, relativePosition * -7f, Time.deltaTime * 10f);

			option.assignedPhysicalOption.GetComponent<RectTransform> ().localEulerAngles = new Vector3 (0f, 0f, option.angle);

			option.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (semicircle.GetComponent<RectTransform>().anchoredPosition.x - semicircle.GetComponent<RectTransform>().sizeDelta.x/2f, 0f);
			Vector2 right = new Vector2 (option.assignedPhysicalOption.GetComponent<RectTransform> ().right.x, option.assignedPhysicalOption.GetComponent<RectTransform> ().right.y);
			option.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition = option.assignedPhysicalOption.GetComponent<RectTransform> ().anchoredPosition + right*(460f + option.assignedPhysicalOption.GetComponent<RectTransform> ().sizeDelta.x/2f * option.assignedPhysicalOption.transform.localScale.x);



		}

	
	}

	GameObject GetPhysicalOption() {
		
		GameObject newPhysicalOption = MonoBehaviour.Instantiate (physicalOptionSource);
		newPhysicalOption.transform.SetParent (physicalInGameMenu.transform);
		newPhysicalOption.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f, 0f);
		newPhysicalOption.transform.localScale = physicalOptionSource.transform.localScale;
		newPhysicalOption.SetActive (true);

		return newPhysicalOption;
	}

	public class Option {

		public Option parentOption;
		public string text = "";
		public Action action;
		public List<Option> children = new List<Option> ();
		public int selectedChild = 0;
		public GameObject assignedPhysicalOption = null;
		public float angle = 0f;

		public Option (string auxText, Action auxAction) {

			text = auxText;
			action = auxAction;

		}

		public void AddChild(Option o) {
			children.Add (o);
			o.parentOption = this;
		}

		public enum Action
		{
			None,
			Exit
		};

	}

}
