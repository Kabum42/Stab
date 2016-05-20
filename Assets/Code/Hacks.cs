using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Hacks {

	private static List<AudioSource> hacksAudioSources = new List<AudioSource> ();
	private static GameObject parentAudioSources;

    // TEXT ALPHA
    public static void TextAlpha(GameObject g, float a)
    {
        g.GetComponent<TextMesh>().color = new Color(g.GetComponent<TextMesh>().color.r, g.GetComponent<TextMesh>().color.g, g.GetComponent<TextMesh>().color.b, a);
    }

    public static void TextAlpha(TextMesh t, float a)
    {
        t.color = new Color(t.color.r, t.color.g, t.color.b, a);
    }

    // SPRITE RENDERER ALPHA
    public static void SpriteRendererAlpha(GameObject g, float a)
    {
        SpriteRendererAlpha(g.GetComponent<SpriteRenderer>(), a);
    }

    public static void SpriteRendererAlpha(SpriteRenderer s, float a)
    {
        s.color = new Color(s.color.r, s.color.g, s.color.b, a);
    }

    // SPRITE RENDERER COLOR
    public static void SpriteRendererColor(GameObject g, Color c)
    {
        SpriteRendererColor(g.GetComponent<SpriteRenderer>(), c);
    }

    public static void SpriteRendererColor(SpriteRenderer s, Color c)
    {
        s.color = new Color(c.r, c.g, c.b, s.color.a);
    }

	// COLOR ALPHA
	public static Color ColorLerpAlpha(Color c, float alpha, float t) {

		c = Color.Lerp (c, new Color (c.r, c.g, c.b, alpha), t);
		return c;

	}

	// LERP VECTOR
	public static Vector3 LerpVector3(Vector3 origin, Vector3 target, float delta) {

		float resultX = Mathf.Lerp(origin.x, target.x, delta);
		float resultY = Mathf.Lerp(origin.y, target.y, delta);
		float resultZ = Mathf.Lerp(origin.z, target.z, delta);

		return new Vector3 (resultX, resultY, resultZ);

	}

	public static Vector3 LerpVector3Angle(Vector3 origin, Vector3 target, float delta) {
		
		float resultX = Mathf.LerpAngle(origin.x, target.x, delta);
		float resultY = Mathf.LerpAngle(origin.y, target.y, delta);
		float resultZ = Mathf.LerpAngle(origin.z, target.z, delta);
		
		return new Vector3 (resultX, resultY, resultZ);
		
	}

    // BINARY PERLIN
    public static int BinaryPerlin(int bits, float seedX, float seedY)
    {
        int result = 0;
        float aux;

        for (int i = 1; i <= bits; i++)
        {
            aux = (Mathf.Clamp(Mathf.PerlinNoise(seedX, seedY), 0f, 1f));
            seedX += 1.573576868f;
            if (aux > 0.5f) { result += (int) Mathf.Pow(2, i-1); }
        }

        return result;
    }

    public static float BinaryPerlin(float min, float max, int bits, float seedX, float seedY)
    {
        float result = min + ((float)Hacks.BinaryPerlin(bits, seedX, seedY)) / (Mathf.Pow(2, bits) -1) * (max - min);

        return result;
    }

    // XBOX CONTROLLER
    public static bool ControllerAnyConnected()
    {
        for (int i = 0; i < Input.GetJoystickNames().Length; i++)
        {
            if (Input.GetJoystickNames()[i] != "")
            {
                return true;
            }
        }
        return false;
    }


	// TEXT
	public static string TextMultilineCentered(GameObject g, string s) {


        string previousString = g.GetComponent<TextMesh>().text;

		string result = "";
		string[] lines = s.Split('\n');
		float maxSizeX = 0f;

		for (int i = 0; i < lines.Length; i++) {
			g.GetComponent<TextMesh>().text = lines[i];
			if (g.GetComponent<Renderer>().bounds.size.x > maxSizeX) {
				maxSizeX = g.GetComponent<Renderer>().bounds.size.x;
			}
		}

		for (int i = 0; i < lines.Length; i++) {
			g.GetComponent<TextMesh>().text = lines[i];
			while (g.GetComponent<Renderer>().bounds.size.x < maxSizeX) {
				lines[i] = " "+lines[i]+" "; 
				g.GetComponent<TextMesh>().text = lines[i];
			}
			result += lines[i];
			if (i < lines.Length-1) { result += "\n"; }
		}

        g.GetComponent<TextMesh>().text = previousString;

		return result;

	}

	// DETECT MOUSE OVER GAMEOBJECT
	public static bool isOver(GameObject target) {
		return isOver (target, "Clickable");
	}

	public static bool isOver(GameObject target, string mask)
	{
		
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		// CLICKABLE MASK
		RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(ray.origin.x, ray.origin.y), Vector2.zero, 0f, LayerMask.GetMask(mask));
		
		for (int i = 0; i < hits.Length; i++)
		{

			if (hits[i].collider.gameObject == target) { return true; }
			
		}
		
		return false;
		
	}

	// SOUND
	public static AudioSource GetAudioSource() {

		if (parentAudioSources != Camera.main.gameObject) {
			hacksAudioSources.Clear ();
			parentAudioSources = Camera.main.gameObject;
		}

		AudioSource aux = null;

		for (int i = 0; i < hacksAudioSources.Count; i++) {
			if (!hacksAudioSources [i].isPlaying) {
				aux = hacksAudioSources [i];
				break;
			}
		}

		if (aux == null) {
			aux = Camera.main.gameObject.AddComponent<AudioSource> ();
			aux.spatialBlend = 0f;
			aux.loop = false;
			aux.playOnAwake = false;
			hacksAudioSources.Add (aux);
		}

		return aux;
	}

	public static AudioSource GetAudioSource(string resourcesPath) {

		AudioSource aux = GetAudioSource ();
		aux.clip = Resources.Load(resourcesPath) as AudioClip;
		return aux;

	}

	// APPLICATION
	public static IEnumerator LockCursor(float time)
	{
		if (time < 0f) { 
			// USE DEFAULT
			time = 0.1f;
		}

		yield return new WaitForSeconds(time);
		// Code to execute after the delay
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	// TEXTURE2D
	public static Texture2D GlitchTexture2D(Texture2D sourceTex) {

		bool corrupted = false;
		Texture2D newTex = sourceTex;

		while (!corrupted) {

			byte[] bytes = sourceTex.EncodeToJPG();
			string data = System.Convert.ToBase64String(bytes);

			string newData = data;
			int iterations = UnityEngine.Random.Range(1, 100);

			for (int i = 0; i < iterations; i++)
			{
				newData = modifyInfo(newData);
			}

			byte[] newBytes = System.Convert.FromBase64String(newData);

			newTex = MonoBehaviour.Instantiate(sourceTex);
			newTex.LoadImage(newBytes);

			if (newTex.height != 8)
			{
				corrupted = true;
			}

		}

		return newTex;

	}
		
	private static string modifyInfo(string data)
	{
		int delete = 1;
		int position = UnityEngine.Random.Range(0, data.Length -delete);

		string prefix = data.Substring(0, position);
		string sufix = data.Substring(position + delete, data.Length - (position + delete));

		string randomChar = UnityEngine.Random.Range(1, 9).ToString();

		return (prefix + randomChar + sufix);
	}


	public static void SetLayerRecursively(GameObject obj, int newLayer)
	{
		obj.layer = newLayer;

		foreach (Transform child in obj.transform)
		{
			SetLayerRecursively(child.gameObject, newLayer);
		}
	}


}
