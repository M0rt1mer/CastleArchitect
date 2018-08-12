using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using Unity.Collections;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorInspector : Editor {

    [SerializeField]
    int numSteps = 5;

    [SerializeField]
    int resolution = 512;

    Texture2D heightmapImage = null;

    public void OnEnabled() {
        TerrainGenerator tg = target as TerrainGenerator;
        if(tg.terrainGeneratorData != null)
            UpdateImage( tg.terrainGeneratorData.size, tg.terrainGeneratorData.heightmap, null );
        else
            heightmapImage = null;
    }

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
            UpdateImage( tg.terrainGeneratorData.size, tg.terrainGeneratorData.heightmap, tg.waterErosion.waterHeight );
            tg.UpdateTerrain();
        }

        if(GUILayout.Button( "Init noisy" )) {
            tg.Initialize( numSteps, resolution );
            tg.GenerateHeightMap();
            UpdateImage( tg.terrainGeneratorData.size, tg.terrainGeneratorData.heightmap, tg.waterErosion.waterHeight );
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
            UpdateImage( tg.terrainGeneratorData.size, tg.terrainGeneratorData.heightmap, tg.waterErosion.waterHeight );
            tg.UpdateTerrain();
        }
        if (GUILayout.Button("Thermal step"))
        {
            tg.ThermalStep();
            UpdateImage( tg.terrainGeneratorData.size, tg.terrainGeneratorData.heightmap, tg.waterErosion.waterHeight);
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

    private void UpdateImage( int size, NativeArray<float> heighmap, NativeArray<float>? waterHeight ) {

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
        heightmapImage = new Texture2D( size, size, TextureFormat.ARGB32, false );
        for(int x = 0; x < size; x++) {
            for(int y = 0; y < size; y++) {
                float value = heighmap[x * size + y];// * 3 - 0.9f;
                                             //min = Mathf.Min( min, value );
                                             //max = Mathf.Max( max, value );
                Color clr = new Color( value, value, value, 1 );
                if(waterHeight.HasValue)
                    clr = Color.HSVToRGB( 0.66f, waterHeight.Value[x * size + y], value );
                heightmapImage.SetPixel( x, y, clr );
            }
            heightmapImage.Apply();
        }
    }


}
