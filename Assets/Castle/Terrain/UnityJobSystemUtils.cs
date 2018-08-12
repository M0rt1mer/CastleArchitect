using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using System.Linq;

public class DisposableJobHandle : IDisposable {

    public readonly JobHandle job;

    private readonly IEnumerable<IDisposable> resources;

    public DisposableJobHandle( JobHandle job, IEnumerable<IDisposable> resources ) {
        this.job = job;
        this.resources = resources;
    }

    public void Dispose() {
        foreach(IDisposable disp in resources)
            disp.Dispose();
    }

}

public static class JobHandleExtensions {

    public static DisposableJobHandle LinkResources( this JobHandle handle, params IEnumerable<IDisposable>[] resources ) {
        if( resources == null )
            return new DisposableJobHandle( handle, null );
        return new DisposableJobHandle( handle, resources.SelectMany(x=>x) );
    }
    public static DisposableJobHandle LinkResources( this JobHandle handle, IEnumerable<IDisposable> resources ) {
        return new DisposableJobHandle( handle, resources );
    }
}