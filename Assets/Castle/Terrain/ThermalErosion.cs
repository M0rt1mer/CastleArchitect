using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using System;

public class ThermalErosion
{

    private TerrainGeneratorData terrain;

    public ThermalErosion( TerrainGeneratorData terrain )
    {
        this.terrain = terrain;
    }

    public void Step() {

        int size = terrain.heighmap.GetLength(0);

        NativeArray<float> localChanges = new NativeArray<float>(size * size, Allocator.Temp);
        NativeArray<float> eastChanges = new NativeArray<float>(size * size, Allocator.Temp);
        NativeArray<float> westChanges = new NativeArray<float>(size * size, Allocator.Temp);
        NativeArray<float> southChanges = new NativeArray<float>(size * size, Allocator.Temp);
        NativeArray<float> northChanges = new NativeArray<float>(size * size, Allocator.Temp);

        float[] intermediate = new float[size * size];
        terrain.heighmap.CopyTo(intermediate, 0);
        NativeArray<float> heightmap = new NativeArray<float>(intermediate, Allocator.Temp);

        CalcErosionStep calcStep = new CalcErosionStep()
        {
            size = size,
            heightmap = heightmap,
            localChanges = localChanges,
            eastChanges = eastChanges,
            westChanges = westChanges,
            northChanges = northChanges,
            southChanges = southChanges,
            minSlopeForErosion = 0.03f,
            erosionCoeff = 0.05f
        };
        ApplyErosionStep applyStep = new ApplyErosionStep()
        {
            size = size,
            heightmap = heightmap,
            localChanges = localChanges,
            eastChanges = eastChanges,
            westChanges = westChanges,
            northChanges = northChanges,
            southChanges = southChanges,
        };

        JobHandle calcStepHandle = calcStep.Schedule(size * size, 128);
        JobHandle applyStepHandle = applyStep.Schedule(size * size, 128, calcStepHandle);

        applyStepHandle.Complete();

        heightmap.CopyTo(intermediate);
        intermediate.CopyTo(terrain.heighmap, 0);

    }
    
    private struct CalcErosionStep : IJobParallelFor
    {

        [ReadOnly]
        public NativeArray<float> heightmap;

        public NativeArray<float> localChanges;
        public NativeArray<float> eastChanges;
        public NativeArray<float> westChanges;
        public NativeArray<float> southChanges;
        public NativeArray<float> northChanges;

        public int size;
        public float minSlopeForErosion;
        public float erosionCoeff;

        public void Execute(int index)
        {
            //skip for edge - it is just padding
            if (index == 0 || index == size - 1 || index % size == 0 || index % size == size - 1)
                return;

            float westDiff = heightmap[index] - heightmap[index - 1] - minSlopeForErosion;
            westDiff = (westDiff > 0) ? westDiff : 0;
            float eastDiff = heightmap[index] - heightmap[index + 1] - minSlopeForErosion;
            eastDiff = (eastDiff > 0) ? eastDiff : 0;
            float northDiff = heightmap[index] - heightmap[index - size] - minSlopeForErosion;
            northDiff = (northDiff > 0) ? northDiff : 0;
            float southDiff = heightmap[index] - heightmap[index + size] - minSlopeForErosion;
            southDiff = (southDiff > 0) ? southDiff : 0;

            float maxDiffEW = westDiff > eastDiff ? westDiff : eastDiff;
            float maxDiffNS = (northDiff > southDiff) ? northDiff : southDiff;
            float maxDiff = maxDiffEW > maxDiffNS ? maxDiffEW : maxDiffNS;
            float totalDiff = westDiff + eastDiff + northDiff + southDiff;

            localChanges[index] = - maxDiff * erosionCoeff;

            northChanges[index - size] = - localChanges[index] * (northDiff / totalDiff);
            eastChanges[index + 1] = - localChanges[index] * (eastDiff / totalDiff);
            westChanges[index - 1] = - localChanges[index] * (westDiff / totalDiff);
            southChanges[index + size] = - localChanges[index] * (southDiff / totalDiff);
        }
    }

    private struct ApplyErosionStep : IJobParallelFor
    {
        public NativeArray<float> heightmap;

        [ReadOnly]
        public NativeArray<float> localChanges;
        [ReadOnly]
        public NativeArray<float> eastChanges;
        [ReadOnly]
        public NativeArray<float> westChanges;
        [ReadOnly]
        public NativeArray<float> southChanges;
        [ReadOnly]
        public NativeArray<float> northChanges;

        public int size;

        public void Execute(int index)
        {
            //skip for edge - it is just padding
            if (index == 0 || index == size - 1 || index % size == 0 || index % size == size - 1)
                return;
            heightmap[index] += localChanges[index] + eastChanges[index] + westChanges[index] + northChanges[index] + southChanges[index];
        }
    }

}

