using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameScript : MonoBehaviour {

	private ChatManager chatManager;
	private GameObject chatInputField;
	private bool lastTimeChatInputFocused = false;

	// Use this for initialization
	void Start () {

		// AQUI HAY QUE PASARLE AL CHATMANAGER EL GAMEOBJECT QUE ES EL CONTENT DONDE IRAN LOS MENSAJES
		chatManager = new ChatManager(GameObject.Find ("Canvas/ChatPanel/ScrollRect/AllChat"));

		/*
		int rand = Random.Range (2, 3);
		for (int i = 0; i < rand; i++) {
			chatManager.Add(new ChatMessage("kabum42", "hey nigga "+i));
		}
		*/

		chatInputField = GameObject.Find ("Canvas/ChatPanel/InputField");
	
	}
	
	// Update is called once per frame
	void Update () {

		updateChat ();

	}

	[RPC]
	void addChatMessageRPC(string owner, string text)
	{
		if (owner == Network.player.externalIP) { owner = "You"; }

		addChatMessage (new ChatMessage (owner, text));
	}

	void updateChat() {

		if (Input.GetKeyDown(KeyCode.Return)
			&& lastTimeChatInputFocused
		    && chatInputField.GetComponent<InputField> ().text != "") {

			string info = chatInputField.GetComponent<InputField> ().text;
			//addChatMessage(new ChatMessage("kabum42", info));
			GetComponent<NetworkView>().RPC("addChatMessageRPC", RPCMode.All, Network.player.externalIP, info);

			chatInputField.GetComponent<InputField> ().text = "";
			EventSystem.current.SetSelectedGameObject(chatInputField, null);
			chatInputField.GetComponent<InputField> ().OnPointerClick(new PointerEventData(EventSystem.current));

		}

		lastTimeChatInputFocused = chatInputField.GetComponent<InputField> ().isFocused;

	}

	void addChatMessage(ChatMessage chatMessage) {

		chatManager.Add(chatMessage);
		chatManager.Update ();

	}

}


