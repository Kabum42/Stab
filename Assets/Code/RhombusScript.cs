using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RhombusScript : MonoBehaviour {

    public GameObject neuronSource;
    public GameObject synapsisSource;
	public GameObject textSource;
	public GameObject textBackgroundSource;

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

	public int menuSelected = 0;
	private Neuron menuSelectedNeuron;

	// CREATE MENU
	private GameObject createMenu;
	private List<GameObject> createMenuOptions = new List<GameObject> ();
	// MATCH NAME
	private GameObject createMenuNameTitle;
	private GameObject createMenuNameText;
	private GameObject createMenuNameTextBackground;
	// MAP
	private GameObject createMenuMapTitle;
	private GameObject createMenuMapText;
	private GameObject createMenuMapTextBackground;
	// PASSWORD
	private GameObject createMenuPasswordTitle;
	private GameObject createMenuPasswordText;
	private GameObject createMenuPasswordTextBackground;
	// CREATE
	private GameObject createMenuGoTitle;

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

		createMenu = new GameObject ();
		createMenu.name = "CreateMenu";
		createMenu.transform.SetParent (this.transform);
		createMenu.transform.localPosition = new Vector3 (0f, 0f, 0f);

		menuSelectedNeuron.root.transform.SetParent (createMenu.transform);
		menuSelectedNeuron.root.transform.localPosition = new Vector3(0f, 0f, 0f);

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
		createMenuNameText.GetComponent<TextMesh> ().text = "Test";
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

		createMenuMapText = Instantiate (textSource);
		createMenuMapText.GetComponent<TextMesh> ().anchor = TextAnchor.MiddleCenter;
		createMenuMapText.GetComponent<TextMesh> ().fontSize = 140;
		createMenuMapText.GetComponent<TextMesh> ().text = "Portal.zip";
		createMenuMapText.name = "MapText";
		createMenuMapText.transform.SetParent (createMenu.transform);
		createMenuMapText.transform.localPosition = new Vector3 (0f, 0.5f, -0.1f);

		createMenuMapTextBackground = Instantiate (textBackgroundSource);
		createMenuMapTextBackground.SetActive (true);
		createMenuMapTextBackground.name = "MapTextBackground";
		createMenuMapTextBackground.transform.SetParent (createMenuMapText.transform);
		createMenuMapTextBackground.transform.localPosition = new Vector3 (0f, 0f, 0.001f);
		createMenuMapTextBackground.transform.localScale = new Vector3 (25f, 1f, 2f);

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
		createMenuPasswordText.GetComponent<TextMesh> ().text = "...";
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
	
	}

	public void Collapse() {

		NW.root.transform.position = backboneLink.root.transform.position;
		NE.root.transform.position = backboneLink.root.transform.position;
		SW.root.transform.position = backboneLink.root.transform.position;
		SE.root.transform.position = backboneLink.root.transform.position;

		Update ();

	}
	
	// Update is called once per frame
	void Update () {

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

		createMenu.transform.position = (NW.root.transform.position + NE.root.transform.position + SW.root.transform.position + SE.root.transform.position) / 4f + new Vector3(0f, 0f, -1f);
		float targetXDistance_NW_NE = 18f;
		float currentXDistance_NW_NE = Mathf.Abs(NE.root.transform.position.x - NW.root.transform.position.x);
		float aux = currentXDistance_NW_NE / targetXDistance_NW_NE;
		createMenu.transform.localScale = new Vector3 (aux, aux, 1f);

		if (mode == "create") {

			if (Input.GetKeyDown (KeyCode.W) || Input.GetKeyDown (KeyCode.UpArrow)) {
				menuSelected--;
				if (menuSelected < 0) { menuSelected = createMenuOptions.Count - 1; }
			} else if (Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.DownArrow)) {
				menuSelected++;
				if (menuSelected > createMenuOptions.Count-1) { menuSelected = 0; }
			}

			if (Input.GetKeyDown (KeyCode.Return) && aux > 0f) {

				if (createMenuOptions[menuSelected] == createMenuGoTitle) {
					// Start match
					NetworkManager.StartServer("test");
					Application.LoadLevel("Game");
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

		} else if (mode == "join") {



		}

		createMenuNameText.GetComponent<TextMesh> ().color = createMenuNameTitle.GetComponent<TextMesh> ().color;
		createMenuMapText.GetComponent<TextMesh> ().color = createMenuMapTitle.GetComponent<TextMesh> ().color;
		createMenuPasswordText.GetComponent<TextMesh> ().color = createMenuPasswordTitle.GetComponent<TextMesh> ().color;

		menuSelectedNeuron.Update ();

		if (!active && aux <= 0.01f) {
			this.gameObject.SetActive (false);
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
}
