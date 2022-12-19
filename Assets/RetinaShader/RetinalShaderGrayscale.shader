Shader "Retinal/Grayscale Retinal Shader"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _PreviousTex("Previous Image", 2D) = "white" {}
        _BlurAmount("Blur Amount", Range(0.0, 0.5)) = 0.001
        _Focus("Focus", Range(0.0, 1.0)) = 0
        _Distribution("Distribution", Range(0.0, 1.0)) = 0.2
        _Iterations("Iterations", Integer) = 5

            // _RedPass("Red Pass", Range(0.0, 1.0)) = 1
            // _GreenPass("Green Pass", Range(0.0, 1.0)) = 1
            // _BluePass("Blue Pass", Range(0.0, 1.0)) = 1
    }
        SubShader
        {
            // No culling or depth
            Cull Off ZWrite Off ZTest Always

            Tags{
                "Queue" = "Transparent"
             }

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                sampler2D _PreviousTex;
                fixed _BlurAmount;
                fixed _Focus;
                fixed _Distribution;
                int _Iterations;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                float4 tex2DblurCurrent(float2 position, float2 offset)
                {
                    const float2 blur_offset = position.xy + float2(_BlurAmount, _BlurAmount).xy * offset * (1 - _Focus);
                    return tex2D(_MainTex, blur_offset);
                }

                float4 tex2DblurPrevious(float2 position, float2 offset)
                {
                    const float2 blur_offset = position.xy + float2(_BlurAmount, _BlurAmount).xy * offset * (1 - _Focus);
                    return tex2D(_PreviousTex, blur_offset);
                }

                float calculateWeight(float distance)
                {
                    return lerp(1, _Distribution, distance);
                }

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f IN) : SV_Target
                {
                    const int2 iterations = int2(_Iterations, _Iterations);
                    const float centralPixelWeight = 1;

                    fixed4 current = tex2D(_MainTex, IN.uv);
                    fixed4 previous = tex2D(_PreviousTex, IN.uv);

                    float4 color_sum_current = float4(0,0,0,0);
                    float weight_sum_current = 0;

                    float4 color_sum_previous = float4(0, 0, 0, 0);
                    float weight_sum_previous = 0;

                    // Add central pixel
                    color_sum_current += tex2DblurCurrent(IN.uv, float2(0, 0)) * centralPixelWeight;
                    weight_sum_current += centralPixelWeight;

                    color_sum_previous += tex2DblurPrevious(IN.uv, float2(0, 0)) * centralPixelWeight;
                    weight_sum_previous += centralPixelWeight;

                    // Add central column
                    for (int horizontal = 1; horizontal < iterations.x; ++horizontal)
                    {
                        const float offset = (float)horizontal / iterations.x;
                        const float weight = calculateWeight(offset);

                        color_sum_current += tex2DblurCurrent(IN.uv, float2(offset, 0)) * weight; // Current 
                        color_sum_current += tex2DblurCurrent(IN.uv, float2(-offset, 0)) * weight; // Current 
                        weight_sum_current += weight * 2;

                        color_sum_previous += tex2DblurCurrent(IN.uv, float2(offset, 0)) * weight;  // Previous
                        color_sum_previous += tex2DblurCurrent(IN.uv, float2(-offset, 0)) * weight; // Previous
                        weight_sum_previous += weight * 2;
                    }

                    // Add quads
                    for (int x = 1; x < iterations.x; ++x)
                    {
                        for (int y = 1; y < iterations.y; ++y)
                        {
                            float2 offset = float2((float)x / iterations.x, (float)y / iterations.y);
                            const float offsetLength = length(offset);
                            const float weight = calculateWeight(offsetLength);

                            color_sum_current += tex2DblurCurrent(IN.uv, float2(offset.x, offset.y)) * weight;
                            color_sum_current += tex2DblurCurrent(IN.uv, float2(-offset.x, offset.y)) * weight;
                            color_sum_current += tex2DblurCurrent(IN.uv, float2(-offset.x, -offset.y)) * weight;
                            color_sum_current += tex2DblurCurrent(IN.uv, float2(offset.x, -offset.y)) * weight;
                            weight_sum_current += weight * 4;

                            color_sum_previous += tex2DblurPrevious(IN.uv, float2(offset.x, offset.y)) * weight;
                            color_sum_previous += tex2DblurPrevious(IN.uv, float2(-offset.x, offset.y)) * weight;
                            color_sum_previous += tex2DblurPrevious(IN.uv, float2(-offset.x, -offset.y)) * weight;
                            color_sum_previous += tex2DblurPrevious(IN.uv, float2(offset.x, -offset.y)) * weight;
                            weight_sum_previous += weight * 4;
                        }
                    }

                    float4 final_color_current = color_sum_current / weight_sum_current;
                    float4 final_color_previous = color_sum_previous / weight_sum_previous;

                    #ifdef UNITY_UI_CLIP_RECT
                    final_color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                    #endif

                    #ifdef UNITY_UI_ALPHACLIP
                    clip(final_color.a - 0.001);
                    #endif

                    fixed4 result = fixed4(0, 0, 0, 1);
                    //result.r = current.r;
                    //result.g = current.g;
                    //result.b = current.b;

                    result.r = 0.5 + ((final_color_current.r - final_color_previous.r) - (final_color_current.g - final_color_previous.g) - (final_color_current.b - final_color_previous.b)) / 3;
                    result.g = 0.5 + ((final_color_current.r - final_color_previous.r) - (final_color_current.g - final_color_previous.g) - (final_color_current.b - final_color_previous.b)) / 3;
                    result.b = 0.5 + ((final_color_current.r - final_color_previous.r) - (final_color_current.g - final_color_previous.g) - (final_color_current.b - final_color_previous.b)) / 3;

                    return result;
                    // return final_color_current;
                    // return final_color_previous;
                }
                ENDCG
            }
        }
}
