using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Brain : MonoBehaviour {

	public List<Lobe> Lobes;
	public int NumInputs;
	public int NumOutputs;
	public int NumLobes;
	public float DistSize = 0.1f; // the scale of the "bump" in the middle of the distribution

	public List<PseudoNeuron> InNeurons; // of the whole brain
	public List<PseudoNeuron> OutNeurons;
	// distribute neurons per lobe in a gaussian-like way

	public void Init () {
		// init lists
		Lobes = new List<Lobe>(NumLobes);
		InNeurons = new List<PseudoNeuron>(NumInputs);
		OutNeurons = new List<PseudoNeuron>(NumOutputs);

		// init neuron input
		for (int i = 0; i < Lobes.Count; i++) {
			Lobes[i].Neurons = new List<Neuron>(getNumNeurons(i));
			for (int j = 0; j < Lobes[i].Neurons.Count; j++) {
				Lobes[i].Neurons[j].InAxons = new List<Axon>(getNumNeurons(i - 1));
			}
		}

		// init neuron output
		for (int i = 0; i < Lobes.Count - 1; i++) {
			for (int j = 0; j < Lobes[i].Neurons.Count; j++) {
				Lobes[i].Neurons[j].OutAxons = new List<Axon>();
				for (int k = 0; k < Lobes[i + 1].Neurons.Count; k++) {
					Lobes[i].Neurons[j].OutAxons.Add (Lobes[i + 1].Neurons[k].InAxons[j]);
				}
			}
		}

		// special cases for the last lobe(it feeds directly to output), the InNeurons, and OutNeurons
		for (int i = 0; i < InNeurons.Count; i++) {
			InNeurons[i].OutAxons = new List<Axon>();
			for (int j = 0; j < Lobes[0].Neurons.Count; j++) {
				InNeurons[i].OutAxons.Add (Lobes[0].Neurons[j].InAxons[i]);
			}
		}

		for (int i = 0; i < OutNeurons.Count; i++) {
			OutNeurons[i].InAxons = new List<Axon>(getNumNeurons(NumLobes - 1));
		}

		Lobes[NumLobes - 1].Neurons = new List<Neuron>(getNumNeurons(NumLobes - 1));
		for (int i = 0; i < Lobes[NumLobes - 1].Neurons.Count; i++) {
			Lobes[NumLobes - 1].Neurons[i].OutAxons = new List<Axon>();
			for (int j = 0; j < NumOutputs; j++) {
				Lobes[NumLobes - 1].Neurons[i].OutAxons.Add (OutNeurons[j].InAxons[i]);
			}
		}
	}

	public void Mutate (float MaxWeightMutation, float MaxMultMutation) {
		for (int i = 0; i < Lobes.Count; i++) {
			Lobes[i].Mutate(MaxWeightMutation, MaxMultMutation);
		}

		for (int i = 0; i < OutNeurons.Count; i++) {
			OutNeurons[i].Mutate(MaxWeightMutation, MaxMultMutation);
		}
	}

	public float[] Think (float[] Inputs) {
		for (int i = 0; i < NumInputs; i++) {
			InNeurons[i].Value = Inputs[i];
			InNeurons[i].ProcessOutput();
		}

		for (int i = 0; i < NumLobes; i++) {
			Lobes[i].Think();
		}

		float[] Out = new float[NumOutputs];
		for (int i = 0; i < NumOutputs; i++) {
			OutNeurons[i].ProcessInput();
			Out[i] = OutNeurons[i].Value;
		}
		return Out;
	}

	int getNumNeurons (int i) {
		if (i < 0) return NumInputs;
		return Mathf.CeilToInt(Mathf.Lerp((float)NumInputs, (float)NumOutputs, ((float)i) / (float)(NumLobes - 1)) + (float)(NumLobes - i) * (float)(i + 1) * DistSize);
	}
}
