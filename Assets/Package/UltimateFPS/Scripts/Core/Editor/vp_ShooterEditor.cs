/////////////////////////////////////////////////////////////////////////////////
//
//	vp_ShooterEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_FPShooter class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(vp_Shooter))]

public class vp_ShooterEditor : Editor
{

	// target component
	public vp_Shooter m_Component = null;

	// foldouts
	public static bool m_ProjectileFoldout;
	public static bool m_MuzzleFlashFoldout;
	public static bool m_ShellFoldout;
	public static bool m_AmmoFoldout;
	public static bool m_SoundFoldout;
	public static bool m_StateFoldout;
	public static bool m_PresetFoldout = true;

	private bool m_MuzzleFlashVisible = false;		// display the muzzle flash in the editor?
	private static vp_ComponentPersister m_Persister = null;


	/// <summary>
	/// hooks up the object to the inspector target
	/// </summary>
	public virtual void OnEnable()
	{

		m_Component = (vp_Shooter)target;

		if (m_Persister == null)
			m_Persister = new vp_ComponentPersister();
		m_Persister.Component = m_Component;
		m_Persister.IsActive = true;

		if (m_Component.DefaultState == null)
			m_Component.RefreshDefaultState();

	}


	/// <summary>
	/// disables the persister and removes its reference
	/// </summary>
	public virtual void OnDestroy()
	{

		m_Persister.IsActive = false;

	}


	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{

		GUI.color = Color.white;

		string objectInfo = m_Component.gameObject.name;

		if (vp_Utility.IsActive(m_Component.gameObject))
			GUI.enabled = true;
		else
		{
			GUI.enabled = false;
			objectInfo += " (INACTIVE)";
		}

		if (!vp_Utility.IsActive(m_Component.gameObject))
		{
			GUI.enabled = true;
			return;
		}

		if (Application.isPlaying || m_Component.DefaultState.TextAsset == null)
		{

			DoProjectileFoldout();
			DoMuzzleFlashFoldout();
			DoShellFoldout();
			DoSoundFoldout();

		}
		else
			vp_PresetEditorGUIUtility.DefaultStateOverrideMessage();

		// state
		m_StateFoldout = vp_PresetEditorGUIUtility.StateFoldout(m_StateFoldout, m_Component, m_Component.States, m_Persister);

		// preset
		m_PresetFoldout = vp_PresetEditorGUIUtility.PresetFoldout(m_PresetFoldout, m_Component);

		// update
		if (GUI.changed)
		{

			EditorUtility.SetDirty(target);

			// update the default state in order not to loose inspector tweaks
			// due to state switches during runtime
			if (Application.isPlaying)
				m_Component.RefreshDefaultState();

			if (m_Component.Persist)
				m_Persister.Persist();

			m_Component.Refresh();

		}

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoProjectileFoldout()
	{

		m_ProjectileFoldout = EditorGUILayout.Foldout(m_ProjectileFoldout, "Projectile");
		if (m_ProjectileFoldout)
		{

			m_Component.ProjectileFiringRate = Mathf.Max(0.0f, EditorGUILayout.FloatField("Firing Rate", m_Component.ProjectileFiringRate));
			if (m_Component.ProjectileFiringRate == 0.0f)
			{
				GUI.enabled = false;
			}
			GUI.enabled = true;
			m_Component.ProjectilePrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", m_Component.ProjectilePrefab, typeof(GameObject), false);
			GUI.enabled = false;
			GUILayout.Label("Prefab should be a gameobject with a projectile\nlogic script added to it (such as vp_HitscanBullet).", vp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.ProjectileScale = EditorGUILayout.Slider("Scale", m_Component.ProjectileScale, 0, 2);
			m_Component.ProjectileCount = EditorGUILayout.IntField("Count", m_Component.ProjectileCount);
			m_Component.ProjectileSpread = EditorGUILayout.Slider("Spread", m_Component.ProjectileSpread, 0, 360);
			m_Component.ProjectileSpawnDelay = Mathf.Abs(EditorGUILayout.FloatField("Spawn Delay", m_Component.ProjectileSpawnDelay));

			vp_EditorGUIUtility.Separator();

		}

	}

	
	/// <summary>
	/// 
	/// </summary>
	public virtual void DoMuzzleFlashFoldout()
	{
		
		m_MuzzleFlashFoldout = EditorGUILayout.Foldout(m_MuzzleFlashFoldout, "Muzzle Flash");
		if (m_MuzzleFlashFoldout)
		{

			m_Component.MuzzleFlashPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", m_Component.MuzzleFlashPrefab, typeof(GameObject), false);
			GUI.enabled = false;
			GUILayout.Label("Prefab should be a mesh with a Particles/Additive\nshader and a vp_MuzzleFlash script added to it.", vp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			Vector3 currentPosition = m_Component.MuzzleFlashPosition;
			m_Component.MuzzleFlashPosition = EditorGUILayout.Vector3Field("Position", m_Component.MuzzleFlashPosition);
			Vector3 currentScale = m_Component.MuzzleFlashScale;
			m_Component.MuzzleFlashScale = EditorGUILayout.Vector3Field("Scale", m_Component.MuzzleFlashScale);
			m_Component.MuzzleFlashFadeSpeed = EditorGUILayout.Slider("Fade Speed", m_Component.MuzzleFlashFadeSpeed, 0.001f, 0.2f);
			m_Component.MuzzleFlashDelay = Mathf.Abs(EditorGUILayout.FloatField("MuzzleFlash Delay", m_Component.MuzzleFlashDelay));
			if (!Application.isPlaying)
				GUI.enabled = false;
			bool currentMuzzleFlashVisible = m_MuzzleFlashVisible;
			m_MuzzleFlashVisible = EditorGUILayout.Toggle("Show Muzzle Fl.", m_MuzzleFlashVisible);
			if (Application.isPlaying)
			{
				if (m_Component.MuzzleFlashPosition != currentPosition ||
					m_Component.MuzzleFlashScale != currentScale)
					m_MuzzleFlashVisible = true;

				vp_MuzzleFlash mf = (vp_MuzzleFlash)m_Component.MuzzleFlash.GetComponent("vp_MuzzleFlash");
				if (mf != null)
					mf.ForceShow = currentMuzzleFlashVisible;

				GUI.enabled = false;
				GUILayout.Label("Set Muzzle Flash Z to about 0.5 to bring it into view.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
			}
			else
				GUILayout.Label("Muzzle Flash can be shown when the game is playing.", vp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;

			vp_EditorGUIUtility.Separator();
		}

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoShellFoldout()
	{

		m_ShellFoldout = EditorGUILayout.Foldout(m_ShellFoldout, "Shell");
		if (m_ShellFoldout)
		{

			m_Component.ShellPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", m_Component.ShellPrefab, typeof(GameObject), false);
			GUI.enabled = false;
			GUILayout.Label("Prefab should be a mesh with a collider, a rigidbody\nand a vp_Shell script added to it.", vp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.ShellScale = EditorGUILayout.Slider("Scale", m_Component.ShellScale, 0, 2);
			m_Component.ShellEjectPosition = EditorGUILayout.Vector3Field("Eject Position", m_Component.ShellEjectPosition);
			m_Component.ShellEjectDirection = EditorGUILayout.Vector3Field("Eject Direction", m_Component.ShellEjectDirection);
			m_Component.ShellEjectVelocity = EditorGUILayout.Slider("Eject Velocity", m_Component.ShellEjectVelocity, 0, 0.5f);
			m_Component.ShellEjectSpin = EditorGUILayout.Slider("Eject Spin", m_Component.ShellEjectSpin, 0, 1.0f);
			m_Component.ShellEjectDelay = Mathf.Abs(EditorGUILayout.FloatField("Eject Delay", m_Component.ShellEjectDelay));

			vp_EditorGUIUtility.Separator();
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
			m_Component.SoundFire = (AudioClip)EditorGUILayout.ObjectField("Fire", m_Component.SoundFire, typeof(AudioClip), false);
			m_Component.SoundFirePitch = EditorGUILayout.Vector2Field("Fire Pitch (Min:Max)", m_Component.SoundFirePitch);
			EditorGUILayout.MinMaxSlider(ref m_Component.SoundFirePitch.x, ref m_Component.SoundFirePitch.y, 0.5f, 1.5f);
			m_Component.SoundFireDelay = Mathf.Abs(EditorGUILayout.FloatField("Fire Sound Delay", m_Component.SoundFireDelay));
			vp_EditorGUIUtility.Separator();
		}

	}

		
}

