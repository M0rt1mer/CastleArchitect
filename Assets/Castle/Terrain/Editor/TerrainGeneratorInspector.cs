using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorInspector : Editor {

    [SerializeField]
    int numSteps = 5;

    [SerializeField]
    int resolution = 512;


    Texture2D heightmapImage = null;


    public override void OnInspectorGUI() {

        TerrainGenerator tg = target as TerrainGenerator;
        
        GUILayout.BeginHorizontal();
        GUILayout.Label( "Resolution" );
        resolution = Mathf.ClosestPowerOfTwo( EditorGUILayout.IntField( resolution ) ) + 1;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label( "Numsteps" );
        numSteps = EditorGUILayout.IntField( numSteps );
        GUILayout.EndHorizontal();



        GUILayout.BeginHorizontal();
        
        if(GUILayout.Button( "Init flat" )) {
            tg.Initialize( numSteps, resolution );
            UpdateImage( tg.terrainGeneratorData.heighmap, tg.waterErosion.waterHeight );
            tg.UpdateTerrain();
        }

        if(GUILayout.Button( "Init noisy" )) {
            tg.Initialize( numSteps, resolution );
            tg.GenerateHeightMap( numSteps );
            UpdateImage( tg.terrainGeneratorData.heighmap, tg.waterErosion.waterHeight );
            tg.UpdateTerrain();
        }

        GUILayout.EndHorizontal();

        /*GUILayout.BeginHorizontal();
        if(GUILayout.Button( "Initialize Noise" )) {
            SquareDiamondNoise.InitializeBetterSquareDiamondNoise( numSteps );
            UpdateImage( SquareDiamondNoise.smallHeightmap, null );
        }
        if(GUILayout.Button( "Step noise" )) {
            SquareDiamondNoise.BetterSquareDiamondSingleStep();
            UpdateImage( SquareDiamondNoise.smallHeightmap, null );
        }
        GUILayout.EndHorizontal();*/

        GUILayout.BeginHorizontal();
        if (GUILayout.Button( "Water step" )) {
            tg.WaterStep();
            UpdateImage( tg.terrainGeneratorData.heighmap, tg.waterErosion.waterHeight );
            tg.UpdateTerrain();
        }
        if (GUILayout.Button("Thermal step"))
        {
            tg.ThermalStep();
            UpdateImage(tg.terrainGeneratorData.heighmap, tg.waterErosion.waterHeight);
            tg.UpdateTerrain();
        }
        GUILayout.EndHorizontal();

        if (heightmapImage != null) {
            //GUILayout.BeginHorizontal();
            //GUILayout.Box( heightmapImage );
            // GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            Rect reserved = GUILayoutUtility.GetRect( 400, 400, GUILayout.ExpandWidth(true) );
            GUILayout.EndHorizontal();
            EditorGUI.DrawPreviewTexture( reserved, heightmapImage, null, ScaleMode.ScaleToFit );
        }
            
        

    }

    private void UpdateImage( float[,] heighmap, float[,] waterHeight ) {

        //int displayRatio = Mathf.Min( 8, Mathf.Max( Mathf.FloorToInt( 300f / heighmap.GetLength( 0 ) ), 1 ) );

        //Debug.Log( displayRatio );

        /*heightmapImage = new Texture2D( heighmap.GetLength( 0 ) * displayRatio, heighmap.GetLength( 1 ) * displayRatio, TextureFormat.ARGB32, false );
        for(int x = 0; x < heighmap.GetLength( 0 ); x++) {
            for(int y = 0; y < heighmap.GetLength( 1 ); y++) {
                float value = heighmap[x, y];// * 3 - 0.9f;
                                             //min = Mathf.Min( min, value );
                                             //max = Mathf.Max( max, value );
                Color clr = new Color(value,value,value,1);
                if( waterHeight != null )
                    clr = Color.HSVToRGB( 240, Mathf.Max( waterHeight[x, y], 1 ), value );
                for(int x1 = 0; x1 < displayRatio; x1++)
                    for(int y1 = 0; y1 < displayRatio; y1++)
                        heightmapImage.SetPixel( x * displayRatio + x1, y * displayRatio + y1, clr );
            }
        }*/
        heightmapImage = new Texture2D( heighmap.GetLength( 0 ), heighmap.GetLength( 1 ), TextureFormat.ARGB32, false );
        for(int x = 0; x < heighmap.GetLength( 0 ); x++) {
            for(int y = 0; y < heighmap.GetLength( 1 ); y++) {
                float value = heighmap[x, y];// * 3 - 0.9f;
                                             //min = Mathf.Min( min, value );
                                             //max = Mathf.Max( max, value );
                Color clr = new Color( value, value, value, 1 );
                if(waterHeight != null)
                    clr = Color.HSVToRGB( 0.66f, waterHeight[x, y], value );
                heightmapImage.SetPixel( x, y, clr );
            }
            heightmapImage.Apply();
        }
    }


}
