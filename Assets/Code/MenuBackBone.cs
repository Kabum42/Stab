using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuBackBone : MonoBehaviour {

	public GameObject neuronSource;
	public GameObject synapsisSource;
	public GameObject optionSource;
	public Color optionDeselectedColor;
	public Color optionSelectedColor;

	private MenuNeuron currentMenuNeuron;
	private List<MenuNeuron> listMenuNeurons = new List<MenuNeuron>();
	private List<MenuSynapsis> listMenuSynapsis = new List<MenuSynapsis>();

	// Use this for initialization
	void Start () {

		MenuNeuron auxNeuron = new MenuNeuron(this);
		auxNeuron.root.transform.localPosition = new Vector3 (-8f, -7f, -6f);
		listMenuNeurons.Add (auxNeuron);

		MenuSynapsis auxSynapsis = new MenuSynapsis (this);
		auxSynapsis.start = auxNeuron;
		listMenuSynapsis.Add (auxSynapsis);

		auxNeuron = new MenuNeuron(this);
		auxNeuron.root.transform.position = listMenuNeurons[0].root.transform.position + new Vector3 (0f, 3.5f, 7f);
		listMenuNeurons.Add (auxNeuron);

		MenuNeuronOption auxNeuronOption = new MenuNeuronOption(auxNeuron);
		auxNeuronOption.text.GetComponent<TextMesh> ().text = "Stab.exe";
		auxNeuron.options.Add (auxNeuronOption);

		auxNeuronOption = new MenuNeuronOption(auxNeuron);
		auxNeuronOption.text.GetComponent<TextMesh> ().text = "Options";
		auxNeuron.options.Add (auxNeuronOption);

		auxNeuronOption = new MenuNeuronOption(auxNeuron);
		auxNeuronOption.text.GetComponent<TextMesh> ().text = "Shut down system";
		auxNeuron.options.Add (auxNeuronOption);

		/*
		for (int i = 0; i < 10; i++) {
			auxNeuronOption = new MenuNeuronOption(auxNeuron);
			auxNeuronOption.text.GetComponent<TextMesh> ().text = "JEJE_"+i;
			auxNeuron.options.Add (auxNeuronOption);
		}
        */
		

		auxSynapsis.end = auxNeuron;
	
	}
	
	// Update is called once per frame
	void Update () {

		for (int i = 0; i < listMenuNeurons.Count; i++) {
			listMenuNeurons [i].Update ();
		}

		for (int i = 0; i < listMenuSynapsis.Count; i++) {
			listMenuSynapsis [i].Update ();
		}
	
	}

	public class MenuNeuron {

		public MenuBackBone parent;
		public GameObject root;
		public List<MenuNeuronOption> options = new List<MenuNeuronOption> ();
		public int optionSelected = 0;
        private Vector3 randomRotation;

		public MenuNeuron(MenuBackBone mBB) {
			parent = mBB;
			root = GameObject.Instantiate(parent.neuronSource);
			root.SetActive(true);
			root.transform.SetParent(parent.gameObject.transform);
            randomRotation = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            randomRotation.Normalize();
		}

		public void Update() {

			// ESTO ES PROVISIONAL, NO DEBERIA IR AQUI, SINO EN EL UPDATE DE BACKBONE
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {

				optionSelected--;
				if (optionSelected < 0) { optionSelected = options.Count -1; }

			} else if (Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.DownArrow)) {

				optionSelected++;
				if (optionSelected > (options.Count -1)) { optionSelected = 0; }

			}

            root.transform.Rotate(randomRotation * Time.deltaTime * 180f);

			for (int i = 0; i < options.Count; i++) {

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
				//options [i].root.transform.localEulerAngles = new Vector3 (0f, 0f, 0f);

				//options [i].root.transform.RotateAround (root.transform.position, options [i].root.transform.forward, (360f/options.Count) * (i-optionSelected));

			}

		}

	}

	public class MenuNeuronOption {

		public MenuNeuron parent;
		public GameObject root;
		public GameObject text;
		public Vector3 textOriginalScale;

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
