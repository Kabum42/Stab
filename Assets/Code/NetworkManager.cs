using UnityEngine;
using System.Collections;

public static class NetworkManager {

    private const string typeName = "StabTFG";
    //private const string gameName = "RoomName";
    public static HostData[] hostList;
	public static string noPassword = "#NoPassword";

    public static void StartServer(string roomName)
    {
		StartServer (roomName, noPassword);
    }

	public static void StartServer(string roomName, string password)
	{
		Network.InitializeServer(4, 25000, !Network.HavePublicAddress());
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
