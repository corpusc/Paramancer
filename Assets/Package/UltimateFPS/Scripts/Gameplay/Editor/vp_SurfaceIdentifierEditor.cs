/////////////////////////////////////////////////////////////////////////////////
//
//	vp_SurfaceIdentifierEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_FootstepManager class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(vp_SurfaceIdentifier))]

public class vp_SurfaceIdentifierEditor : Editor
{

	// target component
	public vp_SurfaceIdentifier m_Component;
	
	protected static GUIStyle m_HeaderStyle = null;
	protected static GUIStyle m_SmallButtonStyle = null;
	
	static public Texture2D blankTexture
	{
		get
		{
			return EditorGUIUtility.whiteTexture;
		}
	}
	
	public static GUIStyle SmallButtonStyle
	{
		get
		{
			if (m_SmallButtonStyle == null)
			{
				m_SmallButtonStyle = new GUIStyle("Button");
				m_SmallButtonStyle.fontSize = 10;
				m_SmallButtonStyle.alignment = TextAnchor.MiddleCenter;
				m_SmallButtonStyle.margin.left = 1;
				m_SmallButtonStyle.margin.right = 1;
				m_SmallButtonStyle.padding = new RectOffset(0, 4, 0, 2);
			}
			return m_SmallButtonStyle;
		}
	}


	public static GUIStyle HeaderStyleSelected
	{
		get
		{
			if (m_HeaderStyle == null)
			{
				m_HeaderStyle = new GUIStyle("Foldout");
				m_HeaderStyle.fontSize = 11;
				m_HeaderStyle.fontStyle = FontStyle.Bold;
				
			}
			return m_HeaderStyle;
		}
	}



	/// <summary>
	/// hooks up the component object as the inspector target
	/// </summary>
	public virtual void OnEnable()
	{

		m_Component = (vp_SurfaceIdentifier)target;

	}

	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{

		GUI.color = Color.white;

		DoSurfaceTypesFoldout();

		// update
		if (GUI.changed)
		{

			EditorUtility.SetDirty(target);

		}

	}

	/// <summary>
	/// 
	/// </summary>
	public virtual void DoSurfaceTypesFoldout()
	{
		
		vp_FootstepManager[] footstepManagers = vp_FootstepManager.FootstepManagers;

		if(footstepManagers == null || footstepManagers.Length == 0)
		{
			EditorGUILayout.HelpBox("Could not find a vp_FootstepManager component in the hierarchy.", MessageType.Info);
			return;
		}
		
		vp_FootstepManager footstepManager = footstepManagers[0];
		if(footstepManager == null)
		{
			EditorGUILayout.HelpBox("Could not find a vp_FootstepManager component in the hierarchy.", MessageType.Info);
			return;
		}
		
		List<vp_FootstepManager.vp_SurfaceTypes> SurfaceTypes = footstepManager.SurfaceTypes;
		if(SurfaceTypes == null || SurfaceTypes.Count == 0)
		{
			EditorGUILayout.HelpBox("No surface types have been added to the footstep manager.", MessageType.Info);
			return;
		}
		
		string[] surfaces = new string[ SurfaceTypes.Count ];
		for (int i = 0; i < SurfaceTypes.Count; ++i)
		{

			vp_FootstepManager.vp_SurfaceTypes surface = SurfaceTypes[i];
			surfaces[i] = surface.SurfaceName;
			
		}	
		
		GUILayout.Space(15);
		
		GUILayout.BeginHorizontal();
		EditorGUILayout.Popup("Surface Type", m_Component.SurfaceID, surfaces);
		GUILayout.EndHorizontal();
		
		GUILayout.Space(15);
		
	}

}

