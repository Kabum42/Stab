using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChatManager {

	public List<ChatMessage> listMessages = new List<ChatMessage>();
	private int maxMessages = 100;

	public ChatManager() {

	}

	public void Add(ChatMessage chatMessage) {

		listMessages.Add (chatMessage);
		chatMessage.assignManager (this);

		if (listMessages.Count > maxMessages) {
			// HERE WOULD BE HANDLED AN OVERFLOW OF MESSAGES
			// listMessages[0].root.Destroy();
			listMessages.RemoveAt(0);
		}

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
