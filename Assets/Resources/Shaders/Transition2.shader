Shader "Custom/Transition2"
 {
     Properties
     {
         _Color ("Main Color", Color) = (1,1,1,1)
         _MainTex ("Base (RGB)", 2D) = "white" {}
         _AlphaTex ("Alpha", 2D) = "white" {}
		 _ColorEffect ("Main Color", Color) = (1,1,1,1)
		 _MainTex2 ("Base (RGB)", 2D) = "white" {}
		 _AlphaTex2 ("Alpha", 2D) = "white" {}
         _Cutoff ("Alpha cutoff", Range(0.05,0.95)) = 0.5
		 _Effect ("Effecting", Range(0.05,0.95)) = 0.5
     }
 
     SubShader
     {
         Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
         LOD 300
         Lighting Off

         
         CGPROGRAM
         #pragma surface surf Lambert alphatest:_Cutoff
 
         sampler2D _MainTex;
         sampler2D _AlphaTex;
		 sampler2D _MainTex2;
		 sampler2D _AlphaTex2;
         fixed4 _Color;
		 fixed4 _ColorEffect;
		 float _Effect;
 
         struct Input
         {
             float2 uv_MainTex;
             float2 uv_AlphaTex;
			 float2 uv_MainTex2;
			 float2 uv_AlphaTex2;
         };
 
         void surf (Input IN, inout SurfaceOutput o)
         {
             fixed4 MAIN = tex2D(_MainTex, IN.uv_MainTex) * _Color;
             fixed4 ALPHA = tex2D(_AlphaTex, IN.uv_AlphaTex);
			 fixed4 MAIN2 = tex2D(_MainTex2, IN.uv_MainTex2) * _ColorEffect;
			 fixed4 ALPHA2 = tex2D(_AlphaTex2, IN.uv_AlphaTex2);

			 if (ALPHA2.r > _Effect) {
				o.Albedo = MAIN.rgb;
			 }
			 else {
				o.Albedo = MAIN2.rgb;
			 }

			 o.Alpha = ALPHA.rgb*MAIN.a;
             
         }
         ENDCG
     }
 
     FallBack "Transparent/Cutout/Diffuse"
 }