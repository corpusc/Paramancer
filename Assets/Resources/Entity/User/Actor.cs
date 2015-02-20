using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Actor : MonoBehaviour {
	// misc 
	public Light firstPersonLight; // should be in CAM section maybe 

	// frag 
	public int MultiFragCount = 0;
	public float PrevFrag = 0f;

	// cam 
	public GameObject camHolder;
	public float FOV = 90f;
	public bool Spectating = false;
	public int Spectatee = 0; // the id of user being spectated 
	
	// swapper 
	public int swapperCrossX = 0;
	public int swapperCrossY = 0;
	public bool swapperLocked = false;
	private Vector3 swapperLock = Vector3.zero;

	// grav dir indicator 
	public GameObject gravArrowPrefab;
	private GameObject gravArrowObj;
	
	// bball dir arrow 
	public GameObject bballArrowPrefab;
	private GameObject bballArrowObj;
	
	// visuals 
	public Color colA; // colours are needed both in here and in NetEntity, cuz THIS doesn't always exist, 
	public Color colB; // yet we'll want per-user chat colors, for chatting while dead/spectating 
	public Color colC;

	// inventory 
	public Gun GunInHand = Gun.Pistol;
	public Gun GunOnBack = Gun.GrenadeLauncher;
	public GameObject MeshInHand;
	public GameObject MeshOnBack;
	public GameObject weaponSoundObj;

	// network
	public NetEntity User;

	// body 
	public CcBody bod;
	public GameObject meshObj; // the stick figure body (no head), with a scarf.  only used to change its .renderer.materials (colors) 
	public GameObject animObj; // skeletally animated model (full stick figure i think) 
	public GameObject Model; // new kind, intended for mecanim 
	public GameObject[] heads; // FIXME: resources that should be loaded elsewhere 
	public int headType = 0;
	public float SprintEnergy = 1f; // 0-1 
	// 		nearby pickup 
	public string offeredPickup = "";
	public GunPickup currentOfferedPickup;



	// private 
	// 		scripts 
	Hud hud; // FIXME?  won't need this anymore once playingHud gets drawn correctly? *****************
	CcNet net;
	LocalUser locUser;
	int swapperLockTarget = -1;

	// body 
	CharacterController cc;
	float sprintRelease = 0f;
	float maxSprintRelease = 0.7f;
	bool crouchingPrev = false; // crouching in previous frame? 
	bool crouching = false;
	Vector3 moveVec = Vector3.zero;
	float lastYmove = 0f;
	Vector3 lastMoveVector = Vector3.zero;
	double lastUpdateTime = -1f;
	float lastHealth = 0f;

	// cam 
	Vector3 camAngle;
	Vector3 lastCamAngle = Vector3.zero;

	// 		inventory 
	Arsenal arse;
	Gun prevGunInHand = Gun.None;
	Gun prevGunOnBack = Gun.None;
	Vector3 hudGunOffs = new Vector3(0.47f, -0.48f, 0.84f); // offset of gun from camera (self, 1st person)
	Vector3 oobGunOffs = new Vector3(0.074f, 0.46f, 0.84f); // offset of gun from aimBone (out of body view)


	
	void Start() {
		// components 
		if (bod == null) 
			bod = gameObject.AddComponent<CcBody>();

		// get 
		cc = GetComponent<CharacterController>();
		var o = GameObject.Find("Main Program");
		net = o.GetComponent<CcNet>();
		hud = o.GetComponent<Hud>();
		arse = o.GetComponent<Arsenal>();
		locUser = o.GetComponent<LocalUser>();

		// new female model 
		//Model = (GameObject)GameObject.Instantiate(Models.Get("Mia"));
		//Model.SetActive(true);
		//Model.hideFlags = HideFlags.None;    //.DontSave; // ....to the scene. AND don't DESTROY when new scene loads 



		// the rest is all dependent on User class 
		if (User == null)
			return;

		if (User.local && net.CurrMatch.pitchBlack) {
			Camera.main.backgroundColor = Color.black;
			RenderSettings.ambientLight = Color.black;
		}
		
		if (User.lives >= 0) {
			if (User.local) {
				SetModelVisibility(false);
			}else{
				SetModelVisibility(true);
			}
			
			Respawn();
		}else{
			// we joined as a spectator 
			SetModelVisibility(false);
			transform.position = -Vector3.up * 99f;
		}

		// get rid of gun1 and gun2 builtin to the avatar 
		MeshInHand.renderer.enabled = false;
		MeshInHand.SetActive(false);
		MeshOnBack.SetActive(false);
		MeshOnBack.renderer.enabled = false;
	}


	public bool sendRPCUpdate = false;
	bool previouslyLockedCursor = true; // this is just so that clicking back into the screen won't fire explosive or gravgun immediately 
	float rpcCamTime = 0f;
	void Update() {
		if // dead 
		(User.Health <= 0f) {
			makeBombInvisible();
		}

		// frag monitor 
		// if its been long enough since last frag, reset MultiFrag count 
		if (Time.time - PrevFrag > 10f)
			MultiFragCount = 0;

		if (User.local) {
			net.LocEnt.Actor = this;
			// fov adjustment 
			// Camera.main.aspect == the horizontal proportion (compared to the vertical proportion of 1.0) 
			// Camera.main.fieldOfView == VERTICAL FOV 
			Camera.main.fieldOfView = (1.0f/Camera.main.aspect) * FOV;

			if (Spectating) {
				stickToSpectated();
				return;
			}else{
				localUserUpdate();
			}
		}else{
			if (lastUpdateTime > 0f) {
				NonLocalUpdate();
			}

			// visible avatar anims 
			camHolder.transform.localEulerAngles = camAngle;
		}

		setEyeHeight();

		Vector3 lookDir = camHolder.transform.forward;
		//lookDir.y = 0;
		lookDir.Normalize();
		animObj.transform.LookAt(animObj.transform.position + lookDir, transform.up);
		animObj.transform.localEulerAngles = new Vector3(0, animObj.transform.localEulerAngles.y, 0);



		showCorrectGuns();
		//animate();
		setVulnerability();
		setHudGunVis();

		// sync model to body 
		var v = transform.position;
		v.y -= 0.9f; // FIXME: i think height is 1.8 
		//Model.transform.position = v;
		//Model.transform.rotation = transform.rotation;
		//Model.transform.localEulerAngles = new Vector3(0, bod.transform.eulerAngles.y, 0);

		previouslyLockedCursor = Screen.lockCursor;
	}



	void setEyeHeight() {
		if (crouching) {
			camHolder.transform.localPosition = Vector3.zero;
		}else{
			camHolder.transform.localPosition = Vector3.up * 0.7f;
		}
	}



	void animate() {
		//		// animations
		//		if (User.health > 0f) {
		//			if (yMove == 0f) {
		//				if (moveVec.magnitude > 0.1f) {
		//					if (crouched) {
		//						animObj.animation.Play("crouchrun");
		//					}else{
		//						animObj.animation.Play("run");
		//					}
		//					
		//					if (Vector3.Dot(moveVec, lookDir) < -0.5f) {
		//						animObj.animation["crouchrun"].speed = -1;
		//						animObj.animation["run"].speed = -1;
		//					}else{
		//						animObj.animation["crouchrun"].speed = 1;
		//						animObj.animation["run"].speed = 1;
		//					}
		//				}else{
		//					if (crouched) {
		//						animObj.animation.Play("crouch");
		//					}else{
		//						animObj.animation.Play("idle");
		//					}
		//				}
		//			}else{
		//				if (yMove > 0f) {
		//					animObj.animation.Play("rise");
		//				}else{
		//					animObj.animation.Play("fall");
		//				}
		//			}
		//		}else{
		//			animObj.animation.Play("die");
		//		}
	}



	void setVulnerability() {
		int vuln = 8;
		int invuln = 2;

		// if alive 
		if (User.Health > 0f) {
			gameObject.layer = vuln;
		}else{ // dead 
			gameObject.layer = invuln;
		}
		
		// if same team & no friendly fire 
		if (net.CurrMatch.teamBased && !net.CurrMatch.FriendlyFire) {
			if (User.team == net.LocEnt.team) {
				gameObject.layer = invuln;
			}
		}
	}



	void setHudGunVis() {
		if (User.hasBall) {
			if (User.local && MeshInHand && MeshInHand.renderer) {
				if (User.Health > 0f) {
					MeshInHand.renderer.enabled = true;
				}else{
					MeshInHand.renderer.enabled = false;
				}
			}
		}
	}



	void switchWeapon() {
		bool nex, pre;
		CcInput.PollScrollWheel(out nex, out pre);
		bool next = CcInput.Started(UserAction.Next);
		bool prev = CcInput.Started(UserAction.Previous);
		if (next || prev || nex || pre) {
			Gun juggledItem = GunInHand;
			
			// switch weapon 
			while (GunInHand == juggledItem || 
			       !arse.Guns[(int)GunInHand].Carrying) 
			{
				if (next || nex) {
					GunInHand++;
					if ((int)GunInHand >= arse.Guns.Length)
						GunInHand = Gun.Pistol;
				}else{
					GunInHand--;
					if (GunInHand < Gun.Pistol)
						GunInHand = (Gun)arse.Guns.Length-1;
				}
			}
			
			GunOnBack = juggledItem;
			weaponSwitchingSoundAndVisual();
		}
	}



	void weaponSwitchingSoundAndVisual() {
		gunRecoil += Vector3.right * 3f;
		gunRecoil -= Vector3.up * 4f;
		PlaySound("guncocked");
		net.SendTINYUserUpdate(User.viewID, UserAction.Next);
	}

	void showCorrectGuns() {
		var ch = arse.Guns[(int)GunInHand]; // current (gun in) hand 
		// previous gun is now on back, taken care of below 

		if (GunInHand != prevGunInHand) {
			MeshInHand = ch.Instance;
			MeshInHand.SetActive(true);
			ch.Renderer.enabled = true;
			MeshInHand.transform.localEulerAngles = new Vector3(0, 270, 90) + ch.EulerOffset;

			if (User.local) {
				MeshInHand.transform.parent = Camera.main.transform;
				MeshInHand.transform.localPosition = hudGunOffs + ch.PosOffset;
			}else{
				MeshInHand.transform.parent = aimBone.transform;
				MeshInHand.transform.localPosition = oobGunOffs + ch.PosOffset;
			}

			sendRPCUpdate = true;
			prevGunInHand = GunInHand;
		}

		if (GunOnBack != prevGunOnBack) {
			var cb = arse.Guns[(int)GunOnBack]; // current (gun on) back 

			if (prevGunOnBack != Gun.None) {
				var pb = arse.Guns[(int)prevGunOnBack]; // previous (gun on) back 

				if // not swapping hand & back guns 
					(pb != ch) {
					pb.Renderer.enabled = false;
					pb.Instance.SetActive(false);
					// if () // it's the bomb 
					//FIXME makeBombInvisible().... right now its set to change GunInhand? 
					// ......needs to do it for GunOnBack 
				}
			}

			MeshOnBack = cb.Instance;

			MeshOnBack.transform.localEulerAngles = new Vector3(0, 180, 90);
			MeshOnBack.transform.localPosition =  new Vector3(0.012f, 0.47f, -0.002f);

			sendRPCUpdate = true;
			prevGunOnBack = GunOnBack;
		}
	}
	
	public GameObject aimBone;
	// private 
	Vector3 gunInertia = Vector3.zero;
	Vector3 gunRecoil = Vector3.zero;
	float gunBounce = 0f;
	void moveFPGun() {
		if (MeshInHand == null) 
			return;
		
		// angle 
		Quaternion fromRot = MeshInHand.transform.rotation;
		MeshInHand.transform.localEulerAngles = new Vector3(-90, 0, 0) + arse.Guns[(int)GunInHand].EulerOffset;
		MeshInHand.transform.rotation = Quaternion.Slerp(fromRot, MeshInHand.transform.rotation, Time.deltaTime * 30f);

		MeshInHand.transform.localPosition = new Vector3(0.47f, -0.48f, 0.84f);
		
		gunInertia -= (gunInertia-new Vector3(0f, bod.yMove, 0f)) * Time.deltaTime * 5f;
		if (gunInertia.y < -3f) 
			gunInertia.y = -3f;
		MeshInHand.transform.localPosition += gunInertia * 0.1f;
		
		float recoilRest = 5f;
		switch ((Gun)GunInHand) {
			case Gun.Pistol:
				recoilRest = 5f; break;
			case Gun.GrenadeLauncher:
				recoilRest = 8f; break;
			case Gun.MachineGun:
				recoilRest = 8f; break;
			case Gun.RailGun:
				recoilRest = 2f; break;
			case Gun.NapalmLauncher:
				recoilRest = 1f; break;
			case Gun.Spatula:
				recoilRest = 2f; break;
		}
		
		gunRecoil -= gunRecoil * Time.deltaTime * recoilRest;
		MeshInHand.transform.localPosition += gunRecoil * 0.1f;
		
		if (bod.grounded) {
			if (moveVec.magnitude > 0.1f && net.gunBobbing){
				if (crouching){
					gunBounce += Time.deltaTime * 6f;
				}else{
					gunBounce += Time.deltaTime * 15f;
				}

				MeshInHand.transform.position += Vector3.up * Mathf.Sin(gunBounce) * 0.05f;
			}
			
		}
	}
	
	void Fire(GunData gunData, bool alt = false) {
		Gun gun = (Gun)GunInHand;
		var ct = Camera.main.transform;
		gunData.Cooldown += gunData.Delay;

		switch (gun) {
			case Gun.Pistol:
				FireBullet(gun);
				gunRecoil -= Vector3.forward * 2f;
				break; 
			case Gun.GrenadeLauncher:
				net.Shoot(gun, ct.position, ct.forward, ct.position + ct.forward, net.LocEnt.viewID, false, alt, Vector3.zero, bod.sprinting);
				gunRecoil += Vector3.forward * 6f;
				break; 
			case Gun.MachineGun:
				FireBullet(gun);
				gunRecoil -= Vector3.forward * 2f;
				gunRecoil += new Vector3(
					Random.Range(-1f, 1f),
					Random.Range(-1f, 1f),
					Random.Range(-1f, 1f)).normalized * 0.2f;
				break; 
			case Gun.RailGun:
				FireBullet(gun);
				gunRecoil -= Vector3.forward * 5f;
				break; 
			case Gun.NapalmLauncher:
				net.Shoot(gun, ct.position, ct.forward, ct.position + ct.forward, net.LocEnt.viewID, false, alt, Vector3.zero);
				gunRecoil -= Vector3.forward * 5f;
				break; 
			case Gun.Swapper:
				if (swapperLockTarget == -1) {
					// not locked on, we miss
					FireBullet(gun);
				}else{
					// locked on, we hit
					net.Shoot(gun, transform.position, 
				          net.Entities[swapperLockTarget].Actor.transform.position - transform.position, 
				          net.Entities[swapperLockTarget].Actor.transform.position , net.LocEnt.viewID, 
				          true, alt, Vector3.zero);
					net.RegisterHit(gun, net.LocEnt.viewID, 
		                net.Entities[swapperLockTarget].viewID, 
		                net.Entities[swapperLockTarget].Actor.transform.position);
				}
				gunRecoil -= Vector3.forward * 5f;
				break; 
			case Gun.Gravulator:
				Ray gravRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
				RaycastHit gravHit = new RaycastHit();
				int gravLayer = 1<<0;
			
				if (Physics.Raycast(gravRay, out gravHit, 999f, gravLayer)) {
					Vector3 lookPos = Camera.main.transform.position + Camera.main.transform.forward;
					Quaternion tempRot = Camera.main.transform.rotation;
					transform.LookAt(transform.position + Vector3.Cross(Camera.main.transform.forward,gravHit.normal), gravHit.normal);
					ForceLook(lookPos);
					camHolder.transform.localEulerAngles = camAngle;
					Camera.main.transform.rotation = tempRot;
					PlaySound("Gravulator");
				}
				
				sendRPCUpdate = true;
				gunRecoil -= Vector3.forward * 5f;
				break; 
			case Gun.Bomb:
				net.Detonate(gun, transform.position, User.viewID, User.viewID);
				break; 
			case Gun.Spatula:
				gunRecoil += Vector3.forward * 3 + Vector3.up * 2f;
				//gunRecoil -= Vector3.right * 4f;
				FireBullet(gun, alt);
				break;
		}
	}
	
	public void SetModelVisibility(bool visible) {
		Material[] mats = meshObj.renderer.materials;
		var inv = Mats.Get("InvisibleShadow");

		var a = new Material(Mats.Get("ColorA"));
		var b = new Material(Mats.Get("ColorB"));
		var c = new Material(Mats.Get("ColorC"));
		a.color = User.colA;
		b.color = User.colB;
		c.color = User.colC;
		
		if (net.CurrMatch.teamBased) {
			if (User.team == 1) {
				a.color = Color.red;
			}
			if (User.team == 2) {
				a.color = Color.cyan;
			}
		}
		
		if (!visible) {
			mats[0] = inv;
			mats[1] = inv;
			mats[2] = inv;
			meshObj.renderer.materials = mats;
		}else{
			mats[0] = a;
			mats[1] = b;
			mats[2] = c;
			meshObj.renderer.materials = mats;
		}
		
		// heads 
		for (int i=0; i<heads.Length; i++) {
			if (i != headType) {
				heads[i].renderer.enabled = false;
			}
			
			if (!visible) {
				heads[i].renderer.material = inv;
			}	
		}
		
		if (visible) { // OPTME: don't set these regardless? 
			heads[0].renderer.material = a;
			heads[1].renderer.material = Mats.Get("CardboardBox");
			heads[2].renderer.material = Mats.Get("Fish");
			heads[3].renderer.material = Mats.Get("Banana");
			heads[4].renderer.material = Mats.Get("Creeper");
			heads[5].renderer.material = Mats.Get("Elephant");
			heads[6].renderer.material = Mats.Get("MoonTM0360");
			heads[7].renderer.material = Mats.Get("Pyramid");
			heads[8].renderer.material = Mats.Get("Chocobo");
			heads[9].renderer.material = Mats.Get("Spike");
		}
		
		if (User.local && 
		    MeshInHand != null && 
			MeshInHand.renderer && 
			GunInHand >= Gun.Pistol
		) {
			if (visible) {
				MeshInHand.renderer.enabled = false;
			}else{
				MeshInHand.renderer.enabled = true;
				MeshInHand.renderer.material = arse.Guns[(int)GunInHand].Mat;
			}
		}
		
		if (!net.CurrMatch.pitchBlack || !User.local) {
			firstPersonLight.enabled = false;
		} else {
			firstPersonLight.enabled = true;
		}
		
		if (!User.local && net.CurrMatch.pitchBlack) {
			if (net.CurrMatch.teamBased && User.team == net.LocEnt.team) {
				firstPersonLight.enabled = true;

				if (User.team == 1) {
					firstPersonLight.color = Color.red;
				}else{
					firstPersonLight.color = Color.cyan;
				}
			}
		}
	}


	public void ForceLook(Vector3 targetLookPos) {
		GameObject lookObj = new GameObject();
		lookObj.transform.position = Camera.main.transform.position;
		lookObj.transform.LookAt(targetLookPos, transform.up);
		lookObj.transform.parent = camHolder.transform.parent;
		camAngle = lookObj.transform.localEulerAngles;
		while (camAngle.x > 85f) 
			camAngle.x -= 180f;
		while (camAngle.x < -85f) 
			camAngle.x += 180f;
		//Debug.Log("Force look: " + targetLookPos.ToString() + " ??? " + lookObj.transform.position.ToString() + " ??? " + camAngle.ToString());
	}


	void FireBullet(Gun weapon, bool alt = false) {
		// fire hitscan type gun 
		// b ==  bullet/bolt 
		Vector3 bStart = Camera.main.transform.position;
		Vector3 bOri = Camera.main.transform.forward;
		Vector3 bEnd;
		Vector3 hitNorm = Vector3.zero;
		bool hit = false;
		bool registerhit = false;
		int hitPlayer = -1;
		bEnd = bStart + (bOri * arse.Guns[(int)weapon].Range);
	
		if (weapon == Gun.MachineGun) {
			float ex = 0.01f; // extent from center 
			bOri += new Vector3(
				Random.Range(-ex, ex),
				Random.Range(-ex, ex),
				Random.Range(-ex, ex));
			bOri.Normalize();
		}
				
		var bRay = new Ray(bStart, bOri);
		var bHit = new RaycastHit();
		int bulletLayer = (1<<0) | (1<<8); // walls & players
				
		gameObject.layer = 2;
		if (Physics.Raycast(bRay, out bHit, arse.Guns[(int)weapon].Range, bulletLayer)) {
			bEnd = bHit.point;
					
			if (bHit.collider.gameObject.layer == 8) {
				// hit a player, tell the server
				hit = true;
				
				for (int i=0; i<net.Entities.Count; i++) {
					if (bHit.collider.gameObject == net.Entities[i].Actor.gameObject){
						hitPlayer = i;
					}
				}
			
				registerhit = true;
			} else {
				hitNorm = bHit.normal;
			}
		}
	
		gameObject.layer = 8;
		bStart = transform.position;
		bStart = MeshInHand.transform.position + (Camera.main.transform.forward*0.5f);
		// RPC the shot, regardless 
		net.Shoot(weapon, bStart, bOri, bEnd, net.LocEnt.viewID, hit, alt, hitNorm);

		if (registerhit) 
			net.RegisterHit(weapon, net.LocEnt.viewID, net.Entities[hitPlayer].viewID, bHit.point);
	}

	private Transform getRandomSpawn(string s) {
		var go = GameObject.Find(s); // container for entity spawn positions 
		
		if (go == null) {
			Debug.LogError("*** Could not find a GameObject named: " + s + "!!! ***");
			return null;
		}
		
		int i = (int)Random.Range(0, go.transform.childCount);
		
		return go.transform.GetChild(i);
	}


	public void Respawn() {
		Debug.Log("Respawn()");

		Transform t = null;
		if /***/ (!net.CurrMatch.teamBased) {
			t = getRandomSpawn("FFA");
		}else if (User.team == 1) {
			t = getRandomSpawn("TeamRed");
		}else if (User.team == 2) {
			t = getRandomSpawn("TeamBlue");
		}

		transform.position = t.position + Vector3.up;
		transform.LookAt(transform.position + Vector3.forward, Vector3.up);
		camAngle = t.eulerAngles;
		moveVec = Vector3.zero;
		bod.yMove = 0f;
		bod.sprinting = false;

		// make blocky pink guns invisible 
		if (prevGunInHand == Gun.None) {
			Debug.Log("make blocky pink guns invisible ");
			MeshInHand.renderer.material = Mats.Get("InvisibleShadow");
			MeshInHand.renderer.enabled = false;
			MeshOnBack.renderer.material = Mats.Get("InvisibleShadow");
			MeshOnBack.renderer.enabled = false;

			for (int i = 0; i < gameObject.transform.childCount; i++) {
				var tr = gameObject.transform.GetChild(i);
				Debug.Log("---------> tr.name: " + tr.name);

				var mrs = tr.GetComponentsInChildren<MeshRenderer>();
				for (int j = 0; j < mrs.Length; j++) {
					Debug.Log("---------> mrs[j].name: " + mrs[j].name);
					mrs[j].enabled = false;
					mrs[j].material = Mats.Get("InvisibleShadow");
				}
			}
		}

		// assign spawn guns 
		GunInHand = net.CurrMatch.spawnGunA;
		GunOnBack = net.CurrMatch.spawnGunB;
		prevGunInHand = Gun.None;
		prevGunOnBack = Gun.None;
		arse.Guns[(int)GunInHand].Instance = (GameObject)GameObject.Instantiate(arse.Guns[(int)GunInHand].Prefab);
		arse.Guns[(int)GunOnBack].Instance = (GameObject)GameObject.Instantiate(arse.Guns[(int)GunOnBack].Prefab);
		arse.Guns[(int)GunInHand].Renderer = arse.Guns[(int)GunInHand].Instance.GetComponentInChildren<Renderer>();
		arse.Guns[(int)GunOnBack].Renderer = arse.Guns[(int)GunOnBack].Instance.GetComponentInChildren<Renderer>();

		// clear & setup inventory 
		for (int i = 0; i < arse.Guns.Length; i++) {
			arse.Guns[i].Cooldown = 0f;

			if ((Gun)i == GunInHand || 
			    (Gun)i == GunOnBack
			    // ||Debug.isDebugBuild)	// carry full arsenal if in IDE 
			) {
				arse.Guns[i].Carrying = true;
			}else{
				arse.Guns[i].Carrying = false;
				Destroy(arse.Guns[i].Instance);
				arse.Guns[i].Instance = null;
			}
		}
	}


	void LateUpdate() {
		if (User.Health > 0f) {
			aimBone.transform.localEulerAngles /*+=*/ = new Vector3(0, camAngle.x, 0);
			//gunMesh1.transform.localPosition = gunOffs;
			//gunMesh1.transform.position = aimBone.transform.position;
			animObj.transform.localPosition = (animObj.transform.forward * camAngle.x * -0.002f) - Vector3.up;
		}
	}


	void NonLocalUpdate() {
		if (User.Health <= 0f) 
			moveVec = Vector3.zero;
		
		if (cc == null) 
			cc = GetComponent<CharacterController>();
		if (bod == null) 
			bod = gameObject.AddComponent<CcBody>();
		
		float timeDelta = (float)(Network.time - lastUpdateTime);
		lastUpdateTime = Network.time;

		bod.UpVector = animObj.transform.up;
		
		if (crouching) {
			bod.Move(moveVec * timeDelta * 5f);
		}else{
			bod.Move(moveVec * timeDelta * 10f);
		}
		
		if (bod.yMove <= 0f) {
			bod.Move(transform.up * -0.2f);
			bod.grounded = bod.isGrounded;

			if (!bod.grounded) 
				bod.Move(transform.up * 0.2f);
		}else{
			bod.grounded = false;
		}
		
		if (bod.grounded) {
			bod.yMove = 0f;
		}else{
			bod.yMove -= timeDelta * 10f;
		}
		
		bod.Move(transform.up * bod.yMove * timeDelta * 5f);
	}
	
	public void UpdatePlayer(Vector3 pos, Vector3 ang, bool crouch, Vector3 move, float yMovement, double time, 
		Gun gunA, Gun gunB, Vector3 playerUp, Vector3 playerForward
	) {
		transform.position = pos;
		camHolder.transform.eulerAngles = ang;
		camAngle = ang;
		crouching = crouch;
		moveVec = move;
		bod.yMove = yMovement;
		lastUpdateTime = time;
		GunInHand = gunA;
		GunOnBack = gunB;
		transform.LookAt(transform.position + playerForward, playerUp);
		NonLocalUpdate();
	}
	
	public void PlaySound(UserAction action) { // i believe atm, this is only used by network "tiny updates" 
		switch (action) {
			case UserAction.MoveUp:
				PlaySound(0.6f, Sfx.Get("Jump"));
				break;
			case UserAction.Next:
			case UserAction.Previous:
				PlaySound(0.6f, Sfx.Get("guncocked"));
				break;
		}
	}
	public void PlaySound(string s) {
		CcClip cc = Sfx.GetCc(s);
		PlaySound(cc.Volume, cc.Clip);
	}
	public void PlaySound(float volume, AudioClip clip) {
		audio.clip = clip;
		audio.volume = volume;
		audio.Play();
	}


	void makeBombInvisible() {
		Transform fl = null;
		
		fl = MeshInHand.transform.Find("Flash Light");
		if (MeshInHand != null && fl != null)	
			fl.GetComponent<FlashingLight>().Visible = false;
		
		fl = MeshOnBack.transform.Find("Flash Light");
		if (MeshOnBack != null && fl != null)	
			fl.GetComponent<FlashingLight>().Visible = false;
	}


	void managePickingUpItem() {
		if (offeredPickup != "") {
			if (offeredPickup == "Health") {
				if (User.Health < 100f) {
					net.ConsumeHealth(User.viewID);
					net.LocEnt.Health = 100f;
					User.Health = 100f;
					PlaySound("guncocked");

					currentOfferedPickup.Pickup();
					hud.Log.AddEntry("+", offeredPickup, S.ColToVec(Color.gray));
				}
			}else{ // must be a weapon 
				for (int i=0; i<arse.Guns.Length; i++) {
					if (offeredPickup == arse.Guns[i].Name) {
						if (!arse.Guns[i].Carrying) {
							arse.Guns[i].Carrying = true;
							arse.Guns[i].Instance = (GameObject)GameObject.Instantiate(arse.Guns[i].Prefab);
							arse.Guns[i].Renderer = arse.Guns[i].Instance.GetComponentInChildren<Renderer>();
							arse.Guns[(int)GunOnBack].Cooldown =
								arse.Guns[(int)GunInHand].Cooldown;
							GunOnBack = GunInHand;
							
							GunInHand = (Gun)i;
							arse.Guns[(int)GunInHand].Cooldown = 0f;
							weaponSwitchingSoundAndVisual();
							currentOfferedPickup.Pickup();
							hud.Log.AddEntry("+", offeredPickup, S.ColToVec(Color.gray));
						}
					}
				}
			}
		}
	}

	void stickToSpectated() {
		if (net.Entities.Count > 0) {
			if (MeshInHand) 
				MeshInHand.renderer.enabled = false;
			
			if (CcInput.Started(UserAction.Activate) ||
			    net.Entities[Spectatee].lives <= 0
		    ) {
				Spectatee++;
				
				if (Spectatee >= net.Entities.Count) 
					Spectatee = 0;
			}
			
			Camera.main.transform.parent = null;
			Camera.main.transform.position = net.Entities[Spectatee].Actor.transform.position;
			//CurrModel.transform.position = net.Entities[Spectatee].Visuals.transform.position;
			
			float yChange = 1f; // y angle change 
			if (locUser.LookInvert)
				yChange = -1f;
			
			if (Screen.lockCursor) {
				camAngle.x -= Input.GetAxis("Mouse Y") * Time.deltaTime * 30f * locUser.LookSensitivity * yChange;
				camAngle.y += Input.GetAxis("Mouse X") * Time.deltaTime * 30f * locUser.LookSensitivity;
				
				if (camAngle.x > 85f) 
					camAngle.x = 85f;
				if (camAngle.x < -85f) 
					camAngle.x = -85f;
			}
			
			Camera.main.transform.eulerAngles = camAngle;
			Camera.main.transform.Translate(0,0,-3);
		}
	}

	void throwBall() {
		if (User.hasBall && Screen.lockCursor) {
			net.ThrowBall(Camera.main.transform.position, Camera.main.transform.forward, 20f);
		}
	}

	void selectSpecificWeapon() {
		for (int i = (int)UserAction.Pistol; i <= (int)UserAction.Spatula; i++) {
			if (CcInput.Started((UserAction)i)) {
				if (/* not already equipped, but carrying */ 
				    (Gun)i != GunInHand && 
				    arse.Guns[i].Carrying) 
				{
					GunOnBack = GunInHand;
					GunInHand = (Gun)i;
					weaponSwitchingSoundAndVisual();
				}
			}
		}
	}


	void localUserUpdate() {
		var lastPos = transform.position;
		
		// item pick up 
		if (User.Health > 0f) {
			managePickingUpItem();
		}
		offeredPickup = ""; // must do after the above check 
		
		if (User.Health > 0f) {
			if (Camera.main.transform.parent == null) 
				SetModelVisibility(false);
			
			net.LocEnt.FraggedBy = null;
			Camera.main.transform.parent = camHolder.transform;
			Camera.main.transform.localPosition = Vector3.zero;
			// this makes sure we can walk along walls/ceilings with proper mouselook orientation 
			Camera.main.transform.localRotation = Quaternion.Slerp(
				Camera.main.transform.localRotation, 
				Quaternion.Euler(Vector3.zero), 
				Time.deltaTime * 5f);
			
			float invY = 1f;
			if (locUser.LookInvert) 
				invY = -1f;
			
			if (Screen.lockCursor &&
			    hud.Mode == HudMode.Playing || 
			    hud.Mode == HudMode.Editing
			    ) {
				camAngle.x -= Input.GetAxis("Mouse Y") * locUser.LookSensitivity * invY;
				camAngle.y += Input.GetAxis("Mouse X") * locUser.LookSensitivity;
				float max = 89f; // degrees up or down limit
				if (camAngle.x >  max) 
					camAngle.x =  max;
				if (camAngle.x < -max) 
					camAngle.x = -max;
			}
			
			if (CcInput.Started(UserAction.Sprint)) {
				if (bod.sprinting) {
					bod.sprinting = false;
				} else if (SprintEnergy > 0.2f) {
					bod.sprinting = true;
					sprintRelease = 0f;
				} else {
					PlaySound("Exhausted");
				}
			}
			
			bod.TickEnergy(this);
			
			sprintRelease += Time.deltaTime;
			
			camHolder.transform.localEulerAngles = camAngle;
			var inputVector = Vector3.zero; 
			
			if (CcInput.Holding(UserAction.MoveForward)) 
				inputVector += animObj.transform.forward;
			
			if (CcInput.Holding(UserAction.MoveBackward)) 
				inputVector -= animObj.transform.forward;
			
			if (CcInput.Holding(UserAction.MoveRight)) 
				inputVector += animObj.transform.right;
			
			if (CcInput.Holding(UserAction.MoveLeft)) 
				inputVector -= animObj.transform.right;
			
			if (inputVector != Vector3.zero)
				sprintRelease = 0f;
			
			if (sprintRelease > maxSprintRelease)
				bod.sprinting = false;
			
			//inputVector.y = 0f;
			inputVector.Normalize();
			
			bod.UpVector = animObj.transform.up;
			
			
			
			var speedUpright = 10f;
			var speedCrouching = 5f;
			if (crouching) {
				bod.Move(inputVector * Time.deltaTime * speedCrouching);
			}else{
				bod.Move(inputVector * Time.deltaTime * speedUpright);
			}
			
			
			
			SprintEnergy = bod.GetEnergy();
			bod.VerticalMove(this);
			bod.MaybeJumpOrFall(this, net);
			
			bod.Move(transform.up * bod.yMove * Time.deltaTime * 5f);
			
			crouching = false;
			if (CcInput.Holding(UserAction.MoveDown)) 
				crouching = true;
			
			moveVec = inputVector;
			
			Ray lavaRay = new Ray(lastPos, transform.position - lastPos);
			RaycastHit lavaHit = new RaycastHit();
			float lavaRayLength = Vector3.Distance(transform.position, lastPos);
			int lavaLayer = (1<<10);
			if (Physics.Raycast(lavaRay, out lavaHit, lavaRayLength, lavaLayer)) {
				transform.position = lavaHit.point+ (Vector3.up*0.35f);
				sendRPCUpdate = true;
				inputVector = Vector3.zero;
				net.RegisterHit(Gun.Lava, User.viewID, User.viewID, lavaHit.point);
			}
			
			
			//sendRPCUpdate = false;
			if (camAngle != lastCamAngle && Time.time > rpcCamTime) 
				sendRPCUpdate = true;
			if (moveVec != lastMoveVector) 
				sendRPCUpdate = true;
			if (crouching != crouchingPrev) 
				sendRPCUpdate = true;
			if (bod.yMove != lastYmove) 
				sendRPCUpdate = true;
			if (User.Health != lastHealth) 
				sendRPCUpdate = true;
			if (net.broadcastPos) {
				net.broadcastPos = false;
				sendRPCUpdate = true;
			}
			
			lastCamAngle = camAngle;
			lastMoveVector = moveVec;
			crouchingPrev = crouching;
			lastYmove = bod.yMove;
			lastHealth = User.Health;
			
			if (sendRPCUpdate) {
				net.SendUserUpdate(User.viewID, transform.position, camAngle, crouching, moveVec, bod.yMove, 
				                   (int)GunInHand, (int)GunOnBack, transform.up, transform.forward);
				sendRPCUpdate = false;
				
				rpcCamTime = Time.time; // + 0.02f;
			}
			
			var gun = arse.Guns[(int)GunInHand];
			if (GunInHand >= Gun.Pistol && 
			    gun.Cooldown > 0f && 
			    gun.Cooldown - Time.deltaTime <= 0f && 
			    gun.Delay >= 1f
		    ) 
				PlaySound("click");
			
			gun.Cooldown -= Time.deltaTime;
			if (gun.Cooldown < 0f) 
				gun.Cooldown = 0f;

			manageTargetLocking();

			// basketball arrow 
			if (net.CurrMatch.basketball) {
				if (bballArrowObj == null) {
					bballArrowObj = (GameObject)GameObject.Instantiate(bballArrowPrefab);
					bballArrowObj.transform.parent = Camera.main.transform;
					bballArrowObj.transform.localPosition = Vector3.forward - (Vector3.right*0.8f) + (Vector3.up*0.5f);
				}
				if (User.hasBall) {
					bballArrowObj.renderer.enabled = false;
				}else{
					bballArrowObj.renderer.enabled = true;
					bballArrowObj.transform.LookAt(net.GetBball().transform.position);
					
				}
			}else{
				if (bballArrowObj != null) {
					bballArrowObj.renderer.enabled = false;
				}
			}
			
			// gravulator arrow 
			if (GunInHand == Gun.Gravulator) {
				if (gravArrowObj == null) {
					gravArrowObj = (GameObject)GameObject.Instantiate(gravArrowPrefab);
					//gravArrowObj.layer = 
					gravArrowObj.transform.parent = Camera.main.transform;
					gravArrowObj.transform.localPosition = Vector3.forward;
				}
				
				Ray gravRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
				RaycastHit gravHit = new RaycastHit();
				int gravLayer = 1<<0;
				
				if (Physics.Raycast(gravRay, out gravHit, 999f, gravLayer)) {
					gravArrowObj.transform.LookAt(gravArrowObj.transform.position - gravHit.normal);
					gravArrowObj.renderer.enabled = true;
				}else{
					gravArrowObj.renderer.enabled = false;
				}
			}else{
				if (gravArrowObj != null){
					gravArrowObj.renderer.enabled = false;
				}
			}
			
			if // we can shoot 
			(gun.Cooldown <= 0f &&
				Screen.lockCursor && previouslyLockedCursor &&
				!User.hasBall && 
				GunInHand >= Gun.Pistol
			) {
				if // gun repeats while pressed 
				(arse.Guns[(int)GunInHand].AutoFire) {
					if (CcInput.Holding(UserAction.Activate))
						Fire(gun);
				}else{ // single shot 
					if (CcInput.Started(UserAction.Activate))
						Fire(gun);
					if (CcInput.Started(UserAction.Alt))
						Fire(gun, true);
				}
			}
			
			selectSpecificWeapon();
			
			if (hud.Mode == HudMode.Playing) 
				// ....then allow scrollwheel to cycle weaps/items 
				switchWeapon();
			if (CcInput.Started(UserAction.Activate))   throwBall();					
			if (CcInput.Started(UserAction.Suicide))    net.RegisterHitRPC((int)Gun.Suicide, User.viewID, User.viewID, transform.position);
			
			moveFPGun();
		}else{ // we be dead 
			if (Camera.main.transform.parent != null) 
				SetModelVisibility(true);
			
			if (net.LocEnt.FraggedBy != null) {
				Camera.main.transform.parent = null;
				Camera.main.transform.position = transform.position - animObj.transform.forward;
				Camera.main.transform.LookAt(net.LocEnt.FraggedBy.transform.position, transform.up);
				Camera.main.transform.Translate(0, 0, -2f);
			}
		}
	}


	void manageTargetLocking() {
		swapperLocked = false;
		swapperLockTarget = -1;
		if (GunInHand == Gun.Swapper) {
			// swapper aiming
			var validSwapTargets = new List<int>();
			
			for (int i=0; i<net.Entities.Count; i++) {
				var diff = net.Entities[i].Actor.transform.position - Camera.main.transform.position;
				if (!net.Entities[i].local && 
				    Vector3.Dot(Camera.main.transform.forward, diff.normalized) > 0.94f && 
				    net.Entities[i].Health > 0f
				    ) {						
					Ray swapCheckRay = new Ray(Camera.main.transform.position, net.Entities[i].Actor.transform.position - Camera.main.transform.position);
					RaycastHit swapCheckHit = new RaycastHit();
					int swapCheckLayer = 1<<0;
					float swapCheckLength = Vector3.Distance(net.Entities[i].Actor.transform.position, Camera.main.transform.position);
					
					if (!Physics.Raycast(swapCheckRay, out swapCheckHit, swapCheckLength, swapCheckLayer) ) {
						validSwapTargets.Add(i);
						swapperLocked = true;
					}
				}
			}
			
			int nearestScreenspacePlayer = 0;
			float nearestDistance = 9999f;
			for (int i=0; i<validSwapTargets.Count; i++) {
				Vector3 thisPos = Camera.main.WorldToScreenPoint(net.Entities[validSwapTargets[i]].Actor.transform.position);
				if (Vector3.Distance(thisPos, new Vector3(Screen.width/2, Screen.height/2, 0)) < nearestDistance) {
					nearestScreenspacePlayer = validSwapTargets[i];
				}
			}
			
			if (swapperLocked) {
				// move target to locked on player
				Vector3 screenPos = Camera.main.WorldToScreenPoint(net.Entities[nearestScreenspacePlayer].Actor.transform.position);
				swapperLock -= (swapperLock-screenPos) * Time.deltaTime * 10f;
				swapperLockTarget = nearestScreenspacePlayer;
			}else{
				// move target to center
				swapperLock -= (swapperLock-new Vector3(Screen.width/2, Screen.height/2, 0)) * Time.deltaTime * 10f;
			}
		}else{
			swapperLock = new Vector3(Screen.width/2, Screen.height/2, 0);
		}
		
		swapperCrossX = Mathf.RoundToInt(swapperLock.x);
		swapperCrossY = Mathf.RoundToInt(swapperLock.y);
	}
}
