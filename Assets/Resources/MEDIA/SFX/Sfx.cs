using UnityEngine;
using System.Collections.Generic;



public static class Sfx {
	private static float volumeMaster = 1f;
	public static float VolumeMaster {
		get { return volumeMaster; }
		set {
			AudioListener.volume = value;
			volumeMaster = value;
		}
	}

	// private
	static Dictionary<string, CcClip> clips = new Dictionary<string, CcClip>();



	static Sfx() {
		handleFolder("Announce");
		handleFolder("Av");
		handleFolder("Explode");
		handleFolder("Futuristic weapons set - Raffaele");
		handleFolder("Loop");
		handleFolder("Misc");
		handleFolder("Activate");
	}

	public static void PlayOmni(string s) { // seemingly positionless sound.  FIXME? cuz that requires unchecking 3D/threeD?
		AudioSource.PlayClipAtPoint(Get(s), Camera.main.transform.position);
	}
	
	public static AudioClip Get(string s) { // hash lookups in a Dictionary are fast right? 
		if (clips.ContainsKey(s))
			return clips[s].Clip;
		
		Debug.LogError("______ Sfx: COULDN'T FIND THE FILE NAMED '" + s + "'!!! ______");
		return null;
	}

	public static CcClip GetCc(string s) { // hash lookups in a Dictionary are fast right? 
		if (clips.ContainsKey(s))
			return clips[s];
		
		Debug.LogError("______ Sfx: COULDN'T FIND THE FILE NAMED '" + s + "'!!! ______");
		return null;
	}
	
	public static AudioClip GetFirstWith(string s) { // this is slow i think.  iterating thru key/value pairs 
		foreach (var pair in clips) {
			if (pair.Value.Clip.name.Contains(s))
				return pair.Value.Clip;
		}

		Debug.LogError("______ COULDN'T FIND ANY FILE WITH '" + s + "' IN THE NAME!!! ______");
		return null;
	}

	static void handleFolder(string s) {
		string feedback = "";
		var aClips = Resources.LoadAll<AudioClip>("MEDIA/SFX/" + s);

		switch (s) {
			case "Announce":
				// hmmm, I guess we want them to be 3D for now....to at least hear what it sounds like 
				// with announcements coming from the player who achieved something (like a MultiFrag) 

				// make all these non-locational (2D)
//				foreach (var ac in aClips) {
//					var path = AssetDatabase.GetAssetPath(ac);
//					var aImp = AssetImporter.GetAtPath(path) as AudioImporter;
//					aImp.threeD = false;
//					AssetDatabase.ImportAsset(path);
//				}
				break;
		}
		
		// add to the master collection that includes files from all folders 
		foreach (var cl in aClips) {
			if (clips.ContainsKey(cl.name)) {
				Debug.LogError("______ The name: " + cl.name + " is already a registered AudioClip!!! ______");
			} else {
				var nc = new CcClip();
				nc.Clip = cl;
				nc.Tags.Add(s);

				switch (cl.name) {
					case "Jump": nc.Volume = 0.2f;	break;
					case "Catch": nc.Volume = 0.4f; break;
					case "click": nc.Volume = 0.2f; break;
					case "Die": nc.Volume = 1f; break;
					case "Gravulator": nc.Volume = 0.4f; break;
					case "guncocked": nc.Volume = 0.2f; break;
					case "Land": nc.Volume = 0.5f; break;
					case "Swapped": nc.Volume = 0.4f; break;
				}

				clips.Add(cl.name, nc);
				feedback += cl.name + ",  ";
			}
		}

		Debug.Log("______ SFX/" + s + " ______ " + feedback.TrimEnd(',', ' '));
	}
}
