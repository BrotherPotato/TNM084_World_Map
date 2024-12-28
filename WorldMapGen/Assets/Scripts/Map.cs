using UnityEngine;


public class Map : MonoBehaviour
{
    
    [SerializeField]
    NoiseScript noise;
    //public GameObject plane;
    public int pixWidth = 800;
    public int pixHeight = 800;
    Texture2D noiseTex;
    public float scale = 1.0f;
    public Color water = new Color(0.0f, 0.0f, 1.0f, 1.0f);
    public Color sand = new Color(0.9f, 0.8f, 0.5f, 1.0f);
    public Color grass = new Color(0.1f, 1.0f, 0.1f, 1.0f);
    public Color forest = new Color(0.4f, 1.0f, 0.4f, 1.0f);
    public Color mountains = new Color(0.6f, 0.5f, 0.4f, 1.0f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        noise.Test();
        CreateMap();
    }

    // Update is called once per frame
    void Update()
    {
        
        //plane.GetComponent<MeshRenderer>
        
    }

    public void CreateMap()
    {
        // Set up the texture and a Color array to hold pixels during processing.
        noiseTex = new Texture2D(pixWidth, pixHeight);
        Color[] pix = new Color[noiseTex.width * noiseTex.height];
        float randomorg = Random.Range(0, 100);

        // For each pixel in the texture...
        float y = 0.0F;


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
}
