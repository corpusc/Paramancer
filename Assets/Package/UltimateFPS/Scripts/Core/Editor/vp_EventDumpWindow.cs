/////////////////////////////////////////////////////////////////////////////////
//
//	vp_EventDumpWindow.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	debug class that creates event handler text dumps for use in
//					the event handler editor window or console / log output
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEditor;


public class vp_EventDumpWindow : EditorWindow
{

	static public bool m_MessagesFoldout;
	static public bool m_ValuesFoldout;
	static public bool m_AttemptsFoldout;
	static public bool m_Activity2Foldout;
	static vp_EventHandler m_EventHandler = null;

	public Vector2 m_MessageScrollPos = Vector2.zero;
	public Vector2 m_ValueScrollPos = Vector2.zero;
	public Vector2 m_AttemptScrollPos = Vector2.zero;
	public Vector2 m_ActivityScrollPos = Vector2.zero;
	public Vector2 m_Activity2ScrollPos = Vector2.zero;
	static string m_MessageDump = "";
	static string m_ValueDump = "";
	static string m_AttemptDump = "";
	static string m_ActivityDump = "";


	/// <summary>
	/// 
	/// </summary>
	public static void Create(vp_EventHandler eventHandler)
	{

		vp_EventDumpWindow window = (vp_EventDumpWindow)EditorWindow.GetWindow(typeof(vp_EventDumpWindow));

		m_EventHandler = eventHandler;

		window.title = "Event Dump";

		window.position = new Rect(
			0,
			100,
			600,
			Screen.currentResolution.height - 150);
		window.Show();

	}
	

	/// <summary>
	/// 
	/// </summary>
	void Update()
	{


		if (!EditorApplication.isPlaying)
			Close();

		if (vp_EventHandler.RefreshEditor)
		{
			Refresh();
			vp_EventHandler.RefreshEditor = false;
			this.Repaint();
		}

	}


	/// <summary>
	/// 
	/// </summary>
	void OnGUI()
	{


		GUILayout.Space(20);
		EditorGUILayout.BeginVertical();
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);

		GUILayout.Label(m_EventHandler.ToString());
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(5);

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical();

		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);

		if (GUILayout.Button("Refresh", GUILayout.MaxWidth(80)))
			Refresh();
		GUILayout.Space(20);
		EditorGUILayout.EndHorizontal();
		vp_EditorGUIUtility.Separator();
		GUILayout.Space(20);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		EditorGUILayout.BeginVertical();
		DoFoldout(ref m_MessagesFoldout, ref m_MessageScrollPos, m_MessageDump, "MESSAGES");
		DoFoldout(ref m_ValuesFoldout, ref m_ValueScrollPos, m_ValueDump, "VALUES");
		DoFoldout(ref m_AttemptsFoldout, ref m_AttemptScrollPos, m_AttemptDump, "ATTEMPTS");
		DoFoldout(ref m_Activity2Foldout, ref m_Activity2ScrollPos, m_ActivityDump, "ACTIVITIES");
		GUILayout.FlexibleSpace();
		GUILayout.Space(20);
		EditorGUILayout.EndVertical();
		GUILayout.Space(20);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();

    }


	/// <summary>
	/// 
	/// </summary>
	void DoFoldout(ref bool foldout, ref Vector2 scrollPos, string dump, string caption)
	{

		foldout = EditorGUILayout.Foldout(foldout, caption);

		if (!foldout)
			return;

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		GUILayout.Space(20);
		GUILayout.TextArea(dump, "Label");
		EditorGUILayout.EndScrollView();
		vp_EditorGUIUtility.Separator();

	}


	/// <summary>
	/// 
	/// </summary>
	public void Refresh()
	{

		m_MessageDump = vp_EventDump.Dump(m_EventHandler, new string[] { "vp_Message" });
		m_ValueDump = vp_EventDump.Dump(m_EventHandler, new string[] { "vp_Value" });
		m_AttemptDump = vp_EventDump.Dump(m_EventHandler, new string[] { "vp_Attempt" });
		m_ActivityDump = vp_EventDump.Dump(m_EventHandler, new string[] { "vp_Activity" });

	}


}


