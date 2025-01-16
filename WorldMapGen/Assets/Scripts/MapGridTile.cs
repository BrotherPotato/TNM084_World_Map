using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;

// https://catlikecoding.com/unity/tutorials/procedural-grid/
// early check transform

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MapGridTile : MonoBehaviour
{
    public int tileSize;
	private int xTileSize, zTileSize;
	public int numberOfTiles = 3;
	public float tileScale = 0.5f;
	private Mesh[] meshes;
    private Vector3[][] tileVertices;
	private Vector3[][] origVertices;

    

    private void Awake () {
        //Generate();
		xTileSize = zTileSize = tileSize;
		meshes = new Mesh[numberOfTiles*numberOfTiles];
		tileVertices = new Vector3[meshes.Length][];
		origVertices = new Vector3[meshes.Length][];
        GenerateTiles();
		for (int tile = 0; tile < origVertices.Length; tile++) { // go through each tile
			origVertices[tile] = meshes[tile].vertices;
		}
		//ApplyHeight(GenerateRandomHeightMap());
    }

	// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //GenerateMaterial();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	// 800 x 800 är för mycket för gizmos
    private void OnDrawGizmos () {
        if (tileVertices == null) {
			return;
		}
		Gizmos.color = Color.black;
		for (int tile = 0; tile < meshes.Length; tile++) { // go through all tiles
			for (int i = 0; i < tileVertices[tile].Length; i++) { // go through each vertex in each tile
				Gizmos.DrawSphere(tileVertices[tile][i], 0.1f);
			}
		}	
	}

	private void GenerateTile (int tileIndex, float xStartPos, float zStartPos, float uvXstart, float uvZstart) {
		// create empty object
		GameObject newTile = new GameObject("Tile nr:" + tileIndex);
		// add components
		newTile.AddComponent<MeshFilter>();
		newTile.AddComponent<MeshRenderer>();
		// set tile parent to Map
		newTile.transform.SetParent(gameObject.transform);
		// create new mesh

		newTile.GetComponent<MeshFilter>().mesh = meshes[tileIndex] = new Mesh();
		meshes[tileIndex].name = "Procedural GridTile";

		tileVertices[tileIndex] = new Vector3[(xTileSize + 1) * (zTileSize + 1)];
        Vector2[] uv = new Vector2[tileVertices[tileIndex].Length];
        Vector4[] tangents = new Vector4[tileVertices[tileIndex].Length];
		Vector4 tangent = new Vector4(0f, 1f, 0f, -1f);

		

		for (int i = 0, z = 0; z <= zTileSize; z++) {
			for (int x = 0; x <= xTileSize; x++, i++) {
				tileVertices[tileIndex][i] = new Vector3(xStartPos + x * tileScale, 0, zStartPos + z * tileScale);
                Vector2 uv01 = new Vector2((float)x / tileSize, (float)z / tileSize);
				uv[i] = new Vector2(uvXstart + uv01.x / numberOfTiles, uvZstart + uv01.y / numberOfTiles);
                tangents[i] = tangent;
			}
		}
		meshes[tileIndex].vertices = tileVertices[tileIndex];
        meshes[tileIndex].uv = uv;
        meshes[tileIndex].tangents = tangents;

		int[] triangles = new int[xTileSize * zTileSize * 6];
		for (int ti = 0, vi = 0, z = 0; z < zTileSize; z++, vi++) {
			for (int x = 0; x < xTileSize; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xTileSize + 1;
				triangles[ti + 5] = vi + xTileSize + 2;
			}
		}
		meshes[tileIndex].triangles = triangles;
        meshes[tileIndex].RecalculateNormals();

	}

	private void GenerateTiles(){
		int tileConter = 0;
		for (int tileZ = 0; tileZ < numberOfTiles; tileZ++) {
			for (int tileX = 0; tileX < numberOfTiles; tileX++) {
				float uvZstart = 0;
				float uvXstart = 0;
				if(tileZ != 0){
					uvZstart = tileZ / (float)numberOfTiles;
				}
				if(tileX != 0){
					uvXstart = tileX / (float)numberOfTiles;
				}
				float xHalf = ((xTileSize + 1) * numberOfTiles * tileScale) / 2;
				float zHalf = ((zTileSize + 1) * numberOfTiles * tileScale) / 2;


				float xStart = xTileSize * tileX * tileScale - xHalf;
				float zStart = zTileSize * tileZ * tileScale - zHalf;
				
				GenerateTile(tileConter, xStart, zStart, uvXstart, uvZstart);
				tileConter++;
			}
		}
	}

	public void ApplyHeight(float[] heightMap){
		//check size
		if(heightMap.Length != tileVertices.Length * tileVertices[0].Length - numberOfTiles){ // fixa sen
			heightMap = UpscaleHeightMap(heightMap);
		}

		int sampleSideLength = (int)Mathf.Sqrt(heightMap.Length);
		int vertPerTileSide = sampleSideLength / numberOfTiles + 1;
		int tileRow = 0;
		for (int tile = 0; tile < tileVertices.Length; tile++) { // go through each tile
			int tileCol = tile % numberOfTiles;
			if(tile % numberOfTiles == 0 && tile != 0){
				tileRow++;
			}

			for (int i = 0, z = 0; z <= zTileSize; z++) {
				for (int x = 0; x <= xTileSize; x++, i++) {
					int sampleRow = tileRow * vertPerTileSide + z - tileRow;
					int sampleCol = tileCol * vertPerTileSide + x - tileCol;
					int samplePoint = sampleCol * sampleSideLength + sampleRow;
					tileVertices[tile][i] = origVertices[tile][i] + new Vector3(0, heightMap[samplePoint], 0);
				}
			}

			meshes[tile].vertices = tileVertices[tile];
			meshes[tile].RecalculateNormals();
		}
	}

	private float[] GenerateRandomHeightMap(){
		//float[] heightMap = new float[(numberOfTiles * xTileSize +1) * (numberOfTiles * zTileSize +1)];
		float[] heightMap = new float[(numberOfTiles * xTileSize +1) + 50];
		int sideLength = (int) Mathf.Sqrt(heightMap.Length);
		int counter = 0;
		for (int i = 0, z = 0; z < sideLength; z++) {
			for (int x = 0; x < sideLength; x++, i++) {
				heightMap[i] = UnityEngine.Random.Range(0f, 3f);
				counter++;
			}
		}
		return heightMap;
	}

	public float[] UpscaleHeightMap(float[] heightMap){
		int heightMapSideLength = (int)Mathf.Sqrt(heightMap.Length);
		int upscaleSideLength = numberOfTiles * xTileSize + numberOfTiles - 1; // numberOfTiles * (xTileSize + 1) - numberOfTiles - 1;
		float scale = heightMapSideLength / (float)upscaleSideLength;
		
		float[] scaledHeightMap = new float[upscaleSideLength * upscaleSideLength];
		//UnityEngine.Debug.Log(scaledHeightMap.Length);
		// Bilinear interpolation is used (translated to code from wikipedia)
		// https://en.wikipedia.org/wiki/Bilinear_interpolation
		for (int zi = 0; zi < upscaleSideLength; zi++) {
			for (int xj = 0; xj < upscaleSideLength; xj++)
			{
				// Map upscaled coordinates to original matrix coordinates
                float x = xj * scale;
                float z = zi * scale;

				
				int x1 = (int)Math.Floor(x);
                int z1 = (int)Math.Floor(z);
                int x2 = Math.Min(x1 + 1, heightMapSideLength - 1);
                int z2 = Math.Min(z1 + 1, heightMapSideLength - 1);
				// Compute interpolation weights

				float w11 = (x2-x)*(z2-z) / ((x2-x1)*(z2-z1));
				float w12 = (x2-x)*(z-z1) / ((x2-x1)*(z2-z1));
				float w21 = (x-x1)*(z2-z) / ((x2-x1)*(z2-z1));
				float w22 = (x-x1)*(z-z1) / ((x2-x1)*(z2-z1));

				float value = w11 * heightMap[x1 * heightMapSideLength + z1] + w12 * heightMap[x1 * heightMapSideLength + z2] +
				w21 * heightMap[x2 * heightMapSideLength + z1] + w22 * heightMap[x2 * heightMapSideLength + z2];
			

				scaledHeightMap[xj * upscaleSideLength + zi] = value;
			}
		}
		
		//float[] nonPaddedScaledHeightMap = new float[numberOfTiles * (xTileSize + 1) * numberOfTiles * (xTileSize + 1)];
		//UnityEngine.Debug.Log(scaledHeightMap.Length);
		//UnityEngine.Debug.Log(nonPaddedScaledHeightMap.Length);
		//Array.Copy(scaledHeightMap, 0, nonPaddedScaledHeightMap, 0, numberOfTiles * (xTileSize + 1) * numberOfTiles * (xTileSize + 1));

		return scaledHeightMap;
	}

	private void GenerateMaterial(){	
		for (int tile = 0; tile < meshes.Length; tile++) {
			GameObject currentTile = transform.Find("Tile nr:" + tile).gameObject;

			MeshRenderer meshRend = currentTile.GetComponent<MeshRenderer>();
			//UnityEngine.Debug.Log(meshR);
			Texture2D mapTexture = new Texture2D(xTileSize+1, zTileSize+1);
			Color tileColor = new Color(UnityEngine.Random.Range(0f, 1f),UnityEngine.Random.Range(0f, 1f),UnityEngine.Random.Range(0f, 1f));
			for (int x = 0; x <= xTileSize; x++) {
				for (int z = 0; z <= zTileSize; z++) {
					mapTexture.SetPixel(x,z, tileColor);
				}
			}

			mapTexture.Apply();

			Material tileMaterial = new Material(Shader.Find("Shader Graphs/MapShader"));

			tileMaterial.SetTexture("_Texture2D", mapTexture);

			meshRend.material = tileMaterial;
		}
	}

	// https://stackoverflow.com/questions/56949217/how-to-resize-a-texture2d-using-height-and-width
	private Texture2D Resize(Texture2D texture2D,int targetX,int targetY)
    {
        RenderTexture rt=new RenderTexture(targetX, targetY,24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D,rt);
        Texture2D result=new Texture2D(targetX,targetY);
        result.ReadPixels(new Rect(0,0,targetX,targetY),0,0);
        result.Apply();
        return result;
    }

	public void LoadMaterial(Texture2D mapTexture){
		Vector2Int wantedSize = new Vector2Int(xTileSize+1, zTileSize+1);
		wantedSize *= 150;
		Texture2D temp = Resize(mapTexture, wantedSize.x, wantedSize.y);

		for (int tile = 0; tile < tileVertices.Length; tile++)
		{
			GameObject currentTile = transform.Find("Tile nr:" + tile).gameObject;
		 	MeshRenderer meshRend = currentTile.GetComponent<MeshRenderer>();

			temp.Apply();

		 	Material tileMaterial = new Material(Shader.Find("Shader Graphs/MapShader"));

		 	tileMaterial.SetTexture("_Texture2D", temp);

		 	meshRend.material = tileMaterial;
		}
	}

}
