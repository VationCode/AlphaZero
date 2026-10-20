Shader "UI/ScopeCutout"
{
    Properties
    {
        _Color ("Outside Color", Color) = (0, 0, 0, 1)
        _Radius ("Hole Radius", Range(0, 0.5)) = 0.32
        _Edge ("Edge Softness", Range(0, 0.02)) = 0.002
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Radius, _Edge;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 size = _ScreenParams.xy;
                float2 position = (i.uv - 0.5) * size;
                float radius = _Radius * min(size.x, size.y);
                float edge = _Edge * min(size.x, size.y);
                float alpha = smoothstep(radius - edge, radius + edge, length(position));
                return fixed4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
}