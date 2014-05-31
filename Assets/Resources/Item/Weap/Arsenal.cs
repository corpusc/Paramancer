using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Arsenal : MonoBehaviour {
	public GameObject Beam;
	public GameObject LightningBeamPrefab;
	public GameObject grenadeBulletPrefab;
	public GameObject BeamParticle;
	public GameObject grenadeFlashPrefab;
	public GameObject muzzleFlashPrefab;
	public GameObject rocketPrefab;
	public GameObject soundObjectPrefab;
	public GameObject BulletMark;

	public int WidestIcon;
	public int TallestIcon;
	public GunData[] Guns;
	
	// private 
	CcNet net;
	List<GrenadeScript> activeGrenades = new List<GrenadeScript>();
	List<RocketScript> activeRockets = new List<RocketScript>();
	
	
	
	void Start() {
		net = GetComponent<CcNet>();

		// setup guns 
		Guns = new GunData[(int)Item.Count];

		string s = "";
		for (int i = 0; i < Guns.Length; i++) {
			Guns[i] = new GunData();

			var n = S.GetSpacedOut("" + (Item)i);
			Guns[i].Name = n;

			switch ((Item)i) {
				case Item.GrenadeLauncher:
					Guns[i].Prefab = (GameObject)Resources.Load("Item/Weap/Gun/Low Poly " + n + " 1.1 - Daniel Mendes/Prefabs/" + n);
					Guns[i].Mat =    (Material)  Resources.Load("Item/Weap/Gun/Low Poly " + n + " 1.1 - Daniel Mendes/Materials/" + n); 
					Guns[i].Pic =      Resources.Load<Texture> ("Item/Weap/Gun/" + n + "/" + n); 
					break;
				default:
					Guns[i].Prefab = (GameObject)Resources.Load("Item/Weap/Gun/" + n + "/" + n + " PREFAB");
					Guns[i].Mat =      Resources.Load<Material>("Item/Weap/Gun/" + n + "/" + n); 
					Guns[i].Pic =      Resources.Load<Texture> ("Item/Weap/Gun/" + n + "/" + n); 
					break;
			}

			s += n + ",  ";
			
			// set widest icon 
			int w = Guns[i].Pic.width;
			if (WidestIcon < w) 
				WidestIcon = w;
			
			// set tallest icon
			int h = Guns[i].Pic.height;
			if (TallestIcon < h) 
				TallestIcon = h;
		}
		Debug.Log("Weapons: " + s);

		// pics   (FOR NOW, cycle by id/index into an alphabetized LoadAll() array 
		for (int i = 0; i < Guns.Length; i++) {
			switch ((Item)i) {
				case Item.Pistol:   Guns[i].Color = Color.white; 
					Guns[i].Delay = 0.3f; 
					Guns[i].DelayAlt = 0.3f; break; 
				case Item.GrenadeLauncher:   Guns[i].Color = Color.green; 
					Guns[i].Delay = 0.25f; 
					Guns[i].DelayAlt = 0.25f; break; 
				case Item.MachineGun:   Guns[i].Color = Color.cyan; 
					Guns[i].Delay = 0.1f; 
					Guns[i].DelayAlt = 0.1f; Guns[i].AutoFire = true; break; // only 1 with AutoFire 
				case Item.RailGun:   Guns[i].Color = Color.cyan; 
					Guns[i].Delay = 2f;
					Guns[i].MarkScale = 2f;
					Guns[i].DelayAlt = 2f; break; 
				case Item.RocketLauncher:   Guns[i].Color = Color.red; 
					Guns[i].Delay = 1.5f; 
					Guns[i].DelayAlt = 0.7f;
					Guns[i].MarkScale = 5f; break; // set for the launcher because the projectile has a negative value in the gun system 
				case Item.Swapper:   Guns[i].Color = Color.magenta; 
					Guns[i].Delay = 2f; 
					Guns[i].DelayAlt = 2f; break; 
				case Item.Gravulator:   Guns[i].Color = Color.green; 
					Guns[i].Delay = 1f; 
					Guns[i].DelayAlt = 1f; break; 
				case Item.Bomb:   Guns[i].Color = Color.yellow; 
					Guns[i].Delay = 1f; 
					Guns[i].DelayAlt = 1f; break; 
				case Item.Spatula:   Guns[i].Color = Color.magenta; 
					Guns[i].Delay = 1f;  
					Guns[i].DelayAlt = 4f;
					Guns[i].Range = 3f; break; // only 1 with Range 
			}
		}
	}
	
	public void Clear() {
		// FIXME?  should we be clearing out rockets here too? if not, change method name to be more specific, since its not a Clear'ing of the whole Arsenal system 
		for (int i=0; i<activeGrenades.Count; i++) {
			if (activeGrenades[i] != null && activeGrenades[i].gameObject != null) 
				Destroy(activeGrenades[i].gameObject);
		}
		
		activeGrenades = new List<GrenadeScript>();
	}

	void shootSwapper (Vector3 origin, Vector3 end, NetworkViewID shooterID, bool hit) {
		bool localFire = false;
		Vector3 localstart = origin;
		for (int i=0; i<net.players.Count; i++){
			if (net.players[i].viewID == shooterID && net.players[i].local) {
				localFire = true;
				localstart = net.players[i].Entity.HudGun.transform.position + (Camera.main.transform.forward*0.5f);
			}
		}
		
		var lbp = (GameObject)GameObject.Instantiate(LightningBeamPrefab);
		var lb = lbp.GetComponent<LightningBeam>();

		if (localFire && !hit) 
			lb.start = localstart;
		else
			lb.start = origin;

		lb.end = end;
		lb.hit = hit;
	}

	void shootHitscan(Vector3 origin, Vector3 end, NetworkViewID shooterID, Item weapon, Vector3 hitNorm) {
		bool localFire = false;
		Vector3 localStart = origin;

		for (int i=0; i<net.players.Count; i++) {
			if (net.players[i].viewID == shooterID && net.players[i].local){
				localFire = true;
				localStart = net.players[i].Entity.HudGun.transform.position + (Camera.main.transform.forward*0.5f);
			}
		}

		if (weapon == Item.Spatula) return; // no trail or effects

		if (hitNorm != Vector3.zero) {
			GameObject nh = (GameObject)GameObject.Instantiate(BulletMark);
			nh.transform.position = end + hitNorm * 0.01f;
			nh.transform.forward = -hitNorm;
			nh.transform.localScale *= Guns[(int)weapon].MarkScale;
			nh.GetComponent<BulletMark>().StartCol = Color.Lerp(Color.gray, Color.black, Random.value);
			nh.GetComponent<BulletMark>().MaxLife = 30f;
		}
		
		// beam 
		//beam.GetComponent<BeamEffect>().start = origin;
		var beam = (GameObject)GameObject.Instantiate(Beam);
		var b = beam.GetComponent<BeamEffect>();

		if (localFire) 
			b.start = localStart;
		else
			b.start = origin;

		b.end = end;
		b.col = Guns[(int)weapon].Color;

		if (weapon == Item.Pistol)
			b.renderer.material.color = Color.white;
		
		// muzzle flash 
		var mf = (GameObject)GameObject.Instantiate(muzzleFlashPrefab);
		mf.light.color = Guns[(int)weapon].Color;
		mf.transform.position = origin;
		if (localFire) { // Sophie didn't allow remote player flashes? 
			mf.transform.position = localStart - (Camera.main.transform.right * 0.2f);
		}
		
		// rail trail 
		if (weapon == Item.RailGun) {
			Vector3 beamStart = origin;
			if (localFire) 
				beamStart = localStart;

			Vector3 beamDir = (end-beamStart).normalized;
			float maxLen = Vector3.Distance(end, beamStart);
			if (maxLen > 160f) 
				maxLen = 160f;

			float angle = 0f;
			float progress = 0f;
			while (progress < maxLen) {
				var np = (GameObject)GameObject.Instantiate(BeamParticle);
				var v = Camera.main.transform.up/4;
				v = Quaternion.AngleAxis(angle, Camera.main.transform.forward) * v;
				var center = beamStart + (beamDir * progress);
				np.transform.position = center + v;
				np.GetComponent<BeamParticle>().MoveVec = v;
				np.GetComponent<BeamParticle>().MinSize = 0.4f;
				np.GetComponent<BeamParticle>().MaxSize = 0.4f;
				np.GetComponent<BeamParticle>().StartColor = Color.blue;
				np.GetComponent<BeamParticle>().EndColor = Color.clear;
				np.GetComponent<BeamParticle>().ParticType = ParticleType.Circle;
				progress += 0.20f;
				angle += 24f;
			}
		}
	}

	public void Shoot(Item weapon, Vector3 origin, Vector3 direction, Vector3 end, 
		NetworkViewID shooterID, NetworkViewID bulletID, double time, bool hit, bool alt, Vector3 hitNorm, bool sprint = false
	) {
		switch (weapon) {
			case Item.Pistol:
			case Item.MachineGun:
			case Item.RailGun:
			case Item.Spatula:
				shootHitscan(origin, end, shooterID, weapon, hitNorm);
				break;
		
			case Item.Swapper:
				shootSwapper(origin, end, shooterID, hit);
				break;
			
			case Item.GrenadeLauncher:
				var ng = (GameObject)GameObject.Instantiate(grenadeBulletPrefab);
				var gs = ng.GetComponent<GrenadeScript>();
				gs.start = origin;
				gs.direction = direction;
				gs.startTime = time;
				gs.viewID = bulletID;
				gs.shooterID = shooterID;
				gs.detonationTime = 3f;
				gs.ThrowFaster = sprint;
				
				activeGrenades.Add(gs);
				break;
			
			case Item.RocketLauncher:
					var nr = (GameObject)GameObject.Instantiate(rocketPrefab);
					nr.transform.position = origin;
					nr.transform.LookAt(origin + direction);
					
					var	rs = nr.GetComponent<RocketScript>();
					rs.viewID = bulletID;
					rs.shooterID = shooterID;
					
					activeRockets.Add(nr.GetComponent<RocketScript>());

					if (alt)
						rs.Turning = true;
				break;
		}
		
		for (int i=0; i<net.players.Count; i++) {
			if (net.players[i].viewID == shooterID) {
				switch (weapon) {
					// case Item.Gravulator: 
					// *** the activation sound is currently located along with jump/land sfx. //FIXME? 
					// it's not sending the shot sound trigger over the net. 
					// so a pursuer has to look around for a flee'er (since you can't hear them) 
					// when they enter into a large space/room.  this may be a good thing? 
					case Item.GrenadeLauncher:  playPitchedSfx(i, Sfx.Get("boosh")); break;
					case Item.RocketLauncher:   playPitchedSfx(i, Sfx.Get("boosh")); break;
					default: playPitchedSfx(i, Sfx.Get(weapon.ToString())); break;
				}
			}
		}
	}
		
	void playPitchedSfx(int i, AudioClip ac) { // randomly pitched for variety 
		net.players[i].Entity.weaponSoundObj.audio.clip = ac;
		
		// if local user 
		if (net.players[i].viewID == net.localPlayer.viewID) 
			net.players[i].Entity.weaponSoundObj.audio.volume = 0.3f;
		
		net.players[i].Entity.weaponSoundObj.audio.pitch = Random.Range(0.9f, 1.1f);
		net.players[i].Entity.weaponSoundObj.audio.Play();
	}
	
	public float GetWeaponDamage(Item weapon) {
		switch (weapon) {
			case Item.Pistol:         return 40f;
			case Item.GrenadeLauncher:        return 60f;
			case Item.MachineGun:     return 15f;
			case Item.Spatula:          return 105f;
			case Item.RailGun:          return 105f;
			case Item.RocketProjectile: return 70f;
			
			case Item.Lava:           return 9999f;
			case Item.Bomb:           return 9999f;
			case Item.Suicide:        return 9999f;
		}

		return 0f;
	}
	
	public void BombBeep(Vector3 pos) {
		var o = (GameObject)GameObject.Instantiate(soundObjectPrefab); // bomb beep object 
		o.transform.position = pos;
		o.audio.clip = Sfx.Get("BombBeep");
		o.audio.volume = 1f;
	}
	
	public void Detonate(Item weapon, Vector3 detPos, NetworkViewID viewID) {
		if (weapon == Item.Bomb) {
			var o = (GameObject)GameObject.Instantiate(grenadeFlashPrefab);
			o.transform.position = detPos;
			o.transform.localScale *= 2f;
				
			var bomb = (GameObject)GameObject.Instantiate(soundObjectPrefab);
			bomb.transform.position = detPos;
			bomb.audio.clip = Sfx.Get("ExplodeBomb");
			bomb.audio.volume = 4f;
		} else if (weapon == Item.RocketProjectile) {
			print ("WARNING: Detonate() was called for a rocket!");
		}
		
		for (int i=0; i<activeGrenades.Count; i++) {
			if (viewID == activeGrenades[i].viewID) {
				
				var o = (GameObject)GameObject.Instantiate(grenadeFlashPrefab);
				o.transform.position = activeGrenades[i].transform.position;
				
				var nade = (GameObject)GameObject.Instantiate(soundObjectPrefab);
				nade.transform.position = activeGrenades[i].transform.position;
				nade.audio.clip = Sfx.Get("ExplodeGrenade");
				nade.audio.volume = 2f;
				
				
				Destroy(activeGrenades[i].gameObject);
				activeGrenades.RemoveAt(i);
				
			}
		}
	}

	public void DetonateRocket(Vector3 detPos, Vector3 hitNorm, NetworkViewID viewID) {
		for (int i=0; i<activeRockets.Count; i++) {
			if (viewID == activeRockets[i].viewID) {
				// rocket jumping
				for (int k=0; k<net.players.Count; k++) {
					if (net.players[k].local) {
						if (Vector3.Distance(
							net.players[k].Entity.transform.position, 
							activeRockets[i].transform.position) 
						    < GetDetonationRadius(Item.RocketProjectile)
						    ) {
							if (net.players[k].Entity.transform.position.y > activeRockets[i].transform.position.y) {
								if (activeRockets[i].shooterID == net.players[k].viewID){
									net.players[k].Entity.yMove = 14;
								}else{
									net.players[k].Entity.yMove = 5;
								}
								net.players[k].Entity.grounded = false;
								net.players[k].Entity.sendRPCUpdate = true;
							}
						}
					}
				}
				
				// detonate rocket 
				GameObject grenadeFlash = (GameObject)GameObject.Instantiate(grenadeFlashPrefab);
				grenadeFlash.transform.position = activeRockets[i].transform.position;
				GameObject rocketSoundObj = (GameObject)GameObject.Instantiate(soundObjectPrefab);
				rocketSoundObj.transform.position = activeRockets[i].transform.position;
				rocketSoundObj.audio.clip = Sfx.Get("ExplodeRocket");
				rocketSoundObj.audio.volume = 4f;
				Destroy(activeRockets[i].gameObject);
				activeRockets.RemoveAt(i);

				if (hitNorm != Vector3.zero) {
					GameObject nh = (GameObject)GameObject.Instantiate(BulletMark);
					nh.transform.position = detPos + hitNorm * 0.03f;
					nh.transform.forward = -hitNorm;
					nh.transform.localScale *= Guns[(int)Item.RocketLauncher].MarkScale;
					nh.GetComponent<BulletMark>().StartCol = Color.Lerp(Color.gray, Color.black, Random.value);
					nh.GetComponent<BulletMark>().MaxLife = 30f;
				}
			}
		}
	}
	
	public float GetDetonationRadius(Item weapon) {
		switch (weapon) {
			case Item.GrenadeLauncher:        return 4;
			case Item.RocketProjectile: return 4;
			case Item.Bomb:           return 10;
		}
		
		return 0;	}
}