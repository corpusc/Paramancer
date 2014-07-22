/////////////////////////////////////////////////////////////////////////////////
//
//	vp_MuzzleFlash.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	renders an additive, randomly rotated, fading out muzzleflash
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

public class vp_MuzzleFlash : MonoBehaviour
{

	protected float m_FadeSpeed = 0.075f;				// amount of alpha to be deducted each frame
	protected bool m_ForceShow = false;					// used to set the muzzleflash 'always on' in the editor
	protected Color m_Color = new Color(1, 1, 1, 0.0f);

	protected Transform m_Transform = null;

	public float FadeSpeed { get { return m_FadeSpeed; } set { m_FadeSpeed = value; } }
	public bool ForceShow { get { return m_ForceShow; } set { m_ForceShow = value; } }


	/// <summary>
	/// 
	/// </summary>
	void Awake()
	{

		m_Transform = transform;

		// the muzzleflash is meant to use the 'Particles/Additive'
		// (unity default) shader which has the 'TintColor' property
		m_Color = renderer.material.GetColor("_TintColor");
		m_Color.a = 0.0f;

		m_ForceShow = false;

	}


	/// <summary>
	/// 
	/// </summary>
	void Start()
	{
		// if parent is a local player, don't clip muzzleflash against world geometry
		if (m_Transform.root.gameObject.layer == vp_Layer.LocalPlayer)
			gameObject.layer = vp_Layer.Weapon;
	}


	/// <summary>
	/// 
	/// </summary>
	void Update()
	{

		// editor force show
		if (m_ForceShow)
			Show();
		else
		{
			// always fade out muzzleflash if it is visible
			if (m_Color.a > 0.0f)
				m_Color.a -= m_FadeSpeed * (Time.deltaTime * 60.0f);
		}
		renderer.material.SetColor("_TintColor", m_Color);

	}


	/// <summary>
	/// makes the muzzleflash show for editing purposes
	/// </summary>
	public void Show()
	{
		m_Color.a = 0.5f;	// the default alpha value for the 'Particles/Additive' shader is 0.5
	}


	/// <summary>
	/// shows and rotates the muzzleflash for when firing a shot
	/// </summary>
	public void Shoot()
	{
		m_Transform.Rotate(0, 0, Random.Range(0, 360));	// rotate randomly 360 degrees around z
		m_Color.a = 0.5f;	// the default alpha value for the 'Particles/Additive' shader is 0.5
	}

	
}

