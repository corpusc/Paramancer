using UnityEngine;
using System.Collections;

public class NeuralAI : MonoBehaviour {

	public Brain MainBrain;

	public Vector3 TargetPos; // "target" = the player the AI is attacking

	public float TargetHealth;
	public float Health;

	public float ForwardDist; // the distance returned by a hitscan going staright ahead, set to a very high value if hitscan found nothing
	public float RightDist; // same as above, but the hitscan was looking 20 degs to the right
	public float LeftDist;
	public float UpDist;
	public float DownDist;

	public float[] Memory;
	public int NumMemories = 5;

	public float[] Output;
	public int NumOutputs;

	void Start () {
		MainBrain = new Brain();
		MainBrain.NumInputs = 15;
		MainBrain.NumOutputs = NumOutputs;
		MainBrain.NumLobes = 6;
		MainBrain.Init();
		Memory = new float[NumMemories];
		Output = new float[NumOutputs + NumMemories];
	}

	public void Think () {
		Output = MainBrain.Think(transform.position.x, transform.position.y, transform.position.z,
		                TargetHealth, Health,
		                ForwardDist, RightDist, LeftDist, UpDist, DownDist,
		                Memory[0], Memory[1], Memory[2], Memory[3], Memory[4]);
		for (int i = 0; i < NumMemories; i++) {
			Memory[i] = Output[NumOutputs + i];
		}
	}

	public void Mutate (float MaxWeightMutation, float MaxMultMutation) {
		MainBrain.Mutate(MaxWeightMutation, MaxMultMutation);
	}
}
