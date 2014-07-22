/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Event.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	base class for events. contains all common fields and methods,
//					along with utilities for advanced generic delegate handling
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public abstract class vp_Event
{

	protected string m_Name = null;
	public string EventName { get { return m_Name; } }

	protected Type m_ArgumentType = null;
	public Type ArgumentType { get { return m_ArgumentType; } }

	protected Type m_ReturnType = null;
	public Type ReturnType { get { return m_ReturnType; } }

	protected FieldInfo[] m_Fields;				// an array of fields that can trigger the event
	protected Type[] m_DelegateTypes;			// delegate signatures for the fields
	protected MethodInfo[] m_DefaultMethods;	// default values for the fields
	public string[] InvokerFieldNames;			// an array of fieldnames for easy event handler access

	public Dictionary<string, int> Prefixes;	// supported prefixes for target script methods

	public abstract void Register(object target, string method, int variant);
	public abstract void Unregister(object target);
	protected abstract void InitFields();


	/// <summary>
	///
	/// </summary>
	public vp_Event(string name = "")
	{

		m_ArgumentType = GetArgumentType;
		m_ReturnType = GetGenericReturnType;
		m_Name = name;

	}


	/// <summary>
	/// stores the names of the involer fields in an array
	/// </summary>
	protected void StoreInvokerFieldNames()
	{

		InvokerFieldNames = new string[m_Fields.Length];
		for (int v = 0; v < m_Fields.Length; v++)
		{
			InvokerFieldNames[v] = m_Fields[v].Name;
		}

	}


	/// <summary>
	/// returns the generic type of this event type and 'type'
	/// </summary>
	protected Type MakeGenericType(Type type)
	{
		if(m_ReturnType == typeof(void))
			return type.MakeGenericType(new Type[] { m_ArgumentType, m_ArgumentType });
		else
			return type.MakeGenericType(new Type[] { m_ArgumentType, m_ReturnType, m_ArgumentType, m_ReturnType });

	}


	/// <summary>
	/// clears the invocation list of 'field' and adds the external
	/// method
	/// </summary>
	protected void SetFieldToExternalMethod(object target, FieldInfo field, string method, Type type)
	{

		Delegate assignment = Delegate.CreateDelegate(type, target, method, false, false);

		if (assignment == null)
		{
			Debug.LogError("Error (" + this + ") Failed to bind: " + target + " -> " + method + ".");
			return;
		}

		field.SetValue(this, assignment);
		
	}


	/// <summary>
	/// adds the external method to the invocation list of 'field'
	/// </summary>
	protected void AddExternalMethodToField(object target, FieldInfo field, string method, Type type)
	{

		Delegate assignment =	Delegate.Combine((Delegate)field.GetValue(this),
								Delegate.CreateDelegate(type, target, method, false, false));

		if (assignment == null)
		{
			Debug.LogError("Error (" + this + ") Failed to bind: " + target + " -> " + method + ".");
			return;
		}

		field.SetValue(this, assignment);

	}
		

	/// <summary>
	/// clears the invocation list of 'field', setting it to the
	/// local method
	/// </summary>
	protected void SetFieldToLocalMethod(FieldInfo field, MethodInfo method, Type type)
	{

		if (method == null)
		    return;

		Delegate assignment = Delegate.CreateDelegate(type, method);

		if (assignment == null)
		{
			Debug.LogError("Error (" + this + ") Failed to bind: " + method + ".");
			return;
		}

		field.SetValue(this, assignment);

	}


	/// <summary>
	/// removes the external method from the invocation list
	/// of 'field'
	/// </summary>
	protected void RemoveExternalMethodFromField(object target, FieldInfo field)
	{

		List<Delegate> assignment = new List<Delegate>(((Delegate)field.GetValue(this)).GetInvocationList());

		if (assignment == null)
		{
			Debug.LogError("Error (" + this + ") Failed to remove: " + target + " -> " + field.Name + ".");
			return;
		}

		for (int v = assignment.Count - 1; v > -1; v--)
		{
			if (assignment[v].Target == target)
				assignment.Remove(assignment[v]);
		}

		if (assignment != null)
			field.SetValue(this, Delegate.Combine(assignment.ToArray()));

	}


	/// <summary>
	/// creates a static generic method from this event type
	/// and the passed types
	/// </summary>
	protected MethodInfo GetStaticGenericMethod(Type e, string name, Type parameterType, Type returnType)
	{

		foreach (MethodInfo m in e.GetMethods((BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)))
		{

			if (m == null)
				continue;

#if (UNITY_IPHONE)

			// NOTE: The purpose of the "Empty" method is 1) to prevent null
			// ref crashes should a delegate be executed without callbacks in
			// it, 2) to provide a suitable label in the event handler debug
			// output showing that a delegate is empty. However: the static,
			// generic "Empty" methods will not compile on IOS (failing with
			// "JIT compile" errors). These "UNITY_IPHONE" defines are to
			// retain the event system as-is for regular platforms, but
			// strip out IOS sensitive code at compile time.

			if (m.Name == "Empty")
				continue;
#endif

			if (m.Name != name)
				continue;

			MethodInfo m2;

			if (GetGenericReturnType == typeof(void))
				m2 = m.MakeGenericMethod(m_ArgumentType);
			else
				m2 = m.MakeGenericMethod(new Type[] { m_ArgumentType, m_ReturnType });

			// skip if more than one parameter
			if ((m2.GetParameters().Length > 1))
				continue;

			// skip if there is a parameter and we don't want any
			if ((m2.GetParameters().Length == 1) && (parameterType == typeof(void)))
				continue;

			// skip if there is no parameter and we want one
			if ((m2.GetParameters().Length == 0) && (parameterType != typeof(void)))
				continue;

			// skip if there is a parameter but it has the wrong type
			if ((m2.GetParameters().Length == 1) && (m2.GetParameters()[0].ParameterType != parameterType))
				continue;

			if (returnType != m2.ReturnType)
				continue;

			return m2;

		}

		return null;

	}
	

	/// <summary>
	/// returns the argument type of this event (internal)
	/// </summary>
	private Type GetArgumentType
	{
		get
		{
			if (!this.GetType().IsGenericType)
				return typeof(void);

			return this.GetType().GetGenericArguments()[0];
		}
	}


	/// <summary>
	/// returns the generic return type (if any) of this event
	/// </summary>
	private Type GetGenericReturnType
	{
		get
		{
			if (!this.GetType().IsGenericType)
				return typeof(void);

			if (this.GetType().GetGenericArguments().Length != 2)
				return typeof(void);

			return this.GetType().GetGenericArguments()[1];
		}
	}


	/// <summary>
	/// returns the input parameter type (if any) of this event
	/// </summary>
	public Type GetParameterType(int index)
	{

		// non-generic events never have a parameter
		if (!this.GetType().IsGenericType)
			return typeof(void);

		// return if 'index' does not exist
		if (index > (m_Fields.Length - 1))
			Debug.LogError("Error: (" + this + ") Event '" + EventName + "' only supports " + m_Fields.Length + " indices. 'GetParameterType' referenced index " + index + ".");

		// event is generic but has no parameter
		if (m_DelegateTypes[index].GetMethod("Invoke").GetParameters().Length == 0)
			return typeof(void);

		// event is generic and has one parameter
		return m_ArgumentType;

	}


	/// <summary>
	/// returns the return type (if any) of this event
	/// </summary>
	public Type GetReturnType(int index)
	{

		if (index > (m_Fields.Length - 1))
		{
			Debug.LogError("Error: (" + this + ") Event '" + EventName + "' only supports " + m_Fields.Length + " indices. 'GetReturnType' referenced index " + index + ".");
			return null;
		}

		if (this.GetType().GetGenericArguments().Length > 1)
			return GetGenericReturnType;

		Type t = m_DelegateTypes[index].GetMethod("Invoke").ReturnType;
		if(t.IsGenericParameter)
			return m_ArgumentType;

		return t;
				
	}


	/// <summary>
	/// refreshes the editor event dump window when the state
	/// of the event handler changes
	/// </summary>
	protected void Refresh()
	{

#if UNITY_EDITOR
		vp_EventHandler.RefreshEditor = true;
#endif

	}


}

