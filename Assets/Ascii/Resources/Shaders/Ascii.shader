///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// \brief   Ascii - Image Effect.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// http://unity3d.com/support/documentation/Components/SL-Shader.html
Shader "Hidden/Ascii"
{
  // http://unity3d.com/support/documentation/Components/SL-Properties.html
  Properties
  {
    _MainTex("Base (RGB)", 2D) = "white" {}
  }

  CGINCLUDE
  #include "UnityCG.cginc"

  // Do not activate. Only for promotional videos.
  //#define ENABLE_DEMO

  uniform sampler2D _MainTex;
  uniform sampler2D _FontTexture;

  uniform float _Amount;
  uniform float _Saturation;
  uniform float _Brightness;
  uniform float _Contrast;
  uniform float _Gamma;
  uniform float _InvertVCoord;
  uniform float4 _Color;
  uniform float4 _FontParams;
  uniform float _FontCount;

  inline float3 sRGB(float3 pixel)
  {
    return (pixel <= float3(0.0031308f, 0.0031308f, 0.0031308f)) ? pixel * 12.9232102f : 1.055f * pow(pixel, 0.41666f) - 0.055f;
  }

  inline float3 Linear(float3 pixel)
  {
    return (pixel <= float3(0.0404482f, 0.0404482f, 0.0404482f)) ? pixel / 12.9232102f : pow((pixel + 0.055f) * 0.9478672f, 2.4f);
  }

#ifdef ENABLE_DEMO
  inline float3 PixelDemo(float3 pixel, float3 final, float2 uv)
  {
    float separator = 0.7f;//(sin(_Time.x * 12.5f) * 0.25f) + 0.5f;
	const float width = 0.05f;

    if (abs(uv.x - separator) < width)
      final = lerp(final, pixel, (uv.x - separator) / width);
    else if (uv.x > separator)
      final = pixel;

    return final;
  }
#endif

  float4 frag_gamma(v2f_img i) : COLOR
  {
	float2 uv = floor(i.uv * _FontParams.zw) / _FontParams.zw;

    float3 pixel = tex2D(_MainTex, uv).rgb;
	
	float luminance = dot(pixel, float3(0.299f, 0.587f, 0.114f));

	float fontIndex = floor(min(luminance, 0.99f) * _FontCount);

	float2 fontUV = (frac((_ScreenParams.xy * i.uv) / _FontParams.xy) + float2(fontIndex, 0.0f)) / float2(_FontCount, _InvertVCoord);

    float3 fontColor = tex2D(_FontTexture, fontUV).rgb;

	float3 finalPixel = lerp(luminance * fontColor, pixel * fontColor, _Saturation) * _Color;

    finalPixel = (finalPixel - 0.5f) * _Contrast + 0.5f + _Brightness;

    finalPixel = clamp(finalPixel, 0.0f, 1.0f);

    finalPixel = pow(finalPixel, _Gamma);

#ifdef ENABLE_DEMO
    finalPixel = PixelDemo(tex2D(_MainTex, i.uv).rgb, finalPixel, i.uv);
#endif

    return float4(lerp(tex2D(_MainTex, i.uv).rgb, finalPixel, _Amount), 1.0f);
  }

  float4 frag_linear(v2f_img i) : COLOR
  {
	float2 uv = floor(i.uv * _FontParams.zw) / _FontParams.zw;

    float3 pixel = sRGB(tex2D(_MainTex, uv).rgb);
	
	float luminance = dot(pixel, float3(0.2125f, 0.7154f, 0.0721f));

	float fontIndex = floor(min(luminance, 0.99f) * _FontCount);

	float2 fontUV = (frac((_ScreenParams.xy * i.uv) / _FontParams.xy) + float2(fontIndex, 0.0f)) / float2(_FontCount, _InvertVCoord);

    float3 fontColor = sRGB(tex2D(_FontTexture, fontUV).rgb);

	float3 finalPixel = lerp(luminance * fontColor, pixel * fontColor, _Saturation) * _Color;

    finalPixel = (finalPixel - 0.5f) * _Contrast + 0.5f + _Brightness;

    finalPixel = clamp(finalPixel, 0.0f, 1.0f);

    finalPixel = pow(finalPixel, _Gamma);

#ifdef ENABLE_DEMO
    finalPixel = PixelDemo(sRGB(tex2D(_MainTex, i.uv).rgb), finalPixel, i.uv);
#endif

    return float4(Linear(lerp(sRGB(tex2D(_MainTex, i.uv).rgb), finalPixel, _Amount)), 1.0f);
  }

  ENDCG

  // Techniques (http://unity3d.com/support/documentation/Components/SL-SubShader.html).
  SubShader
  {
    // Tags (http://docs.unity3d.com/Manual/SL-CullAndDepth.html).
    ZTest Always
    Cull Off
    ZWrite Off
    Fog { Mode off }

    // Pass 0: Color Space Gamma.
    Pass
    {
      CGPROGRAM
      #pragma glsl
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #pragma vertex vert_img
      #pragma fragment frag_gamma
      ENDCG      
    }

    // Pass 1: Color Space Linear.
    Pass
    {
      CGPROGRAM
      #pragma glsl
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #pragma vertex vert_img
      #pragma fragment frag_linear
      ENDCG      
    }
  }

  Fallback off
}