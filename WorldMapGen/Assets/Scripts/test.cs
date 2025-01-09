using UnityEngine;
using System;
using System.Collections;
using static Unity.Mathematics.math;
using Unity.Mathematics;
using UnityEngine.UI;
// noise functions
// https://docs.unity3d.com/Packages/com.unity.mathematics@1.3/api/Unity.Mathematics.noise.html

public class WorleyPoint{
    public WorleyPoint(Vector3 position, Color color){
        pos = position;
        col = color;
    }

    public Vector3 pos; // z är höjd
    public Color col;

}


public class test : MonoBehaviour
{
    
    [SerializeField]
    NoiseScript noiseSc;
    //public GameObject plane;
    public int pixWidth = 800;
    public int pixHeight = 800;
    Texture2D noiseTex;
    float scale = 1.0f;
    public Color water = new Color(0.0f, 0.0f, 1.0f, 1.0f);
    public Color sand = new Color(0.9f, 0.8f, 0.5f, 1.0f);
    public Color grass = new Color(0.1f, 1.0f, 0.1f, 1.0f);
    public Color forest = new Color(0.4f, 1.0f, 0.4f, 1.0f);
    public Color mountains = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    enum TextureTypes {Map, Cellular}
    private TextureTypes currentTexture = TextureTypes.Map;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scale = GameObject.Find("Scale Slider").GetComponent<Slider>().value;
        Worley();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateScale(){
        scale = GameObject.Find("Scale Slider").GetComponent<Slider>().value;

        if(currentTexture == TextureTypes.Map){
            Worley();
        }
        if(currentTexture == TextureTypes.Cellular){
            CellularTex();
        }
    }

