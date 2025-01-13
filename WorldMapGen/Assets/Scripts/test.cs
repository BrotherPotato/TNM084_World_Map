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
    float scale = 5f;
    public Color water = new Color(0.0f, 0.0f, 1.0f, 1.0f);
    public Color sand = new Color(0.9f, 0.8f, 0.5f, 1.0f);
    public Color grass = new Color(0.1f, 1.0f, 0.1f, 1.0f);
    public Color forest = new Color(0.4f, 1.0f, 0.4f, 1.0f);
    public Color mountains = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    public float seaLevel = 0.35f;

    enum TextureTypes {Map, Cellular}
    private TextureTypes currentTexture = TextureTypes.Map;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] MapGrid mesh;
    [SerializeField] MapGridTile meshes;
    public Material mapMat;

    Texture2D[] texs;

    void Start(){

        texs = Worley();
        //this.GetComponent<MeshRenderer>().material.mainTexture = texs[0];

        mapMat.mainTexture = texs[2];
        float[] heightMap = readTexture(texs[0]);
        mesh.ApplyHeight(heightMap);

        //meshes.ApplyHeight(heightMap);
    }

    public void refresh() {texs = Worley();}

    public void showHeight() {
        currentTexture = TextureTypes.Map;
        this.GetComponent<MeshRenderer>().material.mainTexture = texs[0];
    }
    public void showCell() {
        currentTexture = TextureTypes.Cellular;
        this.GetComponent<MeshRenderer>().material.mainTexture = texs[2];
    }

    float[] readTexture(Texture2D tex){

        int meshWidth = mesh.xSize; // för single grid
        //int meshWidth = meshes.tileSize; // för tiles, funkar inte just nu

        float[] heightMap = new float[(meshWidth + 1) * (meshWidth + 1)];
        for (int i = 0, z = 0; z <= meshWidth; z++) {
			for (int x = 0; x <= meshWidth; x++, i++) {

                float xrat = (float)x / (float)meshWidth;
                float zrat = (float)z / (float)meshWidth;
                float xx = pixHeight * xrat;
                float zz = pixHeight * zrat;

				heightMap[i] = tex.GetPixel((int)xx,(int)zz).maxColorComponent * 10f; // svårt att välja rätt höjd
			}
		}
        return heightMap;
    }

    // denna används bara i scenen där man testar noiset
    public void UpdateScale(){
        scale = GameObject.Find("Scale Slider").GetComponent<Slider>().value;
        refresh();
        if(currentTexture == TextureTypes.Map){
            showHeight();
        }
        if(currentTexture == TextureTypes.Cellular){
            showCell();
        }
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
            // points[i].pos.x = (points[i].pos.x + points[secondIndex].pos.x) / 2f; // detta kan fucka med positioneringen av celler
            // points[i].pos.y = (points[i].pos.y + points[secondIndex].pos.y) / 2f;
            points[i].pos.z = points[secondIndex].pos.z;
        }
        return points;
    }

    Texture2D biomes(Color[] heights) {

        Texture2D biomes = new Texture2D(pixWidth, pixHeight);
        Color[] pix = new Color[biomes.width * biomes.height];

        float y = 0.0F;
        while (y < biomes.height)
        {
            // De zoner då vinden istället går från vänster till höger.
            // Förenklat, nu byter den bara håll vid ekvatorn.
            bool switchWindDirection = y < biomes.height / 2f;

            float rain = 1f; // startmängd regn

            float x = 0.0F; // börja från höger
            if (switchWindDirection) x = biomes.width-1f; // börja från vänster
            while ((!switchWindDirection && x < biomes.width)||(switchWindDirection && x >= 0f))
            {                
                float yCoord = y / biomes.height;

                float altitude = heights[(int)y * biomes.width + (int)x].maxColorComponent;
                bool aboveWater = altitude == 0f;

                // Temperatur.  
                float temp = Mathf.Clamp(1f - Mathf.Abs(yCoord - 0.5f), 0f, 1f); // Ju närmre mitten, ju varmare.
                temp *= Mathf.Clamp(1f - (altitude*altitude), 0f, 1f); // Ju högre höjd, ju kallare

                // Regn
                float rainFall = 0f;
                if (aboveWater && rain < 1f) rainFall = -0.1f; // rechargea regn från havet
                else {
                    rainFall = altitude; // Ta bort regn med höjd.
                    rainFall /= 10f; 
                }
                rainFall *= (1f+temp); // Ju varmare, ju mer avdunsting
                rainFall *= 200f / biomes.width; // konstanterna anpassades för 200x200... detta är då för att skala om
                rain -= rainFall;

                // Färglägg
                Color rainCol = new Color(1f-rain, 0f,rain); 
                Color tempCol = new Color(temp, 0f, 1f-temp);
                
                // Temperaturzoner
                if (temp < 0.75f) 
                    tempCol = new Color(0f,0.75f,1f); // Kallt, blå
                else if (temp < 0.9f) 
                    tempCol = new Color(0f, 1f, 0f); // Lagom, grön
                else 
                    tempCol = new Color(1f, 0.5f, 0f); // Hett, orange
                
                // Blöthetszoner
                if (rain < 0.2f)
                    rainCol = new Color(1f,0.9f,0.4f); // Torrt, gul
                else if (rain < 0.9f) 
                    rainCol = new Color(0f,1f,0f); // Lagom, grön
                else
                    rainCol = new Color(0f,0.5f,0.27f); // Blött, blågrön

                Color biomeCol = (rainCol + tempCol) / 2f; // kombinera så får du biomer, YIPPIE!
                pix[(int)y * biomes.width + (int)x] = biomeCol;
                if (aboveWater) pix[(int)y * biomes.width + (int)x] *= 0f; // svart vatten

                if (switchWindDirection) x--; // gå baklänges
                else x++; // gå framlänges
            }
            y++;
        }
        biomes.SetPixels(pix);
        biomes.Apply();

        return biomes;
    }

    // typ baserad på https://youtu.be/4066MndcyCk
    public Texture2D[] Worley(){

        // Height mappen
        Texture2D heightTex = new Texture2D(pixWidth, pixHeight);
        Color[] heightPix = new Color[heightTex.width * heightTex.height];

        // Färga pixlarna
        Texture2D cellTex = new Texture2D(pixWidth, pixHeight);
        Color[] cellPix = new Color[cellTex.width * cellTex.height];
        
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
                    points[i2].pos.z = 0.01f;
                }
                i2++;
            }
        }
        
        // Färglägg pixlarna
        float y = 0.0F;
        while (y < heightTex.height)
        {
            float x = 0.0F;
            while (x < heightTex.width)
            {
                float[] distances = new float[points.Length];
                WorleyPoint[] sortedPoints = points;
                
                // Hitta närmsta punkt
                float xCoord = x / heightTex.width;
                float yCoord = y / heightTex.height;
                Vector2 curPoint = new Vector2(xCoord, yCoord);
                int j = 0;
                foreach(WorleyPoint p in points){
                    float dis = Vector2.Distance(p.pos,curPoint) * 5f; // dis är för lågt
                    distances[j] = Mathf.Clamp(dis,0f,1f); 
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

                sample *= sqrt(val); // platta till kontinenter
                if (sample < seaLevel) sample = 0f; // separera landmassor från havsbotten
                sample = pow(sample,2f); // förstärk bergen

                // Denna kommer användas som height map
                Color pixelCol = new Color(sample,sample,sample); 
                heightPix[(int)y * heightTex.width + (int)x] = pixelCol;

                // Färgläggning för meshet WIP
                Color col2 = sortedPoints[0].col; 
                cellPix[(int)y * cellTex.width + (int)x] =  col2;

                x++;
            }
            y++;
        }

        // Copy the pixel data to the texture and load it into the GPU.
        heightTex.SetPixels(heightPix);
        heightTex.Apply();
        cellTex.SetPixels(cellPix);
        cellTex.Apply();

        Texture2D biomeTex = biomes(heightPix);

        // Vi skickar en array med olika texturer från noiset.
        Texture2D[] texs = {heightTex, cellTex, biomeTex};
        return texs;
        
    }
}
