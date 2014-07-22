/////////////////////////////////////////////////////////////////////////////////
//
//	vp_StateEventHandler.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a version of vp_EventHandler that is aware of the vp_Component
//					state system and can bind its own actions to corresponding
//					states found on the components
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;


public abstract class vp_StateEventHandler : vp_EventHandler
{

	List<vp_Component> m_StateTargets = new List<vp_Component>();


	/// <summary>
	/// 
	/// </summary>
	protected override void Awake()
	{

		base.Awake();

		// fetch all vp_Components that exist at root level in the
		// same hierarchy as the event handler. these will be used to
		// block states recursively (down the hierarchy)
		foreach (vp_Component c in transform.root.GetComponents<vp_Component>())
		{
			m_StateTargets.Add(c);
		}

	}

	
	/// <summary>
	/// binds the event handler's activities to states with the
	/// same names found on components down the hierarchy
	/// </summary>
	protected void BindStateToActivity(vp_Activity a)
	{

		BindStateToActivityOnStart(a);
		BindStateToActivityOnStop(a);

	}


	/// <summary>
	/// binds the event handler's start activity to states with
	/// the same names found on components down the hierarchy
	/// </summary>
	protected void BindStateToActivityOnStart(vp_Activity a)
	{

		if (!ActivityInitialized(a))
			return;

		string s = a.EventName;

		a.StartCallbacks +=
		delegate()
		{
			foreach (vp_Component c in m_StateTargets)
			{
				c.SetState(s, true, true);
			}
		};
		// NOTE: this delegate currently won't show up in an event dump

	}


	/// <summary>
	/// binds the event handler's stop activity to states with
	/// the same names found on components down the hierarchy
	/// </summary>
	protected void BindStateToActivityOnStop(vp_Activity a)
	{

		if (!ActivityInitialized(a))
			return;

		string s = a.EventName;

		a.StopCallbacks +=
		delegate()
		{
			foreach (vp_Component c in m_StateTargets)
			{
				c.SetState(s, false, true);
			}
		};
		// NOTE: this delegate currently won't show up in an event dump

	}


	/// <summary>
	/// refreshes all component states bound to this event handler's
	/// activities
	/// </summary>
	public void RefreshActivityStates()
	{

		foreach (vp_Event a in m_HandlerEvents.Values)
		{
			if ((a is vp_Activity) || (a.GetType().BaseType == typeof(vp_Activity)))
			{
				foreach (vp_Component c in m_StateTargets)
				{
					c.SetState(a.EventName, ((vp_Activity)a).Active, true, false);
				}
			}
		}

	}


	/// <summary>
	/// resets all component states bound to this event handler's
	/// activities
	/// </summary>
	public void ResetActivityStates()
	{

		foreach (vp_Component c in m_StateTargets)
		{
			c.ResetState();
		}

	}


	/// <summary>
	/// sets a state on all component states bound to this event
	/// handler's activities
	/// </summary>
	public void SetState(string state, bool setActive = true, bool recursive = true, bool includeDisabled = false)
	{

		foreach (vp_Component c in m_StateTargets)
		{
			c.SetState(state, setActive, recursive, includeDisabled);
		}

	}


	/// <summary>
	/// returns true if the passed activity has been initialized
	/// yet, false if not
	/// </summary>
	private bool ActivityInitialized(vp_Activity a)
	{

		if (a == null)
		{
			Debug.LogError("Error: (" + this + ") Activity is null.");
			return false;
		}

		if (string.IsNullOrEmpty(a.EventName))
		{
			Debug.LogError("Error: (" + this + ") Activity not initialized. Make sure the event handler has run its Awake call before binding layers.");
			return false;
		}

		return true;

	}

}

