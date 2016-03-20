using UnityEngine;
using System.Collections;

public class GameScript : MonoBehaviour {

	[HideInInspector]public ClientScript clientScript;
	[HideInInspector]public ServerScript serverScript;

	// Use this for initialization
	void Start () {
		
		GlobalData.Start ();

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		clientScript = gameObject.AddComponent<ClientScript> ();
		clientScript.gameScript = this;

		if (Network.isServer) { 
			serverScript = gameObject.AddComponent<ServerScript> ();
			serverScript.gameScript = this; 
		}

	}


	
	// Update is called once per frame
	void Update () {


	}

	void FixedUpdate() {


	}



	/*
    public void requestAttack(Vector3 lookingAt)
    {
        if (Network.isServer)
        {
            requestedAttack(Network.player.ToString(), lookingAt);
        }
        else
        {
            GetComponent<NetworkView>().RPC("requestedAttackRPC", RPCMode.Server, Network.player.ToString(), lookingAt);
        }
    }
	*/



}