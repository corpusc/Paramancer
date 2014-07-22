/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPWeaponEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_FPSWeapon class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(vp_FPWeapon))]

public class vp_FPWeaponEditor : Editor
{

	// target component
	public vp_FPWeapon m_Component = null;

	// weapon foldouts
	// NOTE: these are static so they remain open when toggling
	// between different components. this simplifies copying
	// content (prefabs / sounds) between components
	public static bool m_WeaponRenderingFoldout;
	public static bool m_WeaponPositionFoldout;
	public static bool m_WeaponRotationFoldout;
	public static bool m_WeaponRetractionFoldout;
	public static bool m_WeaponShakeFoldout;
	public static bool m_WeaponBobFoldout;
	public static bool m_WeaponStepFoldout;
	public static bool m_WeaponIdleFoldout;
	public static bool m_WeaponSoundFoldout;
	public static bool m_WeaponAnimationFoldout;
	public static bool m_StateFoldout;
	public static bool m_PresetFoldout = true;

	private bool m_WeaponPivotVisible = false;
	private static vp_ComponentPersister m_Persister = null;


	/// <summary>
	/// hooks up the FPSCamera object to the inspector target
	/// </summary>
	public void OnEnable()
	{

		m_Component = (vp_FPWeapon)target;

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
	void OnDestroy()
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

		GUILayout.Label(objectInfo);
		vp_EditorGUIUtility.Separator();

		if (!vp_Utility.IsActive(m_Component.gameObject))
		{
			GUI.enabled = true;
			return;
		}

		if (Application.isPlaying || m_Component.DefaultState.TextAsset == null)
		{

			DoRenderingFoldout();
			DoPositionFoldout();
			DoRotationFoldout();
			DoRetractionFoldout();
			DoShakeFoldout();
			DoBobFoldout();
			DoStepFoldout();
			DoSoundFoldout();
			DoAnimationFoldout();

		}
		else
			vp_PresetEditorGUIUtility.DefaultStateOverrideMessage();

		// state foldout
		m_StateFoldout = vp_PresetEditorGUIUtility.StateFoldout(m_StateFoldout, m_Component, m_Component.States, m_Persister);

		// preset foldout
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
	public virtual void DoRenderingFoldout()
	{

		m_WeaponRenderingFoldout = EditorGUILayout.Foldout(m_WeaponRenderingFoldout, "Rendering");
		if (m_WeaponRenderingFoldout)
		{

			// weapon model
			GameObject model = m_Component.WeaponPrefab;
			m_Component.WeaponPrefab = (GameObject)EditorGUILayout.ObjectField("Weapon Prefab", m_Component.WeaponPrefab, typeof(GameObject), false);
			if (Application.isPlaying && model != m_Component.WeaponPrefab)
			{
				m_Component.InstantiateWeaponModel();
			}

			if (m_Component.WeaponPrefab == null)
			{
				GUI.enabled = false;
				GUILayout.Label("Drag a weapon prefab into this slot. The weapon will be visible\nat runtime. TIP: Open the 'Position Springs' foldout\nand modify 'Offset' to reposition the weapon.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
			}

			if (Application.isPlaying && (m_Component.WeaponCamera == null) || (m_Component.WeaponCamera != null && !vp_Utility.IsActive(m_Component.WeaponCamera.gameObject)))
				GUI.enabled = false;
			// weapon fov
			Vector2 fovDirty = new Vector2(0.0f, m_Component.RenderingFieldOfView);
			m_Component.RenderingFieldOfView = EditorGUILayout.Slider("Field of View", m_Component.RenderingFieldOfView, 1, 179);
			if (fovDirty != new Vector2(0.0f, m_Component.RenderingFieldOfView))
				m_Component.Zoom();
			m_Component.RenderingZoomDamping = EditorGUILayout.Slider("Zoom Damping", m_Component.RenderingZoomDamping, 0.1f, 5.0f);

			// weapon clipping planes
			m_Component.RenderingClippingPlanes = EditorGUILayout.Vector2Field("Clipping Planes (Near:Far)", m_Component.RenderingClippingPlanes);

			if (GUI.enabled == false)
			{
				GUI.enabled = false;
				GUILayout.Label("The above parameters require an active weapon camera.\nSee the manual for more info.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
			}
			
			GUI.enabled = true;

			float zScale = m_Component.RenderingZScale;
			m_Component.RenderingZScale = EditorGUILayout.Slider("Z Scale", m_Component.RenderingZScale, 0.0f, 1.0f);
			if ((m_Component.RenderingZScale != zScale) && Application.isPlaying && m_Component.WeaponModel != null)
				m_Component.WeaponModel.transform.localScale = new Vector3(1, 1, m_Component.RenderingZScale);
			
			GUI.enabled = false;
			GUILayout.Label("Z Scale can be used for tweaking the appearance of the\nweapon when running the system without a weapon camera.", vp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			
			vp_EditorGUIUtility.Separator();

		}

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoPositionFoldout()
	{

		m_WeaponPositionFoldout = EditorGUILayout.Foldout(m_WeaponPositionFoldout, "Position Springs");
		if (m_WeaponPositionFoldout)
		{

			m_Component.PositionOffset = EditorGUILayout.Vector3Field("Offset", m_Component.PositionOffset);
			m_Component.PositionExitOffset = EditorGUILayout.Vector3Field("Exit Offset", m_Component.PositionExitOffset);
			Vector3 currentPivot = m_Component.PositionPivot;
			m_Component.PositionPivot = EditorGUILayout.Vector3Field("Pivot", m_Component.PositionPivot);
			m_Component.PositionPivotSpringStiffness = EditorGUILayout.Slider("Pivot Stiffness", m_Component.PositionPivotSpringStiffness, 0, 1);
			m_Component.PositionPivotSpringDamping = EditorGUILayout.Slider("Pivot Damping", m_Component.PositionPivotSpringDamping, 0, 1);

			if (!Application.isPlaying)
				GUI.enabled = false;
			bool currentPivotVisible = m_WeaponPivotVisible;
			m_WeaponPivotVisible = EditorGUILayout.Toggle("Show Pivot", m_WeaponPivotVisible);
			if (Application.isPlaying)
			{
				if (m_Component.PositionPivot != currentPivot)
				{
					m_Component.SnapPivot();
					m_WeaponPivotVisible = true;
				}
				if (currentPivotVisible != m_WeaponPivotVisible)
					m_Component.SetPivotVisible(m_WeaponPivotVisible);
				GUI.enabled = false;
				GUILayout.Label("Set Pivot Z to about -0.5 to bring it into view.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
			}
			else
				GUILayout.Label("Pivot can be shown when the game is playing.", vp_EditorGUIUtility.NoteStyle);

			GUI.enabled = true;

			m_Component.PositionSpringStiffness = EditorGUILayout.Slider("Spring Stiffness", m_Component.PositionSpringStiffness, 0, 1);
			m_Component.PositionSpringDamping = EditorGUILayout.Slider("Spring Damping", m_Component.PositionSpringDamping, 0, 1);
			m_Component.PositionSpring2Stiffness = EditorGUILayout.Slider("Spring2 Stiffn.", m_Component.PositionSpring2Stiffness, 0, 1);
			m_Component.PositionSpring2Damping = EditorGUILayout.Slider("Spring2 Damp.", m_Component.PositionSpring2Damping, 0, 1);
			GUI.enabled = false;
			GUILayout.Label("Spring2 is intended for recoil. See the docs for usage.", vp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.PositionKneeling = EditorGUILayout.Slider("Kneeling", m_Component.PositionKneeling, 0, 1);
			m_Component.PositionKneelingSoftness = EditorGUILayout.IntSlider("Kneeling Softness", m_Component.PositionKneelingSoftness, 1, 30);
			GUI.enabled = false;
			GUILayout.Label("Kneeling is positional down force upon fall impact. Softness is \nthe number of frames over which to even out each fall impact.", vp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.PositionFallRetract = EditorGUILayout.Slider("Fall Retract", m_Component.PositionFallRetract, 0, 10);
			m_Component.PositionWalkSlide = EditorGUILayout.Vector3Field("Walk Sliding", m_Component.PositionWalkSlide);
			m_Component.PositionInputVelocityScale = EditorGUILayout.Slider("Input Vel. Scale", m_Component.PositionInputVelocityScale, 0, 10);
			m_Component.PositionMaxInputVelocity = EditorGUILayout.FloatField("Max Input Vel.", m_Component.PositionMaxInputVelocity);

			vp_EditorGUIUtility.Separator();
		}

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoRotationFoldout()
	{

		m_WeaponRotationFoldout = EditorGUILayout.Foldout(m_WeaponRotationFoldout, "Rotation Springs");
		if (m_WeaponRotationFoldout)
		{
			m_Component.RotationOffset = EditorGUILayout.Vector3Field("Offset", m_Component.RotationOffset);
			m_Component.RotationExitOffset = EditorGUILayout.Vector3Field("Exit Offset", m_Component.RotationExitOffset);
			Vector3 currentPivot = m_Component.PositionPivot;
			m_Component.RotationPivot = EditorGUILayout.Vector3Field("Pivot", m_Component.RotationPivot);
			m_Component.RotationPivotSpringStiffness = EditorGUILayout.Slider("Pivot Stiffness", m_Component.RotationPivotSpringStiffness, 0, 1);
			m_Component.RotationPivotSpringDamping = EditorGUILayout.Slider("Pivot Damping", m_Component.RotationPivotSpringDamping, 0, 1);

			if (!Application.isPlaying)
				GUI.enabled = false;
			bool currentPivotVisible = m_WeaponPivotVisible;
			m_WeaponPivotVisible = EditorGUILayout.Toggle("Show Pivot", m_WeaponPivotVisible);
			if (Application.isPlaying)
			{
				if (m_Component.PositionPivot != currentPivot)
				{
					m_Component.SnapPivot();
					m_WeaponPivotVisible = true;
				}
				if (currentPivotVisible != m_WeaponPivotVisible)
					m_Component.SetPivotVisible(m_WeaponPivotVisible);
			}
			else
				GUILayout.Label("Pivot can be shown when the game is playing.", vp_EditorGUIUtility.NoteStyle);

			GUI.enabled = true;
			m_Component.RotationSpringStiffness = EditorGUILayout.Slider("Spring Stiffness", m_Component.RotationSpringStiffness, 0, 1);
			m_Component.RotationSpringDamping = EditorGUILayout.Slider("Spring Damping", m_Component.RotationSpringDamping, 0, 1);
			m_Component.RotationSpring2Stiffness = EditorGUILayout.Slider("Spring2 Stiffn.", m_Component.RotationSpring2Stiffness, 0, 1);
			m_Component.RotationSpring2Damping = EditorGUILayout.Slider("Spring2 Damp.", m_Component.RotationSpring2Damping, 0, 1);
			GUI.enabled = false;
			GUILayout.Label("Spring2 is intended for recoil. See the docs for usage.", vp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.RotationKneeling = EditorGUILayout.Slider("Kneeling", m_Component.RotationKneeling, 0, 100);
			m_Component.RotationKneelingSoftness = EditorGUILayout.IntSlider("Kneeling Softness", m_Component.RotationKneelingSoftness, 1, 30);
			GUI.enabled = false;
			GUILayout.Label("Kneeling is downward pitch upon fall impact. Softness is the\nnumber of frames over which to even out each fall impact.", vp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.RotationLookSway = EditorGUILayout.Vector3Field("Look Sway", m_Component.RotationLookSway);
			m_Component.RotationStrafeSway = EditorGUILayout.Vector3Field("Strafe Sway", m_Component.RotationStrafeSway);
			m_Component.RotationFallSway = EditorGUILayout.Vector3Field("Fall Sway", m_Component.RotationFallSway);
			m_Component.RotationSlopeSway = EditorGUILayout.Slider("Slope Sway", m_Component.RotationSlopeSway, 0, 1);
			GUI.enabled = false;
			GUILayout.Label("SlopeSway multiplies FallSway when grounded\nand will take effect on slopes.", vp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.RotationInputVelocityScale = EditorGUILayout.Slider("Input Rot. Scale", m_Component.RotationInputVelocityScale, 0, 10);
			m_Component.RotationMaxInputVelocity = EditorGUILayout.FloatField("Max Input Rot.", m_Component.RotationMaxInputVelocity);

			vp_EditorGUIUtility.Separator();
		}
	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoRetractionFoldout()
	{

		m_WeaponRetractionFoldout = EditorGUILayout.Foldout(m_WeaponRetractionFoldout, "Retraction");
		if (m_WeaponRetractionFoldout)
		{

			if ((m_Component.WeaponModel == m_Component.gameObject))
			{
				GUI.enabled = false;
				GUILayout.Label("\nNOTE: Retraction only works on weapon models that are\ninstantiated via the 'Rendering->Weapon Model' slot.\n", vp_EditorGUIUtility.NoteStyle);
			}
			else
				m_Component.DrawRetractionDebugLine = true;

			m_Component.RetractionDistance = EditorGUILayout.Slider("Distance", m_Component.RetractionDistance, 0.0f, 3.0f);
			m_Component.RetractionOffset = EditorGUILayout.Vector2Field("Offset", m_Component.RetractionOffset);
			m_Component.RetractionRelaxSpeed = EditorGUILayout.Slider("Relax Speed", m_Component.RetractionRelaxSpeed, 0.05f, 0.5f);

			GUI.enabled = true;

			GUI.enabled = false;
			GUILayout.Label("Retraction pulls back the weapon when it too close to walls.\nIt is intended for running the system without a weapon\ncamera. A typical value for Distance is 0.5.", vp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;


			vp_EditorGUIUtility.Separator();

		}
		else
			m_Component.DrawRetractionDebugLine = false;

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoShakeFoldout()
	{

		m_WeaponShakeFoldout = EditorGUILayout.Foldout(m_WeaponShakeFoldout, "Shake");
		if (m_WeaponShakeFoldout)
		{
			m_Component.ShakeSpeed = EditorGUILayout.Slider("Speed", m_Component.ShakeSpeed, 0, 10);
			m_Component.ShakeAmplitude = EditorGUILayout.Vector3Field("Amplitude", m_Component.ShakeAmplitude);

			vp_EditorGUIUtility.Separator();
		}

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoBobFoldout()
	{

		m_WeaponBobFoldout = EditorGUILayout.Foldout(m_WeaponBobFoldout, "Bob");
		if (m_WeaponBobFoldout)
		{
			m_Component.BobRate = EditorGUILayout.Vector4Field("Rate", m_Component.BobRate);
			m_Component.BobAmplitude = EditorGUILayout.Vector4Field("Amplitude", m_Component.BobAmplitude);
			m_Component.BobInputVelocityScale = EditorGUILayout.Slider("Input Vel. Scale", m_Component.BobInputVelocityScale, 0, 10);
			m_Component.BobMaxInputVelocity = EditorGUILayout.FloatField("Max Input Vel.", m_Component.BobMaxInputVelocity);
			m_Component.BobRequireGroundContact = EditorGUILayout.Toggle("Require ground contact", m_Component.BobRequireGroundContact);

			GUI.enabled = false;
			GUILayout.Label("XYZ is angular bob... W is position along the\nforward vector. Y & Z rate should be (X/2) for a\nclassic weapon bob.", vp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;

			vp_EditorGUIUtility.Separator();
		}

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoStepFoldout()
	{

		m_WeaponStepFoldout = EditorGUILayout.Foldout(m_WeaponStepFoldout, "Step");
		if (m_WeaponStepFoldout)
		{
			m_Component.StepMinVelocity = EditorGUILayout.FloatField("Min Velocity", m_Component.StepMinVelocity);
			GUI.enabled = false;
			GUILayout.Label("Set a positive minimum velocity to enable footstep forces.", vp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.StepSoftness = EditorGUILayout.IntSlider("Step Softness", m_Component.StepSoftness, 1, 30);
			GUI.enabled = false;
			GUILayout.Label("The number of frames over which to even out each footstep.", vp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.StepPositionForce = EditorGUILayout.Vector3Field("Position Force", m_Component.StepPositionForce);
			m_Component.StepRotationForce = EditorGUILayout.Vector3Field("Rotation Force", m_Component.StepRotationForce);
			m_Component.StepForceScale = EditorGUILayout.Slider("Force Scale", m_Component.StepForceScale, -1.0f, 5.0f);
			m_Component.StepPositionBalance = EditorGUILayout.Slider("Position Balance", m_Component.StepPositionBalance, -1.0f, 1.0f);
			m_Component.StepRotationBalance = EditorGUILayout.Slider("Rotation Balance", m_Component.StepRotationBalance, -1.0f, 1.0f);
			GUI.enabled = false;
			GUILayout.Label("Balance can be used to enhance or reduce 'limping'.", vp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			vp_EditorGUIUtility.Separator();
		}

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoSoundFoldout()
	{

		m_WeaponSoundFoldout = EditorGUILayout.Foldout(m_WeaponSoundFoldout, "Sound");
		if (m_WeaponSoundFoldout)
		{
			m_Component.SoundWield = (AudioClip)EditorGUILayout.ObjectField("Wield", m_Component.SoundWield, typeof(AudioClip), false);
			m_Component.SoundUnWield = (AudioClip)EditorGUILayout.ObjectField("Unwield", m_Component.SoundUnWield, typeof(AudioClip), false);

			vp_EditorGUIUtility.Separator();
		}

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoAnimationFoldout()
	{

		m_WeaponAnimationFoldout = EditorGUILayout.Foldout(m_WeaponAnimationFoldout, "Animation");
		if (m_WeaponAnimationFoldout)
		{
			m_Component.AnimationWield = (AnimationClip)EditorGUILayout.ObjectField("Wield", m_Component.AnimationWield, typeof(AnimationClip), false);
			m_Component.AnimationUnWield = (AnimationClip)EditorGUILayout.ObjectField("Unwield", m_Component.AnimationUnWield, typeof(AnimationClip), false);
			vp_EditorGUIUtility.ObjectList("Ambient", m_Component.AnimationAmbient, typeof(AnimationClip));
			if (m_Component.AnimationAmbient.Count != 0)
			{
				GUI.enabled = false;
				GUILayout.Label("A random animation from this list will be played on the\nweapon automatically with random intervals.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
				m_Component.AmbientInterval = EditorGUILayout.Vector2Field("Ambient Interval (Min:Max)", m_Component.AmbientInterval);
				EditorGUILayout.MinMaxSlider(ref m_Component.AmbientInterval.x, ref m_Component.AmbientInterval.y, 1.0f, 60.0f);
				GUI.enabled = false;
				GUILayout.Label("Average interval in seconds between ambient animations.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
			}

			vp_EditorGUIUtility.Separator();
		}

	}


}

