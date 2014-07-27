using UnityEngine;
using System.Collections;

public class Axon : MonoBehaviour {
	// and axon is a one-way connection between 2 neurons
	public float Weight = 0f;
	public float Mult = 1f; // useful for single-neuron XOR gates for example(otherwise would require 3 neurons, and would mutate into the right state slower)
	public bool Active = false;
}
