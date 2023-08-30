 Shader "Custom/PointClouds2"
{
    Properties
    {
        _PointSize("PointSize", Float) = 1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" }
               // Tags { "RenderType" = "Opaque" }
            LOD 200
        //LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            struct appdata
            {
                half4 vertex : POSITION;
                half4 color: COLOR;
            };

            struct v2f
            {
            half4 col : COLOR;
            half4 vertex : SV_POSITION;
            half size : PSIZE;
            };

half _PointSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.size = _PointSize;
                o.col = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.col;
            }
            ENDCG
        }
    }
}

