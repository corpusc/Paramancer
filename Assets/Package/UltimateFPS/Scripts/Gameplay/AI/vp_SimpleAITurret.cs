/////////////////////////////////////////////////////////////////////////////////
//
//	vp_SimpleAITurret.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	just a very quick test to verify that vp_Shooter can be used
//					independently of the FPPlayer
//
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(vp_Shooter))]

public class vp_SimpleAITurret : MonoBehaviour
{

	public float ViewRange = 10.0f;
	public float AimSpeed = 50.0f;
	public float WakeInterval = 2.0f;

	protected vp_Shooter m_Shooter = null;
	protected Transform m_Transform = null;
	protected Transform m_Target = null;
	protected vp_Timer.Handle m_Timer = new vp_Timer.Handle();



	/// <summary>
	/// 
	/// </summary>
	void Start()
	{
		m_Shooter = GetComponent<vp_Shooter>();
		m_Transform = transform;
	}


	/// <summary>
	/// 
	/// </summary>
	void Update()
	{

		// turn on and off with 'UpdateInterval'
		if (!m_Timer.Active)
		{
			vp_Timer.In(WakeInterval, delegate()
			{
				if (m_Target == null)
					m_Target = ScanForLocalPlayer();
				else
					m_Target = null;
			}, m_Timer);
		}

		if (m_Target != null)
			AttackTarget();

	}


	/// <summary>
	/// scans the area for the local player and returns its
	/// transform if present, and if we have line-of-sight
	/// </summary>
	Transform ScanForLocalPlayer()
	{

		Collider[] colliders = Physics.OverlapSphere(m_Transform.position, ViewRange, (1 << vp_Layer.LocalPlayer));
		foreach (Collider hit in colliders)
		{

			// found the local player within range, now see if we
			// have line-of-sight
			RaycastHit blocker;
			Physics.Linecast(m_Transform.position, hit.transform.position + Vector3.up, out blocker);

			// skip if raycast hit an object that wasn't the intended target
			if (blocker.collider != null && blocker.collider != hit)
				continue;

			// we have line of sight to the local player! return its transform
			return hit.transform;

		}

		return null;

	}


	/// <summary>
	/// smoothly aims at target while firing the shooter
	/// </summary>
	void AttackTarget()
	{

		// smoothly aim at target
		Vector3 dir = m_Target.position - m_Transform.position;
		Quaternion targetRotation = Quaternion.LookRotation(dir);
		m_Transform.rotation = Quaternion.RotateTowards(m_Transform.rotation, targetRotation, Time.deltaTime * AimSpeed);

		// fire the shooter
		m_Shooter.TryFire();

	}


}
