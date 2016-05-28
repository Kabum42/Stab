using UnityEngine;
using System.Collections;

public static class GlobalData {

	public static bool loadedMenuScene = false;
    public static bool started = false;
	public static float crossfadeAnimation = 0.15f;
	public static ClientScript clientScript = null;
	//public static float crossfadeAnimation = 0.05f;

	public static bool fullScreen;
	public static int screenWidth;
	public static int screenHeight;
	public static float mouseSensitivity;

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

			FirstTimePlayerPrefs ();

			fullScreen = false;

			if (PlayerPrefs.GetInt ("FullScreen") == 0) { fullScreen = false; }
			else if (PlayerPrefs.GetInt ("FullScreen") == 1) { fullScreen = true; }

			screenWidth = PlayerPrefs.GetInt ("ScreenWidth");
			screenHeight = PlayerPrefs.GetInt ("ScreenHeight");

			Screen.SetResolution (screenWidth, screenHeight, fullScreen);

			mouseSensitivity = PlayerPrefs.GetFloat ("MouseSensitivity");

        }

	}

	public static void FirstTimePlayerPrefs() {

		if (!PlayerPrefs.HasKey ("FullScreen")) {
			PlayerPrefs.SetInt ("FullScreen", 1);
		}

		if (!PlayerPrefs.HasKey ("ScreenWidth")) {
			PlayerPrefs.SetInt ("ScreenWidth", Screen.resolutions[Screen.resolutions.Length -1].width);
		}

		if (!PlayerPrefs.HasKey ("ScreenHeight")) {
			PlayerPrefs.SetInt ("ScreenHeight", Screen.resolutions[Screen.resolutions.Length -1].height);
		}

		if (!PlayerPrefs.HasKey ("MouseSensitivity")) {
			PlayerPrefs.SetFloat ("MouseSensitivity", 0.5f);
		}

	}

}
