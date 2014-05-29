using UnityEngine;
using System.Collections;

public class FlyingStars : MonoBehaviour {

	public GameObject Star;
	public int NumStars = 20;
	[HideInInspector]
	public GameObject[] Stars;

	Vector3 moveVec;

	// Use this for initialization
	void Start () {
		Stars = new GameObject[NumStars];
		moveVec = new Vector3(0f, 0f, -5f);
		for (int i = 0; i < NumStars; i++) {
			Stars[i].transform.position = new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), Random.Range(0f, 100f));
		}
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < NumStars; i++) {
			Stars[i].transform.position += moveVec * Time.deltaTime;
			if (Stars[i].transform.position.z < -5f) {
				Stars[i].transform.position = new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), Random.Range(0f, 100f));
			}
		}
	}
}
