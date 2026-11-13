#ifndef WALL_CUTOUT_INCLUDED
#define WALL_CUTOUT_INCLUDED

float4 _CutoutPlayer;
float _CutoutRadius;
float _CutoutSoftness;
float _CutoutMinAlpha;

void WallCutout_float(float3 WorldPos, out float Alpha)
{
    Alpha = 1;

#if !defined(SHADERGRAPH_PREVIEW)
    float4 pixelClip = TransformWorldToHClip(WorldPos);
    float4 playerClip = TransformWorldToHClip(_CutoutPlayer.xyz);

    float2 pixelScreen = pixelClip.xy / pixelClip.w;
    float2 playerScreen = playerClip.xy / playerClip.w;

    float2 delta = pixelScreen - playerScreen;
    delta.x *= _ScreenParams.x / _ScreenParams.y;

    float circle = smoothstep(_CutoutRadius - _CutoutSoftness, _CutoutRadius, length(delta));

    float camToPixel = distance(_WorldSpaceCameraPos, WorldPos);
    float camToPlayer = distance(_WorldSpaceCameraPos, _CutoutPlayer.xyz);
    float inFront = step(camToPixel, camToPlayer - 0.3);

    Alpha = lerp(1, max(circle, _CutoutMinAlpha), inFront * _CutoutPlayer.w);
#endif
}

#endif
