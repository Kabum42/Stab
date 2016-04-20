using UnityEngine;
using System.Collections;

public class GameScript : MonoBehaviour {

	[HideInInspector]public ClientScript clientScript;
	[HideInInspector]public ServerScript serverScript;

	// Use this for initialization
	void Start () {

		clientScript = gameObject.AddComponent<ClientScript> ();
		clientScript.setGameScript (this);

		if (Network.isServer) { 
			serverScript = gameObject.AddComponent<ServerScript> ();
			serverScript.gameScript = this; 
		} else {
            Destroy(clientScript.map.transform.FindChild("RespawnPoints").gameObject);
		}

	}

}