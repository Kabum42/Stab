using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RhombusScript : MonoBehaviour {

    public GameObject neuronSource;
    public GameObject synapsisSource;
	public GameObject textSource;
	public GameObject textBackgroundSource;
	public GameObject arrowSource;

    public GameObject boneNW;
    public GameObject boneNE;
    public GameObject boneSW;
    public GameObject boneSE;
    
	public Neuron backboneLink;

	public bool active = false;
	public string mode = "create";

    private Neuron NW;
    private Neuron NE;
    private Neuron SW;
    private Neuron SE;

    private List<Synapsis> synapsisList = new List<Synapsis>();
	private float expandedAmount = 0f;

	public bool locked = false;
	public int menuSelected = 0;
	private Neuron menuSelectedNeuron;

	private GameObject typeText;
	private float typeAux = 0f;
	private static float typeBlank = 0.5f;
	private static float typeShow = 0.5f;

	// CREATE MENU
	private GameObject createMenu;
	private List<GameObject> createMenuOptions = new List<GameObject> ();
	// MATCH NAME
	private GameObject createMenuNameTitle;
	private GameObject createMenuNameText;
	private string createMenuNameTextString = "";
	private GameObject createMenuNameTextBackground;
	// MAP
	private GameObject createMenuMapTitle;
	private GameObject createMenuMapText;
	private List<string> createMenuMapList = new List<string> ();
	private int createMenuMapListCurrent = 0;
	private GameObject createMenuMapTextBackground;
	private GameObject createMenuMapArrowLeft;
	private GameObject createMenuMapArrowRight;
	// PASSWORD
	private GameObject createMenuPasswordTitle;
	private GameObject createMenuPasswordText;
	private string createMenuPasswordTextString = "";
	private GameObject createMenuPasswordTextBackground;
	// CREATE
	private GameObject createMenuGoTitle;


	// JOIN MENU
	private GameObject joinMenu;
	private List<Match> currentMatches = new List<Match> ();
	private List<Match> toRecycleMatches = new List<Match> ();
	private int selectedMatch = -1;
	private List<PingMatch> unresolvedPingMatches = new List<PingMatch>();
	//private List<>

	// Use this for initialization
	void Start () {

		backboneLink = new Neuron(this);
		backboneLink.root.name = "Neuron_backboneLink";
		backboneLink.root.transform.localPosition = new Vector3(3f, 3f, 0f);
		backboneLink.SetOriginal();
		backboneLink.root.SetActive (false);

        NW = new Neuron(this);
        NW.root.name = "Neuron_NW";
        NW.root.transform.localPosition = new Vector3(2f, 2f, 0f);
        NW.SetOriginal();

        NE = new Neuron(this);
        NE.root.name = "Neuron_NE";
        NE.root.transform.localPosition = new Vector3(-2f, 2f, 0f);
        NE.SetOriginal();

        SW = new Neuron(this);
        SW.root.name = "Neuron_SW";
        SW.root.transform.localPosition = new Vector3(2f, -2f, 0f);
        SW.SetOriginal();

        SE = new Neuron(this);
        SE.root.name = "Neuron_SE";
        SE.root.transform.localPosition = new Vector3(-2f, -2f, 0f);
        SE.SetOriginal();

		Synapsis sLink = new Synapsis(this);
		sLink.start = backboneLink;
		sLink.end = NW;
		synapsisList.Add(sLink);

        Synapsis s1 = new Synapsis(this);
        s1.start = NW;
        s1.end = NE;
        synapsisList.Add(s1);

        Synapsis s2 = new Synapsis(this);
        s2.start = NE;
        s2.end = SE;
        synapsisList.Add(s2);

        Synapsis s3 = new Synapsis(this);
        s3.start = SE;
        s3.end = SW;
        synapsisList.Add(s3);

        Synapsis s4 = new Synapsis(this);
        s4.start = SW;
        s4.end = NW;
        synapsisList.Add(s4);

		menuSelectedNeuron = new Neuron(this);
		menuSelectedNeuron.root.name = "Neuron_MenuSelected";

		// CREATE MENU
		createMenu = new GameObject ();
		createMenu.name = "CreateMenu";
		createMenu.transform.SetParent (this.transform);
		createMenu.transform.localPosition = new Vector3 (0f, 0f, 0f);

		menuSelectedNeuron.root.transform.SetParent (createMenu.transform);
		menuSelectedNeuron.root.transform.localPosition = new Vector3(0f, 0f, 0f);

		typeText = Instantiate (textSource);
		typeText.GetComponent<TextMesh> ().anchor = TextAnchor.MiddleCenter;
		typeText.GetComponent<TextMesh> ().fontSize = 140;
		typeText.GetComponent<TextMesh> ().text = "|";
		typeText.name = "TypeText";
		typeText.transform.SetParent (createMenu.transform);
		typeText.transform.localPosition = new Vector3 (0f, 3f, -0.1f);
		typeText.GetComponent<TextMesh> ().color = this.transform.parent.GetComponent<MenuBackBone> ().optionSelectedColor;

		// MATCH NAME
		createMenuNameTitle = Instantiate (textSource);
		createMenuNameTitle.GetComponent<TextMesh> ().anchor = TextAnchor.MiddleCenter;
		createMenuNameTitle.GetComponent<TextMesh> ().fontSize = 140;
		createMenuNameTitle.GetComponent<TextMesh> ().text = "Name";
		createMenuNameTitle.name = "NameTitle";
		createMenuNameTitle.transform.SetParent (createMenu.transform);
		createMenuNameTitle.transform.localPosition = new Vector3 (0f, 4f, -0.1f);
		createMenuOptions.Add (createMenuNameTitle);

		createMenuNameText = Instantiate (textSource);
		createMenuNameText.GetComponent<TextMesh> ().anchor = TextAnchor.MiddleCenter;
		createMenuNameText.GetComponent<TextMesh> ().fontSize = 140;
		createMenuNameText.GetComponent<TextMesh> ().text = createMenuNameTextString;
		createMenuNameText.name = "NameText";
		createMenuNameText.transform.SetParent (createMenu.transform);
		createMenuNameText.transform.localPosition = new Vector3 (0f, 3f, -0.1f);

		createMenuNameTextBackground = Instantiate (textBackgroundSource);
		createMenuNameTextBackground.SetActive (true);
		createMenuNameTextBackground.name = "NameTextBackground";
		createMenuNameTextBackground.transform.SetParent (createMenuNameText.transform);
		createMenuNameTextBackground.transform.localPosition = new Vector3 (0f, 0f, 0.001f);
		createMenuNameTextBackground.transform.localScale = new Vector3 (25f, 1f, 2f);

		// MAP
		createMenuMapTitle = Instantiate (textSource);
		createMenuMapTitle.GetComponent<TextMesh> ().anchor = TextAnchor.MiddleCenter;
		createMenuMapTitle.GetComponent<TextMesh> ().fontSize = 140;
		createMenuMapTitle.GetComponent<TextMesh> ().text = "Map";
		createMenuMapTitle.name = "MapTitle";
		createMenuMapTitle.transform.SetParent (createMenu.transform);
		createMenuMapTitle.transform.localPosition = new Vector3 (0f, 1.5f, -0.1f);
		createMenuOptions.Add (createMenuMapTitle);

		createMenuMapList.Add ("Placeholder_01");
		createMenuMapList.Add ("Placeholder_02");
		createMenuMapList.Add ("Placeholder_03");
		createMenuMapList.Add ("Placeholder_04");

		createMenuMapText = Instantiate (textSource);
		createMenuMapText.GetComponent<TextMesh> ().anchor = TextAnchor.MiddleCenter;
		createMenuMapText.GetComponent<TextMesh> ().fontSize = 140;
		createMenuMapText.GetComponent<TextMesh> ().text = createMenuMapList[createMenuMapListCurrent];
		createMenuMapText.name = "MapText";
		createMenuMapText.transform.SetParent (createMenu.transform);
		createMenuMapText.transform.localPosition = new Vector3 (0f, 0.5f, -0.1f);

		createMenuMapTextBackground = Instantiate (textBackgroundSource);
		createMenuMapTextBackground.SetActive (true);
		createMenuMapTextBackground.name = "MapTextBackground";
		createMenuMapTextBackground.transform.SetParent (createMenuMapText.transform);
		createMenuMapTextBackground.transform.localPosition = new Vector3 (0f, 0f, 0.001f);
		createMenuMapTextBackground.transform.localScale = new Vector3 (25f, 1f, 2f);

		createMenuMapArrowLeft = Instantiate (arrowSource);
		createMenuMapArrowLeft.SetActive (true);
		createMenuMapArrowLeft.name = "MapArrowLeft";
		createMenuMapArrowLeft.transform.SetParent (createMenu.transform);
		createMenuMapArrowLeft.transform.localScale = new Vector3 (-0.05f, 1f, createMenuMapText.transform.localScale.z * createMenuMapTextBackground.transform.localScale.z * 0.75f);
		createMenuMapArrowLeft.transform.localPosition = new Vector3 (-createMenuMapTextBackground.GetComponent<MeshRenderer>().bounds.size.x/2f -createMenuMapArrowLeft.GetComponent<MeshRenderer>().bounds.size.x/2f -0.2f, createMenuMapText.transform.localPosition.y, -0.1f);
		createMenuMapArrowLeft.GetComponent<MeshRenderer> ().material.color = new Color (1f, 1f, 1f, 0f);

		createMenuMapArrowRight = Instantiate (arrowSource);
		createMenuMapArrowRight.SetActive (true);
		createMenuMapArrowRight.name = "MapArrowRight";
		createMenuMapArrowRight.transform.SetParent (createMenu.transform);
		createMenuMapArrowRight.transform.localScale = new Vector3 (+0.05f, 1f, createMenuMapText.transform.localScale.z * createMenuMapTextBackground.transform.localScale.z * 0.75f);
		createMenuMapArrowRight.transform.localPosition = new Vector3 (+createMenuMapTextBackground.GetComponent<MeshRenderer>().bounds.size.x/2f +createMenuMapArrowLeft.GetComponent<MeshRenderer>().bounds.size.x/2f +0.2f, createMenuMapText.transform.localPosition.y, -0.1f);
		createMenuMapArrowRight.GetComponent<MeshRenderer> ().material.color = new Color (1f, 1f, 1f, 0f);

		// PASSWORD
		createMenuPasswordTitle = Instantiate (textSource);
		createMenuPasswordTitle.GetComponent<TextMesh> ().anchor = TextAnchor.MiddleCenter;
		createMenuPasswordTitle.GetComponent<TextMesh> ().fontSize = 140;
		createMenuPasswordTitle.GetComponent<TextMesh> ().text = "Password [optional]";
		createMenuPasswordTitle.name = "PasswordTitle";
		createMenuPasswordTitle.transform.SetParent (createMenu.transform);
		createMenuPasswordTitle.transform.localPosition = new Vector3 (0f, -1.5f, -0.1f);
		createMenuOptions.Add (createMenuPasswordTitle);

		createMenuPasswordText = Instantiate (textSource);
		createMenuPasswordText.GetComponent<TextMesh> ().anchor = TextAnchor.MiddleCenter;
		createMenuPasswordText.GetComponent<TextMesh> ().fontSize = 140;
		createMenuPasswordText.GetComponent<TextMesh> ().text = createMenuPasswordTextString;
		createMenuPasswordText.name = "PasswordText";
		createMenuPasswordText.transform.SetParent (createMenu.transform);
		createMenuPasswordText.transform.localPosition = new Vector3 (0f, -2.5f, -0.1f);

		createMenuPasswordTextBackground = Instantiate (textBackgroundSource);
		createMenuPasswordTextBackground.SetActive (true);
		createMenuPasswordTextBackground.name = "PasswordTextBackground";
		createMenuPasswordTextBackground.transform.SetParent (createMenuPasswordText.transform);
		createMenuPasswordTextBackground.transform.localPosition = new Vector3 (0f, 0f, 0.001f);
		createMenuPasswordTextBackground.transform.localScale = new Vector3 (25f, 1f, 2f);

		// CREATE
		createMenuGoTitle = Instantiate (textSource);
		createMenuGoTitle.GetComponent<TextMesh> ().anchor = TextAnchor.MiddleCenter;
		createMenuGoTitle.GetComponent<TextMesh> ().fontSize = 140;
		createMenuGoTitle.GetComponent<TextMesh> ().text = "Create";
		createMenuGoTitle.name = "CreateTitle";
		createMenuGoTitle.transform.SetParent (createMenu.transform);
		createMenuGoTitle.transform.localPosition = new Vector3 (0f, -4f, -0.1f);
		createMenuOptions.Add (createMenuGoTitle);



		// JOIN MENU
		joinMenu = new GameObject ();
		joinMenu.name = "JoinMenu";
		joinMenu.transform.SetParent (this.transform);
		joinMenu.transform.localPosition = new Vector3 (0f, 0f, 0f);

		/*
		for (int i = 0; i < 9; i++) {
			GameObject g;
			g = Instantiate (textSource);
			g.GetComponent<TextMesh> ().anchor = TextAnchor.MiddleLeft;
			g.GetComponent<TextMesh> ().fontSize = 140;
			g.GetComponent<TextMesh> ().text = "Match_"+i;
			g.name = "Match_"+i;
			g.transform.SetParent (joinMenu.transform);
			g.transform.localPosition = new Vector3 (-5f, 2f - i*(0.75f), -0.1f);
		}
		*/
	
	}

	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.HostListReceived)
		{
			
			NetworkManager.hostList = MasterServer.PollHostList();

			NetworkManager.JoinServer (NetworkManager.hostList [0]);

			//flushMatches();
			//addMatches();

			Debug.Log ("HostListReceived");
		}
	}

	private void flushMatches() {

		selectedMatch = -1;

		// STORE OLD MATCHES TO RECYCLE
		while (currentMatches.Count > 0) {

			toRecycleMatches.Add (currentMatches [0]);
			currentMatches.RemoveAt(0);

		}

		// REMOVE ALL PINGMATCHES, THESE ARE NOT RELIABLE
		while (unresolvedPingMatches.Count > 0) {

			unresolvedPingMatches[0] = null;
			unresolvedPingMatches.RemoveAt(0);

		}

	}

	private void addMatches() {

		for (int i = 0; i < NetworkManager.hostList.Length; i++) {

			Debug.Log (i);
			Match m;

			if (toRecycleMatches.Count > 0) {
				m = toRecycleMatches [0];
				m.Recycle (ref NetworkManager.hostList [i], currentMatches.Count);
				toRecycleMatches.RemoveAt (0);
			} else {
				m = new Match (this, ref NetworkManager.hostList[i], currentMatches.Count);
			}
				
			currentMatches.Add (m);

		}

	}
	
	// Update is called once per frame
	void Update () {

		handleRepositions ();

		if (mode == "create") {

			updateCreate ();

		} else if (mode == "join") {

			updateJoin ();

		}

		menuSelectedNeuron.Update ();

		if (!active && expandedAmount <= 0.01f) {
			this.gameObject.SetActive (false);
		}

	}

	private void handleRepositions() {

		if (active) {
			Expand ();
		} else {
			Shrink ();
		}

		NW.Update();
		NE.Update();
		SW.Update();
		SE.Update();

		for (int i = 0; i < synapsisList.Count; i++)
		{
			synapsisList[i].Update();
		}

		boneNW.transform.position = NW.root.transform.position;
		boneNE.transform.position = NE.root.transform.position;
		boneSW.transform.position = SW.root.transform.position;
		boneSE.transform.position = SE.root.transform.position;

		Vector3 menuPositions = (NW.root.transform.position + NE.root.transform.position + SW.root.transform.position + SE.root.transform.position) / 4f + new Vector3 (0f, 0f, -1f);
		float targetXDistance_NW_NE = 18f;
		float currentXDistance_NW_NE = Mathf.Abs(NE.root.transform.position.x - NW.root.transform.position.x);
		expandedAmount = currentXDistance_NW_NE / targetXDistance_NW_NE;

		createMenu.transform.position = menuPositions;
		createMenu.transform.localScale = new Vector3 (expandedAmount, expandedAmount, 1f);
		joinMenu.transform.position = menuPositions;
		joinMenu.transform.localScale = new Vector3 (expandedAmount, expandedAmount, 1f);

	}

	public void Collapse() {

		NW.root.transform.position = backboneLink.root.transform.position;
		NE.root.transform.position = backboneLink.root.transform.position;
		SW.root.transform.position = backboneLink.root.transform.position;
		SE.root.transform.position = backboneLink.root.transform.position;

		Update ();

	}

	void updateJoin() {


		for (int i = 0; i < unresolvedPingMatches.Count; i++) {
			unresolvedPingMatches[i].Check();
		}

	}

	void updateCreate() {

		Color targetArrowColor = new Color (1f, 1f, 1f, 0f);

		if (locked) {

			if (createMenuOptions [menuSelected] == createMenuNameTitle) {

				handleInputString (ref createMenuNameTextString);
				createMenuNameText.GetComponent<TextMesh> ().text = createMenuNameTextString;
				handleTypeIntermitency (createMenuNameText);

			} else if (createMenuOptions [menuSelected] == createMenuMapTitle) {

				if (Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.LeftArrow)) {
					MenuBackBone.SoundMoveSelected ();
					createMenuMapListCurrent--;
					if (createMenuMapListCurrent < 0) { createMenuMapListCurrent = createMenuMapList.Count - 1; }
				} else if (Input.GetKeyDown (KeyCode.D) || Input.GetKeyDown (KeyCode.RightArrow)) {
					MenuBackBone.SoundMoveSelected ();
					createMenuMapListCurrent++;
					if (createMenuMapListCurrent > (createMenuMapList.Count - 1)) { createMenuMapListCurrent = 0; }
				}

				createMenuMapText.GetComponent<TextMesh> ().text = createMenuMapList [createMenuMapListCurrent];

				targetArrowColor = new Color (1f, 1f, 1f, 1f);

			} else if (createMenuOptions [menuSelected] == createMenuPasswordTitle) {

				handleInputString (ref createMenuPasswordTextString);
				createMenuPasswordText.GetComponent<TextMesh> ().text = createMenuPasswordTextString;
				handleTypeIntermitency (createMenuPasswordText);

			}

			createMenuNameText.GetComponent<TextMesh> ().color = Color.Lerp(createMenuNameText.GetComponent<TextMesh> ().color, createMenuNameTitle.GetComponent<TextMesh> ().color, Time.deltaTime*5f);
			createMenuMapText.GetComponent<TextMesh> ().color = Color.Lerp(createMenuMapText.GetComponent<TextMesh> ().color, createMenuMapTitle.GetComponent<TextMesh> ().color, Time.deltaTime*5f);
			createMenuPasswordText.GetComponent<TextMesh> ().color = Color.Lerp(createMenuPasswordText.GetComponent<TextMesh> ().color, createMenuPasswordTitle.GetComponent<TextMesh> ().color, Time.deltaTime*5f);

		} else {

			if (Input.GetKeyDown (KeyCode.W) || Input.GetKeyDown (KeyCode.UpArrow)) {
				MenuBackBone.SoundMoveSelected ();
				menuSelected--;
				if (menuSelected < 0) { menuSelected = createMenuOptions.Count - 1; }
			} else if (Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.DownArrow)) {
				MenuBackBone.SoundMoveSelected ();
				menuSelected++;
				if (menuSelected > createMenuOptions.Count-1) { menuSelected = 0; }
			}

			typeText.SetActive (false);

			createMenuNameText.GetComponent<TextMesh> ().color = Color.Lerp(createMenuNameText.GetComponent<TextMesh> ().color, this.transform.parent.GetComponent<MenuBackBone> ().optionDeselectedColor, Time.deltaTime*5f);
			createMenuMapText.GetComponent<TextMesh> ().color = Color.Lerp(createMenuMapText.GetComponent<TextMesh> ().color, this.transform.parent.GetComponent<MenuBackBone> ().optionDeselectedColor, Time.deltaTime*5f);
			createMenuPasswordText.GetComponent<TextMesh> ().color = Color.Lerp(createMenuPasswordText.GetComponent<TextMesh> ().color, this.transform.parent.GetComponent<MenuBackBone> ().optionDeselectedColor, Time.deltaTime*5f);

		}

		if (Input.GetKeyDown (KeyCode.Return) && expandedAmount > 0f) {

			MenuBackBone.SoundSelection ();

			if (locked) {
				locked = false;
			} else {
				if (createMenuOptions[menuSelected] == createMenuNameTitle) {
					locked = true;
				}
				else if (createMenuOptions[menuSelected] == createMenuMapTitle) {
					locked = true;
				}
				else if (createMenuOptions[menuSelected] == createMenuPasswordTitle) {
					locked = true;
				}
				else if (createMenuOptions[menuSelected] == createMenuGoTitle) {
					if (createMenuNameTextString.Length != 0) {
						// Start match
						NetworkManager.StartServer (createMenuNameTextString);
						Application.LoadLevel ("Game");
					} else {
						locked = true;
						menuSelected = 0;
					}
				}
			}

		}

		if (Input.GetKeyDown (KeyCode.Escape)) {

			if (locked) {
				MenuBackBone.SoundSelection ();
				locked = false;
			}

		}

		for (int i = 0; i < createMenuOptions.Count; i++) {

			Color targetColor = this.transform.parent.GetComponent<MenuBackBone> ().optionDeselectedColor;

			if (i == menuSelected) {
				targetColor = this.transform.parent.GetComponent<MenuBackBone> ().optionSelectedColor;
			}

			createMenuOptions [i].GetComponent<TextMesh> ().color = Color.Lerp (createMenuOptions [i].GetComponent<TextMesh> ().color, targetColor, Time.deltaTime * 5f);

		}

		Vector3 targetPosition = createMenuOptions [menuSelected].transform.position + new Vector3 (-createMenuOptions [menuSelected].GetComponent<MeshRenderer> ().bounds.size.x / 2f - 0.5f, 0f, 0f);
		menuSelectedNeuron.root.transform.position = Vector3.Lerp (menuSelectedNeuron.root.transform.position, targetPosition, Time.deltaTime * 15f);

		createMenuMapArrowLeft.GetComponent<MeshRenderer> ().material.color = Color.Lerp (createMenuMapArrowLeft.GetComponent<MeshRenderer> ().material.color, targetArrowColor, Time.deltaTime * 5f);
		createMenuMapArrowRight.GetComponent<MeshRenderer> ().material.color = Color.Lerp (createMenuMapArrowRight.GetComponent<MeshRenderer> ().material.color, targetArrowColor, Time.deltaTime * 5f);

	}

	public void setMode(string newMode) {

		createMenu.SetActive (false);
		joinMenu.SetActive (false);

		if (newMode == "create") { createMenu.SetActive (true); } 
		else if (newMode == "join") { joinMenu.SetActive (true); }

		mode = newMode;

	}

	private void handleInputString(ref string s) {

		foreach (char c in Input.inputString) {

			if (c == "\b" [0]) {
				if (s.Length != 0) {
					s = s.Substring (0, s.Length - 1);
				}	
			} else if (c == "\n" [0] || c == "\r" [0]) {
				// RETURN KEY
			} else {
				s += c;
			}

		}

	}

	private void handleTypeIntermitency(GameObject g) {

		typeAux += Time.deltaTime;
		if (typeAux > (typeBlank + typeShow)) {
			typeAux -= (typeBlank + typeShow);
		}

		if (typeAux > typeBlank) { 
			typeText.SetActive (true);
			typeText.transform.position = g.transform.position + new Vector3 (g.GetComponent<MeshRenderer> ().bounds.size.x / 2f + 0.05f, 0f, 0f);
		} else {
			typeText.SetActive (false);
		}

	}

	private void Expand() {

		NW.root.transform.position = Vector3.Lerp (NW.root.transform.position, backboneLink.root.transform.position + new Vector3 (-1f, -1f, -0.2f), Time.deltaTime * 5f);
		NE.root.transform.position = Vector3.Lerp (NE.root.transform.position, backboneLink.root.transform.position + new Vector3 (17f, -1f, -0.2f), Time.deltaTime * 5f);
		SW.root.transform.position = Vector3.Lerp (SW.root.transform.position, backboneLink.root.transform.position + new Vector3 (-1f, -11f, -0.2f), Time.deltaTime * 5f);
		SE.root.transform.position = Vector3.Lerp (SE.root.transform.position, backboneLink.root.transform.position + new Vector3 (17f, -11f, -0.2f), Time.deltaTime * 5f);

	}

	private void Shrink() {

		NW.root.transform.position = Vector3.Lerp (NW.root.transform.position, backboneLink.root.transform.position, Time.deltaTime * 8f);
		NE.root.transform.position = Vector3.Lerp (NE.root.transform.position, backboneLink.root.transform.position, Time.deltaTime * 8f);
		SW.root.transform.position = Vector3.Lerp (SW.root.transform.position, backboneLink.root.transform.position, Time.deltaTime * 8f);
		SE.root.transform.position = Vector3.Lerp (SE.root.transform.position, backboneLink.root.transform.position, Time.deltaTime * 8f);

	}

    public class Neuron
    {

        public GameObject root;
        private Vector3 originalPosition;
        private Vector3 newPosition = new Vector3(0f, 0f, 0f);
        private float cooldown = 0f;
        private float interpolationNumber = 0f;
        private Vector3 randomRotation;

        public Neuron(RhombusScript rS)
        {
            root = GameObject.Instantiate(rS.neuronSource);
            root.SetActive(true);
            root.transform.SetParent(rS.transform);
			root.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            randomRotation = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            randomRotation.Normalize();
        }

        public void SetOriginal()
        {
            originalPosition = root.transform.localPosition;
        }

        public void Update()
        {
            root.transform.Rotate(randomRotation * Time.deltaTime * 180f);
			/*
            cooldown -= Time.deltaTime;

            if (cooldown <= 0f)
            {
                cooldown = Random.Range(0.5f, 1f);
                Vector3 direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                direction.Normalize();
                float distance = Random.Range(0f, 0.5f);
                newPosition = root.transform.localPosition + direction * distance;
                if (Vector3.Distance(newPosition, originalPosition) > 1f)
                {
                    Vector3 direction2 = (newPosition - originalPosition).normalized;
                    newPosition = originalPosition + direction2 * 1f;
                }
                interpolationNumber = Mathf.Pow(Random.Range(1f, 2f), 4f);
            }

            root.transform.localPosition = Vector3.Lerp(root.transform.localPosition, newPosition, Time.deltaTime * interpolationNumber);
            */
        }

    }

    public class Synapsis
    {

        public GameObject root;
        public GameObject cylinder;
        public Neuron start;
        public Neuron end;

        public Synapsis(RhombusScript rS)
        {
            root = GameObject.Instantiate(rS.synapsisSource);
            root.SetActive(true);
            root.transform.SetParent(rS.transform);
            cylinder = root.transform.FindChild("Cylinder").gameObject;
			cylinder.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
        }

        public bool HasNeuron(Neuron n)
        {
            return (n == start || n == end);
        }

        public void Destroy()
        {
            GameObject.Destroy(root);
        }

        public void Update()
        {
            root.transform.position = (start.root.transform.position + end.root.transform.position) / 2f;
            root.transform.LookAt(end.root.transform);
            float distance = Vector3.Distance(start.root.transform.position, end.root.transform.position);
            cylinder.transform.localScale = new Vector3(cylinder.transform.localScale.x, distance * 0.5f, cylinder.transform.localScale.z);
        }

    }

	private class Match {

		public RhombusScript owner;
		public GameObject root;
		public string matchName;
		public int position;
		public int players;
		public int pingTime = -1;

		public Match(RhombusScript auxOwner, ref HostData hData, int auxPosition) {

			owner = auxOwner;
			root = Instantiate (owner.textSource);

			Recycle(ref hData, auxPosition);

		}

		public void Recycle(ref HostData hData, int auxPosition) {

			matchName = hData.gameName;
			players = hData.connectedPlayers;
			position = auxPosition;

			root.GetComponent<TextMesh> ().anchor = TextAnchor.MiddleLeft;
			root.GetComponent<TextMesh> ().fontSize = 140;
			root.GetComponent<TextMesh>().text = matchName + "  <color=#00ff00>" + players +"/20" + "</color>";
			root.name = "Match_"+auxPosition;
			root.gameObject.transform.SetParent(owner.joinMenu.transform);
			root.transform.localPosition = new Vector3 (-5f, 2f - auxPosition*(0.75f), -0.1f);

			owner.unresolvedPingMatches.Add(new PingMatch(this, hData.ip[0]));

		}

		public void UpdatePingTime(int auxPingTime) {
			
			pingTime = auxPingTime;
			root.GetComponent<TextMesh>().text = matchName + "  <color=#00ff00>" + players +"/20" + "</color>" + "  <color=#ff9999>"+pingTime+ "ms" + "</color>";

		}


	}

	private class PingMatch {

		public Match match;
		public Ping ping;

		public PingMatch(Match auxMatch, string auxIpAdress) {

			match = auxMatch;
			ping = new Ping(auxIpAdress);

		}

		public void Check() {

			if (ping.isDone) {
				match.UpdatePingTime (ping.time);
				match.owner.unresolvedPingMatches.Remove(this);
			}

		}

	}

}
