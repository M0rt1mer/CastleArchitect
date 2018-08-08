using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Linq;

public class NavMeshTest {

	[Test]
	public void TestOptimize()
	{
        IndexedTriangleMesh itm = new IndexedTriangleMesh();

        Vector3[] vectors = new Vector3[] { new Vector3(1,0,1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1), new Vector3(-1, 0, 1), new Vector3(0, 0, 0) };
        IndexedTriangleMesh.Triangle[] trg = new IndexedTriangleMesh.Triangle[]{    new IndexedTriangleMesh.Triangle( new Vector3[] { vectors[0], vectors[1] , vectors[4] } ),
                                                                                    new IndexedTriangleMesh.Triangle(new Vector3[] { vectors[1], vectors[2], vectors[4] }),
                                                                                    new IndexedTriangleMesh.Triangle(new Vector3[] { vectors[2], vectors[3], vectors[4] }),
                                                                                    new IndexedTriangleMesh.Triangle(new Vector3[] { vectors[3], vectors[0], vectors[4] }) };

        foreach (IndexedTriangleMesh.Triangle trig in trg)
            itm.AddTriangleByReference(trig);
        //Act
		itm.OptimizeMesh();

        //Assert
        //The object has a new name
        Mesh testM = itm.ExtractMeshes().First();

        NUnit.Framework.Assert.AreEqual( 4, testM.vertexCount, "Mesh was not optimized" );
	}

    [Test]
    public void TestAddRemove() {

        Vector3[] rndVectors = Enumerable.Range(1, 11).Select(x => Random.insideUnitCircle.toVec3()).ToArray();
        //IndexedTriangleMesh.Triangle[] trgs = Enumerable.Range(1, numTriagsPerRound * numRounds).Select( x => new IndexedTriangleMesh.Triangle( rndVectors[Random.Range(0,999)], rndVectors[Random.Range(0, 999)], rndVectors[Random.Range(0, 999)])  ).ToArray();

        C5.ArrayList<IndexedTriangleMesh.Triangle> triags = new C5.ArrayList<IndexedTriangleMesh.Triangle>();

        IndexedTriangleMesh itm = new IndexedTriangleMesh();

        for (int j = 0; j < 100; j++)
        {
            for (int i = 0; i < 10; i++)
            {

                if (triags.Count > 0 & Random.value < 0.33f)
                {
                    IndexedTriangleMesh.Triangle t = triags[Random.Range(0, triags.Count - 1)];
                    itm.RemoveTriangle(t);
                    triags.Remove(t);
                }
                else
                {
                    itm.AddTriangleByReference(new IndexedTriangleMesh.Triangle(rndVectors[Random.Range(0, rndVectors.Length - 1)],
                        rndVectors[Random.Range(0, rndVectors.Length - 1)], rndVectors[Random.Range(0, rndVectors.Length - 1)]));
                }

            }
            NUnit.Framework.Assert.IsTrue(itm.SanityCheck(), "Sanity check failed");
        }

    }
}
