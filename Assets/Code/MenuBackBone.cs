using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuBackBone : MonoBehaviour {

	public GameObject neuronSource;
	public GameObject synapsisSource;
	public GameObject optionSource;
	public RhombusScript rhombus;
	public Color optionDeselectedColor;
	public Color optionSelectedColor;

	public MenuNeuron currentMenuNeuron;
	public Stack<MenuNeuron> listPathNeurons = new Stack<MenuNeuron> ();

	public List<MenuNeuron> listMenuNeurons = new List<MenuNeuron>();
	public List<MenuSynapsis> listMenuSynapsis = new List<MenuSynapsis>();

	private bool lastActionAddition = false;

    private PositronicBrain pB;

	public Vector3 neuronOffset = new Vector3(-8f, -3.5f, 1f);

	// Use this for initialization
	void Start () {

		GlobalData.Start ();

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		rhombus.gameObject.SetActive (false);
        pB = GameObject.Find("PositronicBrain").GetComponent<PositronicBrain>();

		CreateBaseNeuron ();

	}

	private void CreateBaseNeuron() {

		MenuNeuron baseNeuron = new MenuNeuron(this);
		baseNeuron.root.transform.localPosition = new Vector3 (-8f, -7f, -6f);
		baseNeuron.visible = true;

		MenuSynapsis baseSynapsis = new MenuSynapsis (this);
		baseSynapsis.start = baseNeuron;

		CreateFirstNeuron (baseSynapsis);

 }

	private void CreateFirstNeuron(MenuSynapsis parentSynapsis) {

		MenuNeuron firstNeuron = new MenuNeuron(this);
		parentSynapsis.end = firstNeuron;
		firstNeuron.parentSynapsis = parentSynapsis;
		firstNeuron.root.transform.position = parentSynapsis.start.root.transform.position + new Vector3 (0f, 3.5f, 7f);
		firstNeuron.visible = true;

		currentMenuNeuron = firstNeuron;
		listPathNeurons.Push (firstNeuron);

		MenuNeuron stabNeuron = CreateStabNeuron (firstNeuron);
		firstNeuron.AddOption ("Stab.exe", "neuron", stabNeuron);

		//MenuNeuron optionsNeuron = CreateOptionsNeuron (firstNeuron);
		//firstNeuron.AddOption ("Options", "neuron", optionsNeuron);

		MenuNeuron creditsNeuron = CreateCreditsNeuron (firstNeuron);
		firstNeuron.AddOption ("Developers", "neuron", creditsNeuron);

		firstNeuron.AddOption ("Shut down system", "exit", null);

	}

	private MenuNeuron CreateStabNeuron(MenuNeuron parentNeuron) {

		MenuNeuron auxNeuron = GenerateMenuNeuron (parentNeuron);

		auxNeuron.AddOption ("Join match", "join", null);
		auxNeuron.AddOption ("Create match", "create", null);

		return auxNeuron;

	}

	/*
	private MenuNeuron CreateOptionsNeuron(MenuNeuron parentNeuron) {

		MenuNeuron auxNeuron = GenerateMenuNeuron (parentNeuron);

		for (int i = 0; i < 10; i++) {
			auxNeuron.AddOption ("HEHE_"+i, "none", null);
		}

		return auxNeuron;

	}
	*/

	private MenuNeuron CreateCreditsNeuron(MenuNeuron parentNeuron) {

		MenuNeuron auxNeuron = GenerateMenuNeuron (parentNeuron);

		auxNeuron.AddOption ("Programmer's <color=#4894e1ff>Twitter</color>  (ง■ ͟ʖ■)ง", "kabum42", null);
		auxNeuron.AddOption ("Artist's <color=#4894e1ff>Twitter</color>  ʕʘᴥʘʔ", "dunistrad", null);

		return auxNeuron;

	}

	private MenuNeuron GenerateMenuNeuron(MenuNeuron parentNeuron) {

		MenuSynapsis parentSynapsis = new MenuSynapsis (this);
		parentSynapsis.start = parentNeuron;

		MenuNeuron newNeuron = new MenuNeuron(this);
		parentSynapsis.end = newNeuron;
		newNeuron.parentSynapsis = parentSynapsis;

		newNeuron.root.GetComponent<MeshRenderer> ().material.color = new Color (1f, 1f, 1f, 0f);
		parentSynapsis.cylinder.GetComponent<MeshRenderer> ().material.color = new Color (1f, 1f, 1f, 0f);
		newNeuron.visible = false;

        newNeuron.AddOption("/..", "back", null);
        newNeuron.optionSelected = 1;

		return newNeuron;

	}

	void OnApplicationFocus(bool focusStatus) {
		if (focusStatus && !Application.isEditor) {
			StartCoroutine(Hacks.LockCursor(-1f));
		}
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

		Vector3 targetPosition = -currentMenuNeuron.root.transform.localPosition + neuronOffset;
		float speedTransition;
		if (lastActionAddition) { speedTransition = 4f; }
		else { speedTransition = 10f; }

		this.transform.position = Vector3.Lerp (this.transform.position, targetPosition, Time.deltaTime * speedTransition);
	
	}

	private void ProcessInput() {

		if (!rhombus.active) {

			if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
			{

				currentMenuNeuron.optionSelected--;
				if (currentMenuNeuron.optionSelected < 0) { currentMenuNeuron.optionSelected = currentMenuNeuron.options.Count -1; }
				currentMenuNeuron.clockwise = true;
				SoundMoveSelected ();

			} else if (Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {

				currentMenuNeuron.optionSelected++;
				if (currentMenuNeuron.optionSelected > (currentMenuNeuron.options.Count -1)) { currentMenuNeuron.optionSelected = 0; }
				currentMenuNeuron.clockwise = false;
				SoundMoveSelected ();

			}

		}



		if (Input.GetKeyDown (KeyCode.Return) && !rhombus.active) {

			string action = currentMenuNeuron.options [currentMenuNeuron.optionSelected].action;

			if (action == "exit") {
				
				Application.Quit ();
				SoundSelection ();

			} else if (action == "play") {

				Application.LoadLevel ("TestChamber2");

			} else if (action == "neuron") {

				MenuNeuron nextNeuron = currentMenuNeuron.options [currentMenuNeuron.optionSelected].connectedNeuron;
				Vector3 direction = new Vector3 (Random.Range (0f, 1f), 0f, Random.Range (0f, 1f));
				direction.Normalize ();
				nextNeuron.root.transform.position = currentMenuNeuron.root.transform.position + direction * 7f + new Vector3 (0f, Random.Range(2f, 4.5f), 0f);

				currentMenuNeuron = nextNeuron;
				listPathNeurons.Push (currentMenuNeuron);

                currentMenuNeuron.CorrectZ();
				currentMenuNeuron.visible = true;
				currentMenuNeuron.root.GetComponent<MeshRenderer> ().material.color = new Color (1f, 1f, 1f, 0f);
				currentMenuNeuron.parentSynapsis.cylinder.GetComponent<MeshRenderer> ().material.color = new Color (1f, 1f, 1f, 0f);

				lastActionAddition = true;

                pB.MenuForward();

				SoundSelection ();

            } else if (action == "back") {

                Back();
				SoundSelection ();

			} else if (action == "create") {
				
				neuronOffset = new Vector3(-8f, 5.5f, 1f);
				rhombus.backboneLink.root.transform.position = currentMenuNeuron.root.transform.position;
				rhombus.Collapse ();
				rhombus.setMode(action);
				rhombus.active = true;
				rhombus.gameObject.SetActive (true);
				SoundSelection ();

			} else if (action == "join") {

				NetworkManager.RefreshHostList ();

				neuronOffset = new Vector3(-8f, 5.5f, 1f);
				rhombus.backboneLink.root.transform.position = currentMenuNeuron.root.transform.position;
				rhombus.Collapse ();
				rhombus.setMode(action);
				rhombus.active = true;
				rhombus.gameObject.SetActive (true);

				SoundSelection ();

			} else if (action == "dunistrad") {

				Application.OpenURL ("https://twitter.com/duniStrad");
				SoundSelection ();

			} else if (action == "kabum42") {

				Application.OpenURL ("https://twitter.com/kabum42");
				SoundSelection ();

			}

		} else if (Input.GetKeyDown (KeyCode.Escape)) {

			if (!rhombus.active) {

				Back ();
				SoundSelection ();

			} else if (!rhombus.locked) {

				neuronOffset = new Vector3(-8f, -3.5f, 1f);
				rhombus.active = false;
				SoundSelection ();

			}

		}

	}

    private void Back()
    {
        if (listPathNeurons.Count > 1)
        {

            if (currentMenuNeuron.optionSelected == 0)
            {
                currentMenuNeuron.optionSelected = 1;
            }
            currentMenuNeuron.visible = false;
            listPathNeurons.Pop();
            currentMenuNeuron = listPathNeurons.Peek();
            lastActionAddition = false;

            pB.MenuBack();

        }
    }

	public static void SoundSelection() {

		AudioSource audio = Hacks.GetAudioSource ("Sound/Effects/tap-mellow");
		audio.volume = 1f;
		audio.pitch = UnityEngine.Random.Range (0.85f, 1.15f);
		audio.Play ();

	}

	public static void SoundMoveSelected() {

		AudioSource audio = Hacks.GetAudioSource ("Sound/Effects/slide-network");
		audio.volume = 1f;
		audio.pitch = UnityEngine.Random.Range (0.85f, 1.15f);
		audio.Play ();

	}

	public class MenuNeuron {

		public MenuBackBone parent;
		public GameObject root;
		public List<MenuNeuronOption> options = new List<MenuNeuronOption> ();
		public int optionSelected = 0;
        private Vector3 randomRotation;
		public MenuSynapsis parentSynapsis;
		private float auxZ = 0f;
		public bool clockwise = true;
		public bool visible = false;

		public MenuNeuron(MenuBackBone mBB) {
			parent = mBB;
			root = GameObject.Instantiate(parent.neuronSource);
			root.SetActive(true);
			root.transform.SetParent(parent.gameObject.transform);
            randomRotation = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            randomRotation.Normalize();
			parent.listMenuNeurons.Add(this);
		}

        public void CorrectZ()
        {
            float targetZ = (360f / options.Count) * (optionSelected);
            auxZ = targetZ;
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

			float speedVisibility = 7.5f;
			Color targetVisibilityColor = new Color (1f, 1f, 1f, 0f);
			if (visible) { 
				targetVisibilityColor = new Color (1f, 1f, 1f, 1f);
				speedVisibility = 5f;
			}

			root.GetComponent<MeshRenderer> ().material.color = Color.Lerp (root.GetComponent<MeshRenderer> ().material.color, targetVisibilityColor, Time.deltaTime * speedVisibility);
			if (parentSynapsis != null) {
				parentSynapsis.cylinder.GetComponent<MeshRenderer> ().material.color = Color.Lerp (parentSynapsis.cylinder.GetComponent<MeshRenderer> ().material.color, targetVisibilityColor, Time.deltaTime * speedVisibility);
			}

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

					float targetZ = (360f / options.Count) * (optionSelected);

					if (clockwise && targetZ > auxZ) {

						auxZ += 360f;

					} else if (!clockwise && targetZ < auxZ) {

						auxZ -= 360f;

					}

					auxZ = Mathf.Lerp(auxZ, targetZ, Time.deltaTime * 5f);
					if (auxZ != targetZ && Mathf.Abs(auxZ - targetZ) < 0.1f) { auxZ = targetZ; } 

					options [i].root.transform.localEulerAngles = new Vector3 (0f, 0f, auxZ - (360f / options.Count)*i );
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
