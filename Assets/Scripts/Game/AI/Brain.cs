using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Brain : MonoBehaviour {

	public List<Lobe> Lobes;
	public int NumInputs;
	public int NumOutputs;
	public int NumLobes;
	// distribute neurons per lobe in a gaussian-like way

	public void Init () {
		Lobes = new List<Lobe>(NumLobes);
		for (int i = 0; i < Lobes.Count; i++) {
			Lobes[i].Neurons = new List<Neuron>(getNumNeurons(i));
		}
	}

	int getNumNeurons (int i) {
		return Mathf.CeilToInt(Mathf.Lerp((float)NumInputs, (float)NumOutputs, ((float)i) / (float)(NumLobes - 1))) + (NumLobes - i) * (i + 1);
	}
}
