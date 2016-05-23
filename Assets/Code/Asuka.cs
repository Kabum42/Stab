using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Asuka : MonoBehaviour {

	[HideInInspector] public AudioSource audioSource;
	[HideInInspector] public Text textComponent;
	[HideInInspector] public Thought currentThought = null;

	[HideInInspector] public Material eyeMaterial;

	// Use this for initialization
	void Start () {

		audioSource = this.gameObject.AddComponent<AudioSource> ();

		textComponent = (Instantiate(Resources.Load ("Prefabs/AsukaText") as GameObject)).GetComponent<Text> ();
		textComponent.gameObject.transform.SetParent (GameObject.Find ("Canvas").transform);
		textComponent.gameObject.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f, 70f);
		textComponent.gameObject.transform.localScale = new Vector3 (0.3f, 0.3f, 0.3f);
		textComponent.text = "";

		eyeMaterial = this.transform.FindChild ("Asuka_Eye").gameObject.GetComponent<Renderer> ().material;
		eyeMaterial.EnableKeyword ("_EMISSION");

		Thought t = new Thought (this, "Welcome, user", "welcome");
		t.Play ();
	
	}
	
	// Update is called once per frame
	void Update () {

		Color targetEyeEmissionColor = new Color (0f, 0f, 0f);

		if (audioSource.isPlaying) {
			targetEyeEmissionColor = new Color (0f, 0.87f, 1f);
			float median_volume = Hacks.GetMedianVolume (audioSource) * 300f;
			median_volume = Mathf.Min (0.3f, median_volume);
			targetEyeEmissionColor *= median_volume;
		}

		Color finalColor = Color.Lerp (eyeMaterial.GetColor ("_EmissionColor"), targetEyeEmissionColor, Time.deltaTime * 10f);
		eyeMaterial.SetColor ("_EmissionColor", targetEyeEmissionColor);

		//eyeRenderer.material.color = targetEyeEmissionColor;

		if (currentThought != null && !audioSource.isPlaying) {
			currentThought = null;
			textComponent.text = "";
		}
	
	}

	public class Thought {

		private Asuka asuka;
		public string text;
		public string audioPath;

		public Thought(Asuka auxAsuka, string auxText, string auxAudioPath) {

			asuka = auxAsuka;
			text = auxText;
			audioPath = auxAudioPath;

		}

		public void Play() {

			asuka.currentThought = this;
			asuka.textComponent.text = this.text;
			asuka.audioSource.clip = Resources.Load ("Sound/Asuka/" + audioPath) as AudioClip;
			asuka.audioSource.Play ();

		}

	}

}
