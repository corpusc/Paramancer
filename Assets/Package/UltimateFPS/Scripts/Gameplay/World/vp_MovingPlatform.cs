/////////////////////////////////////////////////////////////////////////////////
//
//	vp_MovingPlatform.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a very versatile moving platform class, supporting various motion
//					interpolation modes, path behaviors, collision detection, player
//					physics interaction and sound fx.
//					
//					NOTE: this initial version of the class has a few methods beginning
//					with 'On'. except from the standard Unity callbacks, these are not
//					event methods per se - for now they are only hints as to where
//					additional functionality could be triggered. for example, an upcoming
//					feature might be to send a camera stomp in 'OnStop'
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]

public class vp_MovingPlatform : MonoBehaviour
{

	// misc
	protected Transform m_Transform = null;

	// path
	public PathMoveType PathType = PathMoveType.PingPong;
	public GameObject PathWaypoints = null;
	public Direction PathDirection = Direction.Forward;
	public int TargetedWaypoint{ get{ return m_TargetedWayPoint; } }
	public int MoveAutoStartTarget = 1000;
	protected List<Transform> m_Waypoints = new List<Transform>();
	protected int m_NextWaypoint = 0;
	protected Vector3 m_CurrentTargetPosition = Vector3.zero;
	protected Vector3 m_CurrentTargetAngle = Vector3.zero;
	protected int m_TargetedWayPoint = 0;
	protected float m_TravelDistance = 0.0f;
	protected Vector3 m_OriginalAngle = Vector3.zero;
	protected int m_CurrentWaypoint = 0;

	// comparer to sort waypoints in alphabetical order
	protected class WaypointComparer : IComparer
	{
		int IComparer.Compare(System.Object x, System.Object y)
		{ return ((new CaseInsensitiveComparer()).Compare(((Transform)x).name, ((Transform)y).name)); }
	}

	// movement
	public float MoveSpeed = 0.1f;
	public float MoveReturnDelay = 0.0f;		// time before returning to start upon stopping at target (Target mode)
	public float MoveCooldown = 0.0f;			// time to sleep (disable trigger) after returning to start
	public MovementInterpolationMode MoveInterpolationMode = MovementInterpolationMode.EaseInOut;
	protected bool m_Moving = false;
	protected float m_NextAllowedMoveTime = 0.0f;
	protected float m_MoveTime;
	protected vp_Timer.Handle m_ReturnDelayTimer = new vp_Timer.Handle();
	protected Vector3 m_PrevPos = Vector3.zero;		// used to calculate velocity
	protected AnimationCurve m_EaseInOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	protected AnimationCurve m_LinearCurve = AnimationCurve.Linear(0, 0, 1, 1);
	
	// rotation
	public float RotationEaseAmount = 0.1f;
	public Vector3 RotationSpeed = Vector3.zero;
	public RotateInterpolationMode RotationInterpolationMode = RotateInterpolationMode.SyncToMovement;
	protected Vector3 m_PrevAngle = Vector3.zero;	// used to calculate velocity


	// sound
	public AudioClip SoundStart = null;
	public AudioClip SoundStop = null;
	public AudioClip SoundMove = null;
	public AudioClip SoundWaypoint = null;
	protected AudioSource m_Audio = null;

	// physics
	public bool PhysicsSnapPlayerToTopOnIntersect = true;
	public float m_PhysicsPushForce = 2.0f;
	protected Rigidbody m_RigidBody = null;
	protected Collider m_Collider = null;
	protected Collider m_PlayerCollider = null;
	protected vp_FPPlayerEventHandler m_PlayerToPush = null;
	protected float m_PhysicsCurrentMoveVelocity;
	protected float m_PhysicsCurrentRotationVelocity;

	// dictionary to optimize lookup of colliding players
	protected Dictionary<Collider, vp_FPPlayerEventHandler> m_KnownPlayers = new Dictionary<Collider, vp_FPPlayerEventHandler>();

	// enums

	public enum PathMoveType
	{
		PingPong,	// goes to the last waypoint and back the same way without stopping
		Loop,		// goes to the last waypoint, then directly to the first without stopping
		Target		// stands still unless given a target
	}

	public enum Direction
	{
		Forward,
		Backwards,
		Direct
	}
	
	public enum MovementInterpolationMode
	{
		EaseInOut,
		EaseIn,
		EaseOut,
		EaseOut2,
		Slerp,
		Lerp
	}
	
