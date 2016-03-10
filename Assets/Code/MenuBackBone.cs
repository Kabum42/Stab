using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuBackBone : MonoBehaviour {

	public GameObject neuronSource;
	public GameObject synapsisSource;
	public GameObject optionSource;
	public Color optionDeselectedColor;
	public Color optionSelectedColor;

	public MenuNeuron currentMenuNeuron;
	public Stack<MenuNeuron> listPathNeurons = new Stack<MenuNeuron> ();

	public List<MenuNeuron> listMenuNeurons = new List<MenuNeuron>();
	public List<MenuSynapsis> listMenuSynapsis = new List<MenuSynapsis>();

	private bool lastActionAddition = false;

	// Use this for initialization
	void Start () {

		CreateBaseNeuron ();

	}

	private void CreateBaseNeuron() {

		MenuNeuron baseNeuron = new MenuNeuron(this);
		baseNeuron.root.transform.localPosition = new Vector3 (-8f, -7f, -6f);

		MenuSynapsis baseSynapsis = new MenuSynapsis (this);
		baseSynapsis.start = baseNeuron;

		CreateFirstNeuron (baseSynapsis);

	}

	private void CreateFirstNeuron(MenuSynapsis parentSynapsis) {

		MenuNeuron firstNeuron = new MenuNeuron(this);
		parentSynapsis.end = firstNeuron;
		firstNeuron.parentSynapsis = parentSynapsis;
		firstNeuron.root.transform.position = parentSynapsis.start.root.transform.position + new Vector3 (0f, 3.5f, 7f);

		currentMenuNeuron = firstNeuron;
		listPathNeurons.Push (firstNeuron);

		MenuNeuron stabNeuron = CreateStabNeuron (firstNeuron);
		firstNeuron.AddOption ("Stab.exe", "neuron", stabNeuron);

		MenuNeuron optionsNeuron = CreateOptionsNeuron (firstNeuron);
		firstNeuron.AddOption ("Options", "neuron", optionsNeuron);

		firstNeuron.AddOption ("Shut down system", "exit", null);

		/*
		for (int i = 0; i < 10; i++) {
			auxNeuronOption = new MenuNeuronOption(auxNeuron);
			auxNeuronOption.text.GetComponent<TextMesh> ().text = "JEJE_"+i;
			auxNeuron.options.Add (auxNeuronOption);
		}
        */

	}

	private MenuNeuron CreateStabNeuron(MenuNeuron parentNeuron) {

		MenuNeuron auxNeuron = GenerateMenuNeuron (parentNeuron);

		auxNeuron.AddOption ("Join match", "none", null);
		auxNeuron.AddOption ("Create match", "none", null);

		return auxNeuron;

	}

	private MenuNeuron CreateOptionsNeuron(MenuNeuron parentNeuron) {

		MenuNeuron auxNeuron = GenerateMenuNeuron (parentNeuron);

		for (int i = 0; i < 10; i++) {
			auxNeuron.AddOption ("JEJE_"+i, "none", null);
		}

		return auxNeuron;

	}

	private MenuNeuron GenerateMenuNeuron(MenuNeuron parentNeuron) {

		MenuSynapsis parentSynapsis = new MenuSynapsis (this);
		parentSynapsis.start = parentNeuron;

		MenuNeuron newNeuron = new MenuNeuron(this);
		parentSynapsis.end = newNeuron;
		newNeuron.parentSynapsis = parentSynapsis;

		newNeuron.root.SetActive (false);
		parentSynapsis.root.SetActive (false);

		return newNeuron;

	}
	
	// Update is called once per frame
	void Update () {

		ProcessInput ();

		for (int i = 0; i < listMenuNeurons.Count; i++) {
			listMenuNeurons [i].Update ();
		}

		for (int i = 0; i < listMenuSynapsis.Count; i++) {
			listMenuSynapsis [i].Update ();
		}

		Vector3 targetPosition = -currentMenuNeuron.root.transform.localPosition + new Vector3(-8f, -3.5f, 1f);
		float speedTransition;
		if (lastActionAddition) { speedTransition = 4f; }
		else { speedTransition = 10f; }

		this.transform.position = Vector3.Lerp (this.transform.position, targetPosition, Time.deltaTime * speedTransition);
	
	}

	private void ProcessInput() {

		if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
		{
			
			currentMenuNeuron.optionSelected--;
			if (currentMenuNeuron.optionSelected < 0) { currentMenuNeuron.optionSelected = currentMenuNeuron.options.Count -1; }

		} else if (Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.DownArrow)) {
			
			currentMenuNeuron.optionSelected++;
			if (currentMenuNeuron.optionSelected > (currentMenuNeuron.options.Count -1)) { currentMenuNeuron.optionSelected = 0; }

		}

		if (Input.GetKeyDown (KeyCode.Return)) {

			string action = currentMenuNeuron.options [currentMenuNeuron.optionSelected].action;

			if (action == "exit") {
				
				Application.Quit ();

			} else if (action == "play") {

				Application.LoadLevel ("TestChamber");

			} else if (action == "neuron") {

				MenuNeuron nextNeuron = currentMenuNeuron.options [currentMenuNeuron.optionSelected].connectedNeuron;
				Vector3 direction = new Vector3 (Random.Range (0.00000001f, 1f), 0f, Random.Range (0.00000001f, 1f));
				direction.Normalize ();
				nextNeuron.root.transform.position = currentMenuNeuron.root.transform.position + direction * 7f + new Vector3 (0f, 3.5f, 0f);

				currentMenuNeuron = nextNeuron;
				listPathNeurons.Push (currentMenuNeuron);
				currentMenuNeuron.parentSynapsis.root.SetActive (true);
				currentMenuNeuron.root.SetActive (true);

				lastActionAddition = true;

			}

		} else if (Input.GetKeyDown (KeyCode.Escape)) {
			
			if (listPathNeurons.Count > 1) {

				currentMenuNeuron.root.SetActive (false);
				currentMenuNeuron.parentSynapsis.root.SetActive (false);
				listPathNeurons.Pop ();

				currentMenuNeuron = listPathNeurons.Peek ();

				lastActionAddition = false;

			}

		}

	}

	public class MenuNeuron {

		public MenuBackBone parent;
		public GameObject root;
		public List<MenuNeuronOption> options = new List<MenuNeuronOption> ();
		public int optionSelected = 0;
        private Vector3 randomRotation;
		public MenuSynapsis parentSynapsis;

		public MenuNeuron(MenuBackBone mBB) {
			parent = mBB;
			root = GameObject.Instantiate(parent.neuronSource);
			root.SetActive(true);
			root.transform.SetParent(parent.gameObject.transform);
            randomRotation = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            randomRotation.Normalize();
			parent.listMenuNeurons.Add(this);
		}

		public void AddOption(string textToShow, string action, MenuNeuron connectedNeuron) {

			MenuNeuronOption auxNeuronOption = new MenuNeuronOption(this);
			auxNeuronOption.text.GetComponent<TextMesh> ().text = textToShow;
			auxNeuronOption.action = action;
			if (connectedNeuron != null) { auxNeuronOption.connectedNeuron = connectedNeuron; }

			options.Add (auxNeuronOption);

		}

		public void Update() {

            root.transform.Rotate(randomRotation * Time.deltaTime * 180f);

			if (parent.currentMenuNeuron != this) {

				for (int i = 0; i < options.Count; i++) {
					options [i].root.SetActive (false);
				}

			} else {

				for (int i = 0; i < options.Count; i++) {

					options [i].root.SetActive (true);

					float distance = 0.75f;
					Color targetColor;

					if (i == optionSelected) {
						targetColor = parent.optionSelectedColor;
						options [i].text.GetComponent<TextMesh> ().color = parent.optionSelectedColor;
						float auxScale = Mathf.Lerp (options[i].text.transform.localScale.x, options [i].textOriginalScale.x * 2.5f, Time.deltaTime * 10f);
						options [i].text.transform.localScale = new Vector3 (auxScale, auxScale, auxScale);
						distance = 1.25f;

					} else {
						targetColor = parent.optionDeselectedColor;
						float auxScale = Mathf.Lerp (options[i].text.transform.localScale.x, options [i].textOriginalScale.x, Time.deltaTime * 10f);
						options [i].text.transform.localScale = new Vector3 (auxScale, auxScale, auxScale);
					}

					options [i].text.GetComponent<TextMesh> ().color = Color.Lerp (options [i].text.GetComponent<TextMesh> ().color, targetColor, Time.deltaTime * 10f);

					options[i].root.transform.position = root.transform.position;
					float auxRotation = Mathf.LerpAngle (options [i].root.transform.localEulerAngles.z, (360f / options.Count) * (optionSelected -i), Time.deltaTime * 20f);
					options [i].root.transform.localEulerAngles = new Vector3 (0f, 0f, auxRotation);
					options [i].root.transform.position = options[i].root.transform.position + options[i].root.transform.right * distance;

				}

			}

		}

	}

	public class MenuNeuronOption {

		public MenuNeuron parent;
		public GameObject root;
		public GameObject text;
		public Vector3 textOriginalScale;
		public string action = "none";
		public MenuNeuron connectedNeuron;

		public MenuNeuronOption(MenuNeuron mN) {
			parent = mN;
			root = GameObject.Instantiate(parent.root.transform.parent.GetComponent<MenuBackBone>().optionSource);
			root.SetActive(true);
			root.transform.SetParent(parent.parent.transform);
			text = root.transform.FindChild("Text").gameObject;
			textOriginalScale = text.transform.localScale;
		}

	}

	public class MenuSynapsis {

		public GameObject root;
		public GameObject cylinder;
		public MenuNeuron start;
		public MenuNeuron end;

		public MenuSynapsis(MenuBackBone mBB) {
			root = GameObject.Instantiate(mBB.synapsisSource);
			root.SetActive(true);
			root.transform.SetParent(mBB.gameObject.transform);
			cylinder = root.transform.FindChild("Cylinder").gameObject;
			mBB.listMenuSynapsis.Add(this);
		}

		public void Update()
		{
			root.transform.position = (start.root.transform.position + end.root.transform.position)/2f;
			root.transform.LookAt(end.root.transform);
			float distance = Vector3.Distance(start.root.transform.position, end.root.transform.position);
			cylinder.transform.localScale = new Vector3(cylinder.transform.localScale.x, distance*0.5f, cylinder.transform.localScale.z);
		}

	}

}
