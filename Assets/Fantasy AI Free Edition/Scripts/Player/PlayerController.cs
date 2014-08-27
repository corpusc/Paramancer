using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class PlayerController : MonoBehaviour {
	public float movespeed = 4;
	public float Damage = 60;
	public Transform Cam;
	public float CamHeight = 10;
	public float CamHeightPushback = 5;
	public AnimationClip Run;
	public AnimationClip Idle;
	public AnimationClip Attack;
	public AnimationClip die;
	public float AttackSpeed = 0.7f;
	public bool dead;
	public int TotalAICount;
	public bool YouWon;

	public Vector3 For;
	public List<Transform> KillList;
	
	// private 
	bool kill;
	bool playd;
	float atime;
	bool dealdamage;
	bool w;



	void Start() {
		if (Cam)
			Cam.parent = null;
		
		w = true;
		playd = true;
	}
	
	void Update() {
		// Victory 
		if (w) {
			if (TotalAICount <= 0) {
				YouWon = true;
				w = false;
			}
		}
		
		
		var php = (Health)GetComponent("Health");
		if (php) {
			if (php.CurrentHealth <= 0)
				dead = true;
		}
		
		// DEATH 
		if (dead) {
			if (playd) {
				animation.CrossFade(die.name, 0.15f);	
			}

			playd = false;			
		}else{
			hmm();
		}
	}
	
	void OnTriggerEnter(Collider other) {
		// ADD AIS TO LIST TO INFLICT DAMAGE IF IN RANGE 
		Health AI = (Health)other.transform.GetComponent("Health");
		if (AI) {
		KillList.Add(other.transform);
			
		}
	}
	
	void OnTriggerExit(Collider other) {
		// REMOVE FROM LIST WHEN OUT OF RANGE 
		Health AI = (Health)other.transform.GetComponent("Health");

		if (AI) {
			KillList.Remove(other.transform);			
		}
	}
	void OnGUI() {
		// HEALTH BAR AND AI COUNT 
		Health php = (Health)GetComponent("Health");
		if (php) {
			float hpp = php.CurrentHealth;
			GUI.Button(new Rect(0, 30, 300, 26), "Health: " + hpp);
		}

		GUI.Button(new Rect(0, 60, 300, 26), "Evil Skellies Left: " + TotalAICount);
			
		// YOU WON! 
		if (YouWon) {
			GUI.Box(new Rect(200, 200, 330, 260), "Congratulations!  You have defeated all the Evil Skellies!");

			if (GUI.Button(new Rect(310, 400, 120, 26), "Continue Playing"))
				YouWon = false;
		}
	}

	void hmm() {
		For.y = 270;

		// CAMERA POSITION 
		Vector3 ch = transform.position;
		ch.y = transform.position.y + CamHeight;
		ch.x = transform.position.x + CamHeightPushback;
		Cam.transform.position = ch;

		if (CcInput.Started(UserAction.Activate)) {
			dealdamage = true;
			kill = true;	
		}
		
		// LETS DO SOME KILLIN! 
		if (kill) {
			atime += Time.deltaTime;
			animation[Attack.name].speed = animation[Attack.name].length / AttackSpeed;
			animation.CrossFade(Attack.name, 0.15f);
			
			if (atime >= AttackSpeed*0.35f && atime <= AttackSpeed*0.48f) {
				if (KillList.Count > 0 & dealdamage) {
					int	ls = KillList.Count;

					for (int i = 0; i < ls; i++) {
						var hp = (Health)KillList[i].transform.GetComponent("Health");
						
						hp.CurrentHealth = hp.CurrentHealth-Damage;
						if (hp.Dead) {}
						else if (hp.CurrentHealth <= 0)
							TotalAICount = TotalAICount-1;
					}

					dealdamage = false;
				}
			}
			
			if (atime >= AttackSpeed) {
				kill = false;
				atime = 0;
			}
		}else{
			// RUN ANIMATION IF ANY ARROW KEYS ARE PRESSED 
			if (CcInput.Holding(UserAction.MoveLeft) ||
			    CcInput.Holding(UserAction.MoveRight) ||
			    CcInput.Holding(UserAction.MoveForward) ||
			    CcInput.Holding(UserAction.MoveBackward)
		    ) {
				animation.CrossFade(Run.name, 0.15f);	
			}else{
				animation.CrossFade(Idle.name, 0.15f);		
			}
		}

		// MOVEMENT 
		var rot = 0f;
		bool running = false;

		if /***/ (CcInput.Holding(UserAction.MoveForward)) {
			running = true;
			rot += -90;
		}else if (CcInput.Holding(UserAction.MoveBackward)) {
			running = true;
			rot += 90;
		}

		if /***/ (CcInput.Holding(UserAction.MoveRight)) {
			if (running)
				rot /= 2;

			running = true;
		}else if (CcInput.Holding(UserAction.MoveLeft)) {
			if (running) {
				rot += rot/2;
			}else {
				rot = -180;
			}

			running = true;
		}
		
		if (running) {
			var delta = transform.forward * movespeed * Time.deltaTime;
			transform.rotation = Quaternion.Euler(0, rot, 0);
			transform.position += delta;
			Cam.transform.position += delta;
		}
	}
}
