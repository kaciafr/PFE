#ifndef WALL_CUTOUT_INCLUDED
#define WALL_CUTOUT_INCLUDED

float4 _CutoutCenter;
float  _CutoutRadius;
float  _CutoutSmoothness;

// Marge pour ne pas découper les éléments à la même profondeur que le joueur
static const float CUTOUT_DEPTH_BIAS = 0.3;

void WallCutout_float(float3 WorldPos, out float Alpha)
{
    Alpha = 1;

    #if !defined(SHADERGRAPH_PREVIEW)
    float4 pixelClip  = TransformWorldToHClip(WorldPos);
    float4 playerClip = TransformWorldToHClip(_CutoutCenter.xyz);

    // Écart en espace écran, corrigé de l'aspect puis converti en unités monde
    // à la profondeur du joueur (valable en perspective comme en ortho)
    float2 delta = pixelClip.xy / pixelClip.w - playerClip.xy / playerClip.w;
    delta.x *= _ScreenParams.x / _ScreenParams.y;
    
    float dist = length(delta) * playerClip.w / abs(UNITY_MATRIX_P._m11);

    // dist >= radius -> opaque (1) ; dist <= radius - smoothness -> transparent (0)
    float inner = max(_CutoutRadius - _CutoutSmoothness, 0);
    float t = saturate((dist - inner) / max(_CutoutRadius - inner, 0.0001));

    // Seulement ce qui est entre la caméra et le joueur
    float pixelDepth  = -TransformWorldToView(WorldPos).z;
    float playerDepth = -TransformWorldToView(_CutoutCenter.xyz).z;
    float inFront = step(pixelDepth, playerDepth - CUTOUT_DEPTH_BIAS);

    // Si pas devant le joueur -> opaque
    Alpha = lerp(1, t, inFront);
    #endif
}

#endif