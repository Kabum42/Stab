using UnityEngine;
using System.Collections;

public class ConnectionContainer : MonoBehaviour {

	[Range(0, 10)]
	public int pingQuality;

	public Gradient qualityColor;

	public GameObject connectionUnitSource;

	private Material connectionMaterial;
	private GameObject[] connectionUnits = new GameObject[10];

	// Use this for initialization
	void Awake () {

		connectionMaterial = connectionUnitSource.GetComponent<Renderer> ().material;

		connectionUnitSource.SetActive (true);

		setQuality (0);

	}

	public void setQuality(int newQuality) {

		pingQuality = Mathf.Clamp (newQuality, 0, 10);

		connectionMaterial.color = qualityColor.Evaluate (pingQuality / 10f);

		connectionUnitSource.transform.localScale = new Vector3 (pingQuality, 1f, 1f);
		connectionUnitSource.transform.localPosition = new Vector3 (0f, 0f, -0.0001f);

	}

}
