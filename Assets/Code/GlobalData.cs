using UnityEngine;
using System.Collections;

public static class GlobalData {

	public static bool loadedMenuScene = false;
    public static bool started = false;
	public static float crossfadeAnimation = 0.15f;
	//public static float crossfadeAnimation = 0.05f;

    // Use this for initialization
    public static void Start () {

        if (!started) {

            started = true;
            // HERE GOES ALL INITIALIZATION NECESSARY
			string IP = "52.37.198.134";
			//string IP = "127.0.0.1";

			MasterServer.ipAddress = IP;
			MasterServer.port = 23466;
			Network.natFacilitatorIP = IP;
			Network.natFacilitatorPort = 50005;

        }

	}

}
