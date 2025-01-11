using UnityEngine;
using System;
using static Unity.Mathematics.math;
using Unity.Mathematics;



public class NoiseScript : MonoBehaviour
{

    
    public void Test()
    {
        Debug.Log("AAAAAAA");
    }

    // FÖR ATT ANVÄNDA APPLY HEIGHT
    [SerializeField] MapGrid mesh;
    void Start(){
        float[] heightMap = new float[(mesh.xSize + 1) * (mesh.zSize + 1)];
		for (int i = 0, z = 0; z <= mesh.zSize; z++) {
			for (int x = 0; x <= mesh.xSize; x++, i++) {
				heightMap[i] = UnityEngine.Random.Range(0f, 1f);
			}
		}
        mesh.ApplyHeight(heightMap);
    }

    public Texture2D FBMtexture()
    {
        int pixWidth = 800;
        int pixHeight = 800;
        Texture2D noiseTex;

        noiseTex = new Texture2D(pixWidth, pixHeight);
        Color[] pix = new Color[noiseTex.width * noiseTex.height];
        float randomorg = UnityEngine.Random.Range(0, 100);

        // For each pixel in the texture...
        float y = 0.0F;



        int octaves = 2;
        float lacunarity = 1.5f;
        float gain = 0.6f;

        float amp = 0.35f;
        float freq = 0.88f;

        float2 repeatSize = float2(20, 20);
        

        while (y < noiseTex.height)
        {
            
            float x = 0.0F;
            while (x < noiseTex.width)
            {

                float noiseValueR = 0;
                for (int i = 0; i < octaves; i++)
                {
                    noiseValueR += amp * noise.pnoise(float2(x,y), repeatSize);
                    freq *= lacunarity;
                    amp *= gain;
                }
                float noiseValueG = 0;
                for (int i = 0; i < octaves; i++)
                {
                    noiseValueR += amp * noise.pnoise(float2(x,y), repeatSize);
                    Debug.Log("AAAAA" + noiseValueR);
                    freq *= lacunarity;
                    amp *= gain;
                }
                float noiseValueB = 0;
                for (int i = 0; i < octaves; i++)
                {
                    noiseValueR += amp * noise.pnoise(float2(x,y), repeatSize);
                    freq *= lacunarity;
                    amp *= gain;
                }
                pix[(int)y * noiseTex.width + (int)x] = new Color(noiseValueR, noiseValueG, noiseValueB);
                x++;

            }
            y++;
        }

        noiseTex.SetPixels(pix);
        noiseTex.Apply();

        return noiseTex;

    }
}


/*

// Set up the texture and a Color array to hold pixels during processing.
        noiseTex = new Texture2D(pixWidth, pixHeight);
        Color[] pix = new Color[noiseTex.width * noiseTex.height];
        float randomorg = Random.Range(0, 100);


        


*/