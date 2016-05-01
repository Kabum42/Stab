using UnityEngine;
using System.Collections;

public class MapScript : MonoBehaviour {

	// Use this for initialization
	void Start () {

		if (GlobalData.loadedMenuScene) {
			// THIS IS A GAME
			this.gameObject.AddComponent<NetworkView>().observed = null;
			Instantiate (Resources.Load("Prefabs/Game/Canvas") as GameObject).name = "Canvas";
			Instantiate (Resources.Load("Prefabs/Game/EventSystem") as GameObject).name = "EventSystem";
			this.gameObject.AddComponent<ClientScript>();
		} else {
			// THIS IS A TEST
			Instantiate (Resources.Load("Prefabs/Test/Canvas") as GameObject).name = "Canvas";
			Instantiate (Resources.Load("Prefabs/Test/EventSystem") as GameObject).name = "EventSystem";
			Instantiate (Resources.Load("Prefabs/LocalPlayer") as GameObject).name = "LocalPlayer";
		}
	
	}

}
