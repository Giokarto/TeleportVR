Shader "Custom/PointCloudShader"
{
    Properties
    {
        _PointSize("PointSize", Float) = 1
    }

    SubShader
    {
        Pass
        {
            LOD 200

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct VertexInput
            {
                float4 v : POSITION;
                float4 color: COLOR;
            };

            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float4 col : COLOR;
                float size : PSIZE;
            };

            float _PointSize;

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                o.pos = UnityObjectToClipPos(v.v);
                o.size = _PointSize;
                o.col = v.color;

                // Convert from clip space to NDC
                float3 ndcPos = o.pos.xyz / o.pos.w;

                // Center of the circle
                float2 center = float2(-47.1, 16.3);
                // Calculate the radius using the distance formula
                float radius = length(float2(-70.4, 25.1) - center);
                // Calculate the distance from ndcPos to the center
                float distanceToCenter = length(ndcPos.xy - center);

                // Check if the position is outside the circle
                if(distanceToCenter > radius)
                {
                    o.col.w = 0;
                }
                
                return o;
            }

            float4 frag(VertexOutput o) : COLOR
            {
                return o.col;
            }

            ENDCG
        }
    }
}
