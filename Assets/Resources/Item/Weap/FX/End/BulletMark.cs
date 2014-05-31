using UnityEngine;
using System.Collections;

public class BulletMark : MonoBehaviour {

	public float MaxLife = 30f;
	public Color StartCol = Color.gray;

	float life;

	// Use this for initialization
	void Start () {
		life = MaxLife;
	}
	
	// Update is called once per frame
	void Update () {
		renderer.material.color = Color.Lerp(Color.clear, StartCol, life / MaxLife);
		life -= Time.deltaTime;
		if (life < 0f) Destroy(gameObject);
	}
}
