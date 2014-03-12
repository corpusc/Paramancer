using UnityEngine;
using System.Collections;

public class ProcWeap : MonoBehaviour {

	public float bps = 4f;
	public int dmg = 10;
	public bool auto = true;
	public int bnc = 1;
	public int proj = 2;
	public float vamp = 0.0f;
	//these settings are an example of something that fits within the power limit

	public string name = "Non-generated procweap";

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

	//naming settings
	float minVampName = 0.3f; //minimal vamp noticeable enough to mention it in the name
	float minSacrificeName = -0.2f; //minimal/maximal(depends on using abs) negative vamp to include the part "sacrificial" in the name
	float minBpsName = 3f;
	int minDmgName = 50;
	int minBncName = 2;
	int minProjName = 3;

	int minPartsInName = 2;
	int maxPartsInName = 4;

	string[] namePart = {"xan", "tap", "mur", "per", "nix", "hex", "su", "her"};

	//internal vars
	float cooldown = 0f;
	float barrelsRotation = 0f; //changes when weapon is shot, so that barrels take turn to fire
	int n_barrels = 4; //computed later by Start()

	//the weapon is created facing away from the user in the z direction

	Vector3[] barrelPos; //relative to the weapon(does not include weapon rotation)
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
					print("PROCWEAP STATGEN ERROR!(power < minPower)");
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
					print("PROCWEAP STATGEN ERROR!(power > maxPower)");
					break;
				}
			}
		}

		name = GetName();

		if(dmg > minDmgName) shotCol = Color.Lerp(Color.green, Color.red, (dmg - minDmgName) / (maxDmg - minDmgName));
		else if(bnc > minBncName) shotCol = Color.Lerp(Color.blue, Color.red, (bnc - minBncName) / (maxBnc - minBncName));
		else shotCol = Color.Lerp(Color.blue, Color.green, (vamp - minVampName) / (maxVamp - minVampName));
		shotCol = Color.Lerp(shotCol, Color.clear, bps / maxBps);

		shotCol = Color.Lerp(shotCol, new Color(Random.value, Random.value, Random.value), Random.value); //for some randomness

		n_barrels = Mathf.CeilToInt(bps); //rounded up
		barrelScale = new Vector3(0.1f, 0.1f, dmg * 0.01f);
		barrelPos = new Vector3[n_barrels];
	}
	
	float GetPower() {
		return bps * dmg * Mathf.Sqrt(bnc + 1) * proj * (auto && bps > 1.0f ? Mathf.Sqrt(bps) : 1) * (vamp + 1.0f) * Mathf.Sqrt(dmg);
	}

	bool RandomBool() {
		return (Random.value < 0.5f);
	}

	//based on weapon stats
	string GetName() {
		string t = "";
		if(vamp > minVampName) t += "vampiric";
		else if(vamp < minSacrificeName) t += "sacrificial ";
		if(bps > minBpsName) t += "machine ";
		if(dmg > minDmgName) t += "hurting ";
		if(bnc > minBncName) t += "bouncy ";
		if(proj > minProjName) t += "splitting ";

		string s = "";
		int partsInName = Random.Range(minPartsInName, maxPartsInName);

		for(int i = 0; i < partsInName; i++)
			s += namePart[Random.Range(0, namePart.Length - 1)];

		s = UppercaseFirst(s);
		t = UppercaseFirst(t);

		t += s;

		return t;
	}

	static string UppercaseFirst(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}
		return char.ToUpper(s[0]) + s.Substring(1);
	}

	void Fire() {
	}
}
