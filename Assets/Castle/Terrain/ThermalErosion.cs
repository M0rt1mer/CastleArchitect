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
        NativeArray<float> localChangesW = new NativeArray<float>(size * size, Allocator.Temp);
        NativeArray<float> localChangesE = new NativeArray<float>(size * size, Allocator.Temp);
        NativeArray<float> localChangesS = new NativeArray<float>(size * size, Allocator.Temp);
        NativeArray<float> localChangesN = new NativeArray<float>(size * size, Allocator.Temp);

        NativeSlice<float> westSlice = new NativeSlice<float>(localChangesW, 1); //shifted by 1 right
        NativeSlice<float> northSlice = new NativeSlice<float>(localChangesN, size); //shifted by 1 down

        NativeSlice<float> eastInverseSlice = new NativeSlice<float>(localChangesE, 1);
        NativeSlice<float> southInverseSlice = new NativeSlice<float>(localChangesS, size);

        float[] intermediate = new float[size * size];
        Buffer.BlockCopy(terrain.heighmap, 0, intermediate, 0, size * size);
        NativeArray<float> heightmap = new NativeArray<float>(intermediate, Allocator.Temp);

        CalcErosionStep calcStep = new CalcErosionStep()
        {
            size = size,
            heightmap = heightmap,
            localChanges = localChanges,
            eastChanges = localChangesE,
            westChanges = westSlice,
            northChanges = northSlice,
            southChanges = localChangesS,
            minSlopeForErosion = 0.00f,
            erosionCoeff = 0.50f
        };
        ApplyErosionStep applyStep = new ApplyErosionStep()
        {
            size = size,
            heightmap = heightmap,
            localChanges = localChanges,
            eastChanges = eastInverseSlice,
            westChanges = localChangesW,
            northChanges = localChangesN,
            southChanges = southInverseSlice,
        };

        JobHandle calcStepHandle = calcStep.Schedule(size * size, 512);
        JobHandle applyStepHandle = applyStep.Schedule(size * size, 512, calcStepHandle);

        applyStepHandle.Complete();

        heightmap.CopyTo(intermediate);
        Buffer.BlockCopy(intermediate, 0, terrain.heighmap, 0, size * size);

        localChanges.Dispose();
        localChangesE.Dispose();
        localChangesW.Dispose();
        localChangesS.Dispose();
        localChangesN.Dispose();
        heightmap.Dispose();
    }
    
    private struct CalcErosionStep : IJobParallelFor
    {

        [ReadOnly]
        public NativeArray<float> heightmap;

        public NativeArray<float> localChanges;
        public NativeArray<float> eastChanges;
        public NativeSlice<float> westChanges;
        public NativeArray<float> southChanges;
        public NativeSlice<float> northChanges;

        public int size;
        public float minSlopeForErosion;
        public float erosionCoeff;

        public void Execute(int index)
        {
            //skip for edge - it is just padding
            if (index < size || index >= (size * size) - size || (index % size) == 0 || (index % size) == size - 1)
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

            if (totalDiff > 0)
            {
                northChanges[index] = maxDiff * erosionCoeff * (northDiff / totalDiff);
                eastChanges[index] = maxDiff * erosionCoeff * (eastDiff / totalDiff);
                westChanges[index] = maxDiff * erosionCoeff * (westDiff / totalDiff);
                southChanges[index] = maxDiff * erosionCoeff * (southDiff / totalDiff);
            }
        }
    }

    private struct ApplyErosionStep : IJobParallelFor
    {
        public NativeArray<float> heightmap;

        [ReadOnly]
        public NativeArray<float> localChanges;
        [ReadOnly]
        public NativeSlice<float> eastChanges;
        [ReadOnly]
        public NativeArray<float> westChanges;
        [ReadOnly]
        public NativeSlice<float> southChanges;
        [ReadOnly]
        public NativeArray<float> northChanges;

        public int size;

        public void Execute(int index)
        {
            //skip for edge - it is just padding
            if (index < size || index >= (size * size) - size || (index % size) == 0 || (index % size) == size - 1)
                return;
            heightmap[index] += localChanges[index] + eastChanges[index] + westChanges[index] + northChanges[index] + southChanges[index];
        }
    }

}