	public enum RotateInterpolationMode
	{
		SyncToMovement,
		EaseOut,
		CustomEaseOut,
		CustomRotate
	}


	/// <summary>
	/// 
	/// </summary>
	void Start()
	{

		m_Transform = transform;
		m_Collider = GetComponentInChildren<Collider>();
		m_RigidBody = rigidbody;
		m_RigidBody.useGravity = false;
		m_RigidBody.isKinematic = true;

		m_NextWaypoint = 0;
		m_Audio = audio;
		m_Audio.loop = true;
		m_Audio.clip = SoundMove;

		// abort if no waypoints
		if (PathWaypoints == null)
			return;

		// init the hierarchy
		gameObject.layer = vp_Layer.MovableObject;
		foreach (Transform w in PathWaypoints.transform)
		{
			if (vp_Utility.IsActive(w.gameObject))
			{
				m_Waypoints.Add(w);
				w.gameObject.layer = vp_Layer.MovableObject;
			}
			if (w.renderer != null)
				w.renderer.enabled = false;
			if (w.collider != null)
				w.collider.enabled = false;
		}

		// sort the waypoints alphabetically
		IComparer comparer = new WaypointComparer();
		m_Waypoints.Sort(comparer.Compare);
		
		// prepare for takeoff
		if (m_Waypoints.Count > 0)
		{
			m_CurrentTargetPosition = m_Waypoints[m_NextWaypoint].position;
			m_CurrentTargetAngle = m_Waypoints[m_NextWaypoint].eulerAngles;
			m_Transform.position = m_CurrentTargetPosition;
			m_Transform.eulerAngles = m_CurrentTargetAngle;
			if (MoveAutoStartTarget > m_Waypoints.Count - 1)
				MoveAutoStartTarget = m_Waypoints.Count - 1;
		}

	}


	/// <summary>
	/// 
	/// </summary>
	void FixedUpdate()
	{
				
		UpdatePath();
		
		UpdateMovement();

		UpdateRotation();
		
		UpdateVelocity();

	}


	/// <summary>
	/// detects touching a waypoint, schedules going to the next one
	/// and triggers effects on various states (start, stop, arrive etc.)
	/// </summary>
	protected void UpdatePath()
	{
		
		if (m_Waypoints.Count < 2)
			return;

		if (GetDistanceLeft() < 0.01f)
		{

			if (Time.time >= m_NextAllowedMoveTime)
			{

				switch (PathType)
				{

					// --- target mode ---
					case PathMoveType.Target:

						if (m_NextWaypoint == m_TargetedWayPoint)
						{
							if (m_Moving)
								OnStop();
							else if(m_NextWaypoint != 0)
								OnArriveAtDestination();
						}
						else
						{
							if (m_Moving)
							{
								if (m_PhysicsCurrentMoveVelocity == 0.0f)
									OnStart();
								else
									OnArriveAtWaypoint();
							}
							GoToNextWaypoint();
						}
						break;

					// --- loop mode ---
					case PathMoveType.Loop:
						OnArriveAtWaypoint();
						GoToNextWaypoint();
						break;

					// --- ping pong mode ---
					case PathMoveType.PingPong:
						if (PathDirection == Direction.Backwards)
						{
							if (m_NextWaypoint == 0)
								PathDirection = Direction.Forward;
						}
						else
						{
							if (m_NextWaypoint == (m_Waypoints.Count - 1))
								PathDirection = Direction.Backwards;
						}
						OnArriveAtWaypoint();
						GoToNextWaypoint();
						break;
				}
			}

		}

	
	}


	/// <summary>
	/// called when a platform in 'Target' mode begins moving
	/// </summary>
	protected void OnStart()
	{

		// play the start sound, if any
		if (SoundStart != null)
			m_Audio.PlayOneShot(SoundStart);

		// SNIPPET: here is how to detect whether we are heading out
		// from the first versus going back from the last waypoint
		/*
		if (m_CurrentWaypoint == 0)
			Debug.Log("... departing: " + Time.time);
		else if (m_NextWaypoint == m_Waypoints.Count - 1)
			Debug.Log("... going back: " + Time.time);
		*/

	}
	

	/// <summary>
	/// called when a platform in any mode touches a waypoint. in 'Target'
	/// mode, the first and last waypoints are not included
	/// </summary>
	protected void OnArriveAtWaypoint()
	{

		// play the waypoint sound, if any
		if (SoundWaypoint != null)
			m_Audio.PlayOneShot(SoundWaypoint);

	}
	

