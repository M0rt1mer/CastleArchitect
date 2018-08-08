using UnityEngine;

static class NavMeshBuilder {

    public static IndexedTriangleMesh BuildTerrainNavmesh( Terrain terrain ) {



        IndexedTriangleMesh itm = new IndexedTriangleMesh();
        TerrainData tdata = terrain.terrainData;

        float[,] heightmap = tdata.GetHeights( 0, 0, tdata.heightmapWidth, tdata.heightmapHeight );
        Vector3[,] vertices = new Vector3[ heightmap.GetLength(0), heightmap.GetLength(1) ];
        for (int y = 0; y < heightmap.GetLength(0); y++){
            for (int x = 0; x < heightmap.GetLength(1); x++){
                vertices[y, x] = HeightmapToVector(heightmap, x, y, tdata.heightmapScale );
            }
        }


        for (int y = 1; y < heightmap.GetLength(0); y++) {
            for(int x = 1; x < heightmap.GetLength( 1 ); x++) {
                /*for(int y = 1; y < 3; y++) {
                    for(int x = 1; x < 3; x++) {*/
                //IndexedTriangleMesh.Triangle triag = new IndexedTriangleMesh.Triangle( HeightmapToVector( heightmap, x, y, tdata.heightmapScale ), HeightmapToVector( heightmap, x, y - 1, tdata.heightmapScale ), HeightmapToVector( heightmap, x - 1, y, tdata.heightmapScale ) );
                IndexedTriangleMesh.Triangle triag = new IndexedTriangleMesh.Triangle(vertices[y, x], vertices[y-1, x], vertices[y,x-1]);
                if ( checkTriangleSlope(triag.vertices) )
                    itm.AddTriangleByReference( triag );
                //triag = new IndexedTriangleMesh.Triangle( HeightmapToVector( heightmap, x - 1, y - 1, tdata.heightmapScale ), HeightmapToVector( heightmap, x - 1, y, tdata.heightmapScale ), HeightmapToVector( heightmap, x, y - 1, tdata.heightmapScale ) );
                triag = new IndexedTriangleMesh.Triangle(vertices[y-1, x-1], vertices[y, x - 1], vertices[y-1, x]);
                if (checkTriangleSlope( triag.vertices ))
                    itm.AddTriangleByReference( triag );
            }
        }

        return itm;
    }

    private static Vector3 HeightmapToVector( float[,] heightmap, int x, int y, Vector3 scale ) {
        return new Vector3( x * scale.x, heightmap[y,x] * scale.y, y*scale.z );
    }


    /// <summary>
    /// CHeck whether given triangle's slope is below walkable limit. Expects 
    /// </summary>
    /// <param name="heightA"></param>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <returns></returns>
    private static bool checkTriangleSlope( Vector3[] vertices ) {

        foreach(Vector3[] pair in vertices.SlidingWindow( 2 )) {
            
            float slope = Mathf.Atan2( pair[0].y - pair[1].y, (pair[0]-pair[1]).toVec2().magnitude );
            if(Mathf.Abs( slope ) > 0.5f) { // angle > half a rad
                return false;
            }
        }

        return true;
    }

}

