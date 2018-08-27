using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class WaterErosion : ITerrainGeneratorStep, IDisposable {

    TerrainGeneratorData terrain;

    private const float rainfall = 0.05f;
    private const float waterToTerrainRatio = 0.05f;

    public NativeArray<float> waterHeight { get; private set; }

#if DEBUG
    public NativeArray<float> waterTerrainHeight { get; private set; }
#endif

    public WaterErosion( TerrainGeneratorData terrain ) {
        this.terrain = terrain;
        waterHeight = new NativeArray<float>( terrain.size * terrain.size, Allocator.Persistent );
#if DEBUG
        waterTerrainHeight = new NativeArray<float>( terrain.size * terrain.size, Allocator.Persistent );
#endif
    }

public void Dispose() {
        if(waterHeight.IsCreated)
            waterHeight.Dispose();
#if DEBUG
        if(waterTerrainHeight.IsCreated)
            waterTerrainHeight.Dispose();
#endif

    }

    /*public void Waterstep() {


        RainfallStep();
        WaterflowStep();

    }*/
    /*private void RainfallStep() {

        for(int x = 0; x < waterHeight.GetLength( 0 ); x++) {
            for(int y = 0; y < waterHeight.GetLength( 1 ); y++) {
                waterHeight[x, y] += rainfall;
            }
        }

    }*/
    /*
    private void WaterflowStep() {

        float[,] waterDifference = new float[waterHeight.GetLength( 0 ), waterHeight.GetLength( 1 )];
        float[,] heightChange = new float[waterHeight.GetLength( 0 ), waterHeight.GetLength( 1 )];

        for(int x = 0; x < waterHeight.GetLength( 0 ); x++) {
            for(int y = 0; y < waterHeight.GetLength( 1 ); y++) {

                //height differences
                float[,] heightDiff = new float[3, 3];

                int minX = (x > 0) ? -1 : 0;
                int maxX = (x + 1 < waterHeight.GetLength( 0 )) ? 1 : 0;
                int minY = (y > 0) ? -1 : 0;
                int maxY = (y + 1 < waterHeight.GetLength( 1 )) ? 1 : 0;

                float flowOut = 0;

                //HeightDiff[] diffs = new HeightDiff[ (maxX-minX) * (maxY-minY) - 1 ];
                List<HeightRecord> diffs = new List<HeightRecord>( 8 );

                for(int dX = -1; dX < 2; dX++)
                    for(int dY = -1; dY < 2; dY++) {
                        if(dX == 0 && dY == 0)
                            continue;
                        if(dX == -1 && x == 0)
                            continue;
                        if(dY == -1 && y == 0)
                            continue;
                        if(dX == 1 && x == waterHeight.GetLength( 0 ) - 1)
                            continue;
                        if(dY == 1 && y == waterHeight.GetLength( 1 ) - 1)
                            continue;

                        diffs.Add( new HeightRecord( dX, dY, terrain.heighmap[x + dX, y + dY] + waterHeight[x + dX, y + dY] ) );

                    }

                diffs.Sort( ( a, b ) => Math.Sign( a.height - b.height ) );

                foreach(HeightRecord diff in diffs) {
                    float thisHeight = (terrain.heighmap[x, y] + waterHeight[x, y] - flowOut);
                    if(diff.height < thisHeight ) {
                        float flow = (thisHeight - diff.height) / 2; //
                        flow = Mathf.Min( flow, waterHeight[x, y] - flowOut );  //limit by current water level
                        waterDifference[x + diff.offsetX, y + diff.offsetY] += flow;
                        heightChange[x + diff.offsetX, y + diff.offsetY] += flow * waterToTerrainRatio;
                        flowOut += flow;
                    }
                }

                waterDifference[x, y] -= flowOut;
                heightChange[x, y] -= flowOut * waterToTerrainRatio;
            }
        }


        for(int x = 0; x < waterHeight.GetLength( 0 ); x++) {
            for(int y = 0; y < waterHeight.GetLength( 1 ); y++) {
                waterHeight[x, y] += waterDifference[x, y];
                waterHeight[x, y] -= waterHeight[x, y] * (rainfall * rainfallToEveporationRatio);
                terrain.heighmap[x, y] += heightChange[x, y];
            }
        }
    }
    */

    DisposableJobHandle ITerrainGeneratorStep.Step( NativeArray<float> heighmapChanges ) {

        int size = terrain.size;

        NativeArray<float> heightChanges = new NativeArray<float>( size * size, Allocator.Temp );
        NativeArray<float> heightChangesW = new NativeArray<float>( size * size, Allocator.Temp );
        NativeArray<float> heightChangesE = new NativeArray<float>( size * size, Allocator.Temp );
        NativeArray<float> heightChangesS = new NativeArray<float>( size * size, Allocator.Temp );
        NativeArray<float> heightChangesN = new NativeArray<float>( size * size, Allocator.Temp );

        NativeArray<float> waterChanges = new NativeArray<float>( size * size, Allocator.Temp );
        NativeArray<float> waterChangesW = new NativeArray<float>( size * size, Allocator.Temp );
        NativeArray<float> waterChangesE = new NativeArray<float>( size * size, Allocator.Temp );
        NativeArray<float> waterChangesS = new NativeArray<float>( size * size, Allocator.Temp );
        NativeArray<float> waterChangesN = new NativeArray<float>( size * size, Allocator.Temp );

        NativeSlice<float> heightWestSlice = new NativeSlice<float>( heightChangesW, 1 ); //shifted by 1 right
        NativeSlice<float> heightNorthSlice = new NativeSlice<float>( heightChangesN, size ); //shifted by 1 down
        NativeSlice<float> heightEastInverseSlice = new NativeSlice<float>( heightChangesN, 1 );
        NativeSlice<float> heightSouthInverseSlice = new NativeSlice<float>( heightChangesS, size );

        NativeSlice<float> waterWestSlice = new NativeSlice<float>( waterChangesW, 1 ); //shifted by 1 right
        NativeSlice<float> waterNorthSlice = new NativeSlice<float>( waterChangesN, size ); //shifted by 1 down
        NativeSlice<float> waterEastInverseSlice = new NativeSlice<float>( waterChangesE, 1 );
        NativeSlice<float> waterSouthInverseSlice = new NativeSlice<float>( waterChangesS, size );

        JobWaterCalc calcJob = new JobWaterCalc() {
            heightmap = terrain.heightmap,
            waterHeightmap = waterHeight,

            localChanges = heightChanges,
            eastChanges = heightChangesE,
            westChanges = heightWestSlice,
            northChanges = heightNorthSlice,
            southChanges = heightChangesS,

            waterChanges = waterChanges,
            eastWaterChanges = waterChangesE,
            westWaterChanges = waterWestSlice,
            northWaterChanges = waterNorthSlice,
            southWaterChanges = waterChangesS,

            size = size,
            speedToErosionCoeff = 0.9f,
            rainfall = 0.01f,
            heightToEvaporationRatio = 0.2f
        };

        ApplyChangesStep applyJob = new ApplyChangesStep() {

            totalHeightmapChange = heighmapChanges,
            waterHeightmap = waterHeight,

            localChanges = heightChanges,
            eastChanges = heightEastInverseSlice,
            westChanges = heightChangesW,
            northChanges = heightChangesN,
            southChanges = heightSouthInverseSlice,

            waterChanges = waterChanges,
            eastWaterChanges = waterEastInverseSlice,
            westWaterChanges = waterChangesW,
            northWaterChanges = waterChangesN,
            southWaterChanges = waterSouthInverseSlice,

            size = size
#if DEBUG
            , oldHeightMap = terrain.heightmap,
            treshold = 0.03f,
            waterToTerrainRatio = 0.3f,
            waterTerrainMap = waterTerrainHeight
#endif

        };

        JobHandle calcJobHandle = calcJob.Schedule( size * size, 512 );
        JobHandle applyHandle = applyJob.Schedule( size * size, 512, calcJobHandle );

        

        return new DisposableJobHandle( applyHandle, new IDisposable[] { heightChanges, heightChangesE, heightChangesW, heightChangesN, heightChangesS, waterChanges, waterChangesE, waterChangesW, waterChangesN, waterChangesS } );

    }

    /*
private struct HeightRecord {

   public int offsetX;
   public int offsetY;
   public float height;

   public HeightRecord( int offsetX, int offsetY, float heighDiff ) {
       this.offsetX = offsetX;
       this.offsetY = offsetY;
       this.height = heighDiff;
   }
}*/

    private struct JobWaterCalc : IJobParallelFor {

        [ReadOnly]
        public NativeArray<float> heightmap;
        [ReadOnly]
        public NativeArray<float> waterHeightmap;

        public NativeArray<float> localChanges;
        public NativeArray<float> eastChanges;
        public NativeSlice<float> westChanges;
        public NativeArray<float> southChanges;
        public NativeSlice<float> northChanges;

        public NativeArray<float> waterChanges;
        public NativeArray<float> eastWaterChanges;
        public NativeSlice<float> westWaterChanges;
        public NativeArray<float> southWaterChanges;
        public NativeSlice<float> northWaterChanges;

        public int size;
        public float speedToErosionCoeff;
        public float rainfall;
        public float heightToEvaporationRatio;


        public void Execute( int index ) {
            if(index < size || index >= (size * size) - size || (index % size) == 0 || (index % size) == size - 1)
                return;

            float westSlope = heightmap[index] - heightmap[index + 1];
            westSlope = (westSlope > 0) ? westSlope : 0;
            float eastSlope = heightmap[index] - heightmap[index - 1];
            eastSlope = (eastSlope > 0) ? eastSlope : 0;
            float northSlope = heightmap[index] - heightmap[index + size];
            northSlope = (northSlope > 0) ? northSlope : 0;
            float southSlope = heightmap[index] - heightmap[index - size];
            southSlope = (southSlope > 0) ? southSlope : 0;

            float maxSlopeEW = westSlope > eastSlope ? westSlope : eastSlope;
            float maxSlopeNS = (northSlope > southSlope) ? northSlope : southSlope;
            float maxSlope = maxSlopeEW > maxSlopeNS ? maxSlopeEW : maxSlopeNS;
            //float totalDiff = westSlope + eastSlope + northSlope + southSlope;

            float localWaterHeight = heightmap[index] + waterHeightmap[index];

            float westWaterSlope = localWaterHeight - heightmap[index + 1] - waterHeightmap[index + 1];
            westWaterSlope = (westWaterSlope > 0) ? westWaterSlope : 0;
            float eastWaterSlope = localWaterHeight - heightmap[index - 1] - waterHeightmap[index - 1];
            eastWaterSlope = (eastWaterSlope > 0) ? eastWaterSlope : 0;
            float northWaterSlope = localWaterHeight - heightmap[index + size] - waterHeightmap[index + size];
            northWaterSlope = (northWaterSlope > 0) ? northWaterSlope : 0;
            float southWaterSlope = localWaterHeight - heightmap[index - size] - waterHeightmap[index - size];
            southWaterSlope = (southWaterSlope > 0) ? southWaterSlope : 0;

            float totalWaterSlope = westWaterSlope + eastWaterSlope + northWaterSlope + southWaterSlope;

            float maxWaterSlopeEW = westWaterSlope > eastWaterSlope ? westWaterSlope : eastWaterSlope;
            float maxWaterSlopeNS = (northWaterSlope > southWaterSlope) ? northSlope : southWaterSlope;
            float maxWaterSlope = maxWaterSlopeEW > maxWaterSlopeNS ? maxWaterSlopeEW : maxWaterSlopeNS;

            //water flowing out of here - 50% of maxdiff (prevents target square from ending up with water higher than this square)
            float totalWaterFlow = 0.5f * maxWaterSlope;
            if (totalWaterFlow > waterHeightmap[index])
                totalWaterFlow = waterHeightmap[index];

            //water flow
            if (totalWaterFlow > 0)
            {
                westWaterChanges[index] = totalWaterFlow * (westWaterSlope / totalWaterSlope);
                eastWaterChanges[index] = totalWaterFlow * (eastWaterSlope / totalWaterSlope);
                northWaterChanges[index] = totalWaterFlow * (northWaterSlope / totalWaterSlope);
                southWaterChanges[index] = totalWaterFlow * (southWaterSlope / totalWaterSlope);
                //calculate water flowing out, rainfall and evaporation
                waterChanges[index] = -totalWaterFlow + rainfall - (waterHeightmap[index] * heightToEvaporationRatio);
                if (waterHeightmap[index] + waterChanges[index] < 0)
                    waterChanges[index] = -waterHeightmap[index];

                //water erosion and deposition
                // waterSpeed = slope / height
                // erosion = speed * volume
                westChanges[index] = westWaterChanges[index] * (westSlope / waterHeightmap[index]) * speedToErosionCoeff;
                eastChanges[index] = eastWaterChanges[index] * (eastSlope / waterHeightmap[index]) * speedToErosionCoeff;
                northChanges[index] = northWaterChanges[index] * (northSlope / waterHeightmap[index]) * speedToErosionCoeff;
                southChanges[index] = southWaterChanges[index] * (southSlope / waterHeightmap[index]) * speedToErosionCoeff;
                localChanges[index] = -westChanges[index] - eastChanges[index] - northChanges[index] - southChanges[index];
            }
            else {
                waterChanges[index] = rainfall - (waterHeightmap[index] * heightToEvaporationRatio);
            }

        }



    }

    private struct ApplyChangesStep : IJobParallelFor {

        public NativeArray<float> totalHeightmapChange; //calculate only change - heightmap itself is calculated in TerrainGenerator's final step
        public NativeArray<float> waterHeightmap; //modify the waterHeightmap itself

        [ReadOnly] public NativeArray<float> localChanges;
        [ReadOnly] public NativeSlice<float> eastChanges;
        [ReadOnly] public NativeArray<float> westChanges;
        [ReadOnly] public NativeSlice<float> southChanges;
        [ReadOnly] public NativeArray<float> northChanges;

        [ReadOnly] public NativeArray<float> waterChanges;
        [ReadOnly] public NativeSlice<float> eastWaterChanges;
        [ReadOnly] public NativeArray<float> westWaterChanges;
        [ReadOnly] public NativeSlice<float> southWaterChanges;
        [ReadOnly] public NativeArray<float> northWaterChanges;

        public int size;

#if DEBUG
        [ReadOnly] public NativeArray<float> oldHeightMap;
        public NativeArray<float> waterTerrainMap;
        public float treshold;
        public float waterToTerrainRatio;
#endif

        public void Execute( int index ) {
            //skip for edge - it is just padding
            if(index < size || index >= (size * size) - size || (index % size) == 0 || (index % size) == size - 1)
                return;
            totalHeightmapChange[index] = localChanges[index] + eastChanges[index] + westChanges[index] + northChanges[index] + southChanges[index];
            waterHeightmap[index] += waterChanges[index] + eastWaterChanges[index] + westWaterChanges[index] + northWaterChanges[index] + southWaterChanges[index];
#if DEBUG
            if(waterHeightmap[index] > treshold)
                waterTerrainMap[index] = oldHeightMap[index] + totalHeightmapChange[index] + waterHeightmap[index] * waterToTerrainRatio;
            else
                waterTerrainMap[index] = oldHeightMap[index] + totalHeightmapChange[index] - 0.01f;
#endif
        }
    }

}