	/// <summary>
	/// called when a platform in 'Target' mode arrives at the last waypoint
	/// </summary>
	protected void OnArriveAtDestination()
	{

		// if the platform should wait before going back, schedule the return
		if ((MoveReturnDelay > 0.0f) && !m_ReturnDelayTimer.Active)
		{
			vp_Timer.In(MoveReturnDelay, delegate()
			{
				GoTo(0);
			}, m_ReturnDelayTimer);
		}

	}
	

	/// <summary>
	/// called when a platform in 'Target' mode stops
	/// </summary>
	protected void OnStop()
	{

		m_Audio.Stop();

		// play the stop sound, if any
		if (SoundStop != null)
			m_Audio.PlayOneShot(SoundStop);

		// snap in place
		m_Transform.position = m_CurrentTargetPosition;
		m_Transform.eulerAngles = m_CurrentTargetAngle;
		m_Moving = false;

		// if we are returning home and have cooldown, disable platform for a while
		if (m_NextWaypoint == 0)
			m_NextAllowedMoveTime = Time.time + MoveCooldown;

	}


	/// <summary>
	/// updates platform position according to the current movement
	/// interpolation mode
	/// </summary>
	protected void UpdateMovement()
	{

		if (m_Waypoints.Count < 2)
			return;

		switch (MoveInterpolationMode)
		{

			case MovementInterpolationMode.EaseInOut:
				m_Transform.position = vp_Utility.NaNSafeVector3(Vector3.Lerp(	m_Transform.position,
																				m_CurrentTargetPosition,
																				m_EaseInOutCurve.Evaluate(m_MoveTime)));
				break;
			case MovementInterpolationMode.EaseIn:
				m_Transform.position = vp_Utility.NaNSafeVector3(Vector3.MoveTowards(	m_Transform.position,
																						m_CurrentTargetPosition,
																						m_MoveTime));
				break;
			case MovementInterpolationMode.EaseOut:
				m_Transform.position = vp_Utility.NaNSafeVector3(Vector3.Lerp(	m_Transform.position,
																				m_CurrentTargetPosition,
																				m_LinearCurve.Evaluate(m_MoveTime)));
				break;
			case MovementInterpolationMode.EaseOut2:
				m_Transform.position = vp_Utility.NaNSafeVector3(Vector3.Lerp(	m_Transform.position,
																				m_CurrentTargetPosition,
																				MoveSpeed * 0.25f));
				break;
			case MovementInterpolationMode.Lerp:
				m_Transform.position = vp_Utility.NaNSafeVector3(Vector3.MoveTowards(	m_Transform.position,
																						m_CurrentTargetPosition,
																						MoveSpeed));
				break;
			case MovementInterpolationMode.Slerp:
				m_Transform.position = vp_Utility.NaNSafeVector3(Vector3.Slerp(	m_Transform.position,
																				m_CurrentTargetPosition,
																				m_LinearCurve.Evaluate(m_MoveTime)));
				break;
		}
	
	}


	/// <summary>
	/// updates platform angle according to the current rotation
	/// interpolation mode
	/// </summary>
	protected void UpdateRotation()
	{

		switch (RotationInterpolationMode)
		{

			case RotateInterpolationMode.SyncToMovement:
				if (!m_Moving)
					break;
				m_Transform.eulerAngles = vp_Utility.NaNSafeVector3(new Vector3(
				Mathf.LerpAngle(m_OriginalAngle.x, m_CurrentTargetAngle.x, 1.0f - (GetDistanceLeft() / m_TravelDistance)),
				Mathf.LerpAngle(m_OriginalAngle.y, m_CurrentTargetAngle.y, 1.0f - (GetDistanceLeft() / m_TravelDistance)),
				Mathf.LerpAngle(m_OriginalAngle.z, m_CurrentTargetAngle.z, 1.0f - (GetDistanceLeft() / m_TravelDistance))));
				break;

			case RotateInterpolationMode.EaseOut:
				m_Transform.eulerAngles = vp_Utility.NaNSafeVector3(new Vector3(
				Mathf.LerpAngle(m_Transform.eulerAngles.x, m_CurrentTargetAngle.x, m_LinearCurve.Evaluate(m_MoveTime)),
				Mathf.LerpAngle(m_Transform.eulerAngles.y, m_CurrentTargetAngle.y, m_LinearCurve.Evaluate(m_MoveTime)),
				Mathf.LerpAngle(m_Transform.eulerAngles.z, m_CurrentTargetAngle.z, m_LinearCurve.Evaluate(m_MoveTime))));
				break;


			case RotateInterpolationMode.CustomEaseOut:
				m_Transform.eulerAngles = vp_Utility.NaNSafeVector3(new Vector3(
				Mathf.LerpAngle(m_Transform.eulerAngles.x, m_CurrentTargetAngle.x, RotationEaseAmount),
				Mathf.LerpAngle(m_Transform.eulerAngles.y, m_CurrentTargetAngle.y, RotationEaseAmount),
				Mathf.LerpAngle(m_Transform.eulerAngles.z, m_CurrentTargetAngle.z, RotationEaseAmount)));
				break;

			case RotateInterpolationMode.CustomRotate:
				m_Transform.Rotate(RotationSpeed);
				break;

		}

	}


