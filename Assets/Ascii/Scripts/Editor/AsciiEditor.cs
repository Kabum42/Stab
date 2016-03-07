///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Ascii - Image Effect.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// Do not activate. Only for promotional videos.
//#define ENABLE_DEMO

using UnityEngine;
using UnityEditor;

namespace AsciiImageEffect
{
  /// <summary>
  /// Ascii editor.
  /// </summary>
  [CustomEditor(typeof(Ascii))]
  public sealed class AsciiEditor : Editor
  {
    /// <summary>
    /// Desc.
    /// </summary>
    private readonly string asciiDesc = @"Text based render.";

    /// <summary>
    /// Help text.
    /// </summary>
    public string Help { get; set; }

    /// <summary>
    /// Warnings.
    /// </summary>
    public string Warnings { get; set; }

    /// <summary>
    /// Errors.
    /// </summary>
    public string Errors { get; set; }

    /// <summary>
    /// OnInspectorGUI.
    /// </summary>
    public override void OnInspectorGUI()
    {
      EditorGUIUtility.LookLikeControls();
      EditorGUI.indentLevel = 0;

      EditorGUIUtility.labelWidth = 100.0f;

      EditorGUILayout.BeginVertical();
      {
        EditorGUILayout.Separator();

#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6)
        if (EditorGUIUtility.isProSkin == true)
#endif
        {
          Ascii thisTarget = (Ascii)target;

          EditorGUILayout.BeginVertical();
          {
            thisTarget.amount = AsciiEditorHelper.IntSliderWithReset(@"Amount", "The strength of the effect.\nFrom 0 (no effect) to 100 (full effect).", Mathf.RoundToInt(thisTarget.amount * 100.0f), 0, 100, 100) * 0.01f;

            thisTarget.saturation = AsciiEditorHelper.IntSliderWithReset(@"Saturation", "The saturation.\nFrom 0 (grey) to 100 (color).", Mathf.RoundToInt(thisTarget.saturation * 100.0f), 0, 100, 100) * 0.01f;

            thisTarget.brightness = AsciiEditorHelper.IntSliderWithReset(@"Brightness", "The Screen appears to be more o less radiating light.\nFrom -100 (dark) to 100 (full light).", Mathf.RoundToInt(thisTarget.brightness * 100.0f), -100, 100, 0) * 0.01f;

            thisTarget.contrast = AsciiEditorHelper.IntSliderWithReset(@"Contrast", "The difference in color and brightness.\nFrom -100 (no constrast) to 100 (full constrast).", Mathf.RoundToInt(thisTarget.contrast * 100.0f), -100, 100, 0) * 0.01f;

            thisTarget.gamma = AsciiEditorHelper.SliderWithReset(@"Gamma", "Optimizes the contrast and brightness in the midtones.\nFrom 0.01 to 10.", thisTarget.gamma, 0.01f, 10.0f, 1.0f);

            thisTarget.invertVCoord = (EditorGUILayout.Toggle(new GUIContent(@"Invert V Coord", @"Inverts V coordinate of font."), thisTarget.invertVCoord == -1.0f) == true ? -1.0f : 1.0f);

            thisTarget.color = EditorGUILayout.ColorField(new GUIContent(@"Color", @"Text color"), thisTarget.color);

            Ascii.AsciiCharset charset = (Ascii.AsciiCharset)EditorGUILayout.EnumPopup(new GUIContent(@"Charset", @"Characters to be used."), thisTarget.Charset);
            if (charset != thisTarget.Charset)
              thisTarget.Charset = charset;

            if (thisTarget.Charset == Ascii.AsciiCharset.Custom)
            {
              EditorGUI.indentLevel++;

              thisTarget.fontCount = EditorGUILayout.IntField(new GUIContent(@"Char count", @"The number of characters in the texture."), (int)thisTarget.fontCount);

              thisTarget.fontTexture = EditorGUILayout.ObjectField(new GUIContent(@"Font texture", @"The texture with the characters."), thisTarget.fontTexture, typeof(Texture), false) as Texture;

              EditorGUI.indentLevel--;
            }
#if ENABLE_DEMO
            thisTarget.showGUI = EditorGUILayout.Toggle("Show GUI", thisTarget.showGUI);

            thisTarget.musicClip = EditorGUILayout.ObjectField("Music", thisTarget.musicClip, typeof(AudioClip)) as AudioClip;
#endif
          }
          EditorGUILayout.EndVertical();

          EditorGUILayout.BeginHorizontal();
          {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent(@"[info]", @"Open website"), GUI.skin.label) == true)
              Application.OpenURL(@"http://labs.ibuprogames.com/ascii");
          }
          EditorGUILayout.EndHorizontal();

          Help += asciiDesc;
        }
#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6)
        else
        {
          this.Help = string.Empty;
          this.Errors += @"'Ascii - Image Effect' require Unity Pro version!";
        }
#endif

        if (string.IsNullOrEmpty(Warnings) == false)
        {
          EditorGUILayout.HelpBox(Warnings, MessageType.Warning);

          EditorGUILayout.Separator();
        }

        if (string.IsNullOrEmpty(Errors) == false)
        {
          EditorGUILayout.HelpBox(Errors, MessageType.Error);

          EditorGUILayout.Separator();
        }

        if (string.IsNullOrEmpty(Help) == false)
          EditorGUILayout.HelpBox(Help, MessageType.Info);
      }
      EditorGUILayout.EndVertical();

      if (GUI.changed == true)
        EditorUtility.SetDirty(target);

      EditorGUIUtility.LookLikeControls();

      Help = Warnings = Errors = string.Empty;
    }
  }
}