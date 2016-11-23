using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUIReferenceScript : MonoBehaviour {

    public Slider healthSlider;
    public Slider energySlider;
    public Slider timeSlider;

	// Use this for initialization
	void Start () {
        if (healthSlider == null)
        {
            Debug.LogError("Health slider not initialized in GUI Holder");
            return;
        }
            
        if (energySlider == null)
        {
            Debug.LogError("Energy slider not initialized in GUI Holder");
            return;
        }
            
        if (timeSlider == null)
        {
            Debug.LogError("Time slider not initialized in GUI Holder");
            return;
        }
        ProceduralComponentConnector.AllocatePlayerGUILink(healthSlider, timeSlider, energySlider);
	}
}
