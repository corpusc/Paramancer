/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Activity.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	an advanced event type implementing a set of common activity
//					related game logics such as event duration, trigger intervals,
//					scheduled disabling, and most importantly conditional enabling
//					and disabling of activities
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Represents an activity that may be active or inactive, and that may
/// trigger callbacks when toggled. The method names in the target script
/// must have either of the prefixes 'CanStart_', 'CanStop_', 'OnStart_'
/// or 'OnStop_'. An unlimited number of conditions and callback methods
/// can be added in this way. Calling 'TryStart' or 'TryStop' on the event
/// returns a boolean indicating whether the action is currently authorized.
/// If so, the activity will also trigger its start or stop callback.
/// Force the event on or off directly using its 'Active' property or
/// 'Start' or 'Stop' method. Optionally supports a single generic
/// argument which can be accessed over the event duration via the
/// 'Argument' property.
/// </summary>
public class vp_Activity : vp_Event			// non-generic version for 0 arguments
{

	public delegate void Callback();
	public delegate bool Condition();

	public Callback StartCallbacks;
	public Callback StopCallbacks;

	public Condition StartConditions;
	public Condition StopConditions;

	public Callback FailStartCallbacks;
	public Callback FailStopCallbacks;

	protected static void Empty() { }
	protected static bool AlwaysOK() { return true; }

	protected vp_Timer.Handle m_ForceStopTimer = new vp_Timer.Handle();

	protected object m_Argument = null;

	protected bool m_Active;

	public float NextAllowedStartTime = 0.0f;
	public float NextAllowedStopTime = 0.0f;
	private float m_MinPause = 0.0f;		// minimum delay between stop and start
	private float m_MinDuration = 0.0f;		// minimum delay between start and stop
	private float m_MaxDuration = -1.0f;		// force stop after this duration


	/// <summary>
	///
	/// </summary>
	public vp_Activity(string name)
		: base(name)
	{
		InitFields();
	}


	/// <summary>
	/// prevents player from restarting an activity too soon
	/// after stopping
	/// </summary>
	public float MinPause { get { return m_MinPause; } set { m_MinPause = Mathf.Max(0.0f, value); } }


	/// <summary>
	/// prevents player from aborting an activity too soon after
	/// starting
	/// </summary>
	public float MinDuration
	{
		get
		{
			return m_MinDuration;
		}
		set
		{
			m_MinDuration = Mathf.Max(0.001f, value);
			if (m_MaxDuration == -1.0f)
				return;
			if (m_MinDuration > m_MaxDuration)
			{
				m_MinDuration = m_MaxDuration;
				Debug.LogWarning("Warning: (vp_Activity) Tried to set MinDuration longer than MaxDuration for '" + EventName + "'. Capping at MaxDuration.");
			}
		}
	}


	/// <summary>
	/// automatically stops an activity after a set timespan
	/// </summary>
	public float AutoDuration
	{
		get
		{
			return m_MaxDuration;
		}
		set
		{
			if (value == -1.0f)
			{
				m_MaxDuration = value;
				return;
			}
			else
				m_MaxDuration = Mathf.Max(0.001f, value);

			if (m_MaxDuration < m_MinDuration)
			{
				m_MaxDuration = m_MinDuration;
				Debug.LogWarning("Warning: (vp_Activity) Tried to set MaxDuration shorter than MinDuration for '" + EventName + "'. Capping at MinDuration.");
			}
		}
	}


	/// <summary>
	/// returns the user-passed argument of this activity,
	/// while active
	/// </summary>
	public object Argument
	{
		get
		{
			if (m_ArgumentType == null)
			{
				Debug.LogError("Error: (" + this + ") Tried to fetch argument from '" + EventName + "' but this activity takes no parameters.");
				return null;
			}
			return m_Argument;
		}
		set
		{
			if (m_ArgumentType == null)
			{
				Debug.LogError("Error: (" + this + ") Tried to set argument for '" + EventName + "' but this activity takes no parameters.");
				return;
			}
			m_Argument = value;
		}
	}


	/// <summary>
	/// initializes the standard fields of this event type and
	/// signature
	/// </summary>
	protected override void InitFields()
	{

		m_DelegateTypes = new Type[] {	typeof(vp_Activity.Callback),
										typeof(vp_Activity.Callback),
										typeof(vp_Activity.Condition),
										typeof(vp_Activity.Condition),
										typeof(vp_Activity.Callback),
										typeof(vp_Activity.Callback)};

		m_Fields = new FieldInfo[] {	this.GetType().GetField("StartCallbacks"),
										this.GetType().GetField("StopCallbacks"),
										this.GetType().GetField("StartConditions"),
										this.GetType().GetField("StopConditions"),
										this.GetType().GetField("FailStartCallbacks"),
										this.GetType().GetField("FailStopCallbacks")};

		StoreInvokerFieldNames();

		m_DefaultMethods = new MethodInfo[]{	this.GetType().GetMethod("Empty"),
												this.GetType().GetMethod("Empty"),
												this.GetType().GetMethod("AlwaysOK"),
												this.GetType().GetMethod("AlwaysOK"),
												this.GetType().GetMethod("Empty"),
												this.GetType().GetMethod("Empty")};

		Prefixes = new Dictionary<string, int>() {	{ "OnStart_", 0 },
													{ "OnStop_", 1 },
													{ "CanStart_", 2 },
													{ "CanStop_", 3 },
													{ "OnFailStart_", 4 },
													{ "OnFailStop_", 5 } };
		
		StartCallbacks = Empty;
		StopCallbacks = Empty;
		StartConditions = AlwaysOK;
		StopConditions = AlwaysOK;
		FailStartCallbacks = Empty;
		FailStopCallbacks = Empty;

	}


