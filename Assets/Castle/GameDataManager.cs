using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class GameDataManager : MonoBehaviour {

    //assigned in inspector
    public TextAsset dataSource;

    CatalogDB catalogDB = new CatalogDB();

    public CatalogDB CatalogDB {
        get {
            return catalogDB;
        }
    }

    // Use this for initialization
    void Awake () {
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor( typeof(PlacedBuilding).TypeHandle );
        foreach (Type type in typeof(GameDataManager).Assembly.GetTypes())
            if( type != typeof(PlacedBuilding) && typeof(PlacedBuilding).IsAssignableFrom ( type ) )
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);

        CatalogLoader ldr = new CatalogLoader(catalogDB, new Type[] { typeof(BuildingData), typeof(TileTerrain) });
        ldr.Load( new StringReader(dataSource.text) );
        catalogDB.InitializeAll();
	}
	
}
