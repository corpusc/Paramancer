using UnityEngine;
using System.Collections;



public class TorchLight : MonoBehaviour {
	public GameObject GO;
	public GameObject MainFlame;
	public GameObject BaseFlame;
	public GameObject Etincelles;
	public GameObject Fumee;
	public float IntensityMax = 4f;
	public float Intensity = 2f; // ...of the light 

	// private 
	float randomFlickerOffset;



	void Start() {
		randomFlickerOffset = Random.Range(0f, Mathf.PI*2);
		GO.light.range = 15f;
		GO.light.intensity = Intensity;
		MainFlame.GetComponent<ParticleSystem>().emissionRate = Intensity*20f;
		BaseFlame.GetComponent<ParticleSystem>().emissionRate = Intensity*15f;	
		Etincelles.GetComponent<ParticleSystem>().emissionRate = Intensity*7f;
		Fumee.GetComponent<ParticleSystem>().emissionRate = Intensity*12f;
	}
	
	void Update() {
		if (Intensity < 0) 
			Intensity = 0;

		if (Intensity > IntensityMax) 
			Intensity = IntensityMax;		

		GO.light.intensity = Intensity / 2f + Mathf.Lerp(Intensity-0.3f, Intensity+0.3f, Mathf.Cos(randomFlickerOffset + Time.time * 30));

		GO.light.color = new Color(Mathf.Min(Intensity/1.5f, 1f), Mathf.Min(Intensity/2f, 1f), 0f);
		MainFlame.GetComponent<ParticleSystem>().emissionRate = Intensity*20f;
		BaseFlame.GetComponent<ParticleSystem>().emissionRate = Intensity*15f;
		Etincelles.GetComponent<ParticleSystem>().emissionRate = Intensity*7f;
		Fumee.GetComponent<ParticleSystem>().emissionRate = Intensity*12f;		

	}
}
