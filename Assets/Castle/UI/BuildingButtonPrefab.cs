using UnityEngine;
using UnityEngine.UI;

public class BuildingButtonPrefab : MonoBehaviour {

    public BuildingData bld;


    Builder bldr;

    bool wasActive;

    void Start() {

        transform.Find("Icon").GetComponent<Image>().sprite = bld.iconSprite;
        transform.Find("Name").GetComponent<Text>().text = bld.name;

        bldr = GameObject.FindObjectOfType<Builder>();
    }

    void Update() {
        bool active = (bldr.SelectedBuildingData == bld);
        if (active != wasActive) {
            wasActive = active;
            if (active) {
                GetComponent<Image>().color = Color.green;
            }
            else {
                GetComponent<Image>().color = Color.white;
            }
        }
    }

    public void Click() {
        if (bldr.SelectedBuildingData == bld)
            bldr.SelectedBuildingData = null;
        else
            bldr.SelectedBuildingData = bld;
    }

}
