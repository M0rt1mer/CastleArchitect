using UnityEngine;
using System;

[System.Serializable]
public class MapCubeCoord{

	public float x;
	public float y;
	public float z;

	public MapCubeCoord (float x, float y, float z){
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public override bool Equals (object obj)
	{
		if (obj == null)
			return false;
		if (ReferenceEquals (this, obj))
			return true;
		if (obj.GetType () != typeof(MapCubeCoord))
			return false;
		MapCubeCoord other = (MapCubeCoord)obj;
		return x == other.x && y == other.y && z == other.z;
	}

	public override int GetHashCode ()
	{
		unchecked {
			return x.GetHashCode () ^ y.GetHashCode () ^ z.GetHashCode ();
		}
	}

	public override string ToString ()
	{
		return string.Format ("[{0}, {1}, {2}]", x, y, z);
	}

	public MapCubeCoord[] getNeighbors(){
		MapCubeCoord[] nb = new MapCubeCoord[6];
		nb [0] = new MapCubeCoord (x+1, y-1, z);
		nb [1] = new MapCubeCoord (x+1, y, z-1);
		nb [2] = new MapCubeCoord (x, y+1, z-1);
		nb [3] = new MapCubeCoord (x-1, y+1, z);
		nb [4] = new MapCubeCoord (x-1, y, z+1);
		nb [5] = new MapCubeCoord (x, y-1, z+1);
		return nb;
	}

	public MapCubeCoord[] getCloseNeighbors(){
		MapCubeCoord[] nb = new MapCubeCoord[6];
		nb [0] = new MapCubeCoord ( x+1,y, z );
		nb [1] = new MapCubeCoord ( x,y-1, z );
		nb [2] = new MapCubeCoord ( x,y, z+1 );
		nb [3] = new MapCubeCoord ( x-1,y, z );
		nb [4] = new MapCubeCoord ( x,y+1, z );
		nb [5] = new MapCubeCoord ( x,y, z-1 );
		return nb;
	}

	public bool isNeigbor(MapCubeCoord other){
		return (Mathf.Abs (x - other.x) + Mathf.Abs (y - other.y) + Mathf.Abs (z - other.z)) == 2;
	}

	float X {
		get {
			return this.x;
		}
	}

	float Y {
		get {
			return this.y;
		}
	}

	float Z {
		get {
			return this.z;
		}
	}
	//----------------------
	public Vector2 toPixel(){
		return toPixel (1);
	}
	public Vector2 toPixel(float size){
		return new Vector2 ( (x - (y + z)/2 ) * size, 
		                    Mathf.Sqrt(3) *(y-z)/2 * size);
	}
	public Vector3 toWorld(){
		Vector2 pixel = toPixel (1);
		return new Vector3 (pixel.x,0,pixel.y);
	}
	public Vector3 toWorld(float size){
		Vector2 pixel = toPixel (size);
		return new Vector3 (pixel.x,0,pixel.y);
	}
	//-------------------------------
	public static MapCubeCoord PixelToHex(Vector2 vct, float size){
		return PixelToHex (vct.x, vct.y,size);
	}
	public static MapCubeCoord PixelToHex(Vector2 vct){
		return PixelToHex (vct, 1);
	}
	public static MapCubeCoord PixelToHex(float x, float y, float size){
		float hX = x*2/3/size;
		float hZ = (-x / 3 + Mathf.Sqrt(3)/3 * y) / size;
		return cube_round(hX,hZ,-hX-hZ);
	}
	public static MapCubeCoord PixelToHex( float x,float y){
		return PixelToHex (x, y, 1);
	}
	public static Vector3 PixelToUnroundedHex( float x, float y, float size ){
		float hX = x*2/3/size;
		float hZ = (-x / 3 + Mathf.Sqrt(3)/3 * y) / size;
		return new Vector3(hX,hZ,-hX-hZ);
	}
	public static MapCubeCoord cube_round(float x, float y, float z){
		int rx = Mathf.RoundToInt(x);
		int ry = Mathf.RoundToInt(y);
		int rz = Mathf.RoundToInt(z);
			
		float x_diff = Mathf.Abs (rx - x);
		float y_diff = Mathf.Abs (ry - y);
		float z_diff = Mathf.Abs(rz - z);
		
		if (x_diff > y_diff && x_diff > z_diff)
			rx = -ry - rz;
		else {
			if(y_diff > z_diff)
				ry = -rx-rz;
			else
				rz = -rx-ry;
			}
		return new MapCubeCoord (rx, ry, rz);
	}

}


