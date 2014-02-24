using UnityEngine;
using System.Collections;

public class ProcWeap : MonoBehaviour {

	public float bps = 4f;
	public int dmg = 10;
	public bool auto = true;
	public int bnc = 1;
	public int proj = 2;
	public float vamp = 0.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	float GetPower(){
		return bps * dmg * Mathf.Sqrt(bnc + 1) * proj * (auto && bps > 1.0f ? Mathf.Sqrt(bps) : 1) * (vamp + 1.0f);
	}
}
