Shader "Custom/SR" {
	Properties {
		_MainColor ("Color (RGBA)", Color) = (1,1,1,1)
		_Tile ("Tile (RGBA, UV1)", 2D) = "white" {}
		_TileColor ("Tile Color (RGBA, UV1)", Color) = (1,1,1,1)
		_TileAlpha ("Tile Alpha (A, UV2)", 2D) = "white" {}
		_Decal ("Decal (RGBA, UV2)", 2D) = "none" {}
	}
	SubShader {
	    Tags { 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		LOD 200
		
		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass {
            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
                #pragma vertex vert
                #pragma fragment frag
    
            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0
    
            sampler2D _Tile;
            sampler2D _Decal;
            sampler2D _TileAlpha;
            half4 _TileColor;
            half4 _MainColor;
    
    		struct appdata_t {
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f {
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float2 texcoord1  : TEXCOORD1;
			};
    
            v2f vert(appdata_t IN) {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.texcoord1 = IN.texcoord1;
                OUT.color = IN.color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
    
                return OUT;
            }
    
    
            fixed4 frag(v2f IN) : SV_Target {
                fixed4 tile = tex2D(_Tile, IN.texcoord) * _TileColor;
    
                if (IN.texcoord1.x >= 0) {
                    tile.a *= tex2D(_TileAlpha, IN.texcoord1).a; 
                    fixed4 decal = tex2D(_Decal, IN.texcoord1);
                    tile.rgb = lerp(tile.rgb, decal.rgb, decal.a);
                    tile.a += (1 - tile.a) * decal.a;
                }
                tile *= _MainColor * IN.color;
                return tile;
            }
		    ENDCG
		}
	}
}
