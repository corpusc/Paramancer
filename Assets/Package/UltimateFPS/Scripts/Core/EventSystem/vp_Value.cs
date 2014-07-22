/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Value.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	event type for getting and setting target script properties.
//					the generic invocation fields, 'Get' and 'Set', expose their
//					property counterparts
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Collections.Generic;


/// <summary>
/// Exposes a C# Property of a single registered script. The property name
/// in the target script must have the prefix 'OnValue_'. Use 'Get' and
/// 'Set' to retrieve or assign the property via this event.
/// </summary>
public class vp_Value<V> : vp_Event
{
	
#if (!UNITY_IPHONE)
	// NOTE: see the 'UNITY_IPHONE' comment in vp_Event.cs for info on this
	protected static T Empty<T>() { return default(T); }
	protected static void Empty<T>(T value) { }
#endif

	public delegate T Getter<T>();
	public delegate void Setter<T>(T o);

	public Getter<V> Get;
	public Setter<V> Set;

	FieldInfo[] Fields
	{
		get
		{
			return m_Fields;
		}
	}

	/// <summary>
	///
	/// </summary>
	public vp_Value(string name) : base(name)
	{

		InitFields();

	}


	/// <summary>
	/// initializes the standard fields of this event type and
	/// signature
	/// </summary>
	protected override void InitFields()
	{

		m_Fields = new FieldInfo[] {	this.GetType().GetField("Get"),
										this.GetType().GetField("Set") };

		StoreInvokerFieldNames();

		m_DelegateTypes = new Type[]{	typeof(vp_Value<>.Getter<>),
									typeof(vp_Value<>.Setter<>)};

		m_DefaultMethods = new MethodInfo[] {	GetStaticGenericMethod(this.GetType(), "Empty", typeof(void), m_ArgumentType),
												GetStaticGenericMethod(this.GetType(), "Empty", m_ArgumentType, typeof(void))};

		Prefixes = new Dictionary<string, int>() { { "get_OnValue_", 0 }, { "set_OnValue_", 1 } };

		if (m_DefaultMethods[0] != null)
			SetFieldToLocalMethod(m_Fields[0], m_DefaultMethods[0], MakeGenericType(m_DelegateTypes[0]));

		if (m_DefaultMethods[1] != null)
			SetFieldToLocalMethod(m_Fields[1], m_DefaultMethods[1], MakeGenericType(m_DelegateTypes[1]));
	
	}


	/// <summary>
	/// registers an external method to this event
	/// </summary>
	public override void Register(object t, string m, int v)
	{

		if (m == null)
			return;

		SetFieldToExternalMethod(t, m_Fields[v], m, MakeGenericType(m_DelegateTypes[v]));
		Refresh();

	}



	/// <summary>
	/// unregisters an external method from this event
	/// </summary>
	public override void Unregister(object t)
	{

		if (m_DefaultMethods[0] != null)
			SetFieldToLocalMethod(m_Fields[0], m_DefaultMethods[0], MakeGenericType(m_DelegateTypes[0]));

		if (m_DefaultMethods[1] != null)
			SetFieldToLocalMethod(m_Fields[1], m_DefaultMethods[1], MakeGenericType(m_DelegateTypes[1]));

		Refresh();

	}


}


