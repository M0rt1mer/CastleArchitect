using UnityEngine;
using System.Collections;

public class MapTile : MonoBehaviour {

    public TileTerrain terrain;
    public MapCubeCoord coord;

    public TileTerrain Terrain {
        get {
            return terrain;
        }

        set {
            terrain = value;
            UpdateObject();
        }
    }

    public MapCubeCoord Coord {
        get {
            return coord;
        }

        set {
            coord = value;
            UpdateObject();
        }
    }

    void UpdateObject () {
        if(GetComponent<SpriteRenderer>() == null) {
            gameObject.AddComponent<SpriteRenderer>();
        }
        SpriteRenderer rndr = GetComponent<SpriteRenderer>();
        if(terrain == null) {
            rndr.enabled = false;
        } else {
            rndr.enabled = true;
            rndr.sprite = terrain.iconSprite;
        }
        Vector2 pos = coord.toPixel();
        transform.position = new Vector2( pos.y, pos.x );
	}


}