using UnityEngine;
using System.Collections;

public class PowerUpPoint : MonoBehaviour{
	public float RespawnTime = 0f;

	void Update () {
		particleSystem.enableEmission = Time.time >= RespawnTime;
	}
}
