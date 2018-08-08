using UnityEngine;
using System.Collections.Generic;
using C5;
using System.Linq;
using System.Collections;

/// <summary>
/// Stores triangles in some spacai tree, and also incidence
/// </summary>
public class IndexedTriangleMesh {

    private Dictionary<Vector3, ArrayList<Triangle>> vertex2Triangle = new Dictionary<Vector3, ArrayList<Triangle>>();
    private Dictionary<Triangle, ArrayList<Triangle>> triangleIncidence = new Dictionary<Triangle, ArrayList<Triangle>>();

    private C5.HashSet<Triangle> removed = new C5.HashSet<Triangle>();

    public IEnumerable<Mesh> ExtractMeshes() {

        List<Mesh> meshes = new List<Mesh>();

        C5.HashSet<Triangle> untouched = new C5.HashSet<Triangle>();
        untouched.AddAll( triangleIncidence.Keys );

        Queue<Triangle> open = new Queue<Triangle>();
        


        //the mesh that is beeing built
        List<Vector3> vertices = new List<Vector3>();
        Dictionary<Vector3, int> vertexToIndex = new Dictionary<Vector3, int>();
        List<int> indices = new List<int>();

        while(!untouched.IsEmpty) {

            if(open.Count == 0) { //empty - open on a new slot
                open.Enqueue( untouched.First() );
                untouched.Remove( open.First() );
            }

            if(vertices.Count > 60000) { //too many - split meshes
                Mesh msh = new Mesh();
                msh.vertices = vertices.ToArray();
                msh.triangles = indices.ToArray();
                meshes.Add( msh );
                vertices.Clear();
                vertexToIndex.Clear();
                indices.Clear();
            }

            //one step of BFS
            Triangle current = open.Dequeue();
            foreach(Triangle expand in triangleIncidence[current]) {
                if(untouched.Contains( expand )) {
                    open.Enqueue( expand );
                    untouched.Remove( expand );
                }
            }
            //add to mesh
            for(int i = 0; i < 3; i++) {
                if(vertexToIndex.ContainsKey( current.vertices[i] ))
                    indices.Add( vertexToIndex[current.vertices[i]] );
                else {
                    vertices.Add( current.vertices[i] );
                    vertexToIndex[current.vertices[i]] = vertices.Count - 1;
                    indices.Add( vertices.Count - 1 );
                }
            }
            
        }

        //buuild last mesh
        Mesh msh2 = new Mesh();
        msh2.vertices = vertices.ToArray();
        msh2.triangles = indices.ToArray();
        meshes.Add( msh2 );

        return meshes;
    }

    private static float maxHeightDifference = 5.5f;

    /// <summary>
    /// Optimizes navmesh by deleting some triangles
    /// </summary>
    /// <param name="itm"></param>
    public IEnumerator OptimizeMesh(){

        C5.HashSet<Vector3> open = new C5.HashSet<Vector3>();
        C5.HashSet<Triangle> openT = new C5.HashSet<Triangle>();
        open.AddAll( vertex2Triangle.Keys );

        while (!open.IsEmpty ) { //loop through all vertices

            Vector3 current = open.Pop();

            if (vertex2Triangle[current].Count < 3) //if less than 3, they cannot form a "circle"
                continue;

            openT.Clear();
            openT.AddAll(vertex2Triangle[current]);

            //Debug.Log("Opening " + current);

            //start at arbitrary triangle
            C5.IList<Triangle> circle = new C5.ArrayList<Triangle>();
            circle.Add(openT.Pop());

            //find connected circle
            while ( openT.Count > 0 ) {

                Triangle[] circleElement = openT.Intersect(triangleIncidence[circle.Last]).ToArray();
                if (circleElement.Length < 1) //no candidate - break
                    break;
                circle.Add(circleElement[0]);

                openT.Remove(circle.Last);
            }
            if( openT.Count == 0 && triangleIncidence[circle.Last].Contains(circle.First)) { 
                //Debug.Log( "Unbroken: "+current );
                //check height differences
                bool isFlatEnough = true;
                foreach (Triangle trg in vertex2Triangle[current]) {
                    foreach (Vector3 vct in trg.vertices)
                        if (Mathf.Abs(vct.y - current.y) > maxHeightDifference)
                            isFlatEnough = false;
                }
                if (!isFlatEnough)
                    continue;

                ArrayList<Vector3> vctList = new ArrayList<Vector3>();

                //remove old triangles
                foreach (Triangle trg in circle) { //ToArray() is to create a buffer (to prevent concurrent modification)
                    RemoveTriangle(trg);
                    for (int i = 0; i < 3; i++) {
                        //find the CURRENT vertex - the one that is beeing removed
                        if (trg.vertices[i] == current)
                            //add the NEXT to the list
                            vctList.Add(trg.vertices[(i + 1) % 3]);
                    }
                }

                if (Outline.IsClockwise(vctList))
                    vctList.Reverse();
                //build new triangles and add them
                foreach (IEnumerable<int> newTriangles in Utils.TriangulatePolygon(vctList, Enumerable.Range(0, vctList.Count)).Chunks(3)) {
                    Triangle newTriag = new Triangle(newTriangles.Select(x => vctList[x]).ToArray());
                    AddTriangleByReference(newTriag);
                }

                //reopen used vertices
                //open.AddAll(vctList); - maybe not necessary????
                //yield return new WaitForSeconds(1);
                yield return null;
            }
            
        }

    }

