using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;

public static class NetCustom {

	public static bool started = false;
    private const string typeName = "StabTFG";
    //private const string gameName = "RoomName";
    public static HostData[] hostList;
	public static string noPassword = "#NoPassword";
	public static NetworkManager manager;

	public static void Start() {

		if (!started) {

			started = true;
			// HERE GOES ALL INITIALIZATION NECESSARY
			manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
			NetworkManager.singleton = manager;

		}

	}

    public static void StartServer(string roomName)
    {
		StartServer (roomName, noPassword);
    }

	public static void StartServer(string roomName, string password)
	{
		manager.StartMatchMaker ();
		manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true, "", manager.OnMatchCreate);
		//NetworkClient aux = manager.StartHost ();
		//Network.InitializeServer(4, 25000, !Network.HavePublicAddress());
		//MasterServer.RegisterHost(typeName, roomName, password);
	}

    public static void RefreshHostList()
    {
		manager.matchMaker.ListMatches(0,20, "", manager.OnMatchList);
        //MasterServer.RequestHostList(typeName);
    }

    public static void JoinServer(HostData hostData)
    {
		//manager.matchName = match.name;
		//manager.matchSize = (uint)match.currentSize;
		//manager.matchMaker.JoinMatch(match.networkId, "", manager.OnMatchJoined);

        //Network.Connect(hostData);
    }

}
