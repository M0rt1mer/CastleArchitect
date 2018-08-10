using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour {

    public TerrainGeneratorData terrainGeneratorData { get; private set; }
    public WaterErosion waterErosion { get; private set; }
    public ThermalErosion thermalErosion { get; private set; }

    private Terrain terrainComponent;

    void Start () {
        terrainComponent = GetComponent<Terrain>();
	}

    public void GenerateHeightMap( int numSteps )
    {
        //terrainComponent = GetComponent<Terrain>();
        //int heightMapResolution = terrainComponent.terrainData.heightmapResolution;

        float[,] heightmap = SquareDiamondNoise.BetterSquareDiamondNoise( terrainGeneratorData.heighmap.GetLength(0) );
        Array.Copy( heightmap, terrainGeneratorData.heighmap, heightmap.GetLength(0)*heightmap.GetLength(1) );
        //terrainComponent.terrainData.SetHeights( 0, 0, heightmap );
    }

    public void WaterStep() {
        waterErosion.Waterstep();
    }

    public void ThermalStep() {
        thermalErosion.Step();
    }


    public void Initialize( int numSteps, int resolution ) {
        terrainGeneratorData = new TerrainGeneratorData(resolution);
        waterErosion = new WaterErosion( terrainGeneratorData );
        thermalErosion = new ThermalErosion(terrainGeneratorData);
    }

    public void UpdateTerrain() {

        terrainComponent.terrainData.heightmapResolution = terrainGeneratorData.heighmap.GetLength( 0 );
        terrainComponent.terrainData.SetHeights( 0, 0, terrainGeneratorData.heighmap );
    }

}
