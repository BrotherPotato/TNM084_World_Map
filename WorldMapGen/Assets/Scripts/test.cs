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
    public Color col; // färg används för att särskilja kluster av celler

}


public class test : MonoBehaviour
{
    
    [SerializeField]
    NoiseScript noiseSc;
    //public GameObject plane;
    public int pixWidth = 800;
    public int pixHeight = 800;
    float scale = 1.0f;
    public Color water = new Color(0.0f, 0.0f, 1.0f, 1.0f);
    public Color sand = new Color(0.9f, 0.8f, 0.5f, 1.0f);
    public Color grass = new Color(0.1f, 1.0f, 0.1f, 1.0f);
    public Color forest = new Color(0.4f, 1.0f, 0.4f, 1.0f);
    public Color mountains = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    enum TextureTypes {Map, Cellular}
    private TextureTypes currentTexture = TextureTypes.Map;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] MapGrid mesh;
    public Material mapMat;

    float[] heightMap;
    void Start(){

        Texture2D[] texs = Worley();
        //this.GetComponent<MeshRenderer>().material.mainTexture = texs[1];

        mapMat.mainTexture = texs[1];
        heightMap = readTexture(texs[0]);
        mesh.ApplyHeight(heightMap);
    }

    float[] readTexture(Texture2D tex){

        // hur överför man koordinaterna för 800x800 till meshets lägre upplösning ????

        float[] heightMap = new float[(mesh.xSize + 1) * (mesh.zSize + 1)];
        for (int i = 0, z = 0; z <= mesh.zSize; z++) {
			for (int x = 0; x <= mesh.xSize; x++, i++) {
                int xx = (pixHeight * x) / mesh.xSize; // eftersom texturen är högre resolution
                int zz = (pixHeight * z) / mesh.xSize;
				heightMap[i] = tex.GetPixel(xx,zz).maxColorComponent * 25f; // svårt att välja rätt höjd
			}
		}
        return heightMap;
    }

    // denna används bara i scenen där man testar noiset
    public void UpdateScale(){
        scale = GameObject.Find("Scale Slider").GetComponent<Slider>().value;
        Texture2D noiseTex = Worley()[0];
        this.GetComponent<MeshRenderer>().material.mainTexture = noiseTex;
    }

    WorleyPoint[] ScatterPoints(int pointsPerRow) {

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
                //if (zRand < 0.1f) color = new Color(0f,0f,0f); // chans att vara vatten
                
                points[i2] = new WorleyPoint(position,color);  
                i2++;
            }
        }
        return points;
    }

    WorleyPoint[] ClusterPoints(WorleyPoint[] points) {
        int pointsPerRow = (int)sqrt(points.Length);
        for (int i = 0; i < points.Length; i++) {
            int secondIndex = i;
            if (UnityEngine.Random.Range(0f, 1f) < 1f) { // chans att klumpa med granne
                if (UnityEngine.Random.Range(0f, 1f) < 0.5f){ // 50% chans att klumpa med punkten ovanför eller under
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5f) { // över
                        if (i - pointsPerRow >= 0) secondIndex = i - pointsPerRow;
                   }
                    else { // under
                        if (i + pointsPerRow < points.Length) secondIndex = i + pointsPerRow;
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
        return points;
    }

    // typ baserad på https://youtu.be/4066MndcyCk
    public Texture2D[] Worley(){

        Texture2D noiseTex = new Texture2D(pixWidth, pixHeight);
        Texture2D cellTex = new Texture2D(pixWidth, pixHeight);
        Color[] pix = new Color[noiseTex.width * noiseTex.height];
        Color[] pix2 = new Color[noiseTex.width * noiseTex.height];
        float randomorg = UnityEngine.Random.Range(0, 100);

        // Strö ut worley points
        int pointsPerRow = 10;
        WorleyPoint[] points = ScatterPoints(pointsPerRow);
        points = ClusterPoints(points);

        // Gör ramen till hav
        int i2 = 0;
        float increment = 1f / (float)pointsPerRow;
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
                    if ((sortedPoints[0].col.r != sortedPoints[i].col.r ||
                        sortedPoints[0].col.g != sortedPoints[i].col.g ||
                        sortedPoints[0].col.b != sortedPoints[i].col.b)) {
                        j = i;
                        break;
                    }
                }
                float val = (1f - distances[j]) * 2f;

                // berg på flera oktaver
                float sample = 0.0f;
                float amp = sortedPoints[0].pos.z; // varje cell har ett z-värde mellan 0-1
                float tot = 0.0f;
                float frq = 1.0f;
                float2 currentPoint = float2((xCoord + randomorg) * scale, (yCoord + randomorg) * scale);
                for (int i = 0; i < 5; i++){
                    sample += amp * noise.cellular(currentPoint * frq).x * noise.cellular(currentPoint * frq).y;
                    tot += amp;
                    amp *= 0.5f;
                    frq *= 2.0f;
                }
                sample /= tot;

                sample *= sqrt(val);
                if (sample < 0.25f) sample = 0.25f; 
                sample = (sample - 0.25f) / (1f - 0.25f);
                sample = pow(sample,2f);

                // Denna kommer användas som height map
                Color pixelCol = new Color(sample,sample,sample); 
                pix[(int)y * noiseTex.width + (int)x] = pixelCol;

                // Färgläggning för meshet WIP
                Color col2 = sortedPoints[0].col; 
                pix2[(int)y * noiseTex.width + (int)x] = col2 * pixelCol * 10f;

                x++;
            }
            y++;
        }

        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.SetPixels(pix);
        noiseTex.Apply();
        cellTex.SetPixels(pix2);
        cellTex.Apply();

        Texture2D[] texs = {noiseTex, cellTex};
        return texs;
        
    }
}