	/// <summary>
	/// updates velocity variables for motion interpolation and physics
	/// </summary>
	protected void UpdateVelocity()
	{

		// step the movement time
		m_MoveTime += MoveSpeed * 0.01f;

		// calculate velocities, mainly for physics
		m_PhysicsCurrentMoveVelocity = (m_Transform.position - m_PrevPos).magnitude;
		m_PhysicsCurrentRotationVelocity = (m_Transform.eulerAngles - m_PrevAngle).magnitude;

		// store the final states from this frame, for next velocity calculation
		m_PrevPos = m_Transform.position;
		m_PrevAngle = m_Transform.eulerAngles;

	}



	/// <summary>
	/// this method can be used by an external script such as trigger
	/// (button / lever) to start a platform and make it go to a specific
	/// waypoint. NOTE: the platform must be in 'Target' Path mode.
	/// </summary>
	public void GoTo(int targetWayPoint)
	{

		if (Time.time < m_NextAllowedMoveTime)
			return;

		if (PathType != PathMoveType.Target)
			return;

		m_TargetedWayPoint = GetValidWaypoint(targetWayPoint);

		if (targetWayPoint > m_NextWaypoint)
		{
			if (PathDirection != Direction.Direct)
				PathDirection = Direction.Forward;
		}
		else
		{
			if (PathDirection != Direction.Direct)
				PathDirection = Direction.Backwards;
		}

		m_Moving = true;

	}


	/// <summary>
	/// calculates the distance left to the next waypoint
	/// </summary>
	protected float GetDistanceLeft()
	{

		if (m_Waypoints.Count < 2)
			return 0.0f;

		return Vector3.Distance(m_Transform.position, m_Waypoints[m_NextWaypoint].position);

	}


	/// <summary>
	/// selects the next waypoint according to platform path direction
	/// and sets it moving
	/// </summary>
	protected void GoToNextWaypoint()
	{

		if (m_Waypoints.Count < 2)
			return;
		m_MoveTime = 0;

		if (!m_Audio.isPlaying)
			m_Audio.Play();

		m_CurrentWaypoint = m_NextWaypoint;
		
		switch (PathDirection)
		{
			case Direction.Forward:
				m_NextWaypoint = GetValidWaypoint(m_NextWaypoint + 1);
				break;
			case Direction.Backwards:
				m_NextWaypoint = GetValidWaypoint(m_NextWaypoint - 1);
				break;
			case Direction.Direct:
				m_NextWaypoint = m_TargetedWayPoint;
				break;
		}
		
		m_OriginalAngle = m_CurrentTargetAngle;
		m_CurrentTargetPosition = m_Waypoints[m_NextWaypoint].position;
		m_CurrentTargetAngle = m_Waypoints[m_NextWaypoint].eulerAngles;
		
		m_TravelDistance = GetDistanceLeft();

		m_Moving = true;

	}


	/// <summary>
	/// converts the passed waypoint number into the nearest valid
	/// waypoint number (unless it is already valid)
	/// </summary>
	protected int GetValidWaypoint(int wayPoint)
	{

		if(wayPoint < 0)
			return (m_Waypoints.Count - 1);
			
		if(wayPoint > (m_Waypoints.Count - 1))
			return 0;

		return wayPoint;

	}


	/// <summary>
	/// 
	/// </summary>
	protected void OnTriggerEnter(Collider col)
	{

		if(!GetPlayer(col))
			return;

		TryPushPlayer();

		TryAutoStart();		
		

	}


	/// <summary>
	/// 
	/// </summary>
	protected void OnTriggerStay(Collider col)
	{

		if (!PhysicsSnapPlayerToTopOnIntersect)
			return;

		if (!GetPlayer(col))
			return;

		TrySnapPlayerToTop();
			
	}


