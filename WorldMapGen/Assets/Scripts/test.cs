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
        float randomorg = UnityEngine.Random.Range(0, 100);

        // Strö ut worley points
        int pointsPerRow = 10; // antalet punkter är dennas kvadrat
        WorleyPoint[] points = new WorleyPoint[pointsPerRow * pointsPerRow];
        float increment = 1f / (float)pointsPerRow;
        int i2 = 0;
        for (float yf = 0f; yf < 1f; yf += increment) {
            for (float xf = 0f; xf < 1f; xf += increment) {

                float xRand = UnityEngine.Random.Range(xf, xf + increment);
                float yRand = UnityEngine.Random.Range(yf, yf + increment);
                float zRand = UnityEngine.Random.Range(0f, 1f);
                Vector3 position = new Vector3(xRand, yRand, zRand);

                // Random färg
                Color color = Color.HSVToRGB(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 0.5f), 1f);
                if (zRand < 0.1f) color = new Color(0f,0f,0f); // chans att vara vatten
                
                points[i2] = new WorleyPoint(position,color);  
                i2++;
            }
        }

        // Klumpa samman celler
        for (int i = 0; i < points.Length; i++) {
            int secondIndex = i;
            if (UnityEngine.Random.Range(0f, 1f) < 1f) { // chans att klumpa med granne
                if (UnityEngine.Random.Range(0f, 1f) < 0.5f){ // 50% chans att klumpa med punkten ovanför eller under
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5f) { // över
                        if (i - pointsPerRow >= 0) secondIndex = i - pointsPerRow;
                   }
                    else { // under
                        if (i2 + pointsPerRow < points.Length) secondIndex = i + pointsPerRow;
                    }            
                }
                else { // 50% chans att klumpa med punkten höger eller vänster
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5f) { // höger
                        if (i - 1 >= 0) secondIndex = i - 1;
                    }
                    else { // vänster
                        if (i + 1 < points.Length) secondIndex = i + 1;
                    }
                } 
                points[i].col = points[secondIndex].col;
                points[i].pos.z = points[secondIndex].pos.z;
            }
        }

        // Gör ramen svart
        i2 = 0;
        for (float yf = 0f; yf < 1f; yf += increment) {
            for (float xf = 0f; xf < 1f; xf += increment) {

                // Gör kantpunkterna svarta, ser typ ut som hav.
                if ((xf + (2*increment) >= 1f || xf <= increment) || (yf + (2*increment) >= 1f || yf <= increment)) { 
                    points[i2].col = new Color(0f, 0f, 0f);
                    points[i2].pos.z = 0.1f;
                }
                i2++;
            }
        }
        
        // Färglägg pixlarna
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
                    float dis = Vector2.Distance(p.pos,curPoint) * 5f; // dis är för lågt
                    distances[j] = dis; 
                    j++;
                }
                Array.Sort(distances, sortedPoints);

                // Leta efter närmsta punkt som inte tillhör den aktuella klumpen.
                j = 0;
                for (int i = 1; i < sortedPoints.Length; i++){
                    // Om det är samma färg. Gå vidare.
                    if ((sortedPoints[0].col.r == sortedPoints[i].col.r &&
                        sortedPoints[0].col.g == sortedPoints[i].col.g &&
                        sortedPoints[0].col.b == sortedPoints[i].col.b)) {
                        continue; 
                    }
                    // Om det är en ny färg eller vatten. Stanna.
                    else {
                        j = i;
                        break;
                    }
                }
                float val = (1f - distances[j]) * 2f;

                // berg på flera oktaver
                float sample = 0.0f;
                float amp = sortedPoints[0].pos.z;
                float tot = 0.0f;
                float frq = 1.0f;
                float2 currentPoint = float2((xCoord + randomorg) * scale, (yCoord + randomorg) * scale);
                for (int i = 0; i < 5; i++){
                    float noiseXY = noise.cellular(currentPoint * frq).x * noise.cellular(currentPoint * frq).y;
                    sample += noiseXY * amp;
                    tot += amp;
                    amp *= 0.5f;
                    frq *= 2.0f;
                }
                sample /= tot;
                sample *= val;
                if (sample < 0.35f) sample = 0f;

                Color pixelCol = new Color(sample,sample,sample); 
                if (sample == 0f) pixelCol = new Color(0f, 0.5f, 1f);

                // För att visa klustrerna som overlays.
                //pixelCol = (pixelCol + sortedPoints[0].col) / 2f;
                
                pix[(int)y * noiseTex.width + (int)x] = pixelCol;
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
