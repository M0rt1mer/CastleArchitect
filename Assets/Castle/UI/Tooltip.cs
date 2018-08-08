using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour {

    public delegate IEnumerable<string> getCurrentTooltip();
    public List<getCurrentTooltip> tooltipCallback = new List<getCurrentTooltip>();

	// Update is called once per frame
	void Update () {
        IEnumerable<string> tooltips = tooltipCallback.SelectMany(x => x());
        transform.GetChild(0).gameObject.SetActive(tooltips.Count<string>() > 0);
        if( tooltips.Count<string>() > 0 )
            GetComponentInChildren<Text>().text = string.Join("\n", tooltips.ToArray<string>() );
        transform.position = Input.mousePosition;
    }
}
