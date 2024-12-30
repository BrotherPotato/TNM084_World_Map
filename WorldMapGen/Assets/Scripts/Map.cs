using UnityEngine;
using System.Collections;
using static Unity.Mathematics.math;
using Unity.Mathematics;
using UnityEngine.UI;
// noise functions
// https://docs.unity3d.com/Packages/com.unity.mathematics@1.3/api/Unity.Mathematics.noise.html

public class Map : MonoBehaviour
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
    public Color mountains = new Color(0.6f, 0.5f, 0.4f, 1.0f);

    enum TextureTypes {Map, Cellular}
    private TextureTypes currentTexture = TextureTypes.Map;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        noiseSc.Test();
        CreateMap();
    }

    // Update is called once per frame
    void Update()
    {
        
        //plane.GetComponent<MeshRenderer>
        
    }

    public void UpdateScale(){
        scale = GameObject.Find("Scale Slider").GetComponent<Slider>().value;
;
        Debug.Log(currentTexture);

        if(currentTexture == TextureTypes.Map){
            CreateMap();
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
        scale = scale * 5;

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

                // .x -> vanlig .y -> cool men skum
                Color greyScale = new Color(noise.cellular(currentPoint).x, 0, 1-noise.cellular(currentPoint).x);
                
                pix[(int)y * noiseTex.width + (int)x] = greyScale;


                x++;
            }
            y++;
        }
        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.SetPixels(pix);
        noiseTex.Apply();

        this.GetComponent<MeshRenderer>().material.mainTexture = noiseTex;
    }
}
