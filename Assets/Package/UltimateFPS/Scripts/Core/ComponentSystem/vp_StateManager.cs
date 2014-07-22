/////////////////////////////////////////////////////////////////////////////////
//
//	vp_StateManager.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	base class for extended MonoBehaviours. these components have
//					the added functionality of support for the VisionPunk component
//					and event systems, an 'Init' method that gets executed
//					once after all 'Start' calls, and caching of gameobject
//					properties and startup gameobject hierarchy
//
/////////////////////////////////////////////////////////////////////////////////


using UnityEngine;
using System.Collections.Generic;


public class vp_StateManager
{

	private vp_Component m_Component = null;			// component to manipulate
	private List<vp_State> m_States = null;				// list sent from the component at startup. IMPORTANT: must not be altered at runtime
	private Dictionary<string, int> m_StateIds = null;	// dictionary of state list-indices, for fast lookup

	private static string m_AppNotPlayingMessage = "Error: StateManager can only be accessed while application is playing.";
	private static string m_DefaultStateNoDisableMessage = "Warning: The 'Default' state cannot be disabled.";

	// work variables
	private int m_DefaultId = 0;	// id of the default state, this will be equal to the amount of states - 1
	private int m_TargetId = 0;		// id of the last modified state (for triggering enable / disable callbacks)


	/// <summary>
	/// manages a list of states and corresponding presets for a
	/// component. loads the presets into memory on startup, and
	/// allows applying them from memory to the component.
	/// states are enabled in a layered manner, that is: the
	/// default state is always alive, and any enabled states are
	/// applied on top of it in the order of which they were enabled.
	/// 
	/// this class doesn't store any preset data between sessions.
	/// it is merely used to manipulate the component using a list
	/// of states that is sent along at startup. it is very silent
	/// and forgiving; it won't complain if a state isn't found
	/// (since providing a certain state should not be considered
	/// mandatory for a component, and states can be set recursively).
	/// it will also ignore empty presets
	/// </summary>
	public vp_StateManager(vp_Component component, List<vp_State> states)
	{

		m_States = states;
		m_Component = component;

		// create default state and add it to the list
		m_Component.RefreshDefaultState();

		// refresh the initial state, needed for being able to save
		// partial presets in the editor
#if UNITY_EDITOR
			m_Component.RefreshInitialState();
#endif

		// load states and initialize the state id dictionary
		m_StateIds = new Dictionary<string, int>(System.StringComparer.CurrentCulture);
		foreach (vp_State s in m_States)
		{

			s.StateManager = this;

			// store the name and list index of each state in a dictionary for
			// fast lookup. IMPORTANT: the state list (m_States) must never be
			// modified at runtime (i.e. have states reordered, added, renamed
			// or removed) or the dictionary (m_StateIds) will be out of date
			if (!m_StateIds.ContainsKey(s.Name))
				m_StateIds.Add(s.Name, m_States.IndexOf(s));
			else
			{
				Debug.LogWarning("Warning: " + m_Component.GetType() + " on '" + m_Component.name + "' has more than one state named: '" + s.Name + "'. Only the topmost one will be used.");
				m_States[m_DefaultId].StatesToBlock.Add(m_States.IndexOf(s));
			}

			// load up the preset of each user assigned state and
			if (s.Preset == null)
				s.Preset = new vp_ComponentPreset();
			if (s.TextAsset != null)
				s.Preset.LoadFromTextAsset(s.TextAsset);

		}

		// the default state of a component is always the last one in the list
		m_DefaultId = m_States.Count - 1;

	}


	/// <summary>
	/// adds this state to the 'CurrentlyBlockedBy' list of all
	/// states in this state's 'StatesToBlock' list
	/// </summary>
	public void ImposeBlockingList(vp_State blocker)
	{
		
		if (blocker == null)
			return;

		if (blocker.StatesToBlock == null)
			return;

		if (m_States == null)
			return;
		
		foreach (int blockee in blocker.StatesToBlock)
		{
			m_States[blockee].AddBlocker(blocker);
		}

	}


	/// <summary>
	/// removes this state from the 'CurrentlyBlockedBy' list of
	/// all states in this state's 'StatesToBlock' list
	/// </summary>
	public void RelaxBlockingList(vp_State blocker)
	{

		if (blocker == null)
			return;

		if (blocker.StatesToBlock == null)
			return;

		if (m_States == null)
			return;

		foreach (int blockee in blocker.StatesToBlock)
		{
			m_States[blockee].RemoveBlocker(blocker);
		}

	}


	/// <summary>
	/// if the passed state exists in the state list, enables or
	/// disables it and recombines all states in the new order.
	/// if there is no matching state, this method will do nothing
	/// </summary>
	public void SetState(string state, bool setEnabled = true)
	{

		if (!AppPlaying())
			return;

		// return if we can't find the passed state
		if (!m_StateIds.TryGetValue(state, out m_TargetId))
			return;

		// prevent manually disabling the default state
		if ((m_TargetId == m_DefaultId) && !setEnabled)
		{
			Debug.LogWarning(m_DefaultStateNoDisableMessage);
			return;
		}

		m_States[m_TargetId].Enabled = setEnabled;

		CombineStates();
		m_Component.Refresh();

	}

		
	/// <summary>
	/// disables all states except the default state, and enables
	/// the default state
	/// </summary>
	public void Reset()
	{

		if (!AppPlaying())
			return;

		foreach (vp_State s in (m_States))
		{
			s.Enabled = false;
		}

		m_States[m_DefaultId].Enabled = true;

		// apply all states in the new order
		m_TargetId = m_DefaultId;
		CombineStates();

	}

	
	/// <summary>
	/// combines all the states in the list to a temporary state,
	/// and sets it on the component
	/// </summary>
	public void CombineStates()
	{	

		// go backwards so default state is applied first
		for(int v = m_States.Count - 1; v > -1; v--)
		{

			if (v != m_DefaultId)
			{

				if (!m_States[v].Enabled)
					continue;

				if (m_States[v].Blocked)
					continue;

				if (m_States[v].TextAsset == null)
					continue;

			}

			if (m_States[v].Preset == null)
				continue;

			if (m_States[v].Preset.ComponentType == null)
				continue;

			vp_ComponentPreset.Apply(m_Component, m_States[v].Preset);

		}


#if UNITY_EDITOR
		m_Component.RefreshInitialState();
#endif

	}


	/// <summary>
	/// returns true if the state associated with the passed string
	/// is on the list and enabled, otherwise returns false
	/// </summary>
	public bool IsEnabled(string state)
	{

		if (!AppPlaying())
			return false;

		if (m_StateIds.TryGetValue(state, out m_TargetId))
			return m_States[m_TargetId].Enabled;

		return false;

	}
	

	/// <summary>
	/// dumps an error message and returns false if state manager
	/// is accessed while the app is not playing in the editor
	/// </summary>
	private static bool AppPlaying()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			Debug.LogError(m_AppNotPlayingMessage);
			return false;
		}
#endif
		return true;
	}


}