	/// <summary>
	/// registers an external method to this event
	/// </summary>
	public override void Register(object t, string m, int v)
	{
		AddExternalMethodToField(t, m_Fields[v], m, m_DelegateTypes[v]);
		Refresh();
	}


	/// <summary>
	/// unregisters an external method from this event
	/// </summary>
	public override void Unregister(object t)
	{

		RemoveExternalMethodFromField(t, m_Fields[0]);
		RemoveExternalMethodFromField(t, m_Fields[1]);
		RemoveExternalMethodFromField(t, m_Fields[2]);
		RemoveExternalMethodFromField(t, m_Fields[3]);
		RemoveExternalMethodFromField(t, m_Fields[4]);
		RemoveExternalMethodFromField(t, m_Fields[5]);
		Refresh();

	}


	/// <summary>
	/// try to start this activity. will only succeed if not already
	/// active, and if every method assigned as the start condition
	/// for the activity returns true. on success, this method will
	/// fire the start callbacks of the activity (if any) and itself
	/// return true. passing 'false' as the 'startIfAllowed' argument
	/// can be used to check whether starting an activity would be
	/// allowed if attempted - without starting it. note however that
	/// any code in its start conditions will be executed either way.
	/// </summary>
	public bool TryStart(bool startIfAllowed = true)
	{

		if (m_Active)
			return false;

		if (Time.time < NextAllowedStartTime)
		{
			m_Argument = null;
			return false;
		}

		foreach (Condition b in StartConditions.GetInvocationList())
		{
			if (!b.Invoke())
			{
				m_Argument = null;
				if(startIfAllowed)
					FailStartCallbacks.Invoke();

				return false;
			}

		}

		if (startIfAllowed)
			Active = true;

		return true;

	}


	/// <summary>
	/// try to stop this activity. will only succeed if the activity
	/// is active, and if every method assigned as the stop condition
	/// for the activity returns true. on success, this method will
	/// fire the stop callbacks of the activity (if any) and itself
	/// return true. passing 'false' as the 'stopIfAllowed' argument
	/// can be used to check whether stopping an activity would be
	/// allowed if attempted - without stopping it. note however that
	/// any code in its stop conditions will be executed either way.
	/// </summary>
	public bool TryStop(bool stopIfAllowed = true)
	{

		if (!m_Active)
			return false;

		if (Time.time < NextAllowedStopTime)
			return false;

		foreach (Condition b in StopConditions.GetInvocationList())
		{
			if (!b.Invoke())
			{
				if (stopIfAllowed)
					FailStopCallbacks.Invoke();

				return false;
			}
		}

		if (stopIfAllowed)
			Active = false;

		return true;

	}


	/// <summary>
	/// when set, starts or stops this activity and fires its
	/// start or stop callbacks (if any)
	/// </summary>
	public bool Active
	{

		set
		{
			if ((value == true) && !m_Active)
			{
				m_Active = true;
				StartCallbacks.Invoke();
				NextAllowedStopTime = Time.time + m_MinDuration;
				if (m_MaxDuration > 0.0f)
					vp_Timer.In(m_MaxDuration, delegate() { Stop(); }, m_ForceStopTimer);
			}
			else if ((value == false) && m_Active)
			{
				m_Active = false;
				StopCallbacks.Invoke();
				NextAllowedStartTime = Time.time + m_MinPause;
				m_Argument = null;
			}
		}
		get
		{
			return m_Active;
		}

	}


	/// <summary>
	/// starts this activity and fires its start callbacks (if any)
	/// </summary>
	public void Start(float forcedActiveDuration = 0.0f)
	{

		Active = true;

		// override active duration, if provided
		if (forcedActiveDuration > 0.0f)
			NextAllowedStopTime = Time.time + forcedActiveDuration;

	}


	/// <summary>
	/// stops this activity and fires its stop callbacks (if any)
	/// </summary>
	public void Stop(float forcedPauseDuration = 0.0f)
	{

		Active = false;

		// override pause duration, if provided
		if (forcedPauseDuration > 0.0f)
			NextAllowedStartTime = Time.time + forcedPauseDuration;

	}


	/// <summary>
	/// disallows this activity for 'duration' seconds
	/// </summary>
	public void Disallow(float duration)
	{
		NextAllowedStartTime = Time.time + duration;
	}
	

}


/// <summary>
/// Represents an activity that may be active or inactive, and that may
/// trigger callbacks when toggled. The method names in the target script
/// must have either of the prefixes 'CanStart_', 'CanStop_', 'OnStart_'
/// or 'OnStop_'. An unlimited number of conditions and callback methods
/// can be added in this way. Calling 'TryStart' or 'TryStop' on the event
/// returns a boolean indicating whether the action is currently allowed.
/// If so, the activity will also trigger its start or stop callback.
/// Force the event on or off directly using its 'Active' property or
/// 'Start' or 'Stop' method. Optionally supports a single generic
/// argument which can be accessed over the event duration via the
/// 'Argument' property.
/// </summary>
public class vp_Activity<V> : vp_Activity			// generic version with 1 argument
{


	/// <summary>
	///
	/// </summary>
	public vp_Activity(string name) : base(name) { }
	

	/// <summary>
	/// try to start this activity. will only succeed if not already
	/// active, and if every method assigned as the start condition
	/// for the activity returns true. on success, this method will
	/// fire the start callbacks of the activity (if any) and itself
	/// return true
	/// </summary>
	public bool TryStart<T>(T argument)
	{

		if (m_Active)
			return false;

		m_Argument = argument;

		return TryStart();

	}

}

