/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FileDialog.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a classic file dialog which identifies filenames to load and
//					save files. for use in the unity editor.
//
/////////////////////////////////////////////////////////////////////////////////


using UnityEngine;
using UnityEditor;
using System;
using System.IO;


public class vp_FileDialog : EditorWindow
{
	
	// rendering & input
	private static Vector2 m_DialogSize = new Vector2(400, 280);
	private Vector2 m_FileScrollPos;

	private struct DoubleClick
	{
		public float time;
		public int item;
	}
	private DoubleClick m_DoubleClick;

	public enum Mode
	{
		Open,
		Save
	}
	private static Mode m_Mode = Mode.Open;

	private static int m_SelectedItem;
	private enum SelectedItemType
	{
		None,
		File,
		Dir
	}
	private SelectedItemType m_SelectedItemType;
	private bool m_FilenameDirty;

	// filenames
	public string Filename;
	public string Path;
	private static string m_Extension = "";

	// execution
	public delegate void Callback(string pathFilename);
	private static Callback m_Callback;
	public static string Result = null;

	// gui styles
	private GUIStyle m_UpButtonFileStyle;
	private GUIStyle m_PathFieldLeftAlignStyle;
	private GUIStyle m_PathFieldRightAlignStyle;
	private GUIStyle m_FileStyle;
	private GUIStyle m_SelectedFileStyle;
	private GUIStyle m_DirStyle;
	private GUIStyle m_SelectedDirStyle;
	private GUIStyle m_NoteStyle;
	private bool m_GUIStylesInitialized = false;


	/// <summary>
	/// creates a dialog in 'mode' Open or Save, starting in the
	/// 'path' folder. when used, the dialog will execute the
	/// 'callback' delegate with the current path as argument.
	/// 'extension' can be used to filter for a specific type
	/// of file.
	/// </summary>
	public static void Create(Mode mode, string caption, string path, Callback callback)
	{ Create(mode, caption, path, callback, ""); }

	
	/// <summary>
	/// creates a dialog in 'mode' Open or Save, starting in the
	/// 'path' folder. when used, the dialog will execute the
	/// 'callback' delegate with the current path as argument.
	/// 'extension' can be used to filter for a specific type
	/// of file.
	/// </summary>
	public static void Create(Mode mode, string caption, string path, Callback callback, string extension)
	{
		
		if (!System.IO.Directory.Exists(path))
		{
			vp_MessageBox.Create(vp_MessageBox.Mode.OK, "Error", "Directory does not exist: '" + path + "'");
			Debug.LogError("Directory does not exist: " + path);
			return;
		}
		
		m_Mode = mode;
		m_Callback = callback;
		m_Extension = extension.Replace(".", "");

		vp_FileDialog dialog = (vp_FileDialog)EditorWindow.GetWindow(typeof(vp_FileDialog));
		if (string.IsNullOrEmpty(caption))
		{
			if (m_Mode == Mode.Open)
				dialog.title = "Open";
			if (m_Mode == Mode.Save)
				dialog.title = "Save As";
		}
		else
			dialog.title = caption;

		dialog.Filename = "";
		dialog.Path = path;

		dialog.minSize = new Vector2(m_DialogSize.x, m_DialogSize.y);
		dialog.maxSize = new Vector2(m_DialogSize.x + 1, m_DialogSize.y + 1);
		dialog.position = new Rect(
			(Screen.currentResolution.width / 2) - (m_DialogSize.x / 2),
			(Screen.currentResolution.height / 2) - (m_DialogSize.y / 2),
			m_DialogSize.x,
			m_DialogSize.y);
		dialog.Show();

		m_SelectedItem = -1;

	}


