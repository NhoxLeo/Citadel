Shader "Custom/ChromaKey" {

    Properties{
      _MainTex("Base (RGB)", 2D) = "white" {}
      _AlphaValue("Alpha Value", Range(0.0,1.0)) = 1.0
      _Color("Color", Color) = (1,1,1,1)

    }

        SubShader{
          Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
          LOD 200

          CGPROGRAM
          #pragma surface surf Lambert alpha
          sampler2D _MainTex;
          float _AlphaValue;
          float4 _Color;

          struct Input {
           float2 uv_MainTex;
          };
          void surf(Input IN, inout SurfaceOutput o) {
           half4 c = tex2D(_MainTex, IN.uv_MainTex);
           o.Emission = c.rgb;

           // Green screen level - leaves minor green glow
           // Note how the green value operator check is greater than and rest is less than
           if (c.g >= _Color.g && c.r <= _Color.r && c.b <= _Color.b)
                 {
                  o.Alpha = 0.0;
                 }
                 else
                 {
                  o.Alpha = c.a;
                 }
          }
          ENDCG
      }
          FallBack "Diffuse"
}
