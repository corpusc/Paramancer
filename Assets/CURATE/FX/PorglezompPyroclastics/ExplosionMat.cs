using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode()]  

public class ExplosionMat : MonoBehaviour {
	
	private string[] qualitySel = new[] {"QUALITY_LOW", "QUALITY_MED", "QUALITY_HIGH"};
	bool doUpdate = true;
	float _radius;
	
	public Texture2D _ramp;
	public Texture2D _noise;
	public Material ExplosionMaterial;
	
	public float _heat = 1;
	float useheat = 1;
	public float _alpha = 1;
	float usealpha = 1;
	public float _scrollSpeed = 1;	
	float usescroll = 1;
	public float _frequency = 1;
	float usefreq = 1;
	
	public bool _scattering = true;
	bool usescatter = true;
	public int _octaves = 4;
	int useoctaves = 4;
	public int _quality = 2;
	int usequality = 2;
	
	// Use this for initialization
	void Start () {
		renderer.material = new Material(ExplosionMaterial);
		UpdateShaderProperties();
	}
	
	// Update is called once per frame
	void Update () {
		if (doUpdate) {
			Material rsm = renderer.sharedMaterial;
			float minscale = Mathf.Min(transform.lossyScale.x, Mathf.Min(transform.lossyScale.y, transform.lossyScale.z));
			// If anything has changed, update that property.
			if (minscale != _radius) {
				_radius = minscale;
				rsm.SetFloat("_Radius", _radius/2.03f - 2);
			}
			if (useheat != _heat) {
				useheat = _heat;
				rsm.SetFloat("_Heat", _heat);
			}
			if (usealpha != _alpha) {
				usealpha = _alpha;
				rsm.SetFloat("_Alpha", _alpha);
			}
			if (usescroll != _scrollSpeed) {
				usescroll = _scrollSpeed;
				rsm.SetFloat("_ScrollSpeed", _scrollSpeed);
			}
			if (usefreq != _frequency) {
				usefreq = _frequency;
				rsm.SetFloat("_Frequency", _frequency);
			}
			if (usescatter != _scattering || useoctaves != _octaves || usequality != _quality) {
				usescatter = _scattering;
				useoctaves = _octaves;
				usequality = _quality;
				SetShaderKeywords();
			}
		}
	}
	
	public void UpdateShaderProperties() {
		Material rsm = renderer.sharedMaterial;
		rsm.SetTexture("_RampTex", _ramp);
		rsm.SetTexture("_MainTex", _noise);
		rsm.SetFloat("_Heat", _heat);
		rsm.SetFloat("_Alpha", _alpha);
		rsm.SetFloat("_ScrollSpeed", _scrollSpeed);
		rsm.SetFloat("_Frequency", _frequency);
		SetShaderKeywords();
	}
	
	void SetShaderKeywords () {
		var newKeywords = new List<string> {_scattering ? "SCATTERING_ON" : "SCATTERING_OFF", "OCTAVES_" + _octaves, qualitySel[_quality]};
		renderer.sharedMaterial.shaderKeywords = newKeywords.ToArray();
	}
}
