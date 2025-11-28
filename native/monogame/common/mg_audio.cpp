// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

float* MGA_Voice_CalculatePanMatrix(float pan, float scale, float* matrix, int srcChannels)
{
	if (srcChannels == 1)
	{
		matrix[0] = (pan >= 0 ? (1.f - pan) : 1.f) * scale; // Left
		matrix[1] = (pan <= 0 ? (-pan - 1.f) : 1.f) * scale; // Right
	}
	else if (srcChannels == 2)
	{
		if (-1.0f <= pan && pan <= 0.0f)
		{
			matrix[0] = (0.5f * pan + 1.0f) * scale;	// .5 when pan is -1, 1 when pan is 0
			matrix[1] = (0.5f * -pan) * scale;			// .5 when pan is -1, 0 when pan is 0
			matrix[2] = 0.0f;							//  0 when pan is -1, 0 when pan is 0
			matrix[3] = (pan + 1.0f) * scale;			//  0 when pan is -1, 1 when pan is 0
		}
		else
		{
			matrix[0] = (-pan + 1.0f) * scale;			//  1 when pan is 0,   0 when pan is 1
			matrix[1] = 0.0f;							//  0 when pan is 0,   0 when pan is 1
			matrix[2] = (0.5f * pan) * scale;			//  0 when pan is 0, .5f when pan is 1
			matrix[3] = (0.5f * -pan + 1.0f) * scale;	//  1 when pan is 0. .5f when pan is 1
		}
	}

	return matrix;
}