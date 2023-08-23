Shader "Custom/PointCloudShader"
{
    Properties
    {
        _PointSize("PointSize", Float) = 1
        _Alpha ("Transparency", Range(0,1)) = 1.0
    }

    SubShader
    {    
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
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
                
                //float3 worldPosOb = mul(unity_ObjectToWorld,v.v);
                o.pos = UnityObjectToClipPos(v.v);
                // if(worldPosOb.z < -30)
                // {
                //     v.color = float4(0.0,0,0,0);
                // }
                o.size = _PointSize;
                o.col = v.color;
                    
                // Convert from clip space to NDC
                //float3 ndcPos = o.pos.xyz / o.pos.w;
                
                // // Center of the circle
                // float4 center = float4(-47.1, 16.3,0.0,0);
                // center = UnityObjectToClipPos(center);
                //
                // float4 finalPoint =  float4(-70.4, 25.1,0.0,0);
                // finalPoint = UnityObjectToClipPos(finalPoint);
                // // Calculate the radius using the distance formula
                // float radius = length(finalPoint - center);
                // // Calculate the distance from ndcPos to the center
                // float distanceToCenter = length(o.pos - center);
                //
                // // Check if the position is outside the circle
                // if(distanceToCenter > radius)
                // {
                //     o.col.w = 0;
                // }
                
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
