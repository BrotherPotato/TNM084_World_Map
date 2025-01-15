using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;

// https://catlikecoding.com/unity/tutorials/procedural-grid/
// early check transform

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MapGrid : MonoBehaviour
{
    public int xSize, zSize;
	public float scale = 0.5f;

    private Vector3[] vertices;

	public Material mapMat;

    private Mesh mesh;
	private Vector3[] origVertices;

    private void Awake () {
        //Generate();
        Generate();
		origVertices = mesh.vertices;
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
        if (vertices == null) {
			return;
		}
		Gizmos.color = Color.black;
		for (int i = 0; i < vertices.Length; i++) {
			Gizmos.DrawSphere(vertices[i], 0.1f);
		}
	}

	private void Generate () {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Grid";

		vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
		Vector4 tangent = new Vector4(0f, 1f, 0f, -1f);

		for (int i = 0, z = 0; z <= zSize; z++) {
			for (int x = 0; x <= xSize; x++, i++) {
				vertices[i] = new Vector3(x * scale, 0, z * scale);
                uv[i] = new Vector2((float)x / xSize, (float)z / zSize);
                tangents[i] = tangent;
			}
		}
		mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.tangents = tangents;

		int[] triangles = new int[xSize * zSize * 6];
		for (int ti = 0, vi = 0, z = 0; z < zSize; z++, vi++) {
			for (int x = 0; x < xSize; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
				triangles[ti + 5] = vi + xSize + 2;
			}
		}
		mesh.triangles = triangles;
        mesh.RecalculateNormals();
	}

	private float[] GenerateRandomHeightMap(){
		float[] heightMap = new float[(xSize + 1) * (zSize + 1)];
		for (int i = 0, z = 0; z <= zSize; z++) {
			for (int x = 0; x <= xSize; x++, i++) {
				heightMap[i] = Random.Range(0f, 1f);
			}
		}
		return heightMap;
	}

	public void ApplyHeight(float[] heightMap){

		for (int i = 0, z = 0; z <= zSize; z++) {
			for (int x = 0; x <= xSize; x++, i++) {
				vertices[i] = origVertices[i] + new Vector3(0, heightMap[i], 0);
			}
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
	}

	// private void GenerateMaterial(){
	// 	MeshRenderer meshR = GetComponent<MeshRenderer>();
	// 	UnityEngine.Debug.Log(meshR);
	// 	Texture2D mapTexture = new Texture2D(xSize, zSize);
		

	// 	for (int x = 0; x < xSize; x++)
	// 	{
	// 		for (int z = 0; z < zSize; z++)
	// 		{
	// 			mapTexture.SetPixel(x,z, new Color(1,1,0));
	// 		}
	// 	}
	// 	mapTexture.Apply();
	// 	mapMat.mainTexture = mapTexture;
	// }


}
