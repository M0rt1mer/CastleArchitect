using UnityEngine;
using System.Collections;
using System.Linq;

public class BuildingPanel : MonoBehaviour {

    public GameObject BuildingButtonPrefab;

    public Builder bldr;

	// Use this for initialization
	void Start () {
        bldr = GameObject.FindObjectOfType<Builder>();
        GameDataManager gdm = GameObject.FindObjectOfType<GameDataManager>();
        foreach( BuildingData bldng in gdm.CatalogDB.GetCatalog<BuildingData>() ){
            GameObject gobj = GameObject.Instantiate(BuildingButtonPrefab);
            gobj.transform.SetParent( transform.Find("Scroll View/Viewport/Content"), false );
            gobj.GetComponent<BuildingButtonPrefab>().bld = bldng;
        }
	}

    void Update() {
        transform.GetChild( 0 ).gameObject.SetActive( bldr.activeOutline != null );
        /*if( bldr.activeOutline != null)
            foreach(Transform trn in transform.Find( "Scroll View/Viewport/Content" ))
                trn.gameObject.SetActive( trn.GetComponent<BuildingButtonPrefab>().bld.buildingClass.IsUsable( bldr.activeOutline ) );*/
           
    }
}