/////////////////////////////////////////////////////////////////////////////////
//
//	vp_EventDump.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	debug class for dumping the state of an event handler to
//					an editor window or the console
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Collections.Generic;


public class vp_EventDump
{


	/// <summary>
	/// returns all registered events and actions, along with
	/// their target objects as a formatted string. TIP: call
	/// this from a 'Start' method or from a bound key, since by
	/// then all objects should have called in to the handler.
	/// do not call it from 'Awake'.
	/// </summary>
	public static string Dump(vp_EventHandler handler, string []eventTypes)
	{

		string s = "";// "--- EVENT DUMP ---\n\n";

		foreach (string type in eventTypes)
		{
			switch (type)
			{
				case "vp_Message":
					s += DumpEventsOfType("vp_Message", (eventTypes.Length > 1 ? "MESSAGES:\n\n" : ""), handler);
					break;
				case "vp_Attempt":
					s += DumpEventsOfType("vp_Attempt", (eventTypes.Length > 1 ? "ATTEMPTS:\n\n" : ""), handler);
					break;
				case "vp_Value":
					s += DumpEventsOfType("vp_Value", (eventTypes.Length > 1 ? "VALUES:\n\n" : ""), handler);
					break;
				case "vp_Activity":
					s += DumpEventsOfType("vp_Activity", (eventTypes.Length > 1 ? "ACTIVITIES:\n\n" : ""), handler);
					break;
			}
		}
		return s;

	}


	/// <summary>
	/// 
	/// </summary>
	private static string DumpEventsOfType(string type, string caption, vp_EventHandler handler)
	{

		string s = caption.ToUpper();
		foreach (FieldInfo f in handler.GetType().GetFields((	BindingFlags.Public | BindingFlags.NonPublic |
																BindingFlags.Instance | BindingFlags.DeclaredOnly)))
		{
			string listeners = null;

			switch(type)
			{
				case "vp_Message":
					if (f.FieldType.ToString().Contains("vp_Message"))
					{
						vp_Message e = (vp_Message)f.GetValue(handler);
						listeners = DumpEventListeners(e, new string[] { "Send" });
					}
					break;
				case "vp_Attempt":
					if (f.FieldType.ToString().Contains("vp_Attempt"))
					{
						vp_Event e = (vp_Event)f.GetValue(handler);
						listeners = DumpEventListeners(e, new string[] { "Try" });
					}
					break;
				case "vp_Value":
					if(f.FieldType.ToString().Contains("vp_Value"))
					{
						vp_Event e = (vp_Event)f.GetValue(handler);
						listeners = DumpEventListeners(e, new string[] { "Get", "Set" });
					}
					break;
				case "vp_Activity":
					if (f.FieldType.ToString().Contains("vp_Activity"))
					{
						vp_Event e = (vp_Event)f.GetValue(handler);
						listeners = DumpEventListeners(e, new string[] { "StartConditions", "StopConditions", "StartCallbacks", "StopCallbacks", "FailStartCallbacks", "FailStopCallbacks"	});
					}
					break;
			}
			if (!string.IsNullOrEmpty(listeners))
				s += "\t\t" + f.Name + "\n" + listeners + "\n";
		}
		return s;
	}


	/// <summary>
	/// 
	/// </summary>
	private static string DumpEventListeners(object e, string []invokers)
	{

		Type o = e.GetType();

		string s = "";

		foreach(string i in invokers)
		{

			FieldInfo f = o.GetField(i);

			if (f == null)
				return "";

			Delegate d = (Delegate)f.GetValue(e);


			string[] listeners = null;

			if(d != null)
				listeners = GetMethodNames(d.GetInvocationList());

			s += "\t\t\t\t";

			if (o.ToString().Contains("vp_Value"))
			{
				switch (i)
				{
					case "Get":
						s += "Get";
						break;
					case "Set":
						s += "Set";
						break;
					default:
						s += "Unsupported listener: ";
						break;
				}
			}
			else if (o.ToString().Contains("vp_Attempt"))
				s += "Try";
			else if (o.ToString().Contains("vp_Message"))
				s += "Send";
			else if (o.ToString().Contains("vp_Activity"))
			{
				switch (i)
				{
					case "StartConditions":
						s += "TryStart";
						break;
					case "StopConditions":
						s += "TryStop";
						break;
					case "StartCallbacks":
						s += "Start";
						break;
					case "StopCallbacks":
						s += "Stop";
						break;
					case "FailStartCallbacks":
						s += "FailStart";
						break;
					case "FailStopCallbacks":
						s += "FailStop";
						break;
					default:
						s += "Unsupported listener: ";
						break;
				}
			}
			else
				s += "Unsupported listener";

			// LORE: the first listener is the 'Empty' delegate, so
			// 'Length == 1' actually means zero and will show 'Empty'

			if (listeners != null)
			{

				if(listeners.Length > 2)
					s += ":\n";
				else
					s += ": ";

				s += DumpDelegateNames(listeners);

			}

		}

		return s;
		
	}

	
	/// <summary>
	/// 
	/// </summary>
	private static string[] GetMethodNames(Delegate[] list)
	{

		// only include delegates referencing declared methods
		list = RemoveDelegatesFromList(list);
		
		string [] s = new string[list.Length];
		if (list.Length == 1)
		{
			// returns 'Empty' delegate or get/set accessor
			s[0] = ((list[0].Target == null) ? "" : "(" + list[0].Target + ") ") +
			list[0].Method.Name;
		}
		else
		{
			// skips 'Empty' delegate
			for (int i = 1; i < list.Length; i++)
			{
				s[i] = ((list[i].Target == null) ? "" : "(" + list[i].Target + ") ") +
				list[i].Method.Name;
			}
		}

		return s;

	}


	/// <summary>
	/// 
	/// </summary>
	private static Delegate[] RemoveDelegatesFromList(Delegate[] list)
	{

		List<Delegate> l = new List<Delegate>(list);
		for (int v = l.Count - 1; v > -1; v--)
		{
			if (l[v] == null)
				continue;

			if (l[v].Method.Name.Contains("m_"))
				l.RemoveAt(v);

		}

		return l.ToArray();

	}

	
	/// <summary>
	/// 
	/// </summary>
	private static string DumpDelegateNames(string[] array)
	{

		string s = "";
		foreach (string c in array)
		{
			if (!string.IsNullOrEmpty(c))
			{
				s +=
					((array.Length > 2) ? "\t\t\t\t\t\t\t" : "") +
					c + "\n";
			}
		}
		return s;

	}
	

}


