using UnityEngine;
using System.Collections;

public class TorchSlider : MonoBehaviour {
	public GameObject TorcheObj;
	public GUISkin SkinSlider;
	
    void OnGUI() {
		GUI.Label(new Rect(25,25,150,30),"Light Intensity",SkinSlider.label);
        TorcheObj.GetComponent<TorchLight>().Intensity = GUI.HorizontalSlider(new Rect(25, 50, 150, 30), TorcheObj.GetComponent<TorchLight>().Intensity, 0.0F, TorcheObj.GetComponent<TorchLight>().IntensityMax,SkinSlider.horizontalSlider,SkinSlider.horizontalSliderThumb);
    }
}