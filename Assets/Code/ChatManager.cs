using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChatManager {

	private GameObject physicalChat;
	public List<ChatMessage> listMessages = new List<ChatMessage>();
	private int maxMessages = 10;

	public ChatManager(GameObject auxPhysicalChat) {

		physicalChat = auxPhysicalChat;

	}

	public void Add(ChatMessage chatMessage) {

		listMessages.Add (chatMessage);
		chatMessage.assignManager (this);

		if (listMessages.Count > maxMessages) {
			// HERE WOULD BE HANDLED AN OVERFLOW OF MESSAGES
			listMessages.RemoveAt(0);
		}

	}

	public void Update() {

		string aux = "";

		for (int i = 0; i < listMessages.Count; i++) {

			aux += "<color=#0000FF>["+listMessages[i].owner+"]</color> : ";
			aux += listMessages[i].text;

			if (i < listMessages.Count -1) {
				aux += "\n";
			}

		}

		physicalChat.GetComponent<Text> ().text = aux;

	}

}

public class ChatMessage {
	
	public string owner;
	public string text;
	public ChatManager manager;
	
	public ChatMessage(string auxOwner, string auxText) {
		
		owner = auxOwner;
		text = auxText;
		
	}

	public void assignManager(ChatManager auxManager) {

		manager = auxManager;

	}
	
}
