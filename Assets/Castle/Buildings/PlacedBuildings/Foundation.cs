using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Net3dBool;

using System.IO;
using System;

public class Foundation : PlacedBuilding {

    static List<Solid> partialMeshes = new List<Solid>();
    static List<Solid> cutoffs = new List<Solid>();

    private Solid baseStructure;

    /*public Mesh BuildMesh() {
        Mesh wallMesh = Wall.Build( outline, outline.Height, true );
        Solid wallSolid = Mesh2Solid( wallMesh );

        //build main mesh
        if( (partialMeshes.Count + cutoffs.Count) < 2) {
            partialMeshes.Add( wallSolid );
            return wallMesh;
        } else {
            Solid temp = wallSolid;

            foreach(Solid partialMesh in partialMeshes.Concat( cutoffs )) {
                temp = new BooleanModeller( temp, partialMesh ).getDifference();
            }

            Mesh msh = Solid2Mesh( temp );
            //fix UV
            Vector2[] uvs = new Vector2[msh.vertexCount];
            for(int i = 0; i < msh.vertexCount; i++) {
                if(msh.normals[i].y > 0.9) //upwards
                    uvs[i] = new Vector2( msh.vertices[i].x, msh.vertices[i].z );
                else //sideways
                    uvs[i] = new Vector2( msh.vertices[i].x, msh.vertices[i].y );
            }

            msh.uv = uvs;
            partialMeshes.Add( wallSolid );
            return msh;
        }

    }*/

    private void AddUVToMesh( Mesh msh ) {

        Vector2[] uvs = new Vector2[msh.vertexCount];
        for(int i = 0; i < msh.vertexCount; i++) {
            if(msh.normals[i].y > 0.9) //upwards
                uvs[i] = new Vector2( msh.vertices[i].x, msh.vertices[i].z );
            else //sideways
                uvs[i] = new Vector2( msh.vertices[i].x, msh.vertices[i].y );
        }

        msh.uv = uvs;
    }

    /// <summary>
    /// Creates the basic template for this foundation. It will then 
    /// </summary>
    /// <param name="outline"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static Solid BuildSolid( Outline outline, float height ) {

        List<Point3d> vertices = new List<Point3d>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        Vector2[] points = outline.getRawDefiningPointsClockwise().ToArray();
        int totalVertCnt = (points.Length) * 2;
        foreach(Vector2 point in points) { //last point is same is first

            indices.AddRange( new int[] { (vertices.Count + 3) % totalVertCnt, (vertices.Count + 1) % totalVertCnt, (vertices.Count + 2) % totalVertCnt, (vertices.Count + 3) % totalVertCnt, (vertices.Count + 2) % totalVertCnt, (vertices.Count + 4) % totalVertCnt } );

            vertices.Add( new Point3d( point.x, outline.Height, point.y ) ); //top point for walls
            uvs.Add( new Vector2( point.x, outline.Height ) );

            vertices.Add( new Point3d( point.x, outline.Height - height, point.y ) ); //bottom point for walls
            uvs.Add( new Vector2( point.x, outline.Height - height ) );



        }
        int pointCnt = vertices.Count / 2;
        List<int> pointIndices = new List<int>();
        for(int i = 0; i < pointCnt; i++) {
                pointIndices.Add( i * 2 ); //add wall ceiling point, bottom point will be calculated
        }

        indices.AddRange( Utils.TriangulatePolygon( vertices.ToArray(), ((IEnumerable<int>)pointIndices).Reverse() ) );
        indices.AddRange( Utils.TriangulatePolygon( vertices.ToArray(), pointIndices.Select( x=> x+1 ) ) ); //bottom
        //build ceiling and floor
        /*HashSet<int> toRemove = new HashSet<int>();
        
        while(pointIndices.Count > 3) {
            int n = pointIndices.Count;
            for(int i = 0; i < n;) {
                if(Point3d.Cross( vertices[pointIndices[(i - 1 + n) % n]] - vertices[pointIndices[i]], vertices[pointIndices[(i + 1) % n]] - vertices[pointIndices[i]] ).y > 0) { //if concave angle
                    indices.AddRange( new int[] { pointIndices[(i + 1) % n], pointIndices[i], pointIndices[(i - 1 + n) % n] } ); //ceiling
                    indices.AddRange( new int[] { pointIndices[(i + 1) % n] + 1, pointIndices[i] + 1, pointIndices[(i - 1 + n) % n] + 1 } ); //bottom
                    toRemove.Add( pointIndices[i + 1] );
                    i += 2;
                } else i++;
            }
            pointIndices.RemoveAll( x => toRemove.Contains( x ) );
            if(toRemove.Count == 0)
                throw new System.Exception( "toRemove empty" );
            toRemove.Clear();
        }


        //if exactly three points remain, place final triangle
        if(pointIndices.Count == 3) {
            indices.AddRange( new int[] { pointIndices[0], pointIndices[2], pointIndices[1] } ); //ceiling
            indices.AddRange( new int[] { pointIndices[0] + 1, pointIndices[2] + 1, pointIndices[1] + 1 } ); //bottom
        }*/



        return new Solid( vertices.ToArray(), indices.ToArray(), Enumerable.Repeat( new Color3f(0,0,0), vertices.Count ).ToArray() );

    }

    public override bool IsUsable( ) {
        return !outline.Enclosed || !outline.Complete;
    }

    //----------------------- IMPLEMENTS PlacedBuildings
    public override void Build() {

        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshCollider mc = gameObject.AddComponent<MeshCollider>();
        mr.material = bldData.mat;

        baseStructure = BuildSolid( this.outline, this.outline.Height );

        UpdateWorld();

    }

    public override void UpdateWorld() {
        if(baseStructure != null) {
            Solid current = baseStructure;
            foreach(ICutoff cutoff in transform.parent.GetComponentsInChildren<ICutoff>()) {
                if(cutoff.GetCutoff() != null)
                    current = new BooleanModeller( current, cutoff.GetCutoff() ).getDifference();
            }
            Mesh msh = Utils.Solid2Mesh( current );
            AddUVToMesh( msh );
            GetComponent<MeshCollider>().sharedMesh = msh;
            GetComponent<MeshFilter>().mesh = msh;
        }
    }

    protected override void UpdateShape() {
    }

    static Foundation() {
        PlacedBuilding.factoryList.Add( "Foundation", (x) => x.AddComponent<Foundation>() );
    }
}
