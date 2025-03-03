Shader "Unlit/ScatterToolFakeObject"
{
    Properties
    {
    }
    SubShader
    {
        Tags {
            "Queue" = "Transparent" 
            "IgnoreProjector" = "True" 
            "RenderType" = "Transparent"
        }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull front
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(1,1,1,0.3);
            }
            ENDCG
        }
    }
}
