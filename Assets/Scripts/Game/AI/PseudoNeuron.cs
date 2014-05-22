using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PseudoNeuron : MonoBehaviour {
	// this is a neuron in the input/output layer
	public float Value = 0f;
	public List<Axon> InAxons;
	public List<Axon> OutAxons;
	
	public void ProcessOutput () {
		for (int i = 0; i < OutAxons.Count; i++) {
			OutAxons[i].Weight = Value;
			OutAxons[i].Active = true;
		}
	}

	public void ProcessInput () {
		float t = 0f; // temporary value
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
		Value = t;
	}

	// will randomly change the input axon weights
	public void Mutate (float MaxWeightMutation, float MaxMultMutation) {
		for (int i = 0; i < InAxons.Count; i++) {
			InAxons[i].Weight += Random.Range(-MaxWeightMutation, MaxWeightMutation);
			InAxons[i].Mult += Random.Range(-MaxMultMutation, MaxMultMutation);
		}
	}

}
