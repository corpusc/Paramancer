using UnityEngine;
using System.Collections;


public static class Sfx {
	static AudioClip[] clips;



	static Sfx() {
		clips = Resources.LoadAll<AudioClip>("Sfx/Misc");
	}

	public static AudioClip Get(string s) {
		for (int i = 0; i < clips.Length; i++) {
			if (clips[i].name == s)
				return clips[i];
		}

		return null;
	}
}
