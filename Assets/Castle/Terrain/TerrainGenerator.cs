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

    void Start() {
        terrainComponent = GetComponent<Terrain>();
    }

    public void GenerateHeightMap() {
        float[,] heightmap = SquareDiamondNoise.BetterSquareDiamondNoise( terrainGeneratorData.size );
        terrainGeneratorData.ReplaceData( heightmap );
    }

    public void WaterStep() {
        SimulationStep( waterErosion );
    }

    public void ThermalStep() {
        SimulationStep( thermalErosion );
    }

    public void Initialize( int numSteps, int resolution ) {
        DisposeOfResources();
        terrainGeneratorData = new TerrainGeneratorData( resolution );
        waterErosion = new WaterErosion( terrainGeneratorData );
        thermalErosion = new ThermalErosion( terrainGeneratorData );
    }

    public void UpdateTerrain() {

        Vector3 sizeBackup = terrainComponent.terrainData.size;

        float[,] heightmap = new float[terrainGeneratorData.size, terrainGeneratorData.size];
        Buffer.BlockCopy( terrainGeneratorData.heightmap.ToArray(), 0, heightmap, 0, terrainGeneratorData.size * terrainGeneratorData.size * sizeof( float ) );
        terrainComponent.terrainData.heightmapResolution = terrainGeneratorData.size;
        terrainComponent.terrainData.SetHeights( 0, 0, heightmap );

        terrainComponent.terrainData.size = sizeBackup;
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
        SimulationStep( thermalErosion, waterErosion );
    }

    private void SimulationStep( params ITerrainGeneratorStep[] steps ) {

        DisposableJobHandle handle = ScheduleAddOutputs( steps );
        Assert.IsNotNull( handle );

        handle.job.Complete();
        handle.Dispose();
        UpdateTerrain();
        
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
                }.Schedule( totalJopSize, 512, handles[0].job ).LinkResources( handles, partialHeightmaps.Cast<IDisposable>() ) ;
            case 2:
                return new AddTwoOutputs() {
                    output1 = partialHeightmaps[0],
                    output2 = partialHeightmaps[1],
                    heighmap = terrainGeneratorData.heightmap
                }.Schedule( totalJopSize, 512, JobHandle.CombineDependencies(handles[0].job, handles[1].job ) ).LinkResources( handles, partialHeightmaps.Cast<IDisposable>() );
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