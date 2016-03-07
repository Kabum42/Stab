///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Ascii - Image Effect.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// Do not activate. Only for promotional videos.
//#define ENABLE_DEMO

using System;

using UnityEngine;

namespace AsciiImageEffect
{
  /// <summary>
  /// Ascii - Image Effect.
  /// </summary>
  [ExecuteInEditMode]
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("Image Effects/Ascii")]
  public sealed class Ascii : MonoBehaviour
  {
    /// <summary>
    /// Charsets.
    /// </summary>
    public enum AsciiCharset
    {
      Lucida_5x8_94,            // Lucida, 5x8, 94 chars.
      Courier_8x12_94,          // Courier, 8x12, 94 chars.
      TimesNewRoman_11x15_4,    // Times New Roman, 11x15, 4 chars.
      Custom,                   // Use this to define your own charset.
                                // You must set manually the texture and the number of characters.
    }

    /// <summary>
    /// Amount of the effect (0 none, 1 full).
    /// </summary>
    public float amount = 1.0f;

    /// <summary>
    /// Saturation (0 grey, 1 color).
    /// </summary>
    public float saturation = 1.0f;

    /// <summary>
    /// Brightness (-1 .. 1).
    /// </summary>
    public float brightness = 0.0f;

    /// <summary>
    /// Contrast (-1 .. 1).
    /// </summary>
    public float contrast = 0.0f;

    /// <summary>
    /// Gamma (0.1 .. 10).
    /// </summary>
    public float gamma = 1.0f;

    /// <summary>
    /// Text color.
    /// </summary>
    public Color color = Color.white;

    /// <summary>
    /// The number of characters in the texture.
    /// </summary>
    public float fontCount;

    /// <summary>
    /// Inverts V coordinate of font (1.0 normal, -1.0 inverted).
    /// </summary>
    public float invertVCoord = -1.0f;

    /// <summary>
    /// The texture with the characters.
    /// </summary>
    public Texture fontTexture;

    /// <summary>
    /// Use Charset or SetCharset for custom fonts (see below).
    /// </summary>
    [SerializeField]
    private AsciiCharset charset;

    private Shader shader;

    private Material material;

#if ENABLE_DEMO
    public AudioClip musicClip = null;

    public bool showGUI = true;

    private float timeToChange = 0.0f;
    private float timeMode = 0.0f;

    private GUIStyle effectNameStyle;
#endif

    /// <summary>
    /// Sets charset (for custom fonts, use SetCharset).
    /// </summary>
    public AsciiCharset Charset
    {
      get { return charset; }
      set
      {
        switch (value)
        {
          case AsciiCharset.Courier_8x12_94: SetCharset(value, "Textures/Courier_8x12_94", 94); break;
          case AsciiCharset.Lucida_5x8_94: SetCharset(value, "Textures/Lucida_5x8_94", 94); break;
          case AsciiCharset.TimesNewRoman_11x15_4: SetCharset(value, "Textures/TimesNewRoman_11x15_4", 4); break;
          case AsciiCharset.Custom: charset = value; break;
        }
      }
    }

    /// <summary>
    /// Sets charset.
    /// </summary>
    public void SetCharset(AsciiCharset charset, string texturePath, int fontCount)
    {
      this.charset = charset;

      fontTexture = Resources.Load<Texture>(texturePath);
      if (fontTexture != null)
        this.fontCount = fontCount;
      else
      {
        Debug.LogError(string.Format("Texture '{0}' not found!", texturePath));

        this.enabled = false;
      }
    }

    private void Awake()
    {
      shader = Resources.Load<Shader>(@"Shaders/Ascii");
      if (shader == null)
      {
        Debug.LogError(@"Ascii shader not found.");

        this.enabled = false;
      }
    }

    /// <summary>
    /// Check.
    /// </summary>
    private void OnEnable()
    {
      if (SystemInfo.supportsImageEffects == false)
      {
        Debug.LogError(@"Hardware not support Image Effects.");
      
        this.enabled = false;
      }
      else if (shader == null)
      {
        Debug.LogError(string.Format("'{0}' shader null.", this.GetType().ToString()));
      
        this.enabled = false;
      }
      else
      {
        CreateMaterial();

#if ENABLE_DEMO
        if (musicClip != null && Application.isPlaying == true)
        {
          AudioSource audioSource = this.gameObject.GetComponent<AudioSource>();
          audioSource.clip = musicClip;
          audioSource.loop = Application.isWebPlayer;
          audioSource.Play();

          timeToChange = musicClip.length / 12.0f;
          timeMode = timeToChange;
        }
#endif
        if (material == null)
          this.enabled = false;
      }
    }

    /// <summary>
    /// Destroy the material.
    /// </summary>
    private void OnDisable()
    {
      if (material != null)
        DestroyImmediate(material);
    }

#if ENABLE_DEMO
    private void Update()
    {
      if (Application.isPlaying == false)
        return;

      if (Application.isWebPlayer == false && (Input.GetKeyUp(KeyCode.Escape) == true ||
        (musicClip != null && this.gameObject.GetComponent<AudioSource>().time > Time.realtimeSinceStartup)))
        Application.Quit();

      if (Input.GetKeyUp(KeyCode.Alpha1) == true || Input.GetKeyUp(KeyCode.Keypad1) == true)
      {
        Charset = AsciiCharset.Courier_8x12_94;
        timeToChange = 0.0f;
      }
      else if (Input.GetKeyUp(KeyCode.Alpha2) == true || Input.GetKeyUp(KeyCode.Keypad2) == true)
      {
        Charset = AsciiCharset.Lucida_5x8_94;
        timeToChange = 0.0f;
      }
      else if (Input.GetKeyUp(KeyCode.Alpha3) == true || Input.GetKeyUp(KeyCode.Keypad3) == true)
      {
        Charset = AsciiCharset.TimesNewRoman_11x15_4;
        timeToChange = 0.0f;
      }

      if (timeToChange > 0.0f)
      {
        timeMode -= Time.deltaTime;
        if (timeMode <= 0.0f)
        {
          Charset = (Charset == AsciiCharset.Lucida_5x8_94 ? Charset = AsciiCharset.Courier_8x12_94 : (Charset == AsciiCharset.Courier_8x12_94 ? Charset = AsciiCharset.TimesNewRoman_11x15_4 : Charset = AsciiCharset.Lucida_5x8_94));

          timeMode = timeToChange;
        }
      }
    }

    private void OnGUI()
    {
      if (effectNameStyle == null)
      {
        effectNameStyle = new GUIStyle(GUI.skin.textArea);
        effectNameStyle.alignment = TextAnchor.MiddleCenter;
        effectNameStyle.fontSize = 22;
      }

      if (showGUI == true)
      {
        GUILayout.BeginArea(new Rect(20.0f, 20.0f, 160.0f, 30.0f), "ASCII", effectNameStyle);
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(Screen.width - 180.0f, 20.0f, 160.0f, 30.0f), "NORMAL", effectNameStyle);
        GUILayout.EndArea();
      }
    }
