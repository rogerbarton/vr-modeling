#ifndef SELECTIONCOLOR_HLSL
#define SELECTIONCOLOR_HLSL

float4 White, Black;
float4 Red, Green, Blue, Orange, Pink;

void SelectionColor_float (float uv1, out float4 Color)
{
    // Define consts
    White = float4(1,1,1,1);
    Black = float4(0,0,0,0);
    Red = float4(0.7735849, 0.3280911, 0.280972, 1);
    Green = float4(0.4173074, 0.7264151, 0.366634, 1);
    Blue = float4(0.3019607, 0.4429668, 0.858823, 1);
    Orange = float4(0.8784314, 0.5314119, 0.145098, 1);
    Pink = float4(1, 0, 1, 1);
    
    uint maskId = asuint(uv1);
   
    Color = White;
    [unroll(32)] 
    for (uint selectionId = 0; selectionId < 32; ++selectionId) {
		uint m = 1u << selectionId;
		if ((m & maskId) == 0) // Skip selections that are not visible
			continue;
        
        Color = (Color + Pink) / 2; 
	}
}
#endif //SELECTIONCOLOR_HLSL