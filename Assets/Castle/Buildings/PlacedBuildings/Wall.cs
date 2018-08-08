using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

[Obsolete]
public class Wall : PlacedBuilding {

    public Mesh Build( Outline outline ) {
        return Build( outline, 10, true );
    }

    public static Mesh Build(Outline outline, float height, bool collapsed) {
        Mesh outMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        Vector2[] points = outline.getRawDefiningPointsClockwise().ToArray();
        int totalVertCnt = (points.Length) * (collapsed ? 2 : 6);
        foreach (Vector2 point in points ) { //last point is same is first

            indices.AddRange(new int[] { (vertices.Count +3) % totalVertCnt, (vertices.Count + 1) % totalVertCnt, (vertices.Count + 2) % totalVertCnt, (vertices.Count + 3) % totalVertCnt, (vertices.Count + 2) % totalVertCnt, (vertices.Count + 4) % totalVertCnt } );
            /*if (vertices.Count > 5) { //ceiling
                indices.AddRange(new int[] { vertices.Count - 3, vertices.Count + 2, vertices.Count - 8 });
            }*/

            vertices.Add(new Vector3(point.x, outline.Height, point.y)); //top point for walls
            uvs.Add(new Vector2(point.x, outline.Height));

            vertices.Add(new Vector3(point.x, outline.Height - height, point.y)); //bottom point for walls
            uvs.Add(new Vector2(point.x, outline.Height - height ) );

            if(!collapsed) {
                vertices.Add( new Vector3( point.x, outline.Height, point.y ) ); //top point
                uvs.Add( new Vector2( point.x, point.y ) );

                vertices.Add( new Vector3( point.x, outline.Height - height, point.y ) ); //bottom point
                uvs.Add( new Vector2( point.x, point.y ) );

                vertices.Add( new Vector3( point.x, outline.Height, point.y ) );
                uvs.Add( new Vector2( point.x, outline.Height ) );

                vertices.Add( new Vector3( point.x, outline.Height - height, point.y ) );
                uvs.Add( new Vector2( point.x, outline.Height - height ) );
            }

        }
        int pointCnt =  collapsed? vertices.Count / 2 : vertices.Count/ 6;
        List<int> pointIndices = new List<int>();
        for(int i = 0; i < pointCnt; i++) {
            if(collapsed)
                pointIndices.Add( i*2  ); //add wall ceiling point, bottom point will be calculated
            else
                pointIndices.Add( i*6 + 2 ); //add ceiling point, bottom point will be calculated
        }
        //build ceiling and floor
        HashSet<int> toRemove = new HashSet<int>();
        while(pointIndices.Count > 3) {
            for(int i = 0; i < pointIndices.Count-1; ) {
                Debug.Log( Vector3.Cross( vertices[pointIndices[(i + 2)% pointIndices.Count]] - vertices[pointIndices[i + 1]], vertices[pointIndices[i + 1]] - vertices[pointIndices[i]] ).y );
                if(Vector3.Cross( vertices[pointIndices[(i + 2) % pointIndices.Count]] - vertices[pointIndices[i + 1]], vertices[pointIndices[i + 1]] - vertices[pointIndices[i]] ).y > 0) { //if concave angle
                    indices.AddRange( new int[] { pointIndices[i], pointIndices[(i + 2) % pointIndices.Count], pointIndices[i + 1] } ); //ceiling
                    indices.AddRange( new int[] { pointIndices[i] + 1, pointIndices[(i + 2) % pointIndices.Count] + 1, pointIndices[i + 1] + 1 } ); //bottom
                    toRemove.Add( pointIndices[i + 1] );
                    i += 3;
                }
                else i++;
            }
            pointIndices.RemoveAll( x => toRemove.Contains(x) );
            if(toRemove.Count == 0)
                throw new System.Exception( "toRemove empty" );
            toRemove.Clear();
        }
        //if exactly three points remain, place final triangle
        if(pointIndices.Count == 3) { 
            indices.AddRange( new int[] { pointIndices[0], pointIndices[2], pointIndices[1] } ); //ceiling
            indices.AddRange( new int[] { pointIndices[0] + 1, pointIndices[2] + 1, pointIndices[1] + 1 } ); //bottom
        }

        /*FileStream flstr = File.Open( "testOut"+Random.Range(0,10000)+".obj", FileMode.OpenOrCreate, FileAccess.Write );
        StreamWriter strwrtr = new StreamWriter( flstr );
        foreach(Vector3 point in vertices)
            strwrtr.WriteLine( string.Format( "v {0} {1} {2}", point.x, point.y, point.z ) );
        for(int i = 0; i < indices.Count; i += 3) {
            strwrtr.WriteLine( string.Format( "f {0} {1} {2}", indices[i], indices[i + 1], indices[i + 2] ) );
        }
        strwrtr.Close();
        flstr.Close();*/

        outMesh.vertices = vertices.ToArray();
        outMesh.triangles = indices.ToArray();
        outMesh.uv = uvs.ToArray();
        outMesh.RecalculateBounds();
        outMesh.RecalculateNormals();

        return outMesh;

    }

    public override bool IsUsable() {
        return !outline.Complete || !outline.Enclosed;
    }

    public override void Build() {
        throw new NotImplementedException();
    }

    protected override void UpdateShape() {
        throw new NotImplementedException();
    }

    public override void UpdateWorld() {
        throw new NotImplementedException();
    }

    static Wall(){
        PlacedBuilding.factoryList.Add("Wall", (gobj) => gobj.AddComponent<Wall>() );
    }


}
