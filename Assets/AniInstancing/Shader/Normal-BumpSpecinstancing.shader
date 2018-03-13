// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "AnimationInstancing/Bumped SpecularInstancing" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
}

CGINCLUDE

#include "AnimationInstancingBase.cginc"

sampler2D _MainTex;
sampler2D _BumpMap;
fixed4 _Color;
half _Shininess;

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = tex.rgb * _Color.rgb;
	o.Gloss = tex.a;
	o.Alpha = tex.a * _Color.a;
	o.Specular = _Shininess;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG

SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 400
	
	CGPROGRAM
	#pragma surface surf BlinnPhong addshadow
	#pragma vertex vert 
	#pragma multi_compile_instancing
	//#pragma target 3.0
	//#pragma enable_d3d11_debug_symbols
	ENDCG
}
//
//SubShader { 
//	Tags { "RenderType"="Opaque" }
//	LOD 400
//	
//	CGPROGRAM
//	#pragma surface surf BlinnPhong nodynlightmap
//	ENDCG
//}

//FallBack "Legacy Shaders/Specular"
}
