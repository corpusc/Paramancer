/////////////////////////////////////////////////////////////////////////////////
//
//	vp_RandomSpawner.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	spawns a random object from a user populated list
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]

public class vp_RandomSpawner : MonoBehaviour
{

	// sound
	AudioSource m_Audio = null;
	public AudioClip Sound = null;
	public float SoundMinPitch = 0.8f;
	public float SoundMaxPitch = 1.2f;
	public bool RandomAngle = true;

	public List<GameObject> SpawnObjects = null;

	/// <summary>
	/// 
	/// </summary>
	void Awake()
	{

		if (SpawnObjects == null)
			return;

		int i = (int)Random.Range(0, (SpawnObjects.Count));

		if(SpawnObjects[i] == null)
			return;

		GameObject obj = (GameObject)Object.Instantiate(SpawnObjects[i], transform.position, transform.rotation);

		obj.transform.Rotate(Random.rotation.eulerAngles);
		m_Audio = audio;
		m_Audio.playOnAwake = true;
		
		// play sound
		if (Sound != null)
		{
			m_Audio.rolloffMode = AudioRolloffMode.Linear;
			m_Audio.clip = Sound;
			m_Audio.pitch = Random.Range(SoundMinPitch, SoundMaxPitch) * Time.timeScale;
			m_Audio.Play();
		}

	}


}

