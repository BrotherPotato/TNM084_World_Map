
using UnityEngine;


public class PerlinNoise : MonoBehaviour
{

    
    public int pixWidth = 800;
    public int pixHeight = 800;
    Texture2D noiseTex = new Texture2D(800,800);
    public float scale = 1.0f;
    public Color water = new Color(0.0f, 0.0f, 1.0f, 1.0f);
    public Color sand = new Color(0.9f, 0.8f, 0.5f, 1.0f);
    public Color grass = new Color(0.1f, 1.0f, 0.1f, 1.0f);
    public Color forest = new Color(0.4f, 1.0f, 0.4f, 1.0f);
    public Color mountains = new Color(0.6f, 0.5f, 0.4f, 1.0f);
    
    void Start()
    {
       
    }
    
    
}
