using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Arsenal : MonoBehaviour {
	public int WidestIcon;
	public int TallestIcon;
	public GunData[] Guns;
	
	// private 
	CcNet net;
	List<Grenade> activeGrenades = new List<Grenade>();
	List<Rocket> activeRockets = new List<Rocket>();
	
	
	
	void Start() {
		net = GetComponent<CcNet>();

		// setup guns 
		Guns = new GunData[(int)Gun.Count];

		string s = "";
		for (int i = 0; i < Guns.Length; i++) {
			Guns[i] = new GunData();

			var n = S.GetSpacedOut("" + (Gun)i);
			Guns[i].Name = n;

			switch ((Gun)i) {
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

		// pics   (FOR NOW, cycle by id/index into an alphabetized LoadAll() array 
		for (int i = 0; i < Guns.Length; i++) {
			switch ((Gun)i) {
				case Gun.Pistol:   Guns[i].Color = Color.white; 
					Guns[i].Delay = 0.3f; 
					Guns[i].DelayAlt = 0.3f; break; 
				case Gun.GrenadeLauncher:   Guns[i].Color = Color.green; 
					Guns[i].Delay = 0.25f; 
					Guns[i].DelayAlt = 0.25f; break; 
				case Gun.MachineGun:   Guns[i].Color = Color.yellow; 
					Guns[i].Delay = 0.1f; 
					Guns[i].DelayAlt = 0.1f; Guns[i].AutoFire = true; break; // only 1 with AutoFire 
				case Gun.RailGun:   Guns[i].Color = Color.cyan; 
					Guns[i].Delay = 2f;
					Guns[i].MarkScale = 2f;
					Guns[i].DelayAlt = 2f; break; 
				case Gun.RocketLauncher:   Guns[i].Color = Color.red; 
					Guns[i].Delay = 1.5f; 
					Guns[i].DelayAlt = 0.7f;
					Guns[i].MarkScale = 5f; break; // set for the launcher because the projectile has a negative value in the gun system 
				case Gun.Swapper:   Guns[i].Color = Color.magenta; 
					Guns[i].Delay = 2f; 
					Guns[i].DelayAlt = 2f; break; 
				case Gun.Gravulator:   Guns[i].Color = Color.green; 
					Guns[i].Delay = 1f; 
					Guns[i].DelayAlt = 1f; break; 
				case Gun.Bomb:   Guns[i].Color = Color.yellow; 
					Guns[i].Delay = 1f; 
					Guns[i].DelayAlt = 1f; break; 
				case Gun.Spatula:   Guns[i].Color = Color.magenta; 
					Guns[i].Delay = 1f;  
					Guns[i].DelayAlt = 1f;
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
		
		activeGrenades = new List<Grenade>();
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
		
		var lbp = (GameObject)GameObject.Instantiate(GOs.Get("Lightning"));
		var lb = lbp.GetComponent<Lightning>();

		if (localFire && !hit) 
			lb.start = localstart;
		else
			lb.start = origin;

		lb.end = end;
		lb.hit = hit;
	}

	void shootHitscan(Vector3 origin, Vector3 end, NetworkViewID shooterID, Gun weapon, Vector3 hitNorm) {
		bool localFire = false;
		Vector3 localStart = origin;

		for (int i=0; i<net.players.Count; i++) {
			if (net.players[i].viewID == shooterID && net.players[i].local){
				localFire = true;
				localStart = net.players[i].Entity.HudGun.transform.position + (Camera.main.transform.forward * 0.5f);
			}
		}

		if (weapon == Gun.Spatula) // no trail or effects
			return;

		if (hitNorm != Vector3.zero) {
			// bullet mark
			GameObject nh = (GameObject)GameObject.Instantiate(GOs.Get("BulletMark"));
			nh.transform.position = end + hitNorm * 0.01f;
			nh.transform.forward = -hitNorm;
			nh.transform.localScale *= Guns[(int)weapon].MarkScale;
			nh.GetComponent<BulletMark>().StartCol = Color.Lerp(Color.gray, Color.black, Random.value);
			nh.GetComponent<BulletMark>().MaxLife = 10f;

			// particles
			for (int i = 0; i < 100; i++) {
				Vector3 diagonalVec = Quaternion.Euler(Random.Range(-30f, 30f), Random.Range(-30f, 30f), Random.Range(-30f, 30f)) * hitNorm;
				GameObject np = (GameObject)GameObject.Instantiate(GOs.Get("CcParticle"));
				np.transform.position = end + diagonalVec * Random.Range(0.1f, 0.3f);
				var p = np.GetComponent<CcParticle>();
				p.MoveVec = diagonalVec * Random.Range(2f, 3f);
				p.MinSize = 0.3f;
				p.MaxSize = 0.4f;
				p.StartColor = Guns[(int)weapon].Color;
				p.EndColor = Color.clear;
				p.ParticType = ParticleType.Puff;
				p.life = Random.Range(0.45f, 0.55f);
			}
		}
		
		// beam 
		var beam = (GameObject)GameObject.Instantiate(GOs.Get("Laser"));
		var b = beam.GetComponent<Laser>();

		if (localFire) 
			b.start = localStart;
		else
			b.start = origin;

		b.end = end - Vector3.Normalize(b.end - b.start) * 0.3f; // so that the trail seems to enter the wall instead of having a rectangular ending
		b.col = Guns[(int)weapon].Color;

		// muzzle flash 
		var mf = (GameObject)GameObject.Instantiate(GOs.Get("MuzzleFlash"));
		mf.light.color = Guns[(int)weapon].Color;
		mf.transform.position = origin;
		if (localFire) { // Sophie didn't allow remote player flashes? 
			mf.transform.position = localStart - (Camera.main.transform.right * 0.2f);
		}
		
		// rail trail 
		if (weapon == Gun.RailGun) {
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
				var np = (GameObject)GameObject.Instantiate(GOs.Get("CcParticle"));
				var v = Camera.main.transform.up / 4f;
				v = Quaternion.AngleAxis(angle, Camera.main.transform.forward) * v;
				var center = beamStart + (beamDir * progress);
				np.transform.position = center + v;
				var p = np.GetComponent<CcParticle>();
				p.MoveVec = Quaternion.AngleAxis(90f, Camera.main.transform.forward) * v * 2f;
				p.MinSize = 0.4f;
				p.MaxSize = 0.4f;
				p.StartColor = Color.blue;
				p.EndColor = Color.clear;
				p.ParticType = ParticleType.Circle;
				progress += 0.20f;
				angle += 24f;
			}
		}
	}

	public void Shoot(Gun weapon, Vector3 origin, Vector3 direction, Vector3 end, 
		NetworkViewID shooterID, NetworkViewID bulletID, double time, bool hit, bool alt, Vector3 hitNorm, bool sprint = false
	) {
		switch (weapon) {
			case Gun.Pistol:
			case Gun.MachineGun:
			case Gun.RailGun:
			case Gun.Spatula:
				shootHitscan(origin, end, shooterID, weapon, hitNorm);
				break;
		
			case Gun.Swapper:
				shootSwapper(origin, end, shooterID, hit);
				break;
			
			case Gun.GrenadeLauncher:
				var ng = (GameObject)GameObject.Instantiate(GOs.Get("Grenade"));
				var g = ng.GetComponent<Grenade>();
				g.AvPos = origin;
				g.direction = direction;
				g.startTime = time;
				g.viewID = bulletID;
				g.shooterID = shooterID;
				g.detonationTime = Random.Range(2.5f, 3.5f); // so that there's no effect of an explosion "hanging" in one place when you shoot a few nades w/out moving 
				
				activeGrenades.Add(g);
				break;
			
			case Gun.RocketLauncher:
				var nr = (GameObject)GameObject.Instantiate(GOs.Get("Rocket"));
				nr.transform.position = origin + direction; // start a bit outwards 
				nr.transform.LookAt(origin + direction * 2f);
				
				var	rs = nr.GetComponent<Rocket>();
				rs.viewID = bulletID;
				rs.shooterID = shooterID;
				
				activeRockets.Add(nr.GetComponent<Rocket>());

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
					case Gun.GrenadeLauncher:  playPitchedSfx(i, Sfx.Get("boosh")); break;
					case Gun.RocketLauncher:   playPitchedSfx(i, Sfx.Get("shot_bazooka")); break;
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
	
	public float GetWeaponDamage(Gun weapon) {
		switch (weapon) {
			case Gun.Pistol:         
				return 40f;
			case Gun.GrenadeLauncher:        
				return 60f;
			case Gun.MachineGun:     
				return 15f;
			case Gun.Spatula:          
				return 105f;
			case Gun.RailGun:          
				return 105f;
			case Gun.RocketLauncher: 
				return 70f;
			
			case Gun.Lava:           
				return 9999f;
			case Gun.Bomb:           
				return 9999f;
			case Gun.Suicide:        
				return 9999f;
		}

		return 0f;
	}
	
	// FIXME: pull BombBeep(), and the bomb related part of Detonate(), into a script in the bomb's folder 
	public void BombBeep(Vector3 pos) {
		var o = (GameObject)GameObject.Instantiate(GOs.Get("WeapSound")); // bomb beep/sound object 
		o.transform.position = pos;
		o.audio.clip = Sfx.Get("BombBeep");
		o.audio.volume = 1f;
	}
	
	public void Detonate(Gun weapon, Vector3 detPos, NetworkViewID viewID) {
		if (weapon == Gun.Bomb) {
			var o = (GameObject)GameObject.Instantiate(GOs.Get("GrenadeExplosion"));
			o.transform.position = detPos;
				
			var bomb = (GameObject)GameObject.Instantiate(GOs.Get("WeapSound"));
			bomb.transform.position = detPos;
			bomb.audio.clip = Sfx.Get("ExplodeBomb");
			bomb.audio.volume = 4f;
		} else if (weapon == Gun.RocketLauncher) {
			print ("WARNING: Detonate() was called for a " + Gun.RocketLauncher + "!");
		}
		
		for (int i=0; i<activeGrenades.Count; i++) {
			if (viewID == activeGrenades[i].viewID) {
				
				var o = (GameObject)GameObject.Instantiate(GOs.Get("GrenadeExplosion"));
				o.transform.position = activeGrenades[i].transform.position;
				
				var nade = (GameObject)GameObject.Instantiate(GOs.Get("WeapSound"));
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
						    < GetDetonationRadius(Gun.RocketLauncher)
					    ) {
							if (net.players[k].Entity.transform.position.y > activeRockets[i].transform.position.y) {
								if (activeRockets[i].shooterID == net.players[k].viewID){
									net.players[k].Entity.yMove = 9;
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
				var splo = (GameObject)GameObject.Instantiate(GOs.Get("GrenadeExplosion"));
				splo.transform.position = activeRockets[i].transform.position;
				var ws = (GameObject)GameObject.Instantiate(GOs.Get("WeapSound"));
				ws.transform.position = activeRockets[i].transform.position;
				ws.audio.clip = Sfx.Get("explosion_bazooka");
				ws.audio.volume = 0.99f;
				Destroy(activeRockets[i].gameObject);
				activeRockets.RemoveAt(i);

				// bullet marks 
				if (hitNorm != Vector3.zero) {
					var o = (GameObject)GameObject.Instantiate(GOs.Get("BulletMark"));
					o.transform.position = detPos + hitNorm * 0.03f;
					o.transform.forward = -hitNorm;
					o.transform.localScale *= Guns[(int)Gun.RocketLauncher].MarkScale;
					o.GetComponent<BulletMark>().StartCol = Color.Lerp(Color.gray, Color.black, Random.value);
					o.GetComponent<BulletMark>().MaxLife = 30f;
				}
			}
		}
	}
	
	public float GetDetonationRadius(Gun weapon) {
		switch (weapon) {
			case Gun.GrenadeLauncher: return 4;
			case Gun.RocketLauncher:  return 4;
			case Gun.Bomb:            return 10;
		}
		
		return 0;	}
}