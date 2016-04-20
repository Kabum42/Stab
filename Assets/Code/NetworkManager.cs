using UnityEngine;
using System.Collections;

public static class NetworkManager {

    private const string typeName = "StabTFG";
    //private const string gameName = "RoomName";
    public static HostData[] hostList;
	public static string noPassword = "#NoPassword";

    public static void StartServer(string roomName, int players)
    {
		StartServer (roomName, noPassword, players);
    }

	public static void StartServer(string roomName, string password, int players)
	{
        // IT's PLAYERS - 1 BECAUSE THE ONE MAKING THE SERVER DOESN'T COUNT
        int connections = players - 1;
		Network.InitializeServer(connections, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, roomName, password);
	}

    public static void RefreshHostList()
    {
        MasterServer.RequestHostList(typeName);
    }

    public static void JoinServer(HostData hostData)
    {
        Network.Connect(hostData);
    }

}
