using UnityEngine;

[System.Serializable]
public class TileTerrain : CatalogItem {

    [CatalogLoaded]
    public string name;

    [CatalogLoaded]
    public string icon;
    public Sprite iconSprite;

    public TileTerrain( string id ) : base( id ) {
    }

    public override void Initialize() {
        iconSprite = Resources.Load( icon.Trim(), typeof( Sprite ) ) as Sprite;
    }

}