    /// <summary>
    /// Cleanly removes triangle
    /// </summary>
    /// <param name="trg"></param>
    public void RemoveTriangle(Triangle trg) {
        //Debug.Log("Remove "+trg);
        foreach (Vector3 vct in trg.vertices) {
            vertex2Triangle[vct].Remove(trg);
            if (vertex2Triangle[vct].Count == 0)
                vertex2Triangle.Remove(vct);
        }

        foreach (Triangle incidenct in triangleIncidence[trg]) {
            triangleIncidence[incidenct].Remove(trg);
        }
        triangleIncidence.Remove(trg);
        removed.Add(trg);
    }

    /// <summary>
    /// Adds triangle. If his Vectors should align with another triangles vectors, they need to be the same instances
    /// </summary>
    /// <param name="trngls"></param>
    public void AddTriangleByReference(Triangle trngls) {

        C5.HashSet<Triangle> incidence = new C5.HashSet<Triangle>();
        triangleIncidence.Add(trngls, new ArrayList<Triangle>());
        //register each vertex
        foreach (Vector3 vertex in trngls.vertices) {
            if (!vertex2Triangle.ContainsKey(vertex))
                vertex2Triangle[vertex] = new ArrayList<Triangle>();
            else
                foreach (Triangle triangle in vertex2Triangle[vertex]){
                    if (incidence.Contains(triangle)){ //already found a common vertex
                        triangleIncidence[triangle].Add(trngls);
                        triangleIncidence[trngls].Add(triangle);
                    }
                    else incidence.Add(triangle); //first common vertex found -> remember it
                }
            vertex2Triangle[vertex].Add(trngls);
        }

    }

    public bool SanityCheck() {
        bool sane = true;
        foreach (Vector3 vertex in vertex2Triangle.Keys) {
            foreach (Triangle trg in vertex2Triangle[vertex]){
                if (!trg.vertices.Contains(vertex)){
                    Debug.Log(string.Format("{0} is wrongly registered under {1}", trg, vertex));
                    sane = false;
                }
                if (!triangleIncidence.ContainsKey(trg)){
                    Debug.Log(string.Format("{0} registered for {1}, but deleted", trg, vertex));
                    sane = false;
                }
            }
        }
        foreach (Triangle trg in triangleIncidence.Keys) {
            foreach (Vector3 vector in trg.vertices) {
                if (!vertex2Triangle[vector].Contains(trg)) {
                    Debug.Log( string.Format("Vertex {0} is not registered for {1}", vector, trg ) );
                    sane = false;
                }
            }
        }
        return sane;
    }

    public class Triangle {

        public Triangle(Vector3 a, Vector3 b, Vector3 c) {
            this.vertices = new Vector3[] { a, b, c };
        }

        public Triangle(Vector3[] vertices ) {
            this.vertices = (Vector3[]) vertices.Clone();
            /*if (vertices[0] == vertices[1] || vertices[1] == vertices[2] || vertices[0] == vertices[2])
                throw new System.ArgumentException("Collapsed triangle");*/
        }

        public Vector3[] vertices;

        public override string ToString(){
            return string.Format("T[ {0}, {1}, {2} ]", vertices[0], vertices[1], vertices[2]) ;
        }
    }
}

