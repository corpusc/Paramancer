using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon : MonoBehaviour {
	public GameObject pistolBulletPrefab;
	public GameObject swapperBulletPrefab;
	public GameObject grenadeBulletPrefab;
	public GameObject rifleDissipationPrefab;
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
	
	public GunTypeScript[] gunTypes;
	
	// private 
	CcNet net;
	List<GrenadeScript> activeGrenades = new List<GrenadeScript>();
	List<RocketScript> activeRockets = new List<RocketScript>();
	
	
	
	void Start () {
		net = GetComponent<CcNet>();
		
		// load the 3 kinds of resources that weapons need
		Object[] mats = Resources.LoadAll("Mat/Weap");
		foreach (var m in mats) {
			Debug.Log("mat: " + m.name);
		}
		
		Object[] prefabs = Resources.LoadAll("Prefab/Weap");
		foreach (var p in prefabs) {
			Debug.Log("prefab: " + p.name);
		}
		
		Object[] pics = Resources.LoadAll("Pic/Weap");
		foreach (var p in pics) {
			Debug.Log("pic: " + p.name);
		}
		
		//gunTypes = new GunTypeScript[9];
		//gunTypes[
	}
	
	public void Clear() {
		for (int i=0; i<activeGrenades.Count; i++){
			if (activeGrenades[i] != null && activeGrenades[i].gameObject != null) 
				Destroy(activeGrenades[i].gameObject);
		}
		
		activeGrenades = new List<GrenadeScript>();
	}

	public void Shoot(string weaponType, Vector3 origin, Vector3 direction, Vector3 end, NetworkViewID shooterID, NetworkViewID bulletID, double time, bool hit){
		if (weaponType == "pistol" || weaponType == "machinegun" || weaponType == "rifle"){
			bool localFire = false;
			Vector3 localstart = origin;
			for (int i=0; i<net.players.Count; i++) {
				if (net.players[i].viewID == shooterID && net.players[i].local){
					localFire = true;
					localstart = net.players[i].Entity.firstPersonGun.transform.position + (Camera.main.transform.forward*0.5f);
				}
			}
			
			GameObject newBullet = (GameObject)GameObject.Instantiate(pistolBulletPrefab);
			newBullet.GetComponent<SimplePistolBullet>().start = origin;
			
			if (localFire) 
				newBullet.GetComponent<SimplePistolBullet>().start = localstart;
			
			newBullet.GetComponent<SimplePistolBullet>().end = end;
			
			GameObject muzzleFlash = (GameObject)GameObject.Instantiate(muzzleFlashPrefab);
			muzzleFlash.transform.position = origin;
			
			if (localFire) 
				muzzleFlash.transform.position = localstart 
					- (Camera.main.transform.right * 0.2f);
			
			if (weaponType == "rifle") {
				Vector3 dissipationStart = origin;
				if (localFire) dissipationStart = localstart;
				Vector3 dissipationDirection = (end-dissipationStart).normalized;
				float dissipationLength = Vector3.Distance(end, dissipationStart);
				if (dissipationLength > 40f) dissipationLength = 40f;
				float dissipationProgress = 0f;
				while (dissipationProgress<dissipationLength){
					GameObject newDiss = (GameObject)GameObject.Instantiate(rifleDissipationPrefab);
					newDiss.transform.position = dissipationStart + (dissipationDirection * dissipationProgress);
					dissipationProgress += Random.Range(0.3f,0.7f);
				}
			}
		}
		
		if (weaponType == "grenade") {
			GameObject newGrenade = (GameObject)GameObject.Instantiate(grenadeBulletPrefab);
			newGrenade.GetComponent<GrenadeScript>().start = origin;
			newGrenade.GetComponent<GrenadeScript>().direction = direction;
			newGrenade.GetComponent<GrenadeScript>().startTime = time;
			newGrenade.GetComponent<GrenadeScript>().viewID = bulletID;
			newGrenade.GetComponent<GrenadeScript>().shooterID = shooterID;
			newGrenade.GetComponent<GrenadeScript>().detonationTime = 3f;
			
			activeGrenades.Add(newGrenade.GetComponent<GrenadeScript>());
		}
		
		if (weaponType == "rocketlauncher") {
			GameObject newRocket = (GameObject)GameObject.Instantiate(rocketPrefab);
			newRocket.transform.position = origin;
			newRocket.transform.LookAt(origin + direction);
			newRocket.GetComponent<RocketScript>().viewID = bulletID;
			newRocket.GetComponent<RocketScript>().shooterID = shooterID;
			
			activeRockets.Add(newRocket.GetComponent<RocketScript>());
		}
		
		if (weaponType == "swapper") {
			bool localFire = false;
			Vector3 localstart = origin;
			for (int i=0; i<net.players.Count; i++){
				if (net.players[i].viewID == shooterID && net.players[i].local) {
					localFire = true;
					localstart = net.players[i].Entity.firstPersonGun.transform.position + (Camera.main.transform.forward*0.5f);
				}
			}
			
			var nb = (GameObject)GameObject.Instantiate(swapperBulletPrefab);
			nb.GetComponent<SwapperBullet>().start = origin;
			if (localFire && !hit) nb.GetComponent<SwapperBullet>().start = localstart;
			nb.GetComponent<SwapperBullet>().end = end;
			nb.GetComponent<SwapperBullet>().hit = hit;
		}
		
		for (int i=0; i<net.players.Count; i++) {
			if (net.players[i].viewID == shooterID) {
				switch (weaponType) {
					case "pistol":	   playSfx(i, sfx_pistolshoot); break;
					case "machinegun": playSfx(i, sfx_machinegunshoot); break;
					case "rifle":      playSfx(i, sfx_rifleshoot); break;
					case "grenade":	   playSfx(i, sfx_grenadethrow); break;
					case "swapper":	   playSfx(i, sfx_swappershoot); break;
				}
			}
		}
	}
		
	void playSfx(int i, AudioClip ac) {
		net.players[i].Entity.weaponSoundObj.audio.clip = ac;
		
		if (net.players[i].viewID == net.localPlayer.viewID) 
			net.players[i].Entity.weaponSoundObj.audio.volume = 0.3f;
		
		net.players[i].Entity.weaponSoundObj.audio.pitch = Random.Range(0.9f,1.1f);
		net.players[i].Entity.weaponSoundObj.audio.Play();
	}
	
	public float GetWeaponDamage(string weaponType){
		if (weaponType == "pistol") return 40f;
		if (weaponType == "grenade") return 70f;
		if (weaponType == "machinegun") return 15f;
		if (weaponType == "rifle") return 105f;
		if (weaponType == "suicide") return 9999f;
		if (weaponType == "rocket") return 70f;
		if (weaponType == "lava") return 9999f;
		if (weaponType == "bomb") return 9999f;
		return 0;
	}
	
	public void BombBeep(Vector3 pos){
		GameObject bombBeepObj = (GameObject)GameObject.Instantiate(soundObjectPrefab);
		bombBeepObj.transform.position = pos;
		bombBeepObj.audio.clip = sfx_bombBeep;
		bombBeepObj.audio.volume = 1f;
	}
	
	public void Detonate(string weaponType, Vector3 detPos, NetworkViewID viewID){
		
		if (weaponType == "bomb"){
			GameObject bombFlash = (GameObject)GameObject.Instantiate(grenadeFlashPrefab);
			bombFlash.transform.position = detPos;
			bombFlash.transform.localScale *= 2f;
				
			GameObject bombSoundObj = (GameObject)GameObject.Instantiate(soundObjectPrefab);
			bombSoundObj.transform.position = detPos;
			bombSoundObj.audio.clip = sfx_bombExplode;
			bombSoundObj.audio.volume = 4f;
		}
		
		
		for (int i=0; i<activeGrenades.Count; i++){
			if (viewID == activeGrenades[i].viewID){
				
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
		
		
		for (int i=0; i<activeRockets.Count; i++){
			if (viewID == activeRockets[i].viewID){
				
				//rocket jumping
				for (int k=0; k<net.players.Count; k++){
					if (net.players[k].local){
						if (Vector3.Distance(net.players[k].Entity.transform.position, activeRockets[i].transform.position) < GetDetonationRadius("rocket")){
							if (net.players[k].Entity.transform.position.y > activeRockets[i].transform.position.y){
								if (activeRockets[i].shooterID == net.players[k].viewID){
									net.players[k].Entity.yMove = 8;
								}else{
									net.players[k].Entity.yMove = 3;
								}
								net.players[k].Entity.grounded = false;
								net.players[k].Entity.sendRPCUpdate = true;
							}
						}
					}
				}
				
				//detonate rocket
				
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
	
	public float GetDetonationRadius(string weaponType){
		if (weaponType == "grenade") return 4;
		if (weaponType == "rocket") return 4;
		if (weaponType == "bomb") return 10;
		return 0;
	}
	
}
