using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Map : MonoBehaviour {

    Dictionary<MapCubeCoord, MapTile> tiles = new Dictionary<MapCubeCoord, MapTile>();

	// Use this for initialization
	void Start () {

        MapCubeCoord[][] rings = new MapCubeCoord[7][];
        rings[0] = new MapCubeCoord[1];
        rings[0][0] = new MapCubeCoord( 0, 0, 0 );

        for(int r = 1; r < rings.Length ; r++) {
            rings[r] = new MapCubeCoord[6*r];
            for(int d = 0; d < 6; d++) {
                rings[r][d * r] = rings[r - 1][d * (r - 1)].getNeighbors()[d];
                for(int o = 1; o < r; o++) {
                    rings[r][d*r+o] = rings[r][d * r + o - 1].getNeighbors()[(d+2)%6];
                }
            }
        }

        for(int r = 0; r < rings.Length; r++)
            for(int i = 0; i < rings[r].Length; i++)
                if(rings[r][i] != null) {
                    GameObject gobj = new GameObject( "Map tile at" + rings[r][i], new System.Type[] { typeof( MapTile ) } );
                    gobj.transform.SetParent( transform, false );
                    gobj.layer = gameObject.layer;
                    MapTile tile = gobj.GetComponent<MapTile>();
                    tile.Coord = rings[r][i];
                    tiles.Add( rings[r][i], tile );
                }

        GameDataManager gdm = GameObject.FindObjectOfType<GameDataManager>();
        TileTerrain[] terrains = gdm.CatalogDB.GetCatalog<TileTerrain>().ToArray();
        foreach(MapTile tile in tiles.Values)
            tile.Terrain = terrains[Random.Range( 0, terrains.Length )];
	}
	
}
