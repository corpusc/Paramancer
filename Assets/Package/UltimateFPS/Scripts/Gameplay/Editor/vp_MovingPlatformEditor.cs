/////////////////////////////////////////////////////////////////////////////////
//
//	vp_MovingPlatformEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_MovingPlatformEditor class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(vp_MovingPlatform))]

public class vp_MovingPlatformEditor : Editor
{

	// target component
	public vp_MovingPlatform m_Component;

	// foldouts
	public static bool m_PathFoldout;
	public static bool m_MovementFoldout;
	public static bool m_RotationFoldout;
	public static bool m_PhysicsFoldout;
	public static bool m_SoundFoldout;

	int m_WaypointCount = 0;
	string StandardPathInfo = "Waypoints should be a gameobject with child waypoint gameobjects under it.";
	string NoWaypointsMessage = "No waypoints found. If you want this platform to move around, please drag a gameobject with atleast 2 child waypoint gameobjects into the Path > Waypoints slot.";

	string PathInfo = "";
	MessageType PathMessageType = MessageType.Info;


	/// <summary>
	/// hooks up the MovingPlatform object to the inspector target
	/// </summary>
	public void OnEnable()
	{
		m_Component = (vp_MovingPlatform)target;
	}


	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{

		if (m_PathFoldout || m_MovementFoldout || m_PhysicsFoldout || m_RotationFoldout || m_SoundFoldout)
		{

			if (m_Component.PathWaypoints != null)
				m_WaypointCount = m_Component.PathWaypoints.transform.childCount;
			else
				m_WaypointCount = 0;

			if (m_WaypointCount < 2)
			{
				PathMessageType = MessageType.Warning;
				PathInfo = NoWaypointsMessage;
			}

		}

		GUI.color = Color.white;
		
		DoPathFoldout();
		DoMovementFoldout();
		DoRotationFoldout();
		DoPhysicsFoldout();
		DoSoundFoldout();

		// update
		if (GUI.changed)
			EditorUtility.SetDirty(target);
		
	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoPathFoldout()
	{

		m_PathFoldout = EditorGUILayout.Foldout(m_PathFoldout, "Path");

		if (m_PathFoldout)
		{

			if (string.IsNullOrEmpty(PathInfo))
			{
				PathInfo = StandardPathInfo;
				PathMessageType = MessageType.Info;
			}

			if (m_WaypointCount < 2)
				GUI.enabled = false;
			m_Component.PathType = (vp_MovingPlatform.PathMoveType)EditorGUILayout.EnumPopup("Type", m_Component.PathType);
			GUI.enabled = true;

			m_Component.PathWaypoints = (GameObject)EditorGUILayout.ObjectField("Waypoints", m_Component.PathWaypoints, typeof(GameObject), true);
			if ((m_Component.PathWaypoints) == null || (m_WaypointCount < 2))
				GUI.enabled = false;
				GUILayout.BeginHorizontal();

			// gizmo tool
			EditorGUILayout.PrefixLabel(" ");
			if (GUILayout.Button("Generate Gizmos"))
			{
				if (EditorUtility.DisplayDialog("Headsup!", "This will remove every component under the chosen 'Waypoints' object in favor of pretty new gizmos.", "OK", "Later"))
				{
					// remove all components under waypoint container and add a
					// gizmo to each transform
					foreach (Transform t in m_Component.PathWaypoints.transform)
					{
						foreach (Component c in t.GetComponentsInChildren(typeof(Component)))
						{
							if (c.GetType() != typeof(Transform))
								Object.DestroyImmediate(c);
						}
						t.gameObject.AddComponent(typeof(vp_WaypointGizmo));
					}
				}
			}
			GUILayout.Space(18);
			GUILayout.EndHorizontal();

			if (m_Component.PathType == vp_MovingPlatform.PathMoveType.Target)
			{

				// special logic to get rid of nullref in case waypoint list is empty
				int waypoints = 0;
				if (m_Component != null && m_Component.PathWaypoints != null && m_Component.PathWaypoints.transform.childCount > 1)
					waypoints = m_Component.PathWaypoints.transform.childCount - 1;

				m_Component.MoveAutoStartTarget = EditorGUILayout.IntSlider((m_Component.MoveAutoStartTarget == 0 ? "Auto Start (OFF)" : "Auto Start Target"), m_Component.MoveAutoStartTarget, 0, waypoints);
				m_Component.MoveReturnDelay = EditorGUILayout.Slider("Return Delay" + (m_Component.MoveReturnDelay == 0 ? " (OFF)" : ""), m_Component.MoveReturnDelay, 0.0f, 30.0f);
				m_Component.MoveCooldown = EditorGUILayout.Slider("Cooldown" + (m_Component.MoveCooldown == 0 ? " (OFF)" : ""), m_Component.MoveCooldown, 0.0f, 30.0f);
			}
			vp_MovingPlatform.Direction previousPathDirection = m_Component.PathDirection;
			m_Component.PathDirection = (vp_MovingPlatform.Direction)EditorGUILayout.EnumPopup("Direction", m_Component.PathDirection);
			if (m_Component.PathDirection == vp_MovingPlatform.Direction.Direct &&
				(m_Component.PathType != vp_MovingPlatform.PathMoveType.Target))
			{
				PathInfo = "Direction:Direct requires the Manual path type.";
				PathMessageType = MessageType.Warning;
				m_Component.PathDirection = previousPathDirection;
				if(previousPathDirection == vp_MovingPlatform.Direction.Direct)
					m_Component.PathDirection = vp_MovingPlatform.Direction.Forward;
			}
			else if (m_Component.PathDirection != previousPathDirection)
				PathInfo = "";


			GUI.enabled = true;

			if (m_WaypointCount > 1)
			{
				PathMessageType = MessageType.Info;
				switch (m_Component.PathType)
				{
					case vp_MovingPlatform.PathMoveType.Target:
						PathInfo = "In Target Mode, the platform will stand still, but go to a target waypoint when player stands on it. If Auto Start is OFF, the platform must be operated via the script's public GoTo method.";
						break;
					case vp_MovingPlatform.PathMoveType.PingPong:
						PathInfo = "In PingPong mode, the platform will go to the last waypoint via all waypoints - then back the same way.";
						break;
					case vp_MovingPlatform.PathMoveType.Loop:
						PathInfo = "In Loop mode, the platform will continuously move through all the waypoints without changing direction.";
						break;
				}
			}

			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			EditorGUILayout.HelpBox(PathInfo, PathMessageType);
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

		}

	}

	
	/// <summary>
	/// 
	/// </summary>
	public virtual void DoMovementFoldout()
	{

		m_MovementFoldout = EditorGUILayout.Foldout(m_MovementFoldout, "Movement");

		if (m_MovementFoldout)
		{
			if (m_Component.PathWaypoints != null)
				m_WaypointCount = m_Component.PathWaypoints.transform.childCount;

			if (m_WaypointCount < 2)
			{
				PathMessageType = MessageType.Info;
				PathInfo = NoWaypointsMessage;
				GUI.enabled = false;
			}

			m_Component.MoveInterpolationMode = (vp_MovingPlatform.MovementInterpolationMode)EditorGUILayout.EnumPopup("Interpolation Mode", m_Component.MoveInterpolationMode);
			m_Component.MoveSpeed = EditorGUILayout.Slider("Speed", m_Component.MoveSpeed, 0.001f, 2.0f);
			GUI.enabled = true;

			if (m_WaypointCount < 2)
			{
				GUILayout.Space(10);
				EditorGUILayout.HelpBox("No waypoints found. The platform won't be able to move (but may still rotate).", MessageType.Warning);
				GUILayout.Space(10);
			}

		}
		
	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoRotationFoldout()
	{

		m_RotationFoldout = EditorGUILayout.Foldout(m_RotationFoldout, "Rotation");

		if (m_RotationFoldout)
		{

			m_Component.RotationInterpolationMode = (vp_MovingPlatform.RotateInterpolationMode)EditorGUILayout.EnumPopup("Interpolation Mode", m_Component.RotationInterpolationMode);

			if (m_Component.RotationInterpolationMode == vp_MovingPlatform.RotateInterpolationMode.CustomEaseOut)
				m_Component.RotationEaseAmount = EditorGUILayout.Slider("Ease Speed", m_Component.RotationEaseAmount, 0.01f, 1.0f);
			
			if (m_Component.RotationInterpolationMode == vp_MovingPlatform.RotateInterpolationMode.CustomRotate)
				m_Component.RotationSpeed = EditorGUILayout.Vector3Field("Rotation Speed", m_Component.RotationSpeed);
			
			if (m_WaypointCount < 2)
			{
				GUILayout.Space(10);
				EditorGUILayout.HelpBox("Since this platform has no waypoints yet, note that only the Custom Rotate mode will work.", MessageType.Info);
				GUILayout.Space(10);
			}
		}


	}
	

	/// <summary>
	/// 
	/// </summary>
	public virtual void DoPhysicsFoldout()
	{

		m_PhysicsFoldout = EditorGUILayout.Foldout(m_PhysicsFoldout, "Physics");

		if (m_PhysicsFoldout)
		{

			m_Component.m_PhysicsPushForce = EditorGUILayout.Slider("Push Force", m_Component.m_PhysicsPushForce, 0.0f, 10.0f);
			m_Component.PhysicsSnapPlayerToTopOnIntersect = EditorGUILayout.Toggle("Snap player to top", m_Component.PhysicsSnapPlayerToTopOnIntersect);

			GUILayout.Space(10);
			EditorGUILayout.HelpBox("Push Force will move players out of the way on contact with the sides or bottom of the platform. Snap takes effect if a player gets inside a horizontally aligned platform for any reason.", MessageType.Info);
			GUILayout.Space(10);

		}


	}

	
	/// <summary>
	/// 
	/// </summary>
	public virtual void DoSoundFoldout()
	{

		m_SoundFoldout = EditorGUILayout.Foldout(m_SoundFoldout, "Sound");

		if (m_SoundFoldout)
		{

			if (m_Component.PathType != vp_MovingPlatform.PathMoveType.Target)
				GUI.enabled = false;
			m_Component.SoundStart = (AudioClip)EditorGUILayout.ObjectField("Start", m_Component.SoundStart, typeof(AudioClip), false);
			GUI.enabled = true;
			m_Component.SoundMove = (AudioClip)EditorGUILayout.ObjectField("Move", m_Component.SoundMove, typeof(AudioClip), false);
			m_Component.SoundWaypoint = (AudioClip)EditorGUILayout.ObjectField("Waypoint", m_Component.SoundWaypoint, typeof(AudioClip), false);
			if (m_Component.PathType != vp_MovingPlatform.PathMoveType.Target)
				GUI.enabled = false;
			m_Component.SoundStop = (AudioClip)EditorGUILayout.ObjectField("Stop", m_Component.SoundStop, typeof(AudioClip), false);
			GUI.enabled = true;

			if (m_Component.PathType != vp_MovingPlatform.PathMoveType.Target)
			{
				GUILayout.Space(10);
				EditorGUILayout.HelpBox("Start and Stop are for the Target Path type.", MessageType.Info);
				GUILayout.Space(10);
			}

		}

	}
	

}

