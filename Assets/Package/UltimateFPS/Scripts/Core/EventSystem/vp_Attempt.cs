/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Attempt.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	event type for evaluating and executing the results of attempted
//					user actions that may succeed or fail. supports 0-1 generic
//					arguments and returns bool
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Collections.Generic;


/// <summary>
/// Represents a user action that may succeed or fail. The property
/// name in the target script must have the prefix 'OnAttempt_'. Call
/// 'Try' to invoke this event. The result is indicated by the
/// boolean return value.
/// </summary>
public class vp_Attempt : vp_Event			// non-generic version for 0 arguments
{
	
	public delegate bool Tryer();
	public Tryer Try;

	protected static bool AlwaysOK() { return true; }


	/// <summary>
	///
	/// </summary>
	public vp_Attempt(string name) : base(name)
	{
		InitFields();
	}


	/// <summary>
	/// initializes the standard fields of this event type and
	/// signature
	/// </summary>
	protected override void InitFields()
	{

		m_Fields = new FieldInfo[]{	this.GetType().GetField("Try")	};

		StoreInvokerFieldNames();

		m_DefaultMethods = new MethodInfo[]{	this.GetType().GetMethod("AlwaysOK")};

		m_DelegateTypes = new Type[] { typeof(vp_Attempt.Tryer) };
		Prefixes = new Dictionary<string, int>()	{	{"OnAttempt_", 0}	};

		Try = AlwaysOK;

	}


	/// <summary>
	/// registers an external method to this event
	/// </summary>
	public override void Register(object t, string m, int v)
	{
		Try = (vp_Attempt.Tryer)vp_Attempt.Tryer.CreateDelegate(m_DelegateTypes[v], t, m);
		Refresh();
	}


	/// <summary>
	/// unregisters an external method from this event
	/// </summary>
	public override void Unregister(object t)
	{
		Try = AlwaysOK;
		Refresh();
	}


}


/// <summary>
/// Represents a user action that may succeed or fail. The property
/// name in the target script must have the prefix 'OnAttempt_'. Call
/// 'Try' with a single argument of any type to invoke this event. The
/// result is indicated by the boolean return value.
/// </summary>
public class vp_Attempt<V> : vp_Attempt			// generic version with 1 argument
{

	protected static bool AlwaysOK<T>(T value) { return true; }

	public delegate bool Tryer<T>(T value);
	public new Tryer<V> Try;


	/// <summary>
	///
	/// </summary>
	public vp_Attempt(string name) : base(name){}


	/// <summary>
	/// initializes the standard fields of this event type and
	/// signature
	/// </summary>
	protected override void InitFields()
	{

		m_Fields = new FieldInfo[]{	this.GetType().GetField("Try")};

		StoreInvokerFieldNames();

		m_DefaultMethods = new MethodInfo[]{	GetStaticGenericMethod(this.GetType(), "AlwaysOK", m_ArgumentType, typeof(bool))};

		m_DelegateTypes = new Type[] { typeof(vp_Attempt<>.Tryer<>) };
		Prefixes = new Dictionary<string, int>() { { "OnAttempt_", 0 } };

		if (m_DefaultMethods[0] != null)
			SetFieldToLocalMethod(m_Fields[0], m_DefaultMethods[0], MakeGenericType(m_DelegateTypes[0]));

	}


	/// <summary>
	/// registers an external method to this event
	/// </summary>
	public override void Register(object t, string m, int v)
	{

		if (((Delegate)m_Fields[v].GetValue(this)).Method.Name != m_DefaultMethods[v].Name)
			UnityEngine.Debug.LogWarning("Warning: Event '" + this.EventName + "' of type (vp_Attempt) targets multiple methods. Events of this type must reference a single method (only the last reference will be functional).");

		if(m != null)
			SetFieldToExternalMethod(t, m_Fields[0], m, MakeGenericType(m_DelegateTypes[v]));

	}

	
	/// <summary>
	/// unregisters an external method from this event
	/// </summary>
	public override void Unregister(object t)
	{

		if (m_DefaultMethods[0] != null)
			SetFieldToLocalMethod(m_Fields[0], m_DefaultMethods[0], MakeGenericType(m_DelegateTypes[0]));

	}


}
