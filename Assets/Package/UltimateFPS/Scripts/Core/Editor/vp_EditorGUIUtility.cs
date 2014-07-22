/////////////////////////////////////////////////////////////////////////////////
//
//	vp_EditorGUIUtility.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	helper methods for standard editor GUI tasks
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public static class vp_EditorGUIUtility
{

	private static GUIStyle m_NoteStyle = null;
	private static GUIStyle m_SmallButtonStyle = null;
	private static GUIStyle m_RightAlignedPathStyle = null;
	private static GUIStyle m_LeftAlignedPathStyle = null;
	private static GUIStyle m_CenteredBoxStyle = null;
	private static GUIStyle m_CenteredBoxStyleBold = null;
	private static GUIStyle m_CenteredStyleBold = null;

	
	/// <summary>
	/// creates a foldout button to clearly distinguish a section
	/// of controls from others
	/// </summary>
	public static bool SectionButton(string label, bool state)
	{

		GUI.color = new Color(0.9f, 0.9f, 1, 1);
		if (GUILayout.Button((state ? "- " : "+ ") + label.ToUpper(), GUILayout.Height(20)))
			state = !state;
		GUI.color = Color.white;

		return state;

	}


	/// <summary>
	/// creates a big 2-button toggle
	/// </summary>
	public static bool ButtonToggle(string label, bool state)
	{

		GUIStyle onStyle = new GUIStyle("Button");
		GUIStyle offStyle = new GUIStyle("Button");

		if (state)
			onStyle.normal = onStyle.active;
		else
			offStyle.normal = offStyle.active;

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label(label);
		if (GUILayout.Button("ON", onStyle))
			state = true;
		if (GUILayout.Button("OFF", offStyle))
			state = false;
		EditorGUILayout.EndHorizontal();

		return state;

	}


	/// <summary>
	/// creates a small toggle
	/// </summary>
	public static bool SmallToggle(string label, bool state)
	{

		EditorGUILayout.BeginHorizontal();
		state = GUILayout.Toggle(state, label, GUILayout.MaxWidth(12));
		GUILayout.Label(label, LeftAlignedPathStyle);
		EditorGUILayout.EndHorizontal();

		return state;

	}


	/// <summary>
	/// creates an editable list of unity objects
	/// </summary>
	public static void ObjectList(string caption, List<UnityEngine.Object> list, Type type)
	{

		GUILayout.BeginHorizontal();
		GUILayout.Label(caption);
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Add", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.MinHeight(15)))
			list.Add(null);
		GUILayout.Space(37);
		GUILayout.EndHorizontal();

		for (int v = 0; v < list.Count; v++)
		{
			GUILayout.BeginHorizontal();
			list[v] = EditorGUILayout.ObjectField(v.ToString(), list[v], type, false);
			if (GUILayout.Button("X", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(15), GUILayout.MaxWidth(15), GUILayout.MinHeight(15)))
				list.RemoveAt(v);
			GUILayout.EndHorizontal();
		}
		
	}

	
	/// <summary>
	/// creates a horizontal line to visually separate groups of
	/// controls
	/// </summary>
	public static void Separator()
	{

		GUI.color = new Color(1, 1, 1, 0.25f);
		GUILayout.Box("", "HorizontalSlider", GUILayout.Height(16));
		GUI.color = Color.white;

	}


	//////////////////////////////////////////////////////////
	// GUI styles
	//////////////////////////////////////////////////////////


	public static GUIStyle NoteStyle
	{
		get
		{
			if (m_NoteStyle == null)
			{
				m_NoteStyle = new GUIStyle("Label");
				m_NoteStyle.fontSize = 9;
				m_NoteStyle.alignment = TextAnchor.LowerCenter;
			}
			return m_NoteStyle;
		}
	}

	public static GUIStyle SmallButtonStyle
	{
		get
		{
			if (m_SmallButtonStyle == null)
			{
				m_SmallButtonStyle = new GUIStyle("Button");
				m_SmallButtonStyle.fontSize = 8;
				m_SmallButtonStyle.alignment = TextAnchor.MiddleCenter;
				m_SmallButtonStyle.margin.left = 1;
				m_SmallButtonStyle.margin.right = 1;
				m_SmallButtonStyle.padding = new RectOffset(0, 4, 0, 2);
			}
			return m_SmallButtonStyle;
		}
	}
		
	public static GUIStyle RightAlignedPathStyle
	{
		get
		{
			if (m_RightAlignedPathStyle == null)
			{
				m_RightAlignedPathStyle = new GUIStyle("Label");
				m_RightAlignedPathStyle.fontSize = 9;
				m_RightAlignedPathStyle.alignment = TextAnchor.LowerRight;
			}
			return m_RightAlignedPathStyle;
		}
	}

	public static GUIStyle LeftAlignedPathStyle
	{
		get
		{
			if (m_LeftAlignedPathStyle == null)
			{
				m_LeftAlignedPathStyle = new GUIStyle("Label");
				m_LeftAlignedPathStyle.fontSize = 9;
				m_LeftAlignedPathStyle.alignment = TextAnchor.LowerLeft;
				m_LeftAlignedPathStyle.padding = new RectOffset(0, 0, 2, 0);
			}
			return m_LeftAlignedPathStyle;
		}
	}

	public static GUIStyle CenteredBoxStyle
	{
		get
		{
			if (m_CenteredBoxStyle == null)
			{
				m_CenteredBoxStyle = new GUIStyle("Label");
				m_CenteredBoxStyle.fontSize = 10;
				m_CenteredBoxStyle.alignment = TextAnchor.LowerLeft;
			}
			return m_CenteredBoxStyle;
		}
	}

	public static GUIStyle CenteredStyleBold
	{
		get
		{
			if (m_CenteredStyleBold == null)
			{
				m_CenteredStyleBold = new GUIStyle("Label");
				m_CenteredStyleBold.fontSize = 10;
				m_CenteredStyleBold.alignment = TextAnchor.LowerLeft;
				m_CenteredStyleBold.fontStyle = FontStyle.Bold;
			}
			return m_CenteredStyleBold;
		}
	}

	public static GUIStyle CenteredBoxStyleBold
	{
		get
		{
			if (m_CenteredBoxStyleBold == null)
			{
				m_CenteredBoxStyleBold = new GUIStyle("TextField");
				m_CenteredBoxStyleBold.fontSize = 10;
				m_CenteredBoxStyleBold.alignment = TextAnchor.LowerLeft;
				m_CenteredBoxStyleBold.fontStyle = FontStyle.Bold;
			}
			return m_CenteredBoxStyleBold;
		}
	}


}