	/// <summary>
	/// draws the dialog and collects input
	/// </summary>
	void OnGUI()
	{

		// --- initialize gui styles ---
		if (!m_GUIStylesInitialized)
			InitGUIStyles();

		// --- file area ---
		GUILayout.BeginArea(new Rect(20, 20, Screen.width - 40, Screen.height - 40));

		// --- path field ---
		if (Path == Application.dataPath)
			GUI.enabled = false;
		Vector2 pathPixelSize = m_PathFieldLeftAlignStyle.CalcSize(new GUIContent(Path));
		GUIStyle pathFieldStyle = (pathPixelSize.x > Screen.width - 66) ?
									m_PathFieldRightAlignStyle : m_PathFieldLeftAlignStyle;
		GUI.TextField(new Rect(0, 0, Screen.width - 66, 20), Path, pathFieldStyle);

		// --- up button ---
		if (GUI.Button(new Rect(Screen.width - 62, 0, 22, 20), "^", m_UpButtonFileStyle))
			GoUpOneDir();
		GUI.enabled = true;

		// --- file box ---
		float height = m_Mode == Mode.Open ? Screen.height - 106 : Screen.height - 130;
		FileBox(0, 24, Screen.width - 20, height);

		GUILayout.EndArea();

		// --- action buttons ---

		Vector2 pos = m_Mode == Mode.Open ?
			new Vector2(20, Screen.height - 58) : new Vector2(20, Screen.height - 82);
		GUILayout.BeginArea(new Rect(pos.x, pos.y, Screen.width - 40, Screen.height - 40));
		GUILayout.BeginHorizontal();
		GUILayout.Space(Screen.width - 120);

		// --- filename text field ---

		if (m_Mode == Mode.Save)
		{
			m_FilenameDirty = false;
			string f = Filename;
			if(m_SelectedItemType == SelectedItemType.Dir)
				GUI.TextField(new Rect(0, 0, Screen.width - 124, 20), "");
			else
				Filename = GUI.TextField(new Rect(0, 0, Screen.width - 124, 20), Filename);
			if (Filename != f)
			{
				m_SelectedItem = -1;
				m_FilenameDirty = true;
			}
		}

		// --- execute button ---

		// grey out button if nothing is selected
		if (string.IsNullOrEmpty(Filename) && m_SelectedItemType == SelectedItemType.None)
			GUI.enabled = false;

		// the button name is contextual name (Open / Save). in a save
		// dialog, the button should say 'Open' if a folder is selected.
		string executeButtonName = m_Mode == Mode.Open ? "Open" : "Save";
		executeButtonName = (m_SelectedItemType == SelectedItemType.Dir) ? "Open" : executeButtonName;

		// the layout depends on whether this is an Open or a Save Dialog
		pos = m_Mode == Mode.Open ?
			new Vector2(Screen.width - 204, 0) : new Vector2(Screen.width - 120, 0);
		if (GUI.Button(new Rect(pos.x, pos.y, 80, 20), executeButtonName))
		{
			if (m_Mode == Mode.Open)
				DoOpen();
			else
				DoSave();
		}

		GUI.enabled = true;

		GUILayout.EndHorizontal();


		// --- cancel button ---
		pos = m_Mode == Mode.Open ?
			new Vector2(Screen.width - 120, 0) : new Vector2(Screen.width - 120, 24);
		if (GUI.Button(new Rect(pos.x, pos.y, 80, 20), "Cancel"))
			Cancel();

		GUILayout.EndArea();

	}


	/// <summary>
	/// draws a box with a list of directories and files
	/// </summary>
	private void FileBox(float x, float y, float width, float height)
	{

		// draw the box
		GUI.Box(new Rect(x, y, width - 20, height), "");

		// setup directory info
		if (!System.IO.Directory.Exists(Path))
		{
			vp_MessageBox.Create(vp_MessageBox.Mode.OK, "Error", "Directory does not exist: '" + Path + "'");
			Debug.LogError("Directory does not exist: " + Path);
			return;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(Path);
		FileInfo[] files = directoryInfo.GetFiles();
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		
		// create a scrollview for the appropriate amount of
		// files and directories
		int dirCount = directoryInfo.GetDirectories().Length;
		int fileCount = 0;
		foreach (FileInfo file in files)
		{
			if (!HasCorrectExtension(file.Name))
				continue;
			fileCount++;
		}
		float viewSize = ((dirCount * 20) + (fileCount * 20) + 20);
		m_FileScrollPos = GUI.BeginScrollView(new Rect(x + 1, y + 1, width - 20 - 2, height - 2),
									m_FileScrollPos, new Rect(0, 0, 0, viewSize), false, false);

		// reset selection
		m_SelectedItemType = SelectedItemType.None;

		// --- draw directories ---
		int yPos = 0;
		foreach (DirectoryInfo directory in directories)
		{

			// possibly set selection to this item
			GUIStyle style;
			if (m_SelectedItem == yPos)
			{
				style = m_SelectedDirStyle;
				m_SelectedItemType = SelectedItemType.Dir;
				GUI.color = new Color(0.9f, 0.9f, 1, 1);
			}
			else
			{
				style = m_DirStyle;
				GUI.color = new Color(1, 1, 1, 1);
			}

			// draw the directory name and check for input
			if (GUI.Button(new Rect(5, (yPos * 20) + 5, width - 47, 20), directory.Name, style))
			{

				// select item on single click
				m_SelectedItem = yPos;
				Filename = directory.Name;

				// open folder on double click
				if (DetectDoubleClick())
					OpenDir();

			}

			// draw the yellow folder icon (we just use a regular button for this).
			GUI.color = new Color(1, 0.94f, 0.47f, 1);
			GUI.Button(new Rect(10, (yPos * 20) + 3 + 5, 17, 14), "");
			GUI.color = Color.white;

			// if the user has manually changed the filename to the same name
			// as this item, select it (even if it has not been clicked)
			if (m_FilenameDirty)
			{
				if (directory.Name == Filename)
					m_SelectedItem = yPos;
			}
			
			yPos++;
		
		}
		
		// --- draw files ---
		foreach (FileInfo file in files)
		{

			// if we're filtering for extension, skip items with the wrong one
			if (!HasCorrectExtension(file.Name))
				continue;

			// possibly set selection to this item
			GUIStyle style;
			if (m_SelectedItem == yPos)
			{
				style = m_SelectedFileStyle;
				m_SelectedItemType = SelectedItemType.File;
				GUI.color = new Color(0.9f, 0.9f, 1, 1);
			}
			else
			{
				style = m_FileStyle;
				GUI.color = new Color(1, 1, 1, 1);
			}

			// draw the filename and check for input
			string name = file.Name;
			if (GUI.Button(new Rect(5, (yPos * 20) + 5, width - 47, 20), name, style))
			{

				// select item on single click
				m_SelectedItem = yPos;
				Filename = file.Name;

				// open or save file on double click
				if (DetectDoubleClick())
				{
					if (m_Mode == Mode.Open)
						DoOpen();
					else
						DoSave();
				}

			}

			// if the user has manually changed the filename to the same name
			// as this item, select it (even if it has not been clicked)
			if (m_FilenameDirty)
			{
				if (file.Name == Filename)
					m_SelectedItem = yPos;
			}

			yPos++;

		}
		 
		GUI.color = new Color(1, 1, 1, 1);

		GUI.EndScrollView();

	}


	/// <summary>
	/// returns true if the passed filename ends with m_Extension
	/// or if m_Extension is empty
	/// </summary>
	private bool HasCorrectExtension(string filename)
	{

		if (!string.IsNullOrEmpty(m_Extension))
		{
			if (!filename.EndsWith("." + m_Extension, StringComparison.CurrentCultureIgnoreCase))
				return false;
		}
		return true;

	}


	/// <summary>
	/// returns true if the currently selected item was double
	/// clicked
	/// </summary>
	private bool DetectDoubleClick()
	{

		// if the same item was clicked twice in the last half second ...
		if ((m_SelectedItem == m_DoubleClick.item) &&
			(System.Environment.TickCount < m_DoubleClick.time))
		{
			// ... we have detected a double click: reset click item and time
			m_DoubleClick.item = -1;
			m_DoubleClick.time = 0;
			return true;
		}

		// the selected item has only been clicked once: store the item number
		// for detection of a second click within the next half second.
		m_DoubleClick.item = m_SelectedItem;
		m_DoubleClick.time = System.Environment.TickCount + 500;

		return false;

	}


	/// <summary>
	/// interprets the user choice as an open operation and executes it
	/// </summary>
	private void DoOpen()
	{

		Result = null;

		// open directories
		if (m_SelectedItemType == SelectedItemType.Dir)
		{
			OpenDir();
			return;
		}
		
		// make sure file exists
		if (!System.IO.File.Exists(Path + "/" + Filename))
		{
			vp_MessageBox.Create(vp_MessageBox.Mode.OK, "Error", "File not found. Check the file name and try again.");
			Debug.LogError("File not found: " + Path + "/" + Filename);
			Filename = "";
			return;
		}

		// execute callback
		if (m_SelectedItemType == SelectedItemType.File)
			ExecuteCallback();

		if (!string.IsNullOrEmpty(Result))
			vp_MessageBox.Create(vp_MessageBox.Mode.OK, "Error", Result);

	}


	/// <summary>
	/// interprets the user choice as a save operation and executes it
	/// </summary>
	private void DoSave()
	{
		DoSave(false);
	}

	
	/// <summary>
	/// interprets the user choice as a save operation and executes it
	/// </summary>
	private void DoSave(bool overwriteExisting)
	{

		Result = null;

		// open directories
		if (m_SelectedItemType == SelectedItemType.Dir)
		{
			OpenDir();
			return;
		}

		// force 'File' item type for non-existing file
		if (m_SelectedItemType == SelectedItemType.None)
			m_SelectedItemType = SelectedItemType.File;

		// check for bad filename
		if (Filename.Contains("\\") ||
			Filename.Contains("/") ||
			Filename.Contains(":") ||
			Filename.Contains("*") ||
			Filename.Contains("?") ||
			Filename.Contains("\"") ||
			Filename.Contains("<") ||
			Filename.Contains(">") ||
			Filename.Contains("|"))
		{
			vp_MessageBox.Create(vp_MessageBox.Mode.OK, "Error", "A filename cannot contain any of the following characters: \\ / : * ? \" < > |");
			return;
		}

		// append extension if none present in the filename
		if (!Filename.EndsWith(m_Extension))
			Filename += "." + m_Extension;

		// execute callback
		if (m_SelectedItemType == SelectedItemType.File)
			ExecuteCallback();

		// confirm if there was some problem with an existing file, and
		// if the user so chooses, save again after clearing the file
		if (!overwriteExisting && !string.IsNullOrEmpty(Result))
		{

			vp_MessageBox.Create(vp_MessageBox.Mode.YesNo, "Confirm", Result, delegate(vp_MessageBox.Answer answer)
			{
				if (answer == vp_MessageBox.Answer.Yes)
				{
					try
					{
						File.Delete(Path + "/" + Filename);
						DoSave(true);	// execute again without warning
					}
					catch
					{
						Debug.LogError("Error: Failed to overwrite file.");
					}
				}
			});

			return;
		}

	}


	/// <summary>
	/// executes the delegate passed in the constructor, using
	/// the current path + filename as argument
	/// </summary>
	private void ExecuteCallback()
	{

		if(this != null)
			this.Close();

		if (m_Callback == null)
		{
			vp_MessageBox.Create(vp_MessageBox.Mode.OK, "Error", "Failed to perform file operation.");
			Debug.LogError("Failed to perform file operation (callback was null).");
			return;
		}

		// execute delegate
		m_Callback(Path + "/" + Filename);

	}


	/// <summary>
	/// navigates to the directory selected by the user
	/// </summary>
	private void OpenDir()
	{
		m_SelectedItem = -1;
		m_SelectedItemType = SelectedItemType.None;
		Path += "/" + Filename;
		Filename = "";
	}

	
	/// <summary>
	/// navigates to the folder above current folder
	/// </summary>
	private void GoUpOneDir()
	{

		string upPath = RemoveLastFolder(Path);

		if (System.IO.Directory.Exists(upPath))
		{
			m_SelectedItem = -1;
			Path = upPath;
			Filename = "";
		}
		else
		{
			vp_MessageBox.Create(vp_MessageBox.Mode.OK, "Error", "Directory does not exist: '" + upPath + "'");
			Debug.LogError("Directory does not exist: " + upPath);
		}

	}


	/// <summary>
	/// returns the filename present in the current path (if any)
	/// including extension
	/// </summary>
	public static string ExtractFilenameFromPath(string path)
	{

		int slash_pos = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));

