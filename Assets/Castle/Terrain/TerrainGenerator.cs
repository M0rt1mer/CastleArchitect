using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Jobs;
using Unity.Collections;
using System.Linq;
using UnityEngine.Assertions;

[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour {

    public TerrainGeneratorData terrainGeneratorData { get; private set; }
    public WaterErosion waterErosion { get; private set; }
    public ThermalErosion thermalErosion { get; private set; }

    private Terrain terrainComponent;
    private Terrain waterTerrainComponent;

    void Start() {
        terrainComponent = GetComponent<Terrain>();
        waterTerrainComponent = transform.GetChild( 0 ).GetComponent<Terrain>();
    }

    public void GenerateHeightMap() {
        float[,] heightmap = SquareDiamondNoise.BetterSquareDiamondNoise( terrainGeneratorData.size );
        terrainGeneratorData.ReplaceData( heightmap );
    }

    public void WaterStep() {
        SimulationStep( true, waterErosion );
    }

    public void ThermalStep() {
        SimulationStep( true, thermalErosion );
    }

    public void Initialize( int numSteps, int resolution ) {
        DisposeOfResources();
        terrainGeneratorData = new TerrainGeneratorData( resolution );
        waterErosion = new WaterErosion( terrainGeneratorData );
        thermalErosion = new ThermalErosion( terrainGeneratorData );
    }

    public void UpdateTerrain() {

        AssignHeighmap( terrainComponent, terrainGeneratorData.heightmap, terrainGeneratorData.size );
        AssignHeighmap( waterTerrainComponent, waterErosion.waterTerrainHeight, terrainGeneratorData.size );

    }

    private static void AssignHeighmap( Terrain component, NativeArray<float> data, int size )
    {
        Vector3 sizeBackup = component.terrainData.size;

        float[,] heightmap = new float[size, size];
        Buffer.BlockCopy( data.ToArray(), 0, heightmap, 0, size * size * sizeof( float ) );
        component.terrainData.heightmapResolution = size;
        component.terrainData.SetHeights( 0, 0, heightmap );

        component.terrainData.size = sizeBackup;
    }

    public void OnBeforeSerialize() {
        DisposeOfResources();
    }

    private void DisposeOfResources() {
        if(terrainGeneratorData != null)
            terrainGeneratorData.Dispose();
        if(waterErosion != null)
            waterErosion.Dispose();
    }

    public void OnAfterDeserialize() {
        if(terrainGeneratorData != null) {
            thermalErosion = new ThermalErosion( terrainGeneratorData );
            waterErosion = new WaterErosion( terrainGeneratorData );
        }
    }

    public void SimulationStep() {
        SimulationStep( true, thermalErosion, waterErosion );
    }

    /// <summary>
    /// Runs a single disposable
    /// </summary>
    /// <param name="steps"></param>
    /// <param name="dispose"></param>
    /// <returns></returns>
    private DisposableJobHandle SimulationStep( bool dispose = true, params ITerrainGeneratorStep[] steps ) {

        DisposableJobHandle handle = ScheduleAddOutputs( steps );
        Assert.IsNotNull( handle );

        handle.job.Complete();
        if(dispose)
            handle.Dispose();
        UpdateTerrain();

        return handle;
        
    }

    private DisposableJobHandle ScheduleAddOutputs( params ITerrainGeneratorStep[] steps ) {

        Assert.IsTrue( steps.Length <= 2, "Too many erosion steps, not supported" );

        int totalJopSize = terrainGeneratorData.size * terrainGeneratorData.size;

        NativeArray<float>[] partialHeightmaps = steps.Select( x => new NativeArray<float>( totalJopSize, Allocator.Temp ) ).ToArray(); //prepare temporary heightmaps
        DisposableJobHandle[] handles = steps.Zip( partialHeightmaps, ( fnc, hmp ) => fnc.Step( hmp ) ).ToArray(); // call Step on all erosion steps

        switch(steps.Length) {
            case 1:
                return new AddSingleOutput() {
                    output = partialHeightmaps[0],
                    heighmap = terrainGeneratorData.heightmap
                }.Schedule( totalJopSize, 512, handles[0].job ).LinkResources( handles, partialHeightmaps.AttachDebugInfo("Partial heightmap {0}") ) ;
            case 2:
                return new AddTwoOutputs() {
                    output1 = partialHeightmaps[0],
                    output2 = partialHeightmaps[1],
                    heighmap = terrainGeneratorData.heightmap
                }.Schedule( totalJopSize, 512, JobHandle.CombineDependencies(handles[0].job, handles[1].job ) ).LinkResources( handles, partialHeightmaps.AttachDebugInfo( "Partial heightmap {0}" ) );
        }

        return null;
    }

    private struct AddSingleOutput : IJobParallelFor {

        [ReadOnly]
        public NativeArray<float> output;

        public NativeArray<float> heighmap;

        public void Execute( int index ) {
            heighmap[index] += output[index];
        }
    }

    private struct AddTwoOutputs : IJobParallelFor {

        [ReadOnly]
        public NativeArray<float> output1;
        [ReadOnly]
        public NativeArray<float> output2;

        public NativeArray<float> heighmap;

        public void Execute( int index ) {
            heighmap[index] += output1[index] + output2[index];
        }
    }

}

public interface ITerrainGeneratorStep {

    DisposableJobHandle Step( NativeArray<float> heighmapChanges);

}