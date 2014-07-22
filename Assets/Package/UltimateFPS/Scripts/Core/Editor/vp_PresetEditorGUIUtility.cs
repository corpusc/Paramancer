/////////////////////////////////////////////////////////////////////////////////
//
//	vp_PresetEditorGUIUtility.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	editor gui helper methods for working with states & presets
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class vp_PresetEditorGUIUtility
{

	private static Color m_ColorGrayYellow = new Color32(180, 201, 228, 255);
	private static Color m_ColorTransparentWhite = new Color(1, 1, 1, 0.5f);

	private static vp_State m_ShowBlockListFor = null;

	static Dictionary<string, List<string>> m_BlockListBackups;


	/// <summary>
	/// draws a check box to toggle persist state ON / OFF for a
	/// component. will be disabled if the 'Default' state has
	/// been overridden
	/// </summary>
	public static void PersistToggle(vp_ComponentPersister persister)
	{

		bool oldPersistState = persister.Component.Persist;
		GUI.color = Color.white;
		if (persister.Component.DefaultState.TextAsset != null)
		{
			persister.Component.Persist = false;
			GUI.color = m_ColorTransparentWhite;
		}

		persister.Component.Persist = vp_EditorGUIUtility.SmallToggle("Persist Play Mode Changes", persister.Component.Persist);
		if (persister.Component.DefaultState.TextAsset != null && persister.Component.Persist == true)
		{
			string s = "Can't Persist Play Mode Changes when the 'Default' state has been overridden with a text file preset.";
			if (!Application.isPlaying)
				s += "\n\nClick 'Unlock' to reenable inspector changes to this component.";
			vp_MessageBox.Create(vp_MessageBox.Mode.OK, "Locked", s);
			persister.Component.Persist = false;
		}

		if (oldPersistState != persister.Component.Persist)
			persister.Persist();
		GUI.color = Color.white;
	}


	/// <summary>
	/// draws a foldout with buttons to load and save a preset
	/// </summary>
	public static bool PresetFoldout(bool foldout, vp_Component component)
	{

		foldout = EditorGUILayout.Foldout(foldout, "Preset");
		if (foldout)
		{

			GUI.enabled = true;
			EditorGUILayout.BeginHorizontal();
			if (component.DefaultState.TextAsset != null)
				GUI.enabled = false;
			if (GUILayout.Button("Load"))
				ShowLoadDialog(component);
			GUI.enabled = true;
			if (GUILayout.Button("Save"))
				ShowSaveDialog(component, false);
			if (!Application.isPlaying)
				GUI.enabled = false;
			if (GUILayout.Button("Save Tweaks"))
				ShowSaveDifferenceDialog(component);
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();

		}
		return foldout;

	}


	/// <summary>
	/// opens a dialog for loading presets
	/// </summary>
	static private void ShowLoadDialog(vp_Component component)
	{

		string path = Application.dataPath.Replace("\\", "/");
		vp_FileDialog.Create(vp_FileDialog.Mode.Open, "Load Preset", path, delegate(string filename)
		{
			if (!vp_ComponentPreset.Load(component, filename))
				vp_FileDialog.Result = "Failed to load preset '" + vp_FileDialog.ExtractFilenameFromPath(filename) + "'.\n\nIs it the correct component type? (" + component.GetType().ToString() + ")";
			else
				EditorUtility.SetDirty(component);
		}, ".txt");

	}


	/// <summary>
	/// opens a dialog for saving presets
	/// </summary>
	static private void ShowSaveDialog(vp_Component component)
	{
		ShowSaveDialog(component, false);
	}
	static private void ShowSaveDialog(vp_Component component, bool showLoadDialogAfterwards)
	{
		string path = Application.dataPath;

		vp_FileDialog.Create(vp_FileDialog.Mode.Save, "Save Preset", path, delegate(string filename)
		{
			vp_FileDialog.Result = vp_ComponentPreset.Save(component, filename);

			if (showLoadDialogAfterwards)
				ShowLoadDialog(component);

		}, ".txt");


	}


	/// <summary>
	/// opens a dialog for saving presets
	/// </summary>
	static private void ShowSaveDifferenceDialog(vp_Component component)
	{

		string path = Application.dataPath;

		// LORE: in order not to overwrite existing values in a disk
		// preset, we'll load the disk preset before saving over it.
		// since the file dialog system will execute its callback
		// twice in case of an already existing file (and delete the
		// target file in the process) we need to store the preset
		// in memory outside of the callback and skip loading it on
		// the second iteration
		vp_ComponentPreset diskPreset = null;

		vp_FileDialog.Create(vp_FileDialog.Mode.Save, "Save Tweaks", path, delegate(string filename)
		{

			// only attempt to load the disk preset the first time
			// the callback is executed
			if (diskPreset == null)
			{
				diskPreset = new vp_ComponentPreset();
				// attempt to load target preset into memory, ignoring
				// load errors in the process
				bool logErrorState = vp_ComponentPreset.LogErrors;
				vp_ComponentPreset.LogErrors = false;
				diskPreset.LoadTextStream(filename);
				vp_ComponentPreset.LogErrors = logErrorState;
			}

			vp_FileDialog.Result = vp_ComponentPreset.SaveDifference(component.InitialState.Preset, component, filename, diskPreset);

		}, ".txt");

	}


	/// <summary>
	/// draws an info text replacing the component controls, in
	/// case the component's 'Default' state has been overridden
	/// </summary>
	public static void DefaultStateOverrideMessage()
	{

		GUI.enabled = false;
		GUILayout.Label("\n'Default' state has been overridden by a text preset. This\ncomponent can now be modified at runtime only, and changes\nwill revert when the app stops (presets can still be saved).\nUnlock the 'Default' state below to re-enable editing.\n", vp_EditorGUIUtility.NoteStyle);
		GUI.enabled = true;

	}


	/// <summary>
	/// draws a field allowing the user to create, reorganize,
	/// name, assign presets to and delete states on a component
	/// </summary>
	public static bool StateFoldout(bool foldout, vp_Component component, List<vp_State> stateList, vp_ComponentPersister persister = null)
	{

		bool before = foldout;
		foldout = EditorGUILayout.Foldout(foldout,
			(foldout && !Application.isPlaying) ? "State             Preset" : "States"
			);

		if (foldout != before)
		{
			m_ShowBlockListFor = null;
			component.RefreshDefaultState();
		}

		if (foldout)
		{
			if (m_ShowBlockListFor != null)
			{
				if (!stateList.Contains(m_ShowBlockListFor))
					foldout = false;
			}
		}

		if (foldout)
		{

			for (int v = 0; v < stateList.Count; v++)
			{
				vp_State s = stateList[v];
				if (!Application.isPlaying)
				{
					vp_PresetEditorGUIUtility.StateField(s, stateList, component);
					if ((m_ShowBlockListFor != null) && m_ShowBlockListFor == s)
					{
						StateBlockList(component, s);
					}
				}
				else
				{
					vp_PresetEditorGUIUtility.RunTimeStateField(component, s, stateList);
				}
			}

			GUILayout.BeginHorizontal();
			if (!Application.isPlaying)
			{
				if (GUILayout.Button("Add State", GUILayout.MinWidth(90), GUILayout.MaxWidth(90)))
				{
					m_ShowBlockListFor = null;
					string newStateName = "Untitled";
					int n = 1;
					while (GetStateId(component, newStateName) != -1)
					{
						n++;
						newStateName = newStateName.Substring(0, 8) + (n<10?"0":"") + n.ToString();
					}
					
					stateList.Add(new vp_State(component.GetType().Name, newStateName, ""));
					component.RefreshDefaultState();
					EditorUtility.SetDirty(component);
				}
			}
			else
			{
				GUI.color = Color.clear;
				GUILayout.Button("", GUILayout.MinWidth(36), GUILayout.MaxWidth(36));
				GUI.color = Color.white;
			}
			if (!Application.isPlaying)
				GUILayout.EndHorizontal();
			if (persister != null)
				vp_PresetEditorGUIUtility.PersistToggle(persister);
			if (Application.isPlaying)
				GUILayout.EndHorizontal();

			vp_EditorGUIUtility.Separator();

		}

		return foldout;

	}


	/// <summary>
	/// 
	/// </summary>
	static void StateBlockList(vp_Component component, vp_State blocker)
	{

		GUILayout.BeginHorizontal();
		GUILayout.Space(20);
		GUILayout.BeginVertical();

		string componentName = component.GetType().ToString();
		if (componentName.Contains("vp_"))
			componentName = componentName.Substring(3);

		EditorGUILayout.HelpBox("'" + blocker.Name + "' blocks " + ((blocker.StatesToBlock.Count > 0) ? blocker.StatesToBlock.Count.ToString() : "no") + " state" + ((blocker.StatesToBlock.Count == 1) ? "" : "s") + " on this " + componentName + ".", MessageType.None);

		GUILayout.BeginVertical();
		int e = 0;
		foreach (vp_State blockee in component.States)
		{

			if (blockee == blocker)
				continue;

			if (blockee.Name == "Default")
				continue;

			int i = component.States.IndexOf(blockee);

			if (component.States[i].StatesToBlock.Contains(component.States.IndexOf(blocker)))
				GUI.enabled = false;

			if (e % 2 == 0)
			GUILayout.BeginHorizontal();
			GUILayout.Space(20);
			bool before = blocker.StatesToBlock.Contains(i);
			bool after = before;

			after = GUILayout.Toggle(after, blockee.Name);

			if(before != after)
			{
				if(!before)
					blocker.StatesToBlock.Add(i);
				else
					blocker.StatesToBlock.Remove(i);

				EditorUtility.SetDirty(component);
			}
			if (e % 2 == 1)
			{
				GUILayout.Space(10);
				GUILayout.EndHorizontal();
			}

			e++;
			GUI.enabled = true;

		}
		if (e % 2 == 1)
			GUILayout.EndHorizontal();

		GUILayout.EndVertical();
	
		GUILayout.BeginHorizontal();
		EditorGUILayout.HelpBox("Select states to be disallowed on this " + componentName + " while the '" + blocker.Name + "' state is enabled. A state can not block itself, a state that blocks it or the Default state.", MessageType.Info);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Close", GUILayout.MinWidth(100), GUILayout.MaxWidth(50)))
		{
			m_ShowBlockListFor = null;
			EditorUtility.SetDirty(component);
		}

		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.Space(10);
		GUILayout.EndHorizontal();
		GUILayout.Space(10);

	}


	/// <summary>
	/// draws a button showing if a state is on or off, allowing
	/// the user to toggle states at runtime. will also show
	/// a text saying if the state is currently disallowed
	/// </summary>
	public static void RunTimeStateField(vp_Component component, vp_State state, List<vp_State> stateList)
	{

		EditorGUILayout.BeginHorizontal();

		GUI.color = m_ColorTransparentWhite;
		if (!state.Enabled)
		{
			GUILayout.Space(20);
			GUI.enabled = true;
			GUILayout.Label((stateList.Count - stateList.IndexOf(state) - 1).ToString() + ":", vp_EditorGUIUtility.RightAlignedPathStyle, GUILayout.MinWidth(20), GUILayout.MaxWidth(20));
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(state.Name, vp_EditorGUIUtility.CenteredBoxStyle, GUILayout.MinWidth(90), GUILayout.MaxWidth(90)))
			{
				vp_Component[] compos = component.gameObject.GetComponentsInChildren<vp_Component>();
				foreach (vp_Component c in compos)
				{
					c.StateManager.SetState(state.Name, true);
					c.Refresh();
				}
			}
			GUILayout.EndHorizontal();
			GUI.color = m_ColorTransparentWhite;
		}
		else
		{
			GUILayout.Space(20);
			GUILayout.Label((stateList.Count - stateList.IndexOf(state) - 1).ToString() + ":", vp_EditorGUIUtility.RightAlignedPathStyle, GUILayout.MinWidth(20), GUILayout.MaxWidth(20));
			if (GUILayout.Button(state.Name,
				((!state.Blocked) ? vp_EditorGUIUtility.CenteredBoxStyleBold : vp_EditorGUIUtility.CenteredStyleBold)
				, GUILayout.MinWidth(90), GUILayout.MaxWidth(90)))
			{
				vp_Component[] compos = component.gameObject.GetComponentsInChildren<vp_Component>();
				foreach (vp_Component c in compos)
				{
					c.StateManager.SetState(state.Name, false);
					c.Refresh();
				}
			}
		}
		if (state.Name != "Default")
		{
			if (state.Blocked)
				GUILayout.TextField("(Blocked on " + component.GetType().ToString() + "(" + state.BlockCount + "))", vp_EditorGUIUtility.NoteStyle, GUILayout.MinWidth(100));

			if ((state.TextAsset == null) && (!state.Blocked))
				GUILayout.TextField("(No preset)", vp_EditorGUIUtility.NoteStyle, GUILayout.MinWidth(100));
		}

		EditorGUILayout.EndHorizontal();

	}


	/// <summary>
	/// draws a row displaying a preset state name, a path and
	/// buttons for browsing the path + deleting the state
	/// </summary>
	public static void StateField(vp_State state, List<vp_State> stateList, vp_Component component)
	{

		GUI.enabled = !Application.isPlaying;	// only allow preset field interaction in 'stopped' mode

		EditorGUILayout.BeginHorizontal();

		string orig = state.Name;
		if (state.Name == "Default")
		{
			GUI.enabled = false;
			EditorGUILayout.TextField(state.Name, GUILayout.MinWidth(90), GUILayout.MaxWidth(90));
			GUI.enabled = true;
		}
		else
		{
			if (m_ShowBlockListFor != null)
			{
				if (!component.States.Contains(m_ShowBlockListFor))
					m_ShowBlockListFor = null;
				else if (m_ShowBlockListFor.StatesToBlock.Contains(component.States.IndexOf(state)))
					GUI.color = m_ColorGrayYellow;
			}
			state.Name = EditorGUILayout.TextField(state.Name, GUILayout.MinWidth(90), GUILayout.MaxWidth(90));
			GUI.color = Color.white;
		}

		if (orig != state.Name)
		{

			int collisions = -1;
			foreach (vp_State s in stateList)
			{
				if (s.Name == state.Name)
					collisions++;
			}

			if (state.Name == "Default")
			{
				vp_MessageBox.Create(vp_MessageBox.Mode.OK, "Error", "'Default' is a reserved state name.");
				state.Name = orig;
			}
			else if (state.Name.Length == 0)
			{
				vp_MessageBox.Create(vp_MessageBox.Mode.OK, "Error", "State name can't be empty.");
				state.Name = orig;
			}
			else if (collisions > 0)
			{
				vp_MessageBox.Create(vp_MessageBox.Mode.OK, "Error", "There is already a state named '" + state.Name +"'.\nTIP: If you need a similar state name, begin by adding numbers at the end." );
				state.Name = orig;
			}
			else
				EditorUtility.SetDirty(component);
		}

		PresetField(state);

		if (state.Name == "Default")
		{
			if (state.TextAsset == null)
			{
				GUI.enabled = false;
				GUILayout.TextField("(Inspector)", vp_EditorGUIUtility.NoteStyle, GUILayout.MinWidth(60));
			}
			else
			{
				GUI.enabled = true;
				if (GUILayout.Button("Unlock", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(30), GUILayout.MinHeight(15)))
				{
					state.TextAsset = null;
					EditorUtility.SetDirty(component);
				}
			}
		}
		else
		{
			if (stateList.IndexOf(state) == 0)
				GUI.enabled = false;

			GUI.SetNextControlName("state");
			if (GUILayout.Button("^", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(15), GUILayout.MaxWidth(15), GUILayout.MinHeight(15)))
			{
				BackupBlockLists(component);
				int i = stateList.IndexOf(state);
				if (i != 0)
				{
					stateList.Remove(state);
					stateList.Insert(i - 1, state);
				}
				RestoreBlockLists(component);

				// focus this button to get rid of possible textfield focus,
				// or the textfields won't update properly when moving state
				GUI.FocusControl("state");
				EditorUtility.SetDirty(component);
			}

			GUI.enabled = true;

			if (state.StatesToBlock != null)
			{
				if ((state.StatesToBlock.Count > 0) && ((m_ShowBlockListFor == null) || (!component.States.Contains(m_ShowBlockListFor)) || m_ShowBlockListFor == state))
					GUI.color = m_ColorGrayYellow;
			}

			GUI.enabled = (component.States.Count > 2);
			if (GUILayout.Button("B", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(15), GUILayout.MaxWidth(15), GUILayout.MinHeight(15)))
			{
				if (m_ShowBlockListFor == state)
					m_ShowBlockListFor = null;
				else
					m_ShowBlockListFor = state;
				EditorUtility.SetDirty(component);
			}
			GUI.enabled = true;
			GUI.color = Color.white;

			if (GUILayout.Button("X", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(15), GUILayout.MaxWidth(15), GUILayout.MinHeight(15)))
			{
				vp_MessageBox.Create(vp_MessageBox.Mode.YesNo, "Confirm", "Are you sure you want to delete the state '" + state.Name + "'?", delegate(vp_MessageBox.Answer answer)
				{
					if (answer == vp_MessageBox.Answer.Yes)
					{
						BackupBlockLists(component);
						stateList.Remove(state);
						RestoreBlockLists(component);
						EditorUtility.SetDirty(component);
					}
				});

				EditorUtility.SetDirty(component);
			}

		}

		GUI.enabled = true;

		EditorGUILayout.EndHorizontal();

	}
		

	/// <summary>
	/// fills a dictionary with all the states as string keys
	/// (instead of objects) and their block lists as string-list
	/// (instead of int-list) values
	/// </summary>
	static void BackupBlockLists(vp_Component component)
	{

		m_BlockListBackups = new Dictionary<string, List<string>>();
		foreach (vp_State blocker in component.States)
		{
			List<string> blockees = new List<string>();
			foreach (int i in blocker.StatesToBlock)
			{
				string blockee = GetStateName(component, i);
				if(blockee != null)
					blockees.Add(blockee);
			}

			if (!m_BlockListBackups.ContainsKey(blocker.Name))
				m_BlockListBackups.Add(blocker.Name, blockees);

		}

	}


	/// <summary>
	/// 
	/// </summary>
	public static string GetStateName(vp_Component component, int id)
	{
		if (id == -1)
			return null;
		return component.States[id].Name;
	}



	/// <summary>
	/// 
	/// </summary>
	public static int GetStateId(vp_Component component, string state)
	{

		for (int v = 0; v < component.States.Count; v++)
		{
			if (component.States[v].Name == state)
				return v;
		}
		return -1;

	}



	/// <summary>
	/// 
	/// </summary>
	public static vp_State GetState(vp_Component component, string state)
	{

		foreach (vp_State s in component.States)
		{
			if (s.Name == state)
				return s;
		}
		return null;

	}
	

	/// <summary>
	/// restores the int block list onto all states by name, by
	/// fetching block lists in string-list format from the
	/// backup dictionary and converting them back to int-lists
	/// to be set on the components
	/// </summary>
	static void RestoreBlockLists(vp_Component component)
	{

		foreach (string s in m_BlockListBackups.Keys)
		{
			vp_State blocker = GetState(component, s);
			if (blocker == null)
				continue;

			List<string> stringBlockList;
			if (!m_BlockListBackups.TryGetValue(s, out stringBlockList))
				continue;

			List<int> intBlockList = new List<int>();
			foreach (string b in stringBlockList)
			{
				int blockee = GetStateId(component, b);
				if (blockee != -1)
					intBlockList.Add(GetStateId(component, b));
			}

			blocker.StatesToBlock = intBlockList;

		}

	}


	/// <summary>
	/// draws a slot to which the user can drag a preset TextAsset
	/// </summary>
	private static void PresetField(vp_State state)
	{

		TextAsset orig = state.TextAsset;
		state.TextAsset = (TextAsset)EditorGUILayout.ObjectField(state.TextAsset, typeof(TextAsset), false);
		if (state.TextAsset != orig)
		{
			if (state.TextAsset != null)
			{
				if ((vp_ComponentPreset.GetFileTypeFromAsset(state.TextAsset) == null ||
				vp_ComponentPreset.GetFileTypeFromAsset(state.TextAsset).Name != state.TypeName))
				{
					vp_MessageBox.Create(vp_MessageBox.Mode.OK, "Error", "Error: The file '" + state.TextAsset.name + " ' does not contain a preset of type '" + state.TypeName + "'.");
					state.TextAsset = orig;
					return;
				}
			}
		}

	}


	/// <summary>
	/// draws a disabled preset field for the built-in default
	/// state, making the user aware of the state
	/// </summary>
	public static void DefaultStateField(vp_State state)
	{

		EditorGUILayout.BeginHorizontal();

		GUI.enabled = false;
		GUILayout.Label("Default", "Textfield", GUILayout.MinWidth(90), GUILayout.MaxWidth(90));

		PresetField(state);

		GUILayout.TextField("(editor)", vp_EditorGUIUtility.NoteStyle, GUILayout.MinWidth(100));

		EditorGUILayout.Space();
		GUI.color = Color.clear;
		GUILayout.Button("...", GUILayout.MinWidth(24), GUILayout.MaxWidth(24));
		GUILayout.Button("X", GUILayout.MinWidth(24), GUILayout.MaxWidth(24));
		GUI.color = Color.white;
		GUI.enabled = true;

		EditorGUILayout.EndHorizontal();

	}


}


