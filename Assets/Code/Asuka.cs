using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Asuka : MonoBehaviour {

	[HideInInspector] public AudioSource audioSource;
	[HideInInspector] public Text textComponent;
	[HideInInspector] public Thought currentThought = null;

	[HideInInspector] public Material eyeMaterial;

	public int currentLetter = 0;
	public string targetText = "";
	public float showingThought = 0f;

	// Use this for initialization
	void Start () {

		audioSource = this.gameObject.AddComponent<AudioSource> ();

		textComponent = (Instantiate(Resources.Load ("Prefabs/AsukaText") as GameObject)).GetComponent<Text> ();
		textComponent.gameObject.transform.SetParent (GameObject.Find ("Canvas").transform);
		textComponent.gameObject.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f, 70f);
		textComponent.gameObject.GetComponent<RectTransform> ().localPosition = new Vector3 (textComponent.gameObject.GetComponent<RectTransform> ().localPosition.x, textComponent.gameObject.GetComponent<RectTransform> ().localPosition.y, 0f);
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
			float median_volume = Hacks.GetMedianVolume (audioSource) * 500f;
			median_volume = Mathf.Min (0.3f, median_volume);
			targetEyeEmissionColor *= median_volume;
		}

		Color finalColor = Color.Lerp (eyeMaterial.GetColor ("_EmissionColor"), targetEyeEmissionColor, Time.deltaTime * 10f);
		eyeMaterial.SetColor ("_EmissionColor", targetEyeEmissionColor);


		if (audioSource.isPlaying) {
			
			float relativeAudioTime = audioSource.time / audioSource.clip.length;
			currentLetter = (int) Mathf.Min(targetText.Length, relativeAudioTime * targetText.Length);
			string textToShow = targetText.Substring (0, currentLetter);

			if (currentLetter != targetText.Length) {
				Color auxColor = Hacks.ColorLerpAlpha (textComponent.color, 0.5f, 1f);
				textToShow += "<color=#" + Hacks.ColorToHexString(auxColor) + ">" + getRandomChar () + "</color>";
			}

			textComponent.text = textToShow;

		} else if (currentThought != null && !audioSource.isPlaying) {
			
			showingThought = 0.5f;
			textComponent.text = targetText;
			currentThought = null;

		}  else if (currentThought == null && showingThought > 0f) {
			
			showingThought = Mathf.Max (0f, showingThought - Time.deltaTime);
			if (showingThought == 0f) {
				textComponent.text = "";
			}

		} 
	
	}

	string getRandomChar() {
		char c = (char) Random.Range(33, 1300);
		return c.ToString ();
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
			asuka.currentLetter = 0;
			asuka.targetText = this.text;

			asuka.textComponent.gameObject.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (asuka.textComponent.gameObject.GetComponent<RectTransform> ().sizeDelta.x/2f * asuka.textComponent.gameObject.GetComponent<RectTransform> ().localScale.x, asuka.textComponent.gameObject.GetComponent<RectTransform> ().anchoredPosition.y);
			asuka.textComponent.text = asuka.targetText;
			asuka.textComponent.gameObject.GetComponent<RectTransform> ().anchoredPosition = asuka.textComponent.gameObject.GetComponent<RectTransform> ().anchoredPosition + new Vector2 (-asuka.textComponent.preferredWidth/2f * asuka.textComponent.gameObject.GetComponent<RectTransform> ().localScale.x, 0f);
			asuka.textComponent.text = "";

			asuka.audioSource.clip = Resources.Load ("Sound/Asuka/" + audioPath) as AudioClip;
			asuka.audioSource.Play ();

		}

	}

}
