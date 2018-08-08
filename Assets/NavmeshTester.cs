using UnityEngine;
using System.Collections;
using System;

public class NavmeshTester : MonoBehaviour {

    public Material mat;
    IndexedTriangleMesh itm;

	// Use this for initialization
	void Start () {
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        itm = NavMeshBuilder.BuildTerrainNavmesh( GameObject.FindObjectOfType<Terrain>() );
        Debug.Log( "Terrain built in " + watch.Elapsed );
        watch.Reset(); watch.Start();
        StartCoroutine(itm.OptimizeMesh());
        StartCoroutine(Draw());
	}

    public IEnumerator Draw() {
        while (true) {
            foreach (Transform t in transform)
                Destroy(t.gameObject);
            foreach (Mesh msh in itm.ExtractMeshes()){
                GameObject go = new GameObject("testMesh", new Type[] { typeof(MeshFilter), typeof(MeshRenderer) });
                go.GetComponent<MeshFilter>().mesh = msh;
                go.GetComponent<MeshRenderer>().material = mat;
                go.transform.SetParent(transform,false);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

}
