using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Neuron : MonoBehaviour {
	// this has to be used in the form of a feedforward net
	public float Threshold = 1f;
	public float Base = 0f;
	public List<Axon> InAxons;
	public List<Axon> OutAxons;

	public void Think () {
		float t = Base; // temporary value
		for (int i = 0; i < InAxons.Count; i++) {
			if (InAxons[i].Active) {
				t += InAxons[i].Weight;
			}
		}
		for (int i = 0; i < InAxons.Count; i++) {
			if (InAxons[i].Active) {
				t *= InAxons[i].Mult;
			}
		}

		// written like this instead of OutAxons[i].Active = t > Threshold on purpose, for max performance
		if (t > Threshold) {
			for (int i = 0; i < OutAxons.Count; i++) {
				OutAxons[i].Active = true;
			}
		} else {
			for (int i = 0; i < OutAxons.Count; i++) {
				OutAxons[i].Active = false;
			}
		}
	}

	// will randomly change the input axon weights
	public void Mutate (float MaxWeightMutation, float MaxMultMutation) {
		for (int i = 0; i < InAxons.Count; i++) {
			InAxons[i].Weight += Random.Range(-MaxWeightMutation, MaxWeightMutation);
			InAxons[i].Mult += Random.Range(-MaxMultMutation, MaxMultMutation);
		}
	}
}
