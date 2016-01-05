﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameScript : MonoBehaviour {

	private ChatManager chatManager;
	private GameObject chatInputField;
	private bool lastTimeChatInputFocused = false;

	private GameObject localPlayer;
	private List<OtherPlayer> listOtherPlayers = new List<OtherPlayer>();

	private bool test = false;

	// Use this for initialization
	void Start () {

		// AQUI HAY QUE PASARLE AL CHATMANAGER EL GAMEOBJECT QUE ES EL CONTENT DONDE IRAN LOS MENSAJES
		chatManager = new ChatManager(GameObject.Find ("Canvas/ChatPanel/ScrollRect/AllChat"));

		chatInputField = GameObject.Find ("Canvas/ChatPanel/InputField");

		localPlayer = Instantiate (Resources.Load("Prefabs/LocalPlayer") as GameObject);
	
	}
	
	// Update is called once per frame
	void Update () {

		updateChat ();
		updateMyPositionInOtherClients ();

	}

	void OnDisconnectedFromServer(NetworkDisconnection info) {
		if (Network.isServer)
			Debug.Log("Local server connection disconnected");
		else
			if (info == NetworkDisconnection.LostConnection)
				Debug.Log("Lost connection to the server");
		else
			Debug.Log("Successfully diconnected from the server");

		Application.LoadLevel ("Menu");
	}

	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log("Clean up after player " + player);
		Network.RemoveRPCs(player);
		//Network.DestroyPlayerObjects(player);
		GetComponent<NetworkView>().RPC("removePlayerRPC", RPCMode.All, player.ToString());
	}

	[RPC]
	void addChatMessageRPC(string playerCode, string text)
	{
		string owner = "Player " + playerCode;
		if (playerCode == Network.player.ToString()) { owner = "You"; }

		addChatMessage (new ChatMessage (owner, text));
	}

	[RPC]
	void removePlayerRPC(string playerCode) {

		for (int i = 0; i < listOtherPlayers.Count; i++) {
			
			if (listOtherPlayers[i].playerCode == playerCode) {

				Destroy(listOtherPlayers[i].visualAvatar);
				listOtherPlayers.RemoveAt(i);

				break;
			}
			
		}

	}

	[RPC]
	void updatePlayerRPC(string playerCode, Vector3 position, Vector3 rotation, string currentAnimation)
	{
		bool foundPlayer = false;

		for (int i = 0; i < listOtherPlayers.Count; i++) {

			if (listOtherPlayers[i].playerCode == playerCode) {

				listOtherPlayers[i].visualAvatar.transform.position = position;
				listOtherPlayers[i].visualAvatar.transform.eulerAngles = rotation;
				listOtherPlayers[i].SmartCrossfade(currentAnimation);
				foundPlayer = true;
				break;
			}

		}

		if (!foundPlayer) {

			OtherPlayer aux = new OtherPlayer(playerCode);
			listOtherPlayers.Add(aux);
			aux.visualAvatar.transform.position = position;
			aux.visualAvatar.transform.localEulerAngles = rotation;
			aux.SmartCrossfade(currentAnimation);

		}

	}

	void updateChat() {

		if (Input.GetKeyDown(KeyCode.Return)
			&& lastTimeChatInputFocused
		    && chatInputField.GetComponent<InputField> ().text != "") {

			string info = chatInputField.GetComponent<InputField> ().text;
			GetComponent<NetworkView>().RPC("addChatMessageRPC", RPCMode.All, Network.player.ToString(), info);

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

	void updateMyPositionInOtherClients() {

		if (localPlayer != null && GetComponent<NetworkView> () != null) {

			GameObject localVisualAvatar = localPlayer.GetComponent<LocalPlayerScript> ().visualAvatar;
			GetComponent<NetworkView>().RPC("updatePlayerRPC", RPCMode.Others, Network.player.ToString(), localVisualAvatar.transform.position, localVisualAvatar.transform.eulerAngles, localPlayer.GetComponent<LocalPlayerScript> ().lastAnimationOrder);

		}

	}

	public class OtherPlayer {

		public string playerCode;
		public GameObject visualAvatar;
		public string lastAnimationOrder = "Idle01";

		public OtherPlayer(string auxPlayerCode) {

			playerCode = auxPlayerCode;
			visualAvatar = Instantiate (Resources.Load("Prefabs/ToonSoldier") as GameObject);
			visualAvatar.name = "VisualAvatar "+playerCode;

		}

		public void SmartCrossfade(string animation) {

			Animator animator = visualAvatar.GetComponent<Animator> ();

			if (lastAnimationOrder != animation && !animator.GetCurrentAnimatorStateInfo(0).IsName(animation)) {
				animator.CrossFade(animation, GlobalData.crossfadeAnimation);
				lastAnimationOrder = animation;
			}
			
		}

	}

}


