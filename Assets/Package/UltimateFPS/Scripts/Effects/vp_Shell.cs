/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Shell.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a shell casing with rigidbody physics adapted for more
//					realistic behavior
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(AudioSource))]

public class vp_Shell : MonoBehaviour
{

	Transform m_Transform = null;
	Rigidbody m_Rigidbody = null;
	AudioSource m_Audio = null;

	public float LifeTime = 10;				// time to live in seconds for this type of shell
	protected float m_RemoveTime = 0.0f;	// the exact time of removal for this particular shell (calculated in Start)

	// physics
	public float m_Persistence = 1.0f;		// chance of shell _not_ being removed after settling on the ground
	public delegate void RestAngleFunc();	// function pointer for the chosen forced rest state
	protected RestAngleFunc m_RestAngleFunc;
	protected float m_RestTime = 0.0f;		// after this many seconds a rest state will be forced (calculated in Start)

	// sound
	public List<AudioClip> m_BounceSounds = new List<AudioClip>();	// list of sounds to be randomly played on each ground impact


	/// <summary>
	/// 
	/// </summary>
	void Start()
	{

		m_Transform = transform;
		m_Rigidbody = rigidbody;
		m_Audio = audio;

		m_RestAngleFunc = null;

		m_RemoveTime = Time.time + LifeTime;
		m_RestTime = Time.time + (LifeTime * 0.25f);

		m_Rigidbody.maxAngularVelocity = 100;		// allow shells to spin faster than rigidbody default
		
		audio.playOnAwake = false;
		audio.dopplerLevel = 0.0f;

	}


	/// <summary>
	/// 
	/// </summary>
	void Update()
	{

		if (m_RestAngleFunc == null)
		{
			// after a while we decide how the shell should come to rest.
			// see comments on 'DecideRestAngle' for details.
			if (Time.time > m_RestTime)
				DecideRestAngle();
		}
		else
			m_RestAngleFunc();

		if (Time.time > m_RemoveTime)
		{
			m_Transform.localScale = Vector3.Lerp(m_Transform.localScale, Vector3.zero, (Time.deltaTime * 60.0f) * 0.2f);
			if (Time.time > m_RemoveTime + 0.5f)
				Object.Destroy(gameObject);
		}


	}


	/// <summary>
	/// modifies the shell's behavior upon a hard impact, and
	/// on soft impact determines whether to remove it early.
	/// this is an optional optimization feature for weapons
	/// that emit large amounts of shells.
	/// </summary>
	void OnCollisionEnter(Collision collision)
	{
		
		// if collision velocity is sufficient, make a 'hard bounce'
		if (collision.relativeVelocity.magnitude > 2)
		{

			// on a hard bounce, we apply more random rotation velocity to make
			// the shell behave a bit unpredictably, like real brass shells do
			if (Random.value > 0.5f)
				m_Rigidbody.AddRelativeTorque(-Random.rotation.eulerAngles * 0.15f);
			else
				m_Rigidbody.AddRelativeTorque(Random.rotation.eulerAngles * 0.15f);

			// also, we play a random bounce sound
			if (m_Audio != null && m_BounceSounds.Count > 0)
			{
				m_Audio.pitch = Time.timeScale;
				m_Audio.PlayOneShot(m_BounceSounds[(int)Random.Range(0, (m_BounceSounds.Count))]);
			}

		}
		// soft collision = time to determine if this shell lives or dies
		else if (Random.value > m_Persistence)
		{
			// allow this shell to fall through geometry and
			// remove it after half a second
			collider.enabled = false;
			m_RemoveTime = Time.time + 0.5f;
		}

	}


	/// <summary>
	/// by default a rigidbody will not come to rest in a
	/// manner reminiscent of a brass shell, no matter what
	/// physics material you have set on it. this method
	/// determines if the shell should rest in its upright or
	/// tipped over angle, for more realistic shell motion.
	/// </summary>
	protected void DecideRestAngle()
	{

		float up = Mathf.Abs(m_Transform.eulerAngles.x - 270);		// see how close shell is to its upright position

		// if shell is close to standing up with its (heavier) base
		// toward the ground
		if (up < 55)
		{

			// see if the ground is flat
			Ray ray = new Ray(m_Transform.position, Vector3.down);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 1))
			{
				// if so, we will force the shell to its upright position
				if (hit.normal == Vector3.up)
				{
					m_RestAngleFunc = UpRight;
					m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
				}
			}

			return;

		}
		
		// either the shell is fairly tilted or the ground is not flat,
		// so we will force the shell to lie down
		m_RestAngleFunc = TippedOver;

	}


	/// <summary>
	/// quickly rotates the shell to an upright position
	/// </summary>
	protected void UpRight()
	{
		m_Transform.rotation = Quaternion.Lerp(m_Transform.rotation,
			Quaternion.Euler(-90, m_Transform.rotation.y, m_Transform.rotation.z),
								Time.time * ((Time.deltaTime * 60.0f) * 0.05f));
	}


	/// <summary>
	/// smoothly rotates the shell to a lying down position
	/// </summary>
	protected void TippedOver()
	{
		m_Transform.localRotation = Quaternion.Lerp(m_Transform.localRotation,
			Quaternion.Euler(0, m_Transform.localEulerAngles.y, m_Transform.localEulerAngles.z),
								Time.time * ((Time.deltaTime * 60.0f) * 0.005f));
	}


}


	