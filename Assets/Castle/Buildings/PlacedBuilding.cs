using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class PlacedBuilding : MonoBehaviour  {

    protected BuildingData bldData;
    public Outline outline;

    protected virtual void Start() {
        outline.OnShapeChange += UpdateShape;
        outline.OnStateChange += UpdateShape;
    }

    /// <summary>
    /// Called when the building is finalized
    /// </summary>
    abstract public void Build();

    /// <summary>
    /// Called automatically on Outline update
    /// </summary>
    /// <returns></returns>
    abstract protected void UpdateShape();

    /// <summary>
    /// Called from builder whenever some other building changes
    /// </summary>
    abstract public void UpdateWorld();

    abstract public bool IsUsable();

    #region instantiating based on name (from Xml GameData)
    /// <summary>
    /// A function that adds required child of PlacedBuilding on given gameobject
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public delegate PlacedBuilding Factory( GameObject obj );
    public static Dictionary<string, Factory> factoryList = new Dictionary<string, Factory>();
    public static PlacedBuilding CreatePlacedBuilding( BuildingData bldData, Outline outline, GameObject obj ) {
        PlacedBuilding pb = factoryList[bldData.buildingClass]( obj );
        pb.bldData = bldData;
        pb.outline = outline;
        return pb;
    }
    #endregion

}
