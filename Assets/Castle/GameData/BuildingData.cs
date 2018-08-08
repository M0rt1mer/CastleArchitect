using UnityEngine;
using System.Collections.Generic;

[CatalogEntryInfo(name = "Building")]
public class BuildingData : CatalogItem {



    [CatalogLoaded]
    public string name;
    [CatalogLoaded]
    public string icon;
    public Sprite iconSprite;
    [CatalogLoaded(name ="class")]
    public string buildingClass;
    [CatalogLoaded(name="misc")]
    public Dictionary<string, string> extraInfo;
    [CatalogLoaded]
    public string material;
    public Material mat;


    //public BuildingClass specificBuilding;

    public BuildingData(string id):base(id){ }

    public BuildingData(string id, string name, string icon, bool meta):base(id){
        this.name = name;
        this.icon = icon;
    }

    public override string ToString() {
        return string.Format("[BuildingClass: {0}]", name);
    }

    public override void Initialize() {
        iconSprite = Resources.Load(icon.Trim(), typeof(Sprite) ) as Sprite;
        if( material != null )
            mat = Resources.Load<Material>( material.Trim() );
    }

}
