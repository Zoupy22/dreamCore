Shader "UI/PuzzleSlice"
{
    Properties
    {
        _MainTex ("Base (RTR)", 2D) = "white" {}
        _Slice   ("Slice (x,y,w,h)", Vector) = (0,0,0.5,0.5)  // 本块在 0~1 范围
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
            sampler2D _MainTex;
            float4 _Slice;   // xy=左下，zw=宽高
            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.texcoord;
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                // 先裁掉不属于本块的区域
                float2 reg = (i.uv - _Slice.xy) / _Slice.zw;
                clip(reg.x); clip(1-reg.x);
                clip(reg.y); clip(1-reg.y);
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}