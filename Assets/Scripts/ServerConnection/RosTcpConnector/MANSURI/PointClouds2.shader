Shader "Custom/PointClouds2"
{
    Properties
    {
        _PointSize("PointSize", Float) = 1
        _MainTex ("Texture", 2D) = "white" {}
        _ReferencePoint("ReferencePoint", Vector) = (0,0,0)
        _MaxDistance("MaxDistance", Float) = 5
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100


        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float4 color: COLOR;
            };

            struct v2f
            {
                float4 col : COLOR;
                float4 vertex : SV_POSITION;
                float size : PSIZE;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _PointSize;
            float3 _ReferencePoint; // The point from which the distance is measured
            float _MaxDistance; // The distance at which the color will be fully green
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.size = _PointSize;
                o.col = v.color;
                // float4 ref = UnityObjectToClipPos(_ReferencePoint);
                // // Calculate the distance from the reference point
                // float distance = length(v.vertex.xyz - _ReferencePoint);
                //
                // // Normalize the distance to [0, 1] based on MaxDistance
                // float normalizedDistance = saturate(distance / _MaxDistance);
                //
                // // Set the color based on the normalized distance
                // if (normalizedDistance < 0.5f)
                // {
                //     // Transition from red to yellow
                //     o.col = lerp(float4(1.0f, 0.0f, 0.0f, 1.0f), float4(1.0f, 1.0f, 0.0f, 1.0f), normalizedDistance * 2.0f);
                //  }
                // else
                // {
                //     // Transition from yellow to green
                //     o.col = lerp(float4(1.0f, 1.0f, 0.0f, 1.0f), float4(0.0f, 1.0f, 0.0f, 1.0f), (normalizedDistance - 0.5f) * 2.0f);
                // }
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = i.col;
                return col;
            }
            ENDCG
        }
    }
}
