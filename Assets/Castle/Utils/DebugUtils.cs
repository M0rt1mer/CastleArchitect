using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using System.Linq;
using Unity.Collections;

public static class DebugExtensions {

    public static IDisposable AttachDebugInfo<T>( this NativeArray<T> array, string debugInfo ) where T : struct {
        return new NativeArrayWithDebugInfo<T>( array, debugInfo );
    }

    public static IEnumerable<IDisposable> AttachDebugInfo<T>( this IEnumerable<NativeArray<T>> enumerable, string debugInfo ) where T : struct {
        int i = 0;
        foreach(NativeArray<T> array in enumerable) {
            yield return new NativeArrayWithDebugInfo<T>( array, string.Format( debugInfo, i ) );
            i++;
        }

    }

}


public class NativeArrayWithDebugInfo<T> : IDisposable where T : struct {

    public readonly NativeArray<T> array;
    public readonly string debugInfo;

    public NativeArrayWithDebugInfo( NativeArray<T> array, string debugInfo ) {
        this.array = array;
        this.debugInfo = debugInfo;
    }

    public void Dispose() {
        array.Dispose();
    }
}