	/// <summary>
	/// this method quickly determines if the colliding object is a
	/// player that we will do something about. a dictionary is used to
	/// remember past players so we don't have to scan their hierarchy
	/// for an event handler every time
	/// </summary>
	protected bool GetPlayer(Collider col)
	{

		// see if this is a valid player object, or auto-approve
		// if it has been recently validated
		if (!m_KnownPlayers.ContainsKey(col))
		{

			if (col.gameObject.layer != vp_Layer.LocalPlayer)
				return false;

			vp_FPPlayerEventHandler player = col.transform.root.GetComponent<vp_FPPlayerEventHandler>();
			if (player == null)
				return false;

			m_KnownPlayers.Add(col, player);

		}

		if (!m_KnownPlayers.TryGetValue(col, out m_PlayerToPush))
			return false;

		m_PlayerCollider = col;

		return true;

	}


	/// <summary>
	/// if the platform runs into a player from the side or the top,
	/// this method will attempt to push the player out of the way
	/// using the player's own horizontal force mechanism
	/// </summary>
	protected void TryPushPlayer()
	{

		if (m_PlayerToPush == null || m_PlayerToPush.Platform == null)
			return;

		// don't push a player that is standing on top of the platform. this
		// assumes a small distance between the player's feet and the top of
		// the platform bounding box (due to charactercontroller 'Skin Width').
		if (m_PlayerToPush.Position.Get().y > m_Collider.bounds.max.y)
			return;

		// don't push around a player if it considers itself attached to the
		// platform. this player has some crazy holding-on-to-skillz!
		if (m_PlayerToPush.Platform.Get() == m_Transform)
			return;

		// alright, try to push player away from the center of the platform
		float speed = m_PhysicsCurrentMoveVelocity;
		if (speed == 0.0f)
			speed = m_PhysicsCurrentRotationVelocity * 0.1f;	// if we have no positional speed, use angular speed
		if(speed > 0.0f)
			m_PlayerToPush.ForceImpact.Send(vp_Utility.HorizontalVector(
				-(m_Transform.position - m_PlayerCollider.bounds.center).normalized *
				speed *
				m_PhysicsPushForce));

		// TIP: player could get hurt here

	}


	/// <summary>
	/// this method is used to instantly snap the player out of the way
	/// if all else fails. that is, the controller has been forced or
	/// squeezed into the platform and is in a buggy state. if this
	/// happens, we snap player to the top of the platform. NOTE: will
	/// abort for platforms that rotate excessively
	/// </summary>
	protected void TrySnapPlayerToTop()
	{

		if (m_PlayerToPush == null || m_PlayerToPush.Platform == null)
			return;

		// don't snap a player that is already on top of the platform
		if (m_PlayerToPush.Position.Get().y > m_Collider.bounds.max.y)
			return;

		// don't push around a player that's attached to the platform
		if (m_PlayerToPush.Platform.Get() == m_Transform)
			return;

		// only snap to top if platform moves in such a way that the top
		// will remain level
		if (RotationSpeed.x != 0.0f || RotationSpeed.z != 0.0f ||
			m_CurrentTargetAngle.x != 0.0f || m_CurrentTargetAngle.z != 0.0f)
			return;

		// only snap if the player bounding box is horizontally and fully
		// encapsulated by the platform bounding box
		if ((m_Collider.bounds.max.x < m_PlayerCollider.bounds.max.x) ||
			(m_Collider.bounds.max.z < m_PlayerCollider.bounds.max.z) ||
			(m_Collider.bounds.min.x > m_PlayerCollider.bounds.min.x) ||
			(m_Collider.bounds.min.z > m_PlayerCollider.bounds.min.z))
			return;

		// approved! snap to top
		Vector3 newPos = m_PlayerToPush.Position.Get();
		newPos.y = m_Collider.bounds.max.y - 0.1f;
		m_PlayerToPush.Position.Set(newPos);

		// TIP: instead of snapping players to top here, this would also
		// be a great place to gib 'em!

	}


	/// <summary>
	/// auto starts the platform if player is on top of it and auto
	/// start is enabled
	/// </summary>
	protected void TryAutoStart()
	{

		if (MoveAutoStartTarget == 0)
			return;

		if (m_PhysicsCurrentMoveVelocity > 0.0f || m_Moving)
			return;

		GoTo(MoveAutoStartTarget);

	}
	



}
