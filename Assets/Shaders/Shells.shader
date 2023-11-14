Shader "Unlit/Shells"
{
    Properties
    {
        _MainTex ("Color", 2D) = "white" {}
        _LengthTex ("Length", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float3 tangent : TANGENT;
            };

            float hash(uint n) {
                // integer hash copied from Hugo Elias
                n = (n << 13U) ^ n;
                n = n * (n * n * 15731U + 7901729U) + 83587310985U;
                return float(n & uint(0x7fffffffU)) / float(0x7fffffff);
            }

            float3 rotateAroundAxis(float3 In, float3 Axis, float Rotation) {
                Rotation = radians(Rotation);
                float s = sin(Rotation);
                float c = cos(Rotation);
                float one_minus_c = 1.0 - c;

                Axis = normalize(Axis);
                float3x3 rot_mat =
                {
                    one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
                    one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
                    one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
                };
                return mul(rot_mat, In);
            }

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float shellHeight : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _LengthTex;
            sampler2D _VectorField;
            uint _Density;
            float _NoiseMax;
            float _MaxHeight;
            float _NormalizedHeight;
            float _Gravity;
            float _ForceInfluence;

            v2f vert(appdata v)
            {
                v2f o;
                float shellHeight = _MaxHeight * _NormalizedHeight;
                float3 gravityDisplacement = float3(0, 0, -1) * _Gravity * _NormalizedHeight * _NormalizedHeight;
                float2 vectorFieldSample = tex2Dlod(_VectorField, float4(v.uv, 0, 0));
                float3 vectorFieldDirection = rotateAroundAxis(v.tangent, v.normal, 360 * vectorFieldSample.x);
                float actualForceInfluence = lerp(0, _ForceInfluence, vectorFieldSample.y);
                float3 forceDisplacement = vectorFieldDirection * actualForceInfluence * _NormalizedHeight * _NormalizedHeight;
                o.vertex = UnityObjectToClipPos(v.vertex + v.normal * shellHeight + gravityDisplacement + forceDisplacement);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.shellHeight = shellHeight;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float sampledLength = tex2D(_LengthTex, i.uv).x;
                float wantedLength = lerp(0.0, _MaxHeight, sampledLength);
                float2 uvScaled = float(_Density) * i.uv;
                float2 uvLocal = frac(uvScaled) * 2.0 - 1;
                uint2 uvInt = uint2(uvScaled);
                float randomValue = hash(uvInt.x + 100U * uvInt.y + 1000U);
                float actualLength = wantedLength - randomValue * _NoiseMax * _MaxHeight;
                float threshold = _NormalizedHeight;
                float width = lerp(1.0, 0.0, i.shellHeight / actualLength);
                float distanceFromCenter = length(uvLocal);
                if (i.shellHeight > actualLength || distanceFromCenter > width)
                {
                    discard;
                }
                fixed4 color = tex2D(_MainTex, i.uv);
                UNITY_APPLY_FOG(i.fogCoord, color);
                return color;
}
            ENDCG
        }
    }
}
