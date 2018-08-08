using UnityEngine;
using System.Collections;

public class TimeOfDaySystem : MonoBehaviour {

    public float speed = 1;

    [Range(0,24)]
    public float timeOfDay;

    [Range(0,365)]
    public float dayOfYear;

    [Range(-90,90)]
    public float latitude;

    public static float earthAxisInclination = 23.5f;
    public static float todToAngle = 360f / 24f;
    public static float dayOfYearToEarthsAngle = Mathf.PI * 2f / 365f;
    public static float offset = 0;

    public GameObject earthRotationObject;
    public GameObject sunObject;

	// Update is called once per frame
	void Update () {
        timeOfDay = (timeOfDay + speed * Time.deltaTime) % 24;
        UpdatePositions();
	}

    private void UpdatePositions() {
        transform.eulerAngles = Vector3.forward * (90 - latitude);
        earthRotationObject.transform.localEulerAngles = Vector3.up * (timeOfDay + offset) * todToAngle;
        sunObject.transform.localEulerAngles = new Vector3( GetEarthAxisInclination(), 90, 0 );
    }

    void OnValidate() {
        UpdatePositions();
    }

    private float GetEarthAxisInclination() {

        return /*(-latitude) +*/ earthAxisInclination * Mathf.Sin( dayOfYear * dayOfYearToEarthsAngle ) /*+ 180*/;

    }
}
