using UnityEngine;
using System.Collections;

public class MapScript : MonoBehaviour {

	// Use this for initialization
	void Start () {

		if (GlobalData.loadedMenuScene) {
			// THIS IS A GAME
			this.gameObject.AddComponent<NetworkView>().observed = null;
			this.gameObject.AddComponent<ClientScript>();
		} else {
			// THIS IS A TEST
			GameObject localPlayer = Instantiate (Resources.Load("Prefabs/LocalPlayer") as GameObject);
			localPlayer.name = "LocalPlayer";
		}
	
	}

}
