using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class WaterErosion {


    TerrainGeneratorData terrain;
    public float[,] waterHeight { get; private set; }

    private const float rainfall = 0.05f;
    private const float rainfallToEveporationRatio = 2; //has to be >1, otherwise map will flood

    private const float waterToTerrainRatio = 0.05f;

    //private int[] xOffset = new int[] { -1, 0, 1, 1, 1, 0, -1, -1 };
    //private int[] yOffset = new int[] { 1, 1, 1, 0, -1, -1, -1, 0 };

    public WaterErosion( TerrainGeneratorData terrain ) {
        this.terrain = terrain;
        waterHeight = new float[ terrain.heighmap.GetLength(0), terrain.heighmap.GetLength(1) ];
    }

    public void Waterstep() {


        RainfallStep();
        WaterflowStep();

    }

    private void RainfallStep() {

        for(int x = 0; x < waterHeight.GetLength( 0 ); x++) {
            for(int y = 0; y < waterHeight.GetLength( 1 ); y++) {
                waterHeight[x, y] += rainfall;
            }
        }

    }

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

    private struct HeightRecord {

        public int offsetX;
        public int offsetY;
        public float height;

        public HeightRecord( int offsetX, int offsetY, float heighDiff ) {
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.height = heighDiff;
        }
    }

}

