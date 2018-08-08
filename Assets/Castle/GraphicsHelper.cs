using UnityEngine;
using Net3dBool;
using System.Linq;

public abstract class GraphicsHelper {

    public static Mesh cubeModel;
    public static Mesh sphereModel;

    //needs to be called in unity thread
    public static void Initialize() {
        //Cube line helper
        GameObject tmpObj = GameObject.CreatePrimitive( PrimitiveType.Cube );
        cubeModel = tmpObj.GetComponent<MeshFilter>().sharedMesh;
        GameObject.Destroy( tmpObj );
        //sphere line helper
        tmpObj = GameObject.CreatePrimitive( PrimitiveType.Sphere );
        sphereModel = tmpObj.GetComponent<MeshFilter>().sharedMesh;
        GameObject.Destroy( tmpObj );
    }

    public static Vector3 getProjectedPoint( Vector3 orig ) {
        RaycastHit hit;
        if(Physics.Raycast( orig + Vector3.up * 200, -Vector3.up, out hit, 1000 )) {
            orig.y = hit.point.y;
        }
        return orig;
    }

    public static void DrawLine( Vector3 start, Vector3 end, Material mat ) {
        if(end != start) {
            Quaternion facing = Quaternion.LookRotation( end - start );
            Graphics.DrawMesh( cubeModel, Matrix4x4.TRS( (start + end) / 2, facing, new Vector3( 0.05f, 0.05f, Vector3.Distance( start, end ) ) ), mat, 0, null, 0, null, false );
        }
    }




}