		// found no slash
		if (slash_pos == -1)
			return path;

		// no file name in this path
		if (slash_pos == path.Length - 1)
			return "";

		return path.Substring(slash_pos + 1, path.Length - slash_pos - 1);

	}


	/// <summary>
	/// removes extension from a path
	/// </summary>
	public static string RemoveExtension(string str)
	{

		if (string.IsNullOrEmpty(str))
			return null;

		int e = str.LastIndexOf(".");
		if (e > 1)
			str = str.Remove(e);

		return str;

	}


	/// <summary>
	/// removes last folder from a path
	/// </summary>
	public static string RemoveLastFolder(string str)
	{

		if (string.IsNullOrEmpty(str))
			return null;

		string bak = str;

		int slash_pos = Math.Max(str.LastIndexOf('/'), str.LastIndexOf('\\'));

		// found no slash
		if (slash_pos < 2)
			return str;

		// no file name in this path
		if (slash_pos == str.Length - 1)
			return "";

		if (slash_pos > 1)
			str = str.Remove(slash_pos);

		if (System.IO.Directory.Exists(str))
			return str;

		return bak;

	}


	/// <summary>
	/// returns the filename present in the current path (if any)
	/// relative to the resources folder, excluding extension
	/// </summary>
	public static string ExtractResourcePath(string path)
	{

		int resources_pos = path.ToLower().LastIndexOf("resources");

		if (resources_pos == -1)
			return null;

		path = RemoveExtension(path);

		return path.Substring(resources_pos + 10, path.Length - resources_pos - 10);

	}


	/// <summary>
	/// closes the file dialog with no further action
	/// </summary>
	private void Cancel()
	{
		this.Close();
	}


	/// <summary>
	/// sets up some special gui styles for the dialog
	/// </summary>
	private void InitGUIStyles()
	{
		
		m_UpButtonFileStyle = new GUIStyle("Button");
		m_UpButtonFileStyle.padding.left = 2;

		m_PathFieldLeftAlignStyle = new GUIStyle("TextField");
		m_PathFieldLeftAlignStyle.alignment = TextAnchor.MiddleLeft;

		m_PathFieldRightAlignStyle = new GUIStyle("TextField");
		m_PathFieldRightAlignStyle.alignment = TextAnchor.MiddleRight;

		m_FileStyle = new GUIStyle("Label");
		m_FileStyle.padding.left = 5;
		m_FileStyle.padding.top = 4;

		m_SelectedFileStyle = new GUIStyle("Box");
		m_SelectedFileStyle.padding.left = 5;
		m_SelectedFileStyle.padding.top = 3;
		m_SelectedFileStyle.alignment = TextAnchor.MiddleLeft;

		m_DirStyle = new GUIStyle("Label");
		m_DirStyle.padding.left = 25;
		m_DirStyle.padding.top = 4;

		m_SelectedDirStyle = new GUIStyle("Box");
		m_SelectedDirStyle.padding.left = 25;
		m_SelectedDirStyle.padding.top = 3;
		m_SelectedDirStyle.alignment = TextAnchor.MiddleLeft;

		m_NoteStyle = new GUIStyle("Label");
		m_NoteStyle.fontSize = 9;

		m_GUIStylesInitialized = true;

	}


}

