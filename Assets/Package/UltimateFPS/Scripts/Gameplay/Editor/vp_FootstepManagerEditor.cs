/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FootstepManagerEditor.cs
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

[CustomEditor(typeof(vp_FootstepManager))]

public class vp_FootstepManagerEditor : Editor
{

	// target component
	public vp_FootstepManager m_Component;

	// foldouts
	public static bool m_SurfaceTypesFoldout;
	
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

		m_Component = (vp_FootstepManager)target;
		m_Component.SetDirty(true);

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
		
		if (m_Component.SurfaceTypes != null)
		{
			for (int i = 0; i < m_Component.SurfaceTypes.Count; ++i)
			{

				vp_FootstepManager.vp_SurfaceTypes surface = m_Component.SurfaceTypes[i];
				
				GUILayout.BeginHorizontal();
				GUILayout.Space(20);
				surface.Foldout = EditorGUILayout.Foldout(surface.Foldout, surface.SurfaceName);
				if(i > 0)
				{
					if (GUILayout.Button("^", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(15), GUILayout.MaxWidth(15), GUILayout.MinHeight(15)))
					{
						int newIndex = i-1;
						List<vp_FootstepManager.vp_SurfaceTypes> newSurfaces = new List<vp_FootstepManager.vp_SurfaceTypes>();
						for(int x = 0; x < m_Component.SurfaceTypes.Count; x++)
						{
							vp_FootstepManager.vp_SurfaceTypes surf = m_Component.SurfaceTypes[x];
							if(x == newIndex)
							{
								newSurfaces.Add(surface);
								newSurfaces.Add(surf);
							}
							else if(surf != surface)
								newSurfaces.Add(surf);
						}
						m_Component.SurfaceTypes = newSurfaces;
						m_Component.SetDirty(true);
						return;
					}
					GUILayout.Space(5);
				}
				
				if (GUILayout.Button("Remove", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.MinHeight(15)))
				{
					m_Component.SurfaceTypes.RemoveAt(i);
					--i;
				}
				GUI.backgroundColor = Color.white;
				
				GUILayout.Space(20);

				GUILayout.EndHorizontal();
				
				GUILayout.Space(5);
				
				if(surface.Foldout)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Space(35);
					if(surface.SurfaceName == "")
						surface.SurfaceName = "Surface "+(i+1);
					surface.SurfaceName = EditorGUILayout.TextField("Surface Name", surface.SurfaceName, GUILayout.MaxWidth(250));
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					GUILayout.Space(35);
					surface.RandomPitch = EditorGUILayout.Vector2Field("Random Pitch", surface.RandomPitch);
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					GUILayout.Space(38);

					if (surface.SoundsFoldout)
						surface.SoundsFoldout = EditorGUILayout.Foldout(surface.SoundsFoldout, "Sounds", HeaderStyleSelected);
					else
						surface.SoundsFoldout = EditorGUILayout.Foldout(surface.SoundsFoldout, "Sounds");
					GUILayout.EndHorizontal();
					
					if(surface.SoundsFoldout)
					{
						if(surface.Sounds != null)
						{
							if(surface.Sounds.Count > 0)
							{
								for (int x = 0; x < surface.Sounds.Count; ++x)
								{
									GUILayout.BeginHorizontal();
									GUILayout.Space(50);
									surface.Sounds[x] = (AudioClip)EditorGUILayout.ObjectField("", surface.Sounds[x], typeof(AudioClip), false);
									if (surface.Sounds[x] == null)
										GUI.enabled = false;
									if (GUILayout.Button(">", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(15), GUILayout.MaxWidth(15), GUILayout.MinHeight(15)))
									{
										AudioSource audio = m_Component.transform.root.GetComponentInChildren<AudioSource>();
										if (audio != null)
											audio.PlayOneShot(surface.Sounds[x]);
									}
									GUI.enabled = true;
									if (GUILayout.Button("X", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(15), GUILayout.MaxWidth(15), GUILayout.MinHeight(15)))
									//									if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(15)))
									{
										surface.Sounds.RemoveAt(x);
										m_Component.SetDirty(true);
										--x;
									}
									GUI.backgroundColor = Color.white;
									GUILayout.Space(20);

									GUILayout.EndHorizontal();
								}
							}
						}
						
						if(surface.Sounds.Count == 0)
						{
							GUILayout.BeginHorizontal();
							GUILayout.Space(50);
							EditorGUILayout.HelpBox("There are no sounds. Click the \"Add Sound\" button to add a sound.", MessageType.Info);
							GUILayout.Space(20);
							GUILayout.EndHorizontal();
						}
						
						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						if (GUILayout.Button("Add Sound", GUILayout.MinWidth(90), GUILayout.MaxWidth(90)))
						{
							AudioClip clip = new AudioClip();
							surface.Sounds.Add(clip);
							m_Component.SetDirty(true);
						}
						GUI.backgroundColor = Color.white;
						GUILayout.EndHorizontal();
						vp_EditorGUIUtility.Separator();

					}
					
					GUILayout.BeginHorizontal();
					GUILayout.Space(38);

					if (surface.TexturesFoldout)
						surface.TexturesFoldout = EditorGUILayout.Foldout(surface.TexturesFoldout, "Textures", HeaderStyleSelected);
					else
						surface.TexturesFoldout = EditorGUILayout.Foldout(surface.TexturesFoldout, "Textures");

					GUILayout.EndHorizontal();
					
					if(surface.TexturesFoldout)
					{
						if(surface.Textures != null)
						{
							if(surface.Textures.Count > 0)
							{
								int counter = 0;
								for (int x = 0; x < surface.Textures.Count; ++x)
								{
									if(counter == 0)
									{
										GUILayout.BeginHorizontal(GUILayout.MinHeight(100));
										GUILayout.Space(50);
									}
									
									GUILayout.BeginVertical(GUILayout.MinHeight(90));
									surface.Textures[x] = (Texture)EditorGUILayout.ObjectField(surface.Textures[x], typeof(Texture), false, GUILayout.MinWidth(50), GUILayout.MaxWidth(75), GUILayout.MinHeight(50), GUILayout.MaxHeight(75));

									if (GUILayout.Button("Delete", GUILayout.MinWidth(50), GUILayout.MaxWidth(75)))
									{
										surface.Textures.RemoveAt(x);
										m_Component.SetDirty(true);
										--x;
									}
									GUI.backgroundColor = Color.white;
									GUILayout.EndVertical();
									
									counter++;
									
									if(counter == 4 || x == surface.Textures.Count - 1)
									{
										GUILayout.Space(20);

										GUILayout.EndHorizontal();
										counter = 0;
									}
								}
							}
						}
						
						if(surface.Textures.Count == 0)
						{
							GUILayout.BeginHorizontal();
							GUILayout.Space(50);
							EditorGUILayout.HelpBox("There are no textures. Click the \"Add Texture\" button to add a texture.", MessageType.Info);
							GUILayout.Space(20);
							GUILayout.EndHorizontal();
						}
						
						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						if (GUILayout.Button("Add Texture", GUILayout.MinWidth(90), GUILayout.MaxWidth(90)))
						{
							Texture texture = new Texture();
							surface.Textures.Add(texture);
							m_Component.SetDirty(true);
						}
						GUI.backgroundColor = Color.white;
						GUILayout.EndHorizontal();
					}
					
					DrawSeparator();
					
					GUILayout.Space(5);
				}
			}
		}
		
		if(m_Component.SurfaceTypes.Count == 0)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(50);
			EditorGUILayout.HelpBox("There are no Surface Types. Click the \"Add Surface Type\" button to add a new surface type.", MessageType.Info);
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
		}
		
		GUILayout.Space(8f);
		
		GUILayout.BeginHorizontal();

		if (GUILayout.Button("Add Surface Type", GUILayout.MinWidth(150), GUILayout.MinHeight(25)))
		{
			vp_FootstepManager.vp_SurfaceTypes surface = new vp_FootstepManager.vp_SurfaceTypes();
			m_Component.SurfaceTypes.Add(surface);
		}
		GUI.backgroundColor = Color.white;
		GUILayout.EndHorizontal();
			
		DrawSeparator();
		
	}
	
	static public void DrawSeparator ()
	{
		
		GUILayout.Space(12f);

		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = blankTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 10f, Screen.width, 4f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 10f, Screen.width, 1f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 13f, Screen.width, 1f), tex);
			GUI.color = Color.white;
		}
		
	}
	

}

