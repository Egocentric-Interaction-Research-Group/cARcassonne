//------------------------------------------------------------------------------------------------//
// Author:  Kasper Skott
// Created: 2021-11-02
//
// Important note:
// The way input data about the tiles and meeples have some hard limitations.
// A player id must have a value below 10. Similarly, having geographies whose 
// integer value is above 9, WILL break things.
//------------------------------------------------------------------------------------------------//

Shader "Carcassonne/Visualization"
{
    Properties
    {
        _DisplayColumns ("Visible Columns", int) = 31
        _DisplayRows    ("Visible Rows", int)    = 31
        _ColumnOffset   ("Column Offset", int)   = 0
        _RowOffset      ("Row Offset", int)      = 0
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

            #include "UnityCG.cginc"

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

            static const uint DEFAULT_GEOGRAPHY = 3; // 3 = Grass

            static const int2 gridSubDimensions = int2(3, 3); // 3x3 sub-tiles per tile.
            static const int  subcellPerTile = gridSubDimensions.x * gridSubDimensions.y;
            static const int2 totalDimensions = int2(31, 31); // Larger numbers will fail the shader compilation.
            static const int  totalTiles = totalDimensions.x * totalDimensions.y;
            static const int  totalSubtiles = totalTiles * subcellPerTile;
            
            static const float4 colCloister = float4(0.75, 0.12, 0.06, 1.00);
            static const float4 colVillage  = float4(0.46, 0.20, 0.10, 1.00);
            static const float4 colGrass    = float4(0.08, 0.38, 0.01, 1.00);
            static const float4 colRoad     = float4(0.52, 0.56, 0.60, 1.00);
            static const float4 colCity     = float4(0.90, 0.55, 0.20, 1.00);
            static const float4 colStream   = float4(0.00, 0.10, 0.80, 1.00);
            static const float4 colCityRoad = float4(0.80, 0.45, 0.10, 1.00);

            static const float4 colPlayer1 = float4(0.00, 0.00, 1.00, 1.00);
            static const float4 colPlayer2 = float4(0.01, 0.75, 0.01, 1.00);
            static const float4 colPlayer3 = float4(1.00, 0.85, 0.02, 1.00);
            static const float4 colPlayer4 = float4(0.68, 0.00, 0.01, 1.00);
            static const float4 colPlayer5 = float4(0.10, 0.10, 0.10, 1.00);
            static const float4 colPlayer6 = float4(0.00, 0.15, 0.00, 1.00);
            static const float4 colPlayer7 = float4(1.00, 0.25, 0.00, 1.00);
            static const float4 colPlayer8 = float4(0.30, 0.00, 0.01, 1.00);

            int _DisplayColumns;                // How many columns of tiles to display.
            int _DisplayRows;                   // How many rows of tiles to display.
            int _ColumnOffset;                  // The starting column to display of the entire tile array.
            int _RowOffset;                     // The starting row to display of the entire tile array.
            float _TileGeography[totalTiles];   // Contains all geographies encoded in one float per tile.
            float _MeeplePlacement[totalTiles]; // Indicates, on each tile, whether a meeple is placed, and which player it belongs to.

            float grid(float2 st, float resolution)
            {
                float2 grid = frac(st * resolution);
                return step(resolution, grid.x) * step(resolution, grid.y);
            }

            // Get color for meeple representation.
            float4 colorByMeeple(uint playerId, float fracX, float fracY)
            {
                if (fracX >= 0.25 && fracX <= 0.75 &&
                    fracY >= 0.25 && fracY <= 0.75)
                {
                    if (step(0.35, fracX) * step(0.35, fracY) < 0.1)
                        return 0.0;

                    if      (playerId == 0) return -1;
                    else if (playerId == 1) return colPlayer1;
                    else if (playerId == 2) return colPlayer2;
                    else if (playerId == 3) return colPlayer3;
                    else if (playerId == 4) return colPlayer4;
                    else if (playerId == 5) return colPlayer5;
                    else if (playerId == 6) return colPlayer6;
                    else if (playerId == 7) return colPlayer7;
                    else if (playerId == 8) return colPlayer8;
                }               

                return -1;
            }

            float4 geographyToColor(uint geoEnumValue)
            {
                if      (geoEnumValue == 0) return colCloister;
                else if (geoEnumValue == 1) return colVillage;
                else if (geoEnumValue == 2) return colRoad;
                else if (geoEnumValue == 3) return colGrass;
                else if (geoEnumValue == 4) return colCity;
                else if (geoEnumValue == 5) return colStream;
                else if (geoEnumValue == 8) return colCityRoad;

                return float4(1.0, 0.0, 1.0, 1.0);
            }

            float4 determineSubTileColor(float geography, float meeple, float iX, float iY)
            {
                // Geographies and meeples are encoded as followed:
                //   - Center  0-9
                //   - East    10, 20, ..., 90
                //   - North   100, 200, ..., 900
                //   - West    1000, 2000, ..., 9000
                //   - South   10000, 20000, ..., 90000

                uint geo  = uint(abs(geography));
                uint meep = uint(abs(meeple));

                float fracX = frac(iX);
                float fracY = frac(iY);

                float4 color;

                // Initialize with default values.
                uint geographyOut = DEFAULT_GEOGRAPHY;
                uint playerId = 0;                  // Player id of the meeple placed (0 if not placed).
                
                // Top column sub-tiles
                if (fracY >= 0.666 && fracY < 1.000) 
                {
                    if (fracX >= 0.333 && fracX < 0.666) // North
                    {
                        geographyOut = (geo / 100) % 10;
                        playerId = (meep / 100) % 10;
                    }
                }
                // Middle column sub-tiles
                else if (fracY >= 0.333 && fracY < 0.666) 
                {
                    if (fracX >= 0.000 && fracX < 0.333) // West
                    {
                        geographyOut = (geo / 1000) % 10;
                        playerId = (meep / 1000) % 10;
                    }
                    else if (fracX >= 0.333 && fracX < 0.666) // Center
                    {
                        geographyOut = geo % 10;
                        playerId = meep % 10;
                    }
                    else if (fracX >= 0.666 && fracX < 1.000) // East
                    {
                        geographyOut = (geo / 10) % 10;
                        playerId = (meep / 10) % 10;
                    }
                }
                // Bottom column sub-tiles
                else if (fracY >= 0.000 && fracY < 0.333) 
                {
                    if (fracX >= 0.333 && fracX < 0.666) // South
                    {
                        geographyOut = geo / 10000;
                        playerId = meep / 10000;
                    }
                }

                // Draw meeple if present.
                if (playerId > 0)
                {
                    fracX = fracX % (1.0 / 3.0);
                    fracY = fracY % (1.0 / 3.0);
                    color = colorByMeeple(playerId, fracX * 3.0, fracY * 3.0);
                    if (color.a >= 0.0f) // Indicates that a color WAS selected.
                        return color;
                }

                // Draw the sub-tile with a color corresponding to the geography.                
                return geographyToColor(geographyOut);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float gridRes = 0.1;
                float2 gridDims = float2(_DisplayColumns, _DisplayRows);
                gridDims.x *= 1.0 / gridRes;
                gridDims.y *= 1.0 / gridRes;

                float2 gridUV = i.uv;
                gridUV *= gridDims;
                float gridFill = grid(gridUV + 0.05 / gridRes, gridRes);

                float iX = gridUV.x * gridRes;
                float iY = gridUV.y * gridRes;

                int idx = int(iX + _ColumnOffset) + int(iY + _RowOffset) * totalDimensions.x;
                float geo = _TileGeography[idx];

                if (geo < 0.0) // If has invalid geography
                    return 0.0;
                else
                    return determineSubTileColor(geo, _MeeplePlacement[idx], iX, iY) * gridFill;
            }
            ENDCG
        }
    }
}
