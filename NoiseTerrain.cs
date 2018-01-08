using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class NoiseTerrain : MonoBehaviour {
    Terrain terrain;
    public NoiseBox noise;
    public float maxHeight = 0.01f;

	// Use this for initialization
	void Start () {
        terrain = GetComponent<Terrain>();
        float[,] heights = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];
        noise.Compute();
        for(int i=0; i < terrain.terrainData.heightmapWidth; i++)
        {
            for(int j = 0; j < terrain.terrainData.heightmapHeight; j++)
            {
                float x = i / (float)terrain.terrainData.heightmapResolution;
                float y = j / (float)terrain.terrainData.heightmapResolution;
                float value = noise.Resolve(x, y);
                heights[i, j] = value*maxHeight;
            }
        }
        terrain.terrainData.SetHeights(0, 0, heights);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
