using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class TerrainGeneratorData : IDisposable {

//    [SerializeField]
//    public float[,] heighmap;

    [SerializeField]
    public NativeArray<float> heightmap;
    public readonly int size;

    public TerrainGeneratorData( float[,] srcHeightmap ) {
        size = srcHeightmap.GetLength( 0 );
        Assert.AreEqual( size, srcHeightmap.GetLength( 1 ) );

        float[] intermediate = new float[size * size];
        System.Buffer.BlockCopy( srcHeightmap, 0, intermediate, 0, size * size * sizeof( float ) );
        heightmap = new NativeArray<float>( intermediate, Allocator.Persistent );
    }

    public TerrainGeneratorData( int resolution ) {
        heightmap = new NativeArray<float>( resolution * resolution, Allocator.Persistent );
        size = resolution;
    }

    public void Dispose() {
        if(heightmap.IsCreated)
            heightmap.Dispose();
    }

    public void ReplaceData( float[,] srcHeightmap ) {
        Assert.AreEqual( size, srcHeightmap.GetLength( 0 ), "Incorrect heightmap size" );
        Assert.AreEqual( size, srcHeightmap.GetLength( 1 ), "Incorrect heightmap size" );

        float[] intermediate = new float[size * size];
        System.Buffer.BlockCopy( srcHeightmap, 0, intermediate, 0, size * size * sizeof( float ) );
        heightmap.CopyFrom( intermediate );
    }

}

