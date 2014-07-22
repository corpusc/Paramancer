/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPWeaponMeleeAttackEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_FPWeaponMeleeAttack class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(vp_FPWeaponMeleeAttack))]

public class vp_FPWeaponMeleeAttackEditor : Editor
{

	// target component
	public vp_FPWeaponMeleeAttack m_Component;

	// foldouts
	public static bool m_WeaponStatesFoldout;
	public static bool m_SwingFoldout;
	public static bool m_ImpactFoldout;
	public static bool m_FXFoldout;
	public static bool m_DamageFoldout;
	public static bool m_SoundFoldout;

	public static bool m_StateFoldout;
	public static bool m_PresetFoldout = true;
	
	private static vp_ComponentPersister m_Persister = null;


	/// <summary>
	/// hooks up the component object as the inspector target
	/// </summary>
	public virtual void OnEnable()
	{

		m_Component = (vp_FPWeaponMeleeAttack)target;

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

		m_Component.DrawDebugObjects = (m_SwingFoldout || m_ImpactFoldout || m_DamageFoldout);

		//if (!Application.isPlaying)
		//{

			DoWeaponStatesFoldout();
			DoSwingFoldout();
			DoImpactFoldout();
			DoDamageFoldout();
			DoFXFoldout();
			DoSoundFoldout();

		//}
		//else
		//{

			//GUI.enabled = false;
			//GUILayout.Label("\nThis component can not be modified at runtime.\n", vp_EditorGUIUtility.NoteStyle);
			//GUI.enabled = true;

		//}

		// state
		m_StateFoldout = vp_PresetEditorGUIUtility.StateFoldout(m_StateFoldout, m_Component, m_Component.States, m_Persister);

		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
		m_Component.AttackPickRandomState = GUILayout.Toggle(m_Component.AttackPickRandomState, "Pick a random state for each attack");
		GUILayout.Space(10);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
		EditorGUILayout.HelpBox("Uncheck to use Default state only (or in case you want to enable specific states via script).", MessageType.Info);
		GUILayout.Space(10);
		GUILayout.EndHorizontal();

		// preset
		m_PresetFoldout = vp_PresetEditorGUIUtility.PresetFoldout(m_PresetFoldout, m_Component);

		// update
		if (GUI.changed)
		{

			EditorUtility.SetDirty(target);

			// update the default state in order not to loose inspector tweaks
			// due to state switches during runtime
			if(Application.isPlaying)
				m_Component.RefreshDefaultState();

			if (m_Component.Persist)
				m_Persister.Persist();

		}

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoWeaponStatesFoldout()
	{

		m_WeaponStatesFoldout = EditorGUILayout.Foldout(m_WeaponStatesFoldout, "Weapon States");

		if (m_WeaponStatesFoldout)
		{

			m_Component.WeaponStatePull = EditorGUILayout.TextField("Pull", m_Component.WeaponStatePull);
			m_Component.WeaponStateSwing = EditorGUILayout.TextField("Swing", m_Component.WeaponStateSwing);

			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			EditorGUILayout.HelpBox("The melee attack will attempt to trigger these states on the vp_FPWeapon component in the same gameobject. First the Pull state will be triggerered, followed by a short delay and finally the Swing state.", MessageType.Info);
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

		}


	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoSwingFoldout()
	{

		m_SwingFoldout = EditorGUILayout.Foldout(m_SwingFoldout, "Swing Motion");

		if (m_SwingFoldout)
		{

			m_Component.SwingDelay = EditorGUILayout.Slider("Delay", m_Component.SwingDelay, 0.0f, 5.0f);
			m_Component.SwingDuration = EditorGUILayout.Slider("Duration", m_Component.SwingDuration, 0.0f, 5.0f);
			m_Component.SwingRate = EditorGUILayout.Slider("Rate", m_Component.SwingRate, 0.0f, 5.0f);
			m_Component.SwingSoftForceFrames = EditorGUILayout.IntSlider("Soft Force Frames", m_Component.SwingSoftForceFrames, 1, 60);
			m_Component.SwingPositionSoftForce = EditorGUILayout.Vector3Field("Position Soft Force", m_Component.SwingPositionSoftForce);
			m_Component.SwingRotationSoftForce = EditorGUILayout.Vector3Field("Rotation Soft Force", m_Component.SwingRotationSoftForce);

		}


	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoImpactFoldout()
	{

		m_ImpactFoldout = EditorGUILayout.Foldout(m_ImpactFoldout, "Impact");

		if (m_ImpactFoldout)
		{
			m_Component.ImpactTime = EditorGUILayout.Slider("Time", m_Component.ImpactTime, 0.0f, 5.0f);
			m_Component.ImpactPositionSpringRecoil = EditorGUILayout.Vector3Field("Position Recoil", m_Component.ImpactPositionSpringRecoil);
			m_Component.ImpactPositionSpring2Recoil = EditorGUILayout.Vector3Field("Position Recoil (Spring2)", m_Component.ImpactPositionSpring2Recoil);
			m_Component.ImpactRotationSpringRecoil = EditorGUILayout.Vector3Field("Rotation Recoil", m_Component.ImpactRotationSpringRecoil);
			m_Component.ImpactRotationSpring2Recoil = EditorGUILayout.Vector3Field("Rotation Recoil (Spring2)", m_Component.ImpactRotationSpring2Recoil);

		}

	}

	
	/// <summary>
	/// 
	/// </summary>
	public virtual void DoDamageFoldout()
	{

		m_DamageFoldout = EditorGUILayout.Foldout(m_DamageFoldout, "Damage");

		if (m_DamageFoldout)
		{
			m_Component.Damage = EditorGUILayout.FloatField("Damage", m_Component.Damage);
			m_Component.DamageForce = EditorGUILayout.FloatField("Force", m_Component.DamageForce);
			m_Component.DamageRange = EditorGUILayout.FloatField("Range", m_Component.DamageRange);
			m_Component.DamageRadius = EditorGUILayout.Slider("Radius", m_Component.DamageRadius, 0.0f, 5.0f);

		}

	}
	

	/// <summary>
	/// 
	/// </summary>
	public virtual void DoFXFoldout()
	{

		m_FXFoldout = EditorGUILayout.Foldout(m_FXFoldout, "FX");

		if (m_FXFoldout)
		{
			m_Component.SparkFactor = EditorGUILayout.Slider("SparkFactor", m_Component.SparkFactor, 0.0f, 1.0f);
			m_Component.m_DustPrefab = (GameObject)EditorGUILayout.ObjectField("Dust Prefab", m_Component.m_DustPrefab, typeof(GameObject), false);
			m_Component.m_SparkPrefab = (GameObject)EditorGUILayout.ObjectField("Spark Prefab", m_Component.m_SparkPrefab, typeof(GameObject), false);
			m_Component.m_DebrisPrefab = (GameObject)EditorGUILayout.ObjectField("Debris Prefab", m_Component.m_DebrisPrefab, typeof(GameObject), false);

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

			vp_EditorGUIUtility.ObjectList("Swing", m_Component.SoundSwing, typeof(AudioClip));
			m_Component.SoundSwingPitch = EditorGUILayout.Vector2Field("Swing Pitch Range (Min:Max)", m_Component.SoundSwingPitch);
			EditorGUILayout.MinMaxSlider(ref m_Component.SoundSwingPitch.x, ref m_Component.SoundSwingPitch.y, 0.5f, 1.5f);
			vp_EditorGUIUtility.ObjectList("Impact", m_Component.SoundImpact, typeof(AudioClip));
			m_Component.SoundImpactPitch = EditorGUILayout.Vector2Field("Impact Pitch Range (Min:Max)", m_Component.SoundImpactPitch);
			EditorGUILayout.MinMaxSlider(ref m_Component.SoundImpactPitch.x, ref m_Component.SoundImpactPitch.y, 0.5f, 1.5f);

			
		}

	}
	

}

