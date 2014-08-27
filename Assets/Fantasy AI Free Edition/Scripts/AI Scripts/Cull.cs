using UnityEngine;
using System.Collections;

public class Cull : MonoBehaviour {
	public bool CullAtStart;
	
	// Use this for initialization
	void Start () {
	if(CullAtStart)renderer.enabled=false;
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
