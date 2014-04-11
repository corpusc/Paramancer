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
	
	public AudioClip sfx_grenadeExplode;
	public AudioClip sfx_rocketExplode;
	public AudioClip sfx_machinegunshoot;
	public AudioClip sfx_pistolshoot;
	public AudioClip sfx_rifleshoot;
	public AudioClip sfx_grenadethrow;
	public AudioClip sfx_swappershoot;
	public AudioClip sfx_bombBeep;
	public AudioClip sfx_bombExplode;
	
	public int WidestIcon;
	public int TallestIcon;
	public GunData[] Guns;
	
	// private 
	CcNet net;
	List<GrenadeScript> activeGrenades = new List<GrenadeScript>();
	List<RocketScript> activeRockets = new List<RocketScript>();
	
	
	
	void Start () {
		net = GetComponent<CcNet>();
		
		// weapon media
		// load the 3 kinds of resources that weapons need
		Object[] mats = Resources.LoadAll("Mat/Weap");
		Object[] prefabs = Resources.LoadAll("Prefab/Weap");
		Texture[] pics = Resources.LoadAll<Texture>("Pic/Weap");

		// setup guns
		Guns = new GunData[9];
		for (int i = 0; i < Guns.Length; i++) {
			Guns[i] = new GunData();
			Guns[i].Name = S.GetSpacedOut("" + (Item)i);
			Guns[i].Pic = pics[i];
			Guns[i].Mat = (Material)mats[i];
			Guns[i].Prefab = (GameObject)prefabs[i];
			
			switch ((Item)i) {
				case Item.Pistol: 
				Guns[i].Delay = 0.3f; Guns[i].AltDelay = 0.3f; break; 
				case Item.Grenade:  
				Guns[i].Delay = 0.25f; Guns[i].AltDelay = 0.25f; break; 
				case Item.MachineGun:  
				Guns[i].Delay = 0.1f; Guns[i].AltDelay = 0.1f; Guns[i].AutoFire = true; Guns[i].ShotCol = Color.green; break; 
				case Item.Rifle:  
				Guns[i].Delay = 2f; Guns[i].AltDelay = 2f; Guns[i].ShotCol = Color.blue; break; 
				case Item.RocketLauncher:  
				Guns[i].Delay = 1.5f; Guns[i].AltDelay = 0.7f; break; 
				case Item.Swapper:  
				Guns[i].Delay = 2f; Guns[i].AltDelay = 2f; break; 
				case Item.GravGun:  
				Guns[i].Delay = 1f; Guns[i].AltDelay = 1f; break; 
				case Item.Bomb:  
				Guns[i].Delay = 1f; Guns[i].AltDelay = 1f; break; 
				case Item.Spatula:  
				Guns[i].Delay = 1f; Guns[i].AltDelay = 1f; break;
			}
			
			// set widest icon
			int w = pics[i].width;
			if (WidestIcon < w) 
				WidestIcon = w;
			
			// set tallest icon
			int h = pics[i].height;
			if (TallestIcon < h) 
				TallestIcon = h;
		}
	}
	
	public void Clear() {
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
				localstart = net.players[i].Entity.firstPersonGun.transform.position + (Camera.main.transform.forward*0.5f);
			}
		}
		
		var nb = (GameObject)GameObject.Instantiate(LightningBeamPrefab);
		nb.GetComponent<LightningBeam>().start = origin;
		if (localFire && !hit) nb.GetComponent<LightningBeam>().start = localstart;
		nb.GetComponent<LightningBeam>().end = end;
		nb.GetComponent<LightningBeam>().hit = hit;
	}

	void shootHitscan(Vector3 origin, Vector3 end, NetworkViewID shooterID, Item weapon) {
		bool localFire = false;
		Vector3 localstart = origin;

		for (int i=0; i<net.players.Count; i++) {
			if (net.players[i].viewID == shooterID && net.players[i].local){
				localFire = true;
				localstart = net.players[i].Entity.firstPersonGun.transform.position + (Camera.main.transform.forward*0.5f);
			}
		}
		
		// beam
		var beam = (GameObject)GameObject.Instantiate(Beam);
		beam.GetComponent<BeamEffect>().start = origin;
		if (localFire) 
			beam.GetComponent<BeamEffect>().start = localstart;
		beam.GetComponent<BeamEffect>().end = end;
		beam.GetComponent<BeamEffect>().col = Guns[(int)weapon].ShotCol;
		if (weapon == Item.Pistol)
			beam.renderer.material.color = Color.white;
		
		// flash
		var muzzleFlash = (GameObject)GameObject.Instantiate(muzzleFlashPrefab);
		muzzleFlash.transform.position = origin;
		if (localFire) 
			muzzleFlash.transform.position = localstart - (Camera.main.transform.right * 0.2f);
		
		// rifle/rail trail
		if (weapon == Item.Rifle) {
			Vector3 beamStart = origin;
			if (localFire) 
				beamStart = localstart;

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
				np.GetComponent<BeamParticle>().ShotFromRifle = true;
				progress += 0.20f;
				angle += 24f;
			}
		}
	}

	public void Shoot(Item weapon, Vector3 origin, Vector3 direction, Vector3 end, 
		NetworkViewID shooterID, NetworkViewID bulletID, double time, bool hit, bool alt, bool sprint = false
	) {
		switch (weapon) {
			case Item.Pistol:
			case Item.MachineGun:
			case Item.Rifle:
				shootHitscan(origin, end, shooterID, weapon);
				break;
		
			case Item.Swapper:
				shootSwapper(origin, end, shooterID, hit);
				break;
			
			case Item.Grenade:
				GameObject newGrenade = (GameObject)GameObject.Instantiate(grenadeBulletPrefab);
				newGrenade.GetComponent<GrenadeScript>().start = origin;
				newGrenade.GetComponent<GrenadeScript>().direction = direction;
				newGrenade.GetComponent<GrenadeScript>().startTime = time;
				newGrenade.GetComponent<GrenadeScript>().viewID = bulletID;
				newGrenade.GetComponent<GrenadeScript>().shooterID = shooterID;
				newGrenade.GetComponent<GrenadeScript>().detonationTime = 3f;
				newGrenade.GetComponent<GrenadeScript>().ThrowFaster = sprint;
				
				activeGrenades.Add(newGrenade.GetComponent<GrenadeScript>());
				break;
			
			case Item.RocketLauncher:
					var nr = (GameObject)GameObject.Instantiate(rocketPrefab);
					nr.transform.position = origin;
					nr.transform.LookAt(origin + direction);
					
					var	rs = nr.GetComponent<RocketScript>();
					rs.viewID = bulletID;
					rs.shooterID = shooterID;
					
					activeRockets.Add(nr.GetComponent<RocketScript>());
					if(alt)
						rs.Turning = true;
				break;
		}
		
		for (int i=0; i<net.players.Count; i++) {
			if (net.players[i].viewID == shooterID) {
				switch (weapon) {
					case Item.Pistol:         playSfx(i, sfx_pistolshoot); break;
					case Item.Grenade:        playSfx(i, sfx_grenadethrow); break;
					case Item.MachineGun:     playSfx(i, sfx_machinegunshoot); break;
					case Item.Rifle:          playSfx(i, sfx_rifleshoot); break;
					case Item.RocketLauncher: playSfx(i, sfx_grenadethrow); break;
					// case Item.GravGun: *** soundwise, the activation sound is current located along with jump/land/etc. sfx
					case Item.Swapper:        playSfx(i, sfx_swappershoot); break;
				}
			}
		}
	}
		
	void playSfx(int i, AudioClip ac) {
		net.players[i].Entity.weaponSoundObj.audio.clip = ac;
		
		
		if /* local user */ (net.players[i].viewID == net.localPlayer.viewID) 
			net.players[i].Entity.weaponSoundObj.audio.volume = 0.3f;
		
		net.players[i].Entity.weaponSoundObj.audio.pitch = Random.Range(0.9f,1.1f);
		net.players[i].Entity.weaponSoundObj.audio.Play();
	}
	
	public float GetWeaponDamage(Item weapon) {
		switch (weapon) {
			case Item.Pistol:         return 40f;
			case Item.Grenade:        return 60f;
			case Item.MachineGun:     return 15f;
			case Item.Rifle:          return 105f;
			case Item.RocketProjectile: return 70f;
			
			case Item.Lava:           return 9999f;
			case Item.Bomb:           return 9999f;
			case Item.Suicide:        return 9999f;
		}

		return 0f;
	}
	
	public void BombBeep(Vector3 pos) {
		GameObject bombBeepObj = (GameObject)GameObject.Instantiate(soundObjectPrefab);
		bombBeepObj.transform.position = pos;
		bombBeepObj.audio.clip = sfx_bombBeep;
		bombBeepObj.audio.volume = 1f;
	}
	
	public void Detonate(Item weapon, Vector3 detPos, NetworkViewID viewID) {
		if (weapon == Item.Bomb) {
			GameObject bombFlash = (GameObject)GameObject.Instantiate(grenadeFlashPrefab);
			bombFlash.transform.position = detPos;
			bombFlash.transform.localScale *= 2f;
				
			GameObject bombSoundObj = (GameObject)GameObject.Instantiate(soundObjectPrefab);
			bombSoundObj.transform.position = detPos;
			bombSoundObj.audio.clip = sfx_bombExplode;
			bombSoundObj.audio.volume = 4f;
		}
		
		for (int i=0; i<activeGrenades.Count; i++) {
			if (viewID == activeGrenades[i].viewID) {
				
				GameObject grenadeFlash = (GameObject)GameObject.Instantiate(grenadeFlashPrefab);
				grenadeFlash.transform.position = activeGrenades[i].transform.position;
				
				GameObject grenadeSoundObj = (GameObject)GameObject.Instantiate(soundObjectPrefab);
				grenadeSoundObj.transform.position = activeGrenades[i].transform.position;
				grenadeSoundObj.audio.clip = sfx_grenadeExplode;
				grenadeSoundObj.audio.volume = 2f;
				
				
				Destroy(activeGrenades[i].gameObject);
				activeGrenades.RemoveAt(i);
				
			}
		}
		
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
				rocketSoundObj.audio.clip = sfx_rocketExplode;
				rocketSoundObj.audio.volume = 4f;
				Destroy(activeRockets[i].gameObject);
				activeRockets.RemoveAt(i);
			}
		}
	}
	
	public float GetDetonationRadius(Item weapon) {
		switch (weapon) {
			case Item.Grenade:        return 4;
			case Item.RocketProjectile: return 4;
			case Item.Bomb:           return 10;
		}
		
		return 0;	}
}