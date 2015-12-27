using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Hacks {

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

}