    public void CreateMap()
    {
        currentTexture = TextureTypes.Map;
        // Set up the texture and a Color array to hold pixels during processing.
        noiseTex = new Texture2D(pixWidth, pixHeight);
        Color[] pix = new Color[noiseTex.width * noiseTex.height];
        float randomorg = UnityEngine.Random.Range(0, 100);

        // For each pixel in the texture...
        float y = 0.0F;
        scale = GameObject.Find("Scale Slider").GetComponent<Slider>().value * 5;

        while (y < noiseTex.height)
        {
            float x = 0.0F;
            while (x < noiseTex.width)
            {
                float xCoord = randomorg + x / noiseTex.width * scale;
                float yCoord = randomorg + y / noiseTex.height * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);

                if (sample == Mathf.Clamp(sample, 0, 0.5f))
                    pix[(int)y * noiseTex.width + (int)x] = water;
                else if (sample == Mathf.Clamp(sample, 0.5f, 0.6f))
                    pix[(int)y * noiseTex.width + (int)x] = sand;
                else if (sample == Mathf.Clamp(sample, 0.6f, 0.7f))
                    pix[(int)y * noiseTex.width + (int)x] = grass;
                else if (sample == Mathf.Clamp(sample, 0.7f, 0.8f))
                    pix[(int)y * noiseTex.width + (int)x] = forest;
                else if (sample == Mathf.Clamp(sample, 0.8f, 1f))
                    pix[(int)y * noiseTex.width + (int)x] = mountains;
                else
                    pix[(int)y * noiseTex.width + (int)x] = water;

                x++;
            }
            y++;
        }
        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.SetPixels(pix);
        noiseTex.Apply();
        this.GetComponent<MeshRenderer>().material.mainTexture = noiseTex;
    }

    public void CellularTex(){
        currentTexture = TextureTypes.Cellular;
        // Set up the texture and a Color array to hold pixels during processing.
        noiseTex = new Texture2D(pixWidth, pixHeight);
        Color[] pix = new Color[noiseTex.width * noiseTex.height];
        float randomorg = UnityEngine.Random.Range(0, 100);

        Color greyScale = new Color(0, 0, 0);


        // For each pixel in the texture...
        float y = 0.0F;
        while (y < noiseTex.height)
        {
            float x = 0.0F;
            while (x < noiseTex.width)
            {

                float2 currentPoint = float2(x,y);
                float xCoord = randomorg + x / noiseTex.width * scale;
                float yCoord = randomorg + y / noiseTex.height * scale;
                currentPoint = float2(xCoord, yCoord);
                
                
                // berg på flera oktaver
                float tot = 0.0f;
                float frq = 1.0f;
                float amp = 1.0f;
                float sample = 0.0f;
                float noiseXY = 0.0f;
                for (int i = 0; i < 5; i++){
                    noiseXY = noise.cellular(currentPoint * frq).x * noise.cellular(currentPoint * frq).y;
                    sample += noiseXY * amp;
                    tot += amp;
                    amp *= 0.5f;
                    frq *= 2.0f;
                }
                sample /= tot;
                
                // havsnivå
                if (sample < 0.33f) sample = 0.0f;
                greyScale = new Color(sample,sample,sample);

                pix[(int)y * noiseTex.width + (int)x] *= grass * greyScale;
                if (sample == 0.0f) pix[(int)y * noiseTex.width + (int)x] = water;


                x++;
            }
            y++;
        }

        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.SetPixels(pix);
        noiseTex.Apply();


        this.GetComponent<MeshRenderer>().material.mainTexture = noiseTex;
    }



    // typ baserad på https://youtu.be/4066MndcyCk
    public void Worley(){
        noiseTex = new Texture2D(pixWidth, pixHeight);
        Color[] pix = new Color[noiseTex.width * noiseTex.height];
        

        // Strö ut worley points
        int numPoints = 100; // kinda hårdkodat till 100 just nu
        WorleyPoint[] points = new WorleyPoint[numPoints];
        float increment = 1f / sqrt(numPoints);
        int i2 = 0;
        for (float yf = 0f; yf < 1f; yf += increment) {
            for (float xf = 0f; xf < 1f; xf += increment) {

                float xRand = UnityEngine.Random.Range(xf, xf + increment);
                float yRand = UnityEngine.Random.Range(yf, yf + increment);
                float zRand = UnityEngine.Random.Range(0f, 1f);
                Vector3 position = new Vector3(xRand, yRand, zRand);

                // Random färg
                Color color = Color.HSVToRGB(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0.5f, 1f), 1f);

                // Klumpa samman celler
                if (UnityEngine.Random.Range(0f, 1f) < 0.75f) {
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5f){ // 50% chans att kopiera punkten ovanför
                        if (i2 - 10 >= 0) {
                            color = points[i2 - 10].col;
                            position.z = points[i2 - 10].pos.z;
                        }
                    }
                    else {
                        if (i2 - 1 >= 0) { // 50% chans att kopiera punkten bredvid
                            color = points[i2 - 1].col;
                            position.z = points[i2 - 1].pos.z;
                        }
                    } 
                }

                // Gör kantpunkterna svarta, ser typ ut som hav.
                if ((xf + increment >= 1f || xf == 0f) || (yf + increment >= 1f || yf == 0f)) { 
                    color = new Color(0f, 0f, 0f);
                }

                points[i2] = new WorleyPoint(position,color);  
                i2++;
            }
        }
        
        float y = 0.0F;
        while (y < noiseTex.height)
        {
            float x = 0.0F;
            while (x < noiseTex.width)
            {
                float[] distances = new float[points.Length];
                WorleyPoint[] sortedPoints = points;
                
                // Hitta närmsta punkt
                float xCoord = x / noiseTex.width;
                float yCoord = y / noiseTex.height;
                Vector2 curPoint = new Vector2(xCoord, yCoord);
                int j = 0;
                foreach(WorleyPoint p in points){
                    float dis = Vector2.Distance(p.pos,curPoint);
                    distances[j] = dis * 5f;
                    j++;
                }
                Array.Sort(distances, sortedPoints);

                float sample = (1f - distances[0]);
                pix[(int)y * noiseTex.width + (int)x] = new Color(sample,sample,sample); // shade:ad med avstånd till worleypunkten
                pix[(int)y * noiseTex.width + (int)x] *= sortedPoints[0].col; // cellerna färgas enligt worleypunktens färg

                x++;
            }
            y++;
        }

        // high pass filter???? sänk upplösningen för att det ska inte ta en evighet

        // y = 0.0F;
        // while (y < noiseTex.height)
        // {
        //     float x = 0.0F;
        //     while (x < noiseTex.width)
        //     {
        //         Color col = pix[(int)y * noiseTex.width + (int)x];
        //         Color cxp1 = new Color(0f,0f,0f);
        //         Color cxm1 = new Color(0f,0f,0f);
        //         Color cyp1 = new Color(0f,0f,0f);
        //         Color cym1 = new Color(0f,0f,0f);
        //         if ((int)y * noiseTex.width + ((int)x+1) < pix.Length)
        //             cxp1 = pix[(int)y * noiseTex.width + ((int)x+1)];
        //         if ((int)y * noiseTex.width + ((int)x-1) > 0)
        //             cxm1 = pix[(int)y * noiseTex.width + ((int)x-1)];
        //         if (((int)y+1) * noiseTex.width + (int)x+1 < pix.Length)    
        //             cyp1 = pix[((int)y+1) * noiseTex.width + (int)x+1];
        //         if (((int)y-1) * noiseTex.width + (int)x+1 > 0)                 
        //             cym1 = pix[((int)y-1) * noiseTex.width + (int)x+1];
        //         col += cxp1;
        //         col += cxm1;
        //         col += cyp1;
        //         col += cym1;
        //         col /= 5f;
        //         pix[(int)y * noiseTex.width + (int)x] -= col;

        //         x++;
        //     }
        //     y++;
        // }

        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.SetPixels(pix);
        noiseTex.Apply();
        this.GetComponent<MeshRenderer>().material.mainTexture = noiseTex;
    }
}
