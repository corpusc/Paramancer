/////////////////////////////////////////////////////////////////////////////////
//
//	vp_ComponentPersister.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	persists changes made to a component at runtime. 
//					NOTE:
//					- a component will only be persisted if it is currently
//					focused in the editor
//					- the persister will run a couple of times during startup.
//					it's not recommended to change component values during
//					Start or Awake, or they may get interpreted as user changes
//					and persisted
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;


public class vp_ComponentPersister
{

	public vp_Component Component = null;	// component to persist
	public bool IsActive = true;			// when the component is destroyed properly, this will be set false
	private bool m_IsPlaying = false;		// used to properly detect when the application starts or stops

	public List<object> PlayModePersistFields = new List<object>();


	/// </summary>
	///
	/// </summary>
	public vp_ComponentPersister()
	{

		EditorApplication.playmodeStateChanged += delegate()
		{
			PlayModeCallback();
		};

	}


	/// <summary>
	/// called every time the editor changes its play mode
	/// </summary>
	public void PlayModeCallback()
	{

		if (!IsActive)
			return;

		if(Component == null)
		{
			Debug.LogError("Error! vp_ComponentPersister: Component is null.");
			return;
		}

		if (!m_IsPlaying && EditorApplication.isPlaying)
		{
			// if we end up here, we have detected that the application
			// was started, so store all the component's variables

			// 'm_IsPlaying' is used to only trigger this once per application
			// start (the callback is triggered several times per start).
			m_IsPlaying = true;
			Persist();
			return;
		}

		if (EditorApplication.isCompiling ||
			EditorApplication.isPaused ||
			EditorApplication.isPlaying ||
			EditorApplication.isPlayingOrWillChangePlaymode)
			return;
		
		// if we end up here, we have detected that the application
		// was stopped, so see if the component should be restored

		if (GetPersistState() == false)
		{
			Component.Persist = false;
			return;
		}

		PersistRestore();

	}
	

	/// <summary>
	/// backs up the fields of the component. this should be done
	/// every time the component is modified in the inspector.
	/// </summary>
	public void Persist()
	{

		if (!IsActive)
			return;

		if (Component == null)
		{
			Debug.LogError("Error! vp_ComponentPersister: Component is null.");
			return;
		}

		PlayModePersistFields.Clear();

		foreach (FieldInfo f in Component.GetType().GetFields())
		{
			PlayModePersistFields.Add(f.GetValue(Component));
			//Debug.Log("(" + f.FieldType.Name + ") " + f.Name + " = " + f.GetValue(Component));
		}

	}

	/// <summary>
	/// restores the backed up fields. this is called every time
	/// the application is stopped, if 'GetPersistState' is true
	/// </summary>
	private void PersistRestore()
	{

		// or persist the rest of the values
		int v = 0;
		foreach (FieldInfo f in Component.GetType().GetFields())
		{

			if (f.FieldType == typeof(float) ||
			f.FieldType == typeof(Vector4) ||
			f.FieldType == typeof(Vector3) ||
			f.FieldType == typeof(Vector2) ||
			f.FieldType == typeof(int) ||
			f.FieldType == typeof(bool) ||
			f.FieldType == typeof(string))
			{
				f.SetValue(Component, PlayModePersistFields[v]);
				//Debug.Log("(" + f.FieldType.Name + ") " + f.Name + " = " + f.GetValue(Component));
			}
			else
			{
				//Debug.LogError("Warning! vp_ComponentPersister can't persist type '" + f.FieldType.Name.ToString() + "'");
			}
			v++;
		}

	}


	/// <summary>
	/// returns true if target component has a bool called 'Persist'
	/// which is currently set to 'true', otherwise returns false
	/// </summary>
	private bool GetPersistState()
	{

		bool state = false;
		try
		{

			// first fetch current persist state. it must always be
			// persisted and may have changed during play.
			int d = 0;

			foreach (FieldInfo f in Component.GetType().GetFields())
			{
				// if there is a field called 'Persist' and it is true,
				// this method will return true, otherwise false.
				if (f.Name == "Persist")
					state = (bool)PlayModePersistFields[d];
				d++;
			}

		}
		catch
		{
			// if we end up here there has been some kind of exception
			// (usually the result of re-compilation or a funky editor
			// state and nothing to worry about).
			//Debug.LogError("Failed to get persist state.");
		}
		
		return state;

	}


}






