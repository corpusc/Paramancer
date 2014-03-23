using UnityEngine;
using UnityEditor;
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
		handleFolder("Misc");
		handleFolder("Announce");
	}

	public static void PlayOmni(string s) { // seemingly positionless sound 
		AudioSource.PlayClipAtPoint(Get(s), Camera.main.transform.position);
	}
	
	public static AudioClip Get(string s) { // hash lookups in a Dictionary are fast right? 
		if (clips.ContainsKey(s))
			return clips[s].Clip;
		
		Debug.LogError("______ COULDN'T FIND THE FILE NAMED '" + s + "'!!! ______");
		return null;
	}
	
	public static AudioClip GetFirstWith(string s) { // this is slow i think 
		foreach (var pair in clips) {
			if (pair.Value.Clip.name.Contains(s))
				return pair.Value.Clip;
		}

		Debug.LogError("______ COULDN'T FIND ANY FILE WITH '" + s + "' IN THE NAME!!! ______");
		return null;
	}

	static void handleFolder(string s) {
		string feedback = "";
		var aClips = Resources.LoadAll<AudioClip>("Sfx/" + s);

		if (s == "Announce") {
			foreach (var ac in aClips) {
				var path = AssetDatabase.GetAssetPath(ac);
				var aImp = AssetImporter.GetAtPath(path) as AudioImporter;
				aImp.threeD = false;
				AssetDatabase.ImportAsset(path);
			}
		}
		
		foreach (var cl in aClips) {
			var nc = new CcClip();
			nc.Clip = cl;
			nc.Tags.Add(s);
			clips.Add(cl.name, nc);
			feedback += cl.name + " ";
		}

		Debug.Log("______" + s + "______ = " + feedback);
	}
}
