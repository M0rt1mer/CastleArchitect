using System;
using UnityEngine;
using Net3dBool;
using System.Linq;
using System.Collections.Generic;

class Gateway : PlacedBuilding, ICutoff {


    Solid baseShape;

    protected override void Start() {
        base.Start();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        MeshFilter mc = gameObject.AddComponent<MeshFilter>();

        mr.material = bldData.mat;

        UpdateShape();

    }

    public override bool IsUsable( ) {
        return !outline.Enclosed && outline.Extrusion > 0;
    }

    public override void Build() {
        GetComponent<MeshRenderer>().enabled = false;
    }

    protected override void UpdateShape() {
        baseShape = createSolid();

        if(baseShape != null) {
            GetComponent<MeshFilter>().mesh = Utils.Solid2Mesh( baseShape );
        }

    }

    private Solid createSolid() {

        if(!IsUsable())
            return null;

        Vector2[] outlinePoints = outline.getExtrudedDefiningPoints().ToArray();

        if(outlinePoints.Length < 4)
            return null;

        //Vector3[] template = new Vector3[] { new Vector3( 0,0,-1 ), new Vector3( 0, 0, 1 ), new Vector3( 0, 1, 1 ), new Vector3( 0, 1, -1 ) };
        Vector3[] template = new Vector3[] { new Vector3(0,0,1), new Vector3(0,1.5f,0.6f), new Vector3(0,2.25f,0.3f), new Vector3(0,3,0), new Vector3(0,2.25f,-0.3f), new Vector3(0,1.5f,-0.6f), new Vector3(0,0,-1) };
        int n = template.Length;

        Point3d[] vertices = new Point3d[ n*outlinePoints.Length/2 ];
        // create points
        for(int i = 0; i < outlinePoints.Length / 2; i++) {
            Vector3 dir = (outlinePoints[outlinePoints.Length  - i - 1] - outlinePoints[i]).toVec3();
            Matrix4x4 templateToWorldSpace = Matrix4x4.TRS( outlinePoints[i].toVec3() + dir/2, Quaternion.LookRotation( dir.normalized, Vector3.up ), new Vector3( dir.magnitude, 1, 1 ) );
            for(int j = 0; j < n; j++) {
                vertices[i * n + j] = (templateToWorldSpace.MultiplyPoint3x4( template[j] )+ Vector3.up * outline.Height ).toPoint3d();
            }

        }
        
        //int[] indices = new int[ (outlinePoints.Length/2 - 1) * n * 6 ];
        List<int> indices = new List<int>(  );

        for(int i = 1; i < outlinePoints.Length / 2; i++) { //start with second points
            //current points i + 0 to i+n
            for(int j = 0; j < n; j++) {
                indices.AddRange( new int[] {  i*n-n+j, i * n + j, i * n+(j+1)%n,   i*n-n+j, i * n + (j + 1) % n, i *n-n+(j+1)%n  } );
            }
        }
        //sides
        indices.AddRange( Utils.TriangulatePolygon( vertices, Enumerable.Range(0,n) ) );
        indices.AddRange( Utils.TriangulatePolygon( vertices, Enumerable.Range( vertices.Length - n, n ).Reverse() ) );

        return new Solid( vertices, indices.ToArray(), Enumerable.Repeat( new Color3f( 0, 0, 0 ), vertices.Length ).ToArray() );
    }

    public Bounds GetBounds() {
        return new Bounds();
    }

    public  Solid GetCutoff() {
        return baseShape;
    }

    public override void UpdateWorld() {
    }

    static Gateway() {
        PlacedBuilding.factoryList.Add( "Gateway", (x) => x.AddComponent<Gateway>() );
    }

}
