using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameScript : MonoBehaviour {

	private ChatManager chatManager = new ChatManager();

	// Use this for initialization
	void Start () {

		// AQUI HAY QUE PASARLE AL CHATMANAGER EL GAMEOBJECT QUE ES EL CONTENT DONDE IRAN LOS MENSAJES

		int rand = Random.Range (20, 90);
		for (int i = 0; i < rand; i++) {
			chatManager.Add(new ChatMessage("kabum42", "hey nigga "+i));
		}
	
	}
	
	// Update is called once per frame
	void Update () {


	
	}
}


