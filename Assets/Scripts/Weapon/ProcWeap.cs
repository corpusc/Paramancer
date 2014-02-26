using UnityEngine;
using System.Collections;

public class ProcWeap : MonoBehaviour {

	public float bps = 4f;
	public int dmg = 10;
	public bool auto = true;
	public int bnc = 1;
	public int proj = 2;
	public float vamp = 0.0f;

	public float MinPower = 200f;
	public float MaxPower = 250f;

	//settings
	float minBps = 0.2f;
	float maxBps = 10f;
	int minDmg = 1;
	int maxDmg = 100;
	int minBnc = 0;
	int maxBnc = 7;
	int minProj = 1;
	int maxProj = 3;
	float minVamp = -0.5f;
	float maxVamp = 1f;
	//these settings are an example of something that fits within the power limit

	//internal vars
	float cooldown = 0f;
	float barrelsRotation = 0f; //changes when weapon is shot - the shooting barrel is always the one at the top
	int n_barrels = 4; //computed after by Start()

	Vector3[] barrelPos; //relative to the weapon(also has to include rotation)
	Vector3 barrelScale; //all barrels are of the same size
	Vector3[] cylPos; //cylinders used in the main body of the weapon
	Vector3[] cylScale;
	Quaternion[] cylRotation;
	Vector3[] cubePos;
	Vector3[] cubeScale;
	Quaternion[] cubeRotation;
	Material barrelMat;
	Material bodyMat;
	Color shotCol = Color.green;

	// Use this for initialization
	void Start () {
		bps = Random.Range(minBps, maxBps);
		dmg = Random.Range(minDmg, maxDmg);
		auto = RandomBool();
		bnc = Random.Range(minBnc, maxBnc);
		proj = Random.Range(minProj, maxProj);
		vamp = Random.Range(minVamp, maxVamp);

		for(int i = 0; (GetPower() < MinPower || GetPower() > MaxPower) && i < 100; i++) {
			if(GetPower() < MinPower) {
				switch(Random.Range(0, 5)) {
				case 0:
					bps = Random.Range(bps, maxBps);
					break;
				case 1:
					dmg = Random.Range(dmg, maxDmg);
					break;
				case 2:
					auto = true;
					break;
				case 3:
					bnc = Random.Range(bnc, maxBnc);
					break;
				case 4:
					proj = Random.Range(proj, maxProj);
					break;
				case 5:
					vamp = Random.Range(vamp, maxVamp);
					break;
				default:
					print("PROCWEAP STATGEN ERROR!");
					break;
				}
			} else {
				switch(Random.Range(0, 5)) {
				case 0:
					bps = Random.Range(minBps, bps);
					break;
				case 1:
					dmg = Random.Range(minDmg, dmg);
					break;
				case 2:
					auto = false;
					break;
				case 3:
					bnc = Random.Range(minBnc, bnc);
					break;
				case 4:
					proj = Random.Range(minProj, proj);
					break;
				case 5:
					vamp = Random.Range(minVamp, vamp);
					break;
				default:
					print("PROCWEAP STATGEN ERROR!");
					break;
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	float GetPower() {
		return bps * dmg * Mathf.Sqrt(bnc + 1) * proj * (auto && bps > 1.0f ? Mathf.Sqrt(bps) : 1) * (vamp + 1.0f) * Mathf.Sqrt(dmg);
	}

	bool RandomBool() {
		return (Random.value < 0.5f);
	}
}
