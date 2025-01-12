using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using Unity.VisualScripting;

// https://catlikecoding.com/unity/tutorials/procedural-grid/
// early check transform

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MapGridTile : MonoBehaviour
{
    public int xTileSize, zTileSize;
	public static int numberOfTiles = 3;
	public float tileScale = 0.5f;
	private Mesh[] meshes;
    private Vector3[][] tileVertices;

	public Material[] tileMaterial;

    

    private void Awake () {
        //Generate();
		meshes = new Mesh[numberOfTiles*numberOfTiles];
		tileVertices = new Vector3[meshes.Length][];
        GenerateTiles();
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

	private void GenerateTile (int tileIndex, float xStartPos, float zStartPos) {
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
                uv[i] = new Vector2((float)x / tileScale, (float)z / tileScale);
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
		for (int tileX = 0; tileX < numberOfTiles; tileX++) {
			for (int tileZ = 0; tileZ < numberOfTiles; tileZ++) {
				GenerateTile(tileConter, xTileSize * tileX * tileScale, zTileSize * tileZ * tileScale);
				tileConter++;
			}
		}
	}

	public void ApplyHeightTile(float[] heightMap){

	
	}
}
