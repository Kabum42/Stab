using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChatManager {

	public GameObject chatInputField;
	public GameObject chatPanel;
	public bool lastTimeChatInputFocused = false;
	public float lastChatPannelInteraction = 0f;
	public float chatPannelInteractionThreshold = 8f;

	private GameObject physicalChat;
	public List<ChatMessage> listMessages = new List<ChatMessage>();
	private int maxMessages = 10;
	private ScrollRect scrollRect;

	public ChatManager(GameObject auxChatPanel) {

		chatPanel = auxChatPanel;
		chatInputField = chatPanel.transform.FindChild ("InputField").gameObject;
		physicalChat = chatPanel.transform.FindChild ("ScrollRect/AllChat").gameObject;
		scrollRect = physicalChat.transform.parent.gameObject.GetComponent<ScrollRect> ();

	}

	public void Add(ChatMessage chatMessage) {

		listMessages.Add (chatMessage);
		chatMessage.assignManager (this);

		if (listMessages.Count > maxMessages) {
			// HERE WOULD BE HANDLED AN OVERFLOW OF MESSAGES
			listMessages.RemoveAt(0);
		}

		Write ();

	}

	public void Update() {

		if (physicalChat.GetComponent<RectTransform> ().sizeDelta.y > 600f) {
			physicalChat.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (physicalChat.GetComponent<RectTransform> ().anchoredPosition.x, 115f + (physicalChat.GetComponent<RectTransform> ().sizeDelta.y - 600f)/(1f/0.35f));
		} else {
			physicalChat.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (physicalChat.GetComponent<RectTransform> ().anchoredPosition.x, 121f);
		}

	}

	public void Write() {

		string aux = "";

		for (int i = 0; i < listMessages.Count; i++) {

			if (listMessages [i].owner == "System") {

				string auxColor = "D7D520";
				aux += "<color=#"+auxColor+">"+listMessages[i].text+"</color>";

				if (i < listMessages.Count -1) {
					aux += "\n";
				}

			} else {

				string auxColor = "FF7777";

				if (listMessages [i].owner == "You") {
					auxColor = "77FF77";
				}


				aux += "<color=#"+auxColor+">["+listMessages[i].owner+"]</color> : ";
				aux += listMessages[i].text;

				if (i < listMessages.Count -1) {
					aux += "\n";
				}

			}

		}

		physicalChat.GetComponent<Text> ().text = aux;

		Canvas.ForceUpdateCanvases ();

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