#endif

    /// <summary>
    /// Creates the material.
    /// </summary>
    private void CreateMaterial()
    {
      if (shader != null)
      {
        if (material != null)
        {
          if (Application.isEditor == true)
            DestroyImmediate(material);
          else
            Destroy(material);
        }

        material = new Material(shader);
        if (material != null)
        {
          switch (charset)
          {
            case AsciiCharset.Courier_8x12_94: SetCharset(charset, "Textures/Courier_8x12_94", 94); break;
            case AsciiCharset.Lucida_5x8_94: SetCharset(charset, "Textures/Lucida_5x8_94", 94); break;
            case AsciiCharset.TimesNewRoman_11x15_4: SetCharset(charset, "Textures/TimesNewRoman_11x15_4", 4); break;
          }
        }
        else
          Debug.LogWarning(string.Format("'{0}' material null.", this.name));
      }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
      if (material != null && fontTexture != null && fontCount > 0)
      {
        float fontWidth = fontTexture.width / fontCount;

        material.SetFloat(@"_Amount", amount);
        material.SetFloat(@"_Saturation", saturation);
        material.SetFloat(@"_Brightness", brightness);
        material.SetFloat(@"_Contrast", contrast + 1.0f);
        material.SetFloat(@"_Gamma", 1.0f / gamma);
        material.SetFloat(@"_InvertVCoord", invertVCoord);
        material.SetColor(@"_Color", color);
        material.SetVector(@"_FontParams", new Vector4(fontWidth, fontTexture.height, Screen.width / fontWidth, Screen.height / fontTexture.height));
        material.SetFloat(@"_FontCount", fontCount);

        material.SetTexture(@"_FontTexture", fontTexture);

        Graphics.Blit(source, destination, material, QualitySettings.activeColorSpace == ColorSpace.Linear ? 1 : 0);
      }
    }
  }
}
