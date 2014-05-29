using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Lobe : MonoBehaviour {
	// a single layer of neurons
	public List<Neuron> Neurons;

	public void Think () {
		for (int i = 0; i < Neurons.Count; i++) {
			Neurons[i].Think();
		}
	}

	public void Mutate (float MaxWeightMutation, float MaxMultMutation) {
		for (int i = 0; i < Neurons.Count; i++) {
			Neurons[i].Mutate(MaxWeightMutation, MaxMultMutation);
		}
	}
}
