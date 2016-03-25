using UnityEngine;
using System.Collections;

public static class GlobalData {

    public static bool started = false;
	public static float crossfadeAnimation = 0.15f;

    // Use this for initialization
    public static void Start () {

        if (!started) {

            started = true;
            // HERE GOES ALL INITIALIZATION NECESSARY
			NetCustom.Start();

        }

	}

}
