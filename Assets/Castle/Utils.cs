using UnityEngine;
using Net3dBool;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

public static class Utils {


    public static Mesh Solid2Mesh( Solid solid ) {
        List<Vector3> vertices = new List<Vector3>();

        int[] origIndices = solid.getIndices();
        int[] indices = new int[origIndices.Length];

        Vector3[] originalVertices = solid.getVertices().Select( f => new Vector3( (float)f.x, (float)f.y, (float)f.z ) ).ToArray();
        for(int i = 0; i < indices.Length; i++) {
            vertices.Add( originalVertices[origIndices[i]] );
            indices[i] = vertices.Count - 1;
        }

        Mesh msh = new Mesh();
        msh.vertices = vertices.ToArray();
        msh.triangles = indices;

        msh.RecalculateBounds();
        msh.RecalculateNormals();

        return msh;
    }

    public static Solid Mesh2Solid( Mesh msh ) {
        return new Solid( msh.vertices.Select( x => x.toPoint3d() ).ToArray() , msh.triangles, Enumerable.Range( 0, msh.vertexCount ).Select( x => new Color3f( 0, 0, 0 ) ).ToArray() );
    }

    public static Vector3 toVector3( this Tuple3d v ) {
        return new Vector3( (float) v.x, (float)v.y, (float)v.z );
    }

    public static Point3d toPoint3d( this Vector3 v ) {
        return new Point3d( v.x, v.y, v.z );
    }

    public static Vector3 toVec3( this Vector2 vec ) {
        return new Vector3( vec.x, 0, vec.y );
    }

    public static Vector2 toVec2( this Vector3 vec ) {
        return new Vector2( vec.x, vec.z );
    }

    // TODO: use custom class and enumerator to run in a single pass
    public static IEnumerable<T[]> SlidingWindow<T>( this IEnumerable<T> e, int width ) {
        
        T[] source = e.ToArray();
        T[][] output = new T[source.Length][];

        for(int i = 0; i < source.Length; i++) {
            output[i] = new T[width];
            for(int j = 0; j < width; j++)
                output[i][j] = source[(i + j) % source.Length];
        }
        return output;
    }

    public static IEnumerable<IEnumerable<T>> Chunks<T>(this IEnumerable<T> enumerable,
                                                        int chunkSize){
        if (chunkSize < 1) throw new ArgumentException("chunkSize must be positive");

        using (var e = enumerable.GetEnumerator())
            while (e.MoveNext())
            {
                var remaining = chunkSize;    // elements remaining in the current chunk
                var innerMoveNext = new Func<bool>(() => --remaining > 0 && e.MoveNext());

                yield return e.GetChunk(innerMoveNext);
                while (innerMoveNext()) {/* discard elements skipped by inner iterator */}
            }
    }

    private static IEnumerable<T> GetChunk<T>(this IEnumerator<T> e,
                                          Func<bool> innerMoveNext)
    {
        do yield return e.Current;
        while (innerMoveNext());
    }

    /// <summary>
    /// Returns first element using IEnumerable.First() and removes it from given collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="e"></param>
    /// <returns></returns>
    public static T Pop<T>(this ICollection<T> e) {
        T ret = e.First();
        e.Remove(ret);
        return ret;
    }

    /// <summary>
    /// Rotates vector 90 degrees around imaginary Z axis
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector2 perpendicular( this Vector2 vec ) {
        return new Vector2( vec.y, -vec.x );
    }

    /// <summary>
    /// Rotates vector 90 degrees around Y axis
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector3 perpendicularY( this Vector3 vec ) {
        return new Vector3( vec.z, vec.y, -vec.x );
    }

    public static float AngleTo2D(this Vector3 ths, Vector3 other) {
        return Mathf.Atan2(other.z, other.x) - Mathf.Atan2(ths.z, ths.x);
    }

    /// <summary>
    /// Builds a list of triangles from a list of vertices. If vertices are clockwise, results in clockwise triangles
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="useIndices"></param>
    /// <returns></returns>
    public static IEnumerable<int> TriangulatePolygon( IList<Point3d> vertices, IEnumerable<int> useIndices ) {

        /*List<int> outTriangles = new List<int>();
        List<int> indicesToUse = new List<int>( useIndices );

        while(indicesToUse.Count > 3) {
            int n = indicesToUse.Count;
            float minAngle = 360;
            int minIndex = 0;
            //find smallest angle
            for(int i = 0; i < indicesToUse.Count; i++) {
                float angle = Vector3.Angle( (vertices[indicesToUse[(i - 1 + n) % n]] - vertices[indicesToUse[i]]).toVector3(), (vertices[indicesToUse[(i - 1 + n) % n]] - vertices[indicesToUse[i]]).toVector3() );
                if(angle < minAngle) {
                    minAngle = angle;
                    minIndex = i;
                }
            }

            outTriangles.AddRange( new int[] { indicesToUse[ (minIndex - 1 + n) %n], indicesToUse[minIndex], indicesToUse[(minIndex + 1)%n] } );
            indicesToUse.RemoveAt( minIndex );
        }

        //create last triangle
        outTriangles.AddRange( indicesToUse );
        return outTriangles;*/
        return TriangulatePolygon(vertices.Select(x => x.toVector3()).ToArray(),useIndices);
    }

    /// <summary>
    /// Builds a list of triangles from a list of vertices. If vertices are clockwise, results in clockwise triangles
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="useIndices"></param>
    /// <returns></returns>
    public static IEnumerable<int> TriangulatePolygon( IList<Vector3> vertices, IEnumerable<int> useIndices)
    {

        List<int> outTriangles = new List<int>();
        List<int> indicesToUse = new List<int>(useIndices);

        while (indicesToUse.Count > 3)
        {
            int n = indicesToUse.Count;
            float minAngle = 360;
            int minIndex = 0;
            //find smallest angle
            for (int i = 0; i < indicesToUse.Count; i++)
            {
                //float angle = Vector3.Angle( vertices[indicesToUse[(i - 1 + n) % n]] - vertices[indicesToUse[i]], vertices[indicesToUse[(i - 1 + n) % n]] - vertices[indicesToUse[i]]);
                float angle = (vertices[indicesToUse[(i - 1 + n) % n]] - vertices[indicesToUse[i]]).AngleTo2D(vertices[indicesToUse[(i - 1 + n) % n]] - vertices[indicesToUse[i]]);
                if (angle < minAngle)
                {
                    minAngle = angle;
                    minIndex = i;
                }
            }

            outTriangles.AddRange(new int[] { indicesToUse[(minIndex - 1 + n) % n], indicesToUse[minIndex], indicesToUse[(minIndex + 1) % n] });
            indicesToUse.RemoveAt(minIndex);
        }

        //create last triangle
        outTriangles.AddRange(indicesToUse);
        return outTriangles;
    }


}
