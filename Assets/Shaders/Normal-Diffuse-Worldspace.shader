Shader "Diffuse - Worldspace" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}

	_Scale ("Texture Scale", Float) = 1.0
	_Offset("Texture Offset X", Float) = 0

	_DecalTex("Decal Texture (RGB)", 2D) = "white" {}

	_Scale2 ("Decal Scale", Float) = 1.0

	_BlendAmount("Blend Amount", Range(0.0, 1.0)) = 0


}
SubShader {
	Tags { "RenderType"="Opaque" }

	LOD 200


CGPROGRAM

#pragma surface surf Lambert
#pragma multi_compile_fwdadd_fullshadows
//#pragma target 3.0


sampler2D _MainTex;


fixed4 _Color;
float _Scale;
float _Scale2;
float _Offset;

sampler2D _DecalTex;
fixed _BlendAmount;

struct Input
{
	float3 worldNormal;
	float3 worldPos;

	float2 uv_DecalTex;
};

void surf (Input IN, inout SurfaceOutput o)
{
	float2 UV;
	fixed4 c;
	fixed4 cd;

	if(abs(IN.worldNormal.x)>0.5)
	{
		UV = IN.worldPos.yz; // side
		c = tex2D(_MainTex, UV* _Scale + _Offset); // use WALLSIDE texture
		cd = tex2D(_DecalTex, UV* _Scale2);
	}
	else if(abs(IN.worldNormal.z)>0.5)
	{
		UV = IN.worldPos.xy; // front
		c = tex2D(_MainTex, UV* _Scale + _Offset); // use WALL texture
		cd = tex2D(_DecalTex, UV* _Scale2);
	}
	else
	{
		UV = IN.worldPos.xz; // top
		c = tex2D(_MainTex, UV* _Scale + _Offset); // use FLR texture
		cd = tex2D(_DecalTex, UV* _Scale2);
	}

	//o.Albedo = c.rgb * _Color;
	o.Albedo = lerp(c, cd.rgb, cd.a * _BlendAmount) * _Color;
}
ENDCG
}

Fallback "VertexLit"
}
