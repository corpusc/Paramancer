/////////////////////////////////////////////////////////////////////////////////
//
//	vp_HitscanBullet.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a script for hitscan projectiles. this script should be
//					attached to a gameobject with a mesh to be used as the impact
//					decal (bullet hole)
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]

public class vp_HitscanBullet : MonoBehaviour
{

	public bool IgnoreLocalPlayer = true;

	// gameplay
	public float Range = 100.0f;			// max travel distance of this type of bullet in meters
	public float Force = 100.0f;			// force applied to any rigidbody hit by the bullet
	public float Damage = 1.0f;				// the damage transmitted to target by the bullet
	public string DamageMethodName = "Damage";	// user defined name of damage method on target
												// TIP: this can be used to apply different types of damage, i.e
												// magical, freezing, poison, electric

	public float m_SparkFactor = 0.5f;		// chance of bullet impact generating a spark

	// these gameobjects will all be spawned at the point and moment
	// of impact. technically they could be anything, but their
	// intended uses are as follows:
	public GameObject m_ImpactPrefab = null;	// a flash or burst illustrating the shock of impact
	public GameObject m_DustPrefab = null;		// evaporating dust / moisture from the hit material
	public GameObject m_SparkPrefab = null;		// a quick spark, as if hitting stone or metal
	public GameObject m_DebrisPrefab = null;	// pieces of material thrust out of the bullet hole and / or falling to the ground

	// sound
	protected AudioSource m_Audio = null;
	public List<AudioClip> m_ImpactSounds = new List<AudioClip>();	// list of impact sounds to be randomly played
	public Vector2 SoundImpactPitch = new Vector2(1.0f, 1.5f);	// random pitch range for impact sounds

	public int [] NoDecalOnTheseLayers;


	/// <summary>
	/// everything happens in the Start method. the script that
	/// spawns the bullet is responsible for setting its position 
	/// and angle. after being instantiated, the bullet immediately
	/// raycasts ahead for its full range, then snaps itself to
	/// the surface of the first object hit. it then spawns a
	/// number of particle effects and plays a random impact sound.
	/// </summary>
	void Start()
	{

		Transform t = transform;
		m_Audio = audio;

		Ray ray = new Ray(t.position, transform.forward);
		RaycastHit hit;

		// raycast against all big, solid objects except the player itself
		if (Physics.Raycast(ray, out hit, Range, (IgnoreLocalPlayer ? vp_Layer.Mask.BulletBlockers : vp_Layer.Mask.IgnoreWalkThru)))
		{

			// NOTE: we can't bail out of this if-statement based on !collider.isTrigger,
			// because that would make bullets _disappear_ if they hit a trigger. to make a
			// trigger not interfere with bullets, put it in the layer: 'vp_Layer.Trigger'
			// (default: 27)

			// move this gameobject instance to the hit object
			Vector3 scale = t.localScale;	// save scale to handle scaled parent objects
			t.parent = hit.transform;
			t.localPosition = hit.transform.InverseTransformPoint(hit.point);
			t.rotation = Quaternion.LookRotation(hit.normal);					// face away from hit surface
			if (hit.transform.lossyScale == Vector3.one)								// if hit object has normal scale
				t.Rotate(Vector3.forward, Random.Range(0, 360), Space.Self);	// spin randomly
			else
			{
				// rotated child objects will get skewed if the parent object has been
				// unevenly scaled in the editor, so on scaled objects we don't support
				// spin, and we need to unparent, rescale and reparent the decal.
				t.parent = null;
				t.localScale = scale;
				t.parent = hit.transform;
			}
			
			// if hit object has physics, add the bullet force to it
			Rigidbody body = hit.collider.attachedRigidbody;
			if (body != null && !body.isKinematic)
			{
				body.AddForceAtPosition((ray.direction * Force) / Time.timeScale, hit.point);
			}

			// spawn impact effect
			if (m_ImpactPrefab != null)
				Object.Instantiate(m_ImpactPrefab, t.position, t.rotation);

			// spawn dust effect
			if (m_DustPrefab != null)
				Object.Instantiate(m_DustPrefab, t.position, t.rotation);

			// spawn spark effect
			if (m_SparkPrefab != null)
			{
				if (Random.value < m_SparkFactor)
					Object.Instantiate(m_SparkPrefab, t.position, t.rotation);
			}

			// spawn debris particle fx
			if (m_DebrisPrefab != null)
				Object.Instantiate(m_DebrisPrefab, t.position, t.rotation);

			// play impact sound
			if (m_ImpactSounds.Count > 0)
			{
				m_Audio.pitch = Random.Range(SoundImpactPitch.x, SoundImpactPitch.y) * Time.timeScale;
				m_Audio.PlayOneShot(m_ImpactSounds[(int)Random.Range(0, (m_ImpactSounds.Count))]);
			}

			// do damage on the target
			hit.collider.SendMessageUpwards(DamageMethodName, Damage, SendMessageOptions.DontRequireReceiver);

			// prevent adding decals to objects based on layer
			if (NoDecalOnTheseLayers.Length > 0)
			{
				foreach (int layer in NoDecalOnTheseLayers)
				{

					if (hit.transform.gameObject.layer != layer)
						continue;

					TryDestroy();
					return;

				}
			}

			// if bullet is visible (i.e. has a decal), cueue it for deletion later
			if (gameObject.renderer != null)
				vp_DecalManager.Add(gameObject);
			else
				vp_Timer.In(1, TryDestroy);		// we have no renderer, so destroy object in 1 sec

		}
		else
			Object.Destroy(gameObject);	// hit nothing, so self destruct immediately

	}


	/// <summary>
	/// sees if the impact sound is still playing and, if not,
	/// destroys the object. otherwise tries again in 1 sec
	/// </summary>
	private void TryDestroy()
	{

		if (this == null)
			return;

		if (!m_Audio.isPlaying)
			Object.Destroy(gameObject);
		else
			vp_Timer.In(1, TryDestroy);

	}


}

