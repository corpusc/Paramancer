/////////////////////////////////////////////////////////////////////////////////
//
//	vp_ComponentPreset.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	loads and saves component field values to text scripts.
//					NOTE: assets (such as 3d objects and sounds) can not be
//					loaded or saved via text presets (since the  project
//					AssetDatabase is not accessible in a build).
//					don't rely on presets for content - always make sure the
//					Inspector slots are set.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

public sealed class vp_ComponentPreset
{

	private static string m_FullPath = null;				// path to save or load from
	private static int m_LineNumber = 0;					// the current line being read
	private static Type m_Type = null;						// for extracting type and field info
	public static bool LogErrors = true;					// whether to output errors to the console
	private static ReadMode m_ReadMode = ReadMode.Normal;	// whether we are currently reading commands or comments
	private enum ReadMode
	{
		Normal,
		LineComment,
		BlockComment
	}

	private Type m_ComponentType = null;				// the type of the monobehaviour being loaded or saved
	public Type ComponentType { get { return m_ComponentType; } }
	private List<Field> m_Fields = new List<Field>();	// a list of all the component's parameters and their data

	// this class holds information about one parameter of the
	// current component: its type and its current data
	private class Field
	{
		public RuntimeFieldHandle FieldHandle;
		public object Args = null;
		public Field(RuntimeFieldHandle fieldHandle, object args)
		{
			FieldHandle = fieldHandle;
			Args = args;
		}
	}

	
	/// <summary>
	/// saves every supported field of 'preset' to text at 'fullPath'
	/// </summary>
	public static string Save(Component component, string fullPath)
	{
		vp_ComponentPreset preset = new vp_ComponentPreset();
		preset.InitFromComponent(component);
		return Save(preset, fullPath);
	}


	/// <summary>
	/// saves every supported field of 'preset' to text at 'fullPath'
	/// </summary>
	public static string Save(vp_ComponentPreset savePreset, string fullPath, bool isDifference = false)
	{

		m_FullPath = fullPath;
		
		// if the targeted file already exists, we take a look
		// at it to see if it has the same type as 'component'

		// attempt to load target preset into memory, ignoring
		// load errors in the process
		bool logErrorState = LogErrors;
		LogErrors = false;
		vp_ComponentPreset preset = new vp_ComponentPreset();
		preset.LoadTextStream(m_FullPath);
		LogErrors = logErrorState;

		// if we got hold of a preset and a component type from
		// the file, confirm overwrite
		if (preset != null)
		{
			if (preset.m_ComponentType != null)
			{
				// warn user if the type is not same as the passed 'component'
				if (preset.ComponentType != savePreset.ComponentType)
					return ("'" + ExtractFilenameFromPath(m_FullPath) + "' has the WRONG component type: " + preset.ComponentType.ToString() + ".\n\nDo you want to replace it with a " + savePreset.ComponentType.ToString() + "?");
				// confirm that the user does in fact want to overwrite this file
				if (System.IO.File.Exists(m_FullPath))
				{
					if (isDifference)
						return ("This will update '" + ExtractFilenameFromPath(m_FullPath) + "' with only the values modified since pressing Play or setting a state.\n\nContinue?");
					else
						return ("'" + ExtractFilenameFromPath(m_FullPath) + "' already exists.\n\nDo you want to replace it?");
				}
			}
			// if we end up here there was a file but it didn't make sense, so confirm overwrite
			if (System.IO.File.Exists(m_FullPath))
				return ("'" + ExtractFilenameFromPath(m_FullPath) + "' has an UNKNOWN component type.\n\nDo you want to replace it?");
		}
		
		// go ahead and save 'component' to the text file

		ClearTextFile();

		Append("///////////////////////////////////////////////////////////");
		Append("// Component Preset Script");
		Append("///////////////////////////////////////////////////////////\n");

		// append component type
		Append("ComponentType " + savePreset.ComponentType.Name);

		// scan component for all its fields. NOTE: any types
		// to be supported must be included here.

		string prefix;
		string value;

		foreach (Field f in savePreset.m_Fields)
		{

			prefix = "";
			value = "";
			FieldInfo fi = FieldInfo.GetFieldFromHandle(f.FieldHandle);


				if (fi.FieldType == typeof(float))
					value = String.Format("{0:0.#######}", ((float)f.Args));
				else if (fi.FieldType == typeof(Vector4))
				{
					Vector4 val = ((Vector4)f.Args);
					value = String.Format("{0:0.#######}", val.x) + " " +
							String.Format("{0:0.#######}", val.y) + " " +
							String.Format("{0:0.#######}", val.z) + " " +
							String.Format("{0:0.#######}", val.w);
				}
				else if (fi.FieldType == typeof(Vector3))
				{
					Vector3 val = ((Vector3)f.Args);
					value = String.Format("{0:0.#######}", val.x) + " " +
							String.Format("{0:0.#######}", val.y) + " " +
							String.Format("{0:0.#######}", val.z);
				}
				else if (fi.FieldType == typeof(Vector2))
				{
					Vector2 val = ((Vector2)f.Args);
					value = String.Format("{0:0.#######}", val.x) + " " +
							String.Format("{0:0.#######}", val.y);
				}
				else if (fi.FieldType == typeof(int))
					value = ((int)f.Args).ToString();
				else if (fi.FieldType == typeof(bool))
					value = ((bool)f.Args).ToString();
				else if (fi.FieldType == typeof(string))
					value = ((string)f.Args);
				else
				{
					prefix = "//";
					value = "<NOTE: Type '" + fi.FieldType.Name.ToString() + "' can't be saved to preset.>";
				}

			// print field name and value to the text file
			if (!string.IsNullOrEmpty(value) && fi.Name != "Persist")
				Append(prefix + fi.Name + " " + value);

		}

		return null;

	}


	/// <summary>
	/// saves only the fields that were changed
	/// </summary>
	public static string SaveDifference(vp_ComponentPreset initialStatePreset, Component modifiedComponent, string fullPath, vp_ComponentPreset diskPreset)
	{

		if (initialStatePreset.ComponentType != modifiedComponent.GetType())
		{
			Error("Tried to save difference between different type components in 'SaveDifference'");
			return null;
		}

		// create a preset to hold the current state of the component
		vp_ComponentPreset modifiedPreset = new vp_ComponentPreset();
		modifiedPreset.InitFromComponent(modifiedComponent);

		// create an empty preset of the same type
		vp_ComponentPreset result = new vp_ComponentPreset();
		result.m_ComponentType = modifiedPreset.ComponentType;

		for (int v = 0; v < modifiedPreset.m_Fields.Count; v++)
		{
			// if the field in the current preset has been changed since
			// the initial preset was created, add the differing field to
			// our result
			if (!initialStatePreset.m_Fields[v].Args.Equals(modifiedPreset.m_Fields[v].Args))
				result.m_Fields.Add(modifiedPreset.m_Fields[v]);
		}

		// if the target filename already contains a preset with values,
		// we copy those values into our new result - but only if they
		// don't exist in the new result

		// for each field in the disk preset
		foreach (Field d in diskPreset.m_Fields)
		{

			bool copyField = true;

			// check it against all the fields in our new preset
			foreach (Field r in result.m_Fields)
			{
				// if the field also exists in the 'result' preset,
				// don't copy it since it has changed
				if (d.FieldHandle == r.FieldHandle)
					copyField = false;
			}

			// only copy the field if it in fact belongs in this type of
			// component (we may be saving over a different preset type
			// and should not be copying unknown parameters)
			bool found = false;
			foreach (Field m in modifiedPreset.m_Fields)
			{
				if (d.FieldHandle == m.FieldHandle)
					found = true;
			}
			if (found == false)
				copyField = false;

			// done, copy the field
			if (copyField)
				result.m_Fields.Add(d);

		}

		return Save(result, fullPath, true);

	}


	/// <summary>
	/// copies a component's type and values into 'this' preset
	/// </summary>
	public void InitFromComponent(Component component)
	{

		m_ComponentType = component.GetType();

		m_Fields.Clear();

		foreach (FieldInfo f in component.GetType().GetFields())
		{
			if (f.IsPublic)
			{
				if (f.FieldType == typeof(float)
				|| f.FieldType == typeof(Vector4)
				|| f.FieldType == typeof(Vector3)
				|| f.FieldType == typeof(Vector2)
				|| f.FieldType == typeof(int)
				|| f.FieldType == typeof(bool)
				|| f.FieldType == typeof(string))
				{
					m_Fields.Add(new Field(f.FieldHandle, f.GetValue(component)));
				}
			}
		}

	}


	/// <summary>
	/// creates an empty preset in memory, copies a component's
	/// type and values into it and returns the preset
	/// </summary>
	public static vp_ComponentPreset CreateFromComponent(Component component)
	{

		vp_ComponentPreset preset = new vp_ComponentPreset();

		preset.m_ComponentType = component.GetType();

		foreach (FieldInfo f in component.GetType().GetFields())
		{
			if (f.IsPublic)
			{
				if (f.FieldType == typeof(float)
				|| f.FieldType == typeof(Vector4)
				|| f.FieldType == typeof(Vector3)
				|| f.FieldType == typeof(Vector2)
				|| f.FieldType == typeof(int)
				|| f.FieldType == typeof(bool)
				|| f.FieldType == typeof(string))
				{
					preset.m_Fields.Add(new Field(f.FieldHandle, f.GetValue(component)));
				}
			}
		}

		return preset;

	}


	/// <summary>
	/// reads the text file at 'fullPath' and fills the preset
	/// </summary>
	public bool LoadTextStream(string fullPath)
	{

		m_FullPath = fullPath;

		// NOTE: the rest of this method won't compile in a webplayer
		// because it uses 'System.IO.FileInfo.OpenText' in order to
		// read files outside the 'Resources' folder. for reading
		// presets from the 'Resources' folder, use 'LoadFromResources'


#if UNITY_WEBPLAYER
		if (Application.isEditor)
			UnityEngine.Debug.LogError("Error: Editor is in 'Web Player' mode. To ensure proper Preset file handling after building a webplayer, go to 'File -> Build Settings' and click 'PC & Mac Standalone -> Switch Platform'.");
#endif

#if !UNITY_WEBPLAYER

		// if we end up here, we're running in the editor or a
		// standalone build, where we can load files from
		// outside the 'Resources' folder.

		FileInfo fileInfo = null;
		TextReader file = null;

		// load file as text
		fileInfo = new FileInfo(m_FullPath);
		if (fileInfo != null && fileInfo.Exists)
		{
			file = fileInfo.OpenText();
		}
		else
		{
			Error("Failed to read file." + " '" + m_FullPath + "'");
			return false;
		}

		List<string> lines = new List<string>();

		// extract lines of text from the TextReader
		string txt;
		while ((txt = file.ReadLine()) != null)
		{
			lines.Add(txt);
		}

		file.Close();

		if (lines == null)
		{
			Error("Preset is empty." + " '" + m_FullPath + "'");
			return false;
		}

		// parse all the lines, reading component type and field values
		ParseLines(lines);
		
#endif

		return true;

	}


	/// <summary>
	/// static overload: creates and loads a preset and sets all
	/// the values on 'component'
	/// </summary>
	public static bool Load(Component component, string fullPath)
	{

		vp_ComponentPreset preset = new vp_ComponentPreset();
		preset.LoadTextStream(fullPath);
		return Apply(component, preset);

	}


	/// <summary>
	/// reads a text file from the resources folder
	/// </summary>
	public bool LoadFromResources(string resourcePath)
	{

		// NOTE: keep in mind that for resource loading the file 
		// extension must always be omitted.

		m_FullPath = resourcePath;

		// load text file as textasset
		TextAsset file = Resources.Load(m_FullPath) as TextAsset;
		if (file == null)
		{
			Error("Failed to read file." + " '" + m_FullPath + "'");
			return false;
		}

		return LoadFromTextAsset(file);

	}


	/// <summary>
	/// static overload: creates and loads a preset and sets all
	/// the values on 'component', then returns the preset
	/// </summary>
	public static vp_ComponentPreset LoadFromResources(Component component, string resourcePath)
	{

		vp_ComponentPreset preset = new vp_ComponentPreset();
		preset.LoadFromResources(resourcePath);
		Apply(component, preset);
		return preset;

	}


	/// <summary>
	/// fills up the preset from a text asset
	/// </summary>
	public bool LoadFromTextAsset(TextAsset file)
	{

		// store filename for error messages
		m_FullPath = file.name;

		List<string> lines = new List<string>();

		// split textasset into lines
		string[] splitLines = file.text.Split('\n');
		foreach (string s in splitLines)
		{
			lines.Add(s);
		}

		if (lines == null)
		{
			Error("Preset is empty." + " '" + m_FullPath + "'");
			return false;
		}

		// parse all the lines, reading component type and field values
		ParseLines(lines);

		// return the loaded preset
		return true;

	}


	/// <summary>
	/// static overload: creates and loads a preset and sets all
	/// the values on 'component', then returns the preset
	/// </summary>
	public static vp_ComponentPreset LoadFromTextAsset(Component component, TextAsset file)
	{

		vp_ComponentPreset preset = new vp_ComponentPreset();
		preset.LoadFromTextAsset(file);
		Apply(component, preset);
		return preset;

	}


	/// <summary>
	/// writes a single line of text to the file at 'm_FullPath'
	/// </summary>
	private static void Append(string str)
	{

		// replace newlines
		str = str.Replace("\n", System.Environment.NewLine);

		StreamWriter file = null;

		try
		{
			file = new StreamWriter(m_FullPath, true);
			file.WriteLine(str);
			if (file != null)
				file.Close();
		}
		catch
		{
			Error("Failed to write to file: '" + m_FullPath + "'");
		}

		if (file != null)
			file.Close();

	}


	/// <summary>
	/// clears the text file at 'm_FullPath'
	/// </summary>
	private static void ClearTextFile()
	{

		StreamWriter file = null;

		try
		{
			file = new StreamWriter(m_FullPath, false);
			if (file != null)
				file.Close();
		}
		catch
		{
			Error("Failed to clear file: '" + m_FullPath + "'");
		}

		if (file != null)
			file.Close();

	}


	/// <summary>
	/// goes through an array of strings, removing comments and
	/// empty lines, and feeding the remaining lines to the
	/// 'Parse' method for inclusion in the preset
	/// </summary>
	private void ParseLines(List<string> lines)
	{

		// reset line number
		m_LineNumber = 0;

		// feed all lines to parser
		foreach (string s in lines)
		{

			m_LineNumber++;

			// ignore line- and block comments
			string line = RemoveComments(s);

			// if line is empty here, skip it
			if (string.IsNullOrEmpty(line))
				continue;

			// done, try parsing the line, but if 'Parse' returns false
			// there has been an error and we'll abort loading the rest
			// of the preset
			if (!Parse(line))
				return;

		}

		// reset line number again. it should always be zero
		// outside of the above loop
		m_LineNumber = 0;

	}


	/// <summary>
	/// parses a string for a field name and its values and,
	/// if they seem healthy, adds them to the preset.
	/// if this method returns false, 'ParseLines' will stop.
	/// </summary>
	private bool Parse(string line)
	{

		line = line.Trim();

		if (string.IsNullOrEmpty(line))
		{
			// return since we have nothing to parse, but don't
			// treat this as an error
			return true;
		}

		// create an array with the tokens
		string[] tokens = line.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
		for (int v = 0; v < tokens.Length; v++)
		{
			tokens[v] = tokens[v].Trim();
		}

		if (m_ComponentType == null)
		{
			if (tokens[0] == "ComponentType" && tokens.Length == 2)
			{

				// gather info about the component type as described in
				// the preset script. if this fails, the script is
				// referencing a non-supported component type.
				m_Type = Type.GetType(tokens[1]);
				if (m_Type == null)
				{
					PresetError("No such ComponentType: '" + tokens[1] + "'");
					return false;
				}

				// store the type in our preset
				m_ComponentType = m_Type;

				// now return since 'ComponentType' is not a field in the component,
				// but allow further parsing to get to the remaining fields
				return true;

			}
			else
			{
				PresetError("Unknown ComponentType.");
				return false;
			}
		}

		// if we reach this point the component type is known so
		// we can try to parse the tokens

		// see if the component has a field with the same name as
		// the first token
		FieldInfo fieldInfo = null;
		foreach (FieldInfo f in m_Type.GetFields())
		{
			if (f.Name == tokens[0])
				fieldInfo = f;
		}

		// return if the field did not exist in the component
		if (fieldInfo == null)
		{
			if(tokens[0] != "ComponentType")
				PresetError("'" + m_Type.Name + "' has no such field: '" + tokens[0] + "'");
			// return, but allow further parsing
			return true;
		}

		// add the extracted field and its values to the preset
		Field field = new Field(fieldInfo.FieldHandle, TokensToObject(fieldInfo, tokens));
		m_Fields.Add(field);

		// return, allowing further parsing
		return true;

	}

	
	/// <summary>
	/// this method applies a preset onto the passed component,
	/// returning true on success 
	/// </summary>
	public static bool Apply(Component component, vp_ComponentPreset preset)
	{

		if (preset == null)
		{
			Error("Tried to apply a preset that was null in '" + vp_Utility.GetErrorLocation() + "'");
			return false;
		}

		if (preset.m_ComponentType == null)
		{
			Error("Preset ComponentType was null in '" + vp_Utility.GetErrorLocation() + "'");
			return false;
		}

		if (component == null)
		{
			Error("Component was null when attempting to apply preset in '" + vp_Utility.GetErrorLocation() + "'");
			return false;
		}

		if (component.GetType() != preset.m_ComponentType)
		{
			string type = "a '" + preset.m_ComponentType + "' preset";
			if (preset.m_ComponentType == null)
				type = "an unknown preset type";
			Error("Tried to apply " + type + " to a '" + component.GetType().ToString() + "' component in '" + vp_Utility.GetErrorLocation() + "'");
			return false;
		}

		// component and preset both seem ok, so set the preset fields
		// onto the component
		foreach (Field f in preset.m_Fields)
		{
			foreach (FieldInfo destField in component.GetType().GetFields())
			{
				if (destField.FieldHandle == f.FieldHandle)
					destField.SetValue(component, f.Args);
			}
		}

		return true;

	}


	/// <summary>
	/// loads the preset at 'fullPath' and returns the component
	/// type described in it
	/// </summary>
	public static Type GetFileType(string fullPath)
	{

		// attempt to load target preset into memory, ignoring
		// load errors in the process
		bool logErrorState = LogErrors;
		LogErrors = false;
		vp_ComponentPreset preset = new vp_ComponentPreset();
		preset.LoadTextStream(fullPath);
		LogErrors = logErrorState;

		// try to get hold of a preset and a component type from the file
		if (preset != null)
		{
			if (preset.m_ComponentType != null)
				return preset.m_ComponentType;
		}

		// the file was not found
		return null;

	}


	/// <summary>
	/// loads the preset at 'fullPath' and returns the component
	/// type described in it
	/// </summary>
	public static Type GetFileTypeFromAsset(TextAsset asset)
	{

		// attempt to load target preset into memory, ignoring
		// load errors in the process
		bool logErrorState = LogErrors;
		LogErrors = false;
		vp_ComponentPreset preset = new vp_ComponentPreset();
		preset.LoadFromTextAsset(asset);
		LogErrors = logErrorState;

		// try to get hold of a preset and a component type from the file
		if (preset != null)
		{
			if (preset.m_ComponentType != null)
				return preset.m_ComponentType;
		}

		// the file was not found
		return null;

	}


	/// <summary>
	/// converts an array of strings into the correct data types
	/// depending on the passed field type and returns them as
	/// a single object
	/// </summary>
	private static object TokensToObject(FieldInfo field, string[] tokens)
	{

		// attempt to set field to the arguments
		if (field.FieldType == typeof(float))
			return ArgsToFloat(tokens);
		else if (field.FieldType == typeof(Vector4))
			return ArgsToVector4(tokens);
		else if (field.FieldType == typeof(Vector3))
			return ArgsToVector3(tokens);
		else if (field.FieldType == typeof(Vector2))
			return ArgsToVector2(tokens);
		else if (field.FieldType == typeof(int))
			return ArgsToInt(tokens);
		else if (field.FieldType == typeof(bool))
			return ArgsToBool(tokens);
		else if (field.FieldType == typeof(string))
			return ArgsToString(tokens);

		return null;

	}


	/// <summary>
	/// removes line and block comments from a string.
	/// preset scripts support both traditional C line '//' and
	/// /* block comments */
	/// </summary>
	private static string RemoveComments(string str)
	{

		string result = "";

		for (int v = 0; v < str.Length; v++)
		{
			switch (m_ReadMode)
			{

				// in Normal mode, we usually copy text but go into
				// BlockComment mode upon /* and into LineComment mode upon //
				case ReadMode.Normal:
					if (str[v] == '/' && str[v + 1] == '*')
					{
						m_ReadMode = ReadMode.BlockComment;
						v++;
						break;
					}
					else if (str[v] == '/' && str[v + 1] == '/')
					{
						m_ReadMode = ReadMode.LineComment;
						v++;
						break;
					}

					// copy non-comment text
					result += str[v];

					break;

				// in LineComment mode, we go into Normal mode upon newline
				case ReadMode.LineComment:
					if (v == str.Length - 1)
					{
						m_ReadMode = ReadMode.Normal;
						break;
					}
					break;

				// in BlockComment mode, we go into normal mode upon */
				case ReadMode.BlockComment:
					if (str[v] == '*' && str[v + 1] == '/')
					{
						m_ReadMode = ReadMode.Normal;
						v++;
						break;
					}
					break;

			}
		}

		return result;

	}


	/// <summary>
	/// 
	/// </summary>
	private static Vector4 ArgsToVector4(string[] args)
	{

		Vector4 v;

		if ((args.Length - 1) != 4)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return Vector4.zero;
		}

		try
		{
			v = new Vector4(Convert.ToSingle(args[1]), Convert.ToSingle(args[2]),
							Convert.ToSingle(args[3]), Convert.ToSingle(args[4]));
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + ", " + args[2] + ", " + args[3] + ", " + args[4] + "'");
			return Vector4.zero;
		}
		return v;

	}


	/// <summary>
	/// 
	/// </summary>
	private static Vector3 ArgsToVector3(string[] args)
	{

		Vector3 v;

		if ((args.Length - 1) != 3)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return Vector3.zero;
		}

		try
		{
			v = new Vector3(Convert.ToSingle(args[1]), Convert.ToSingle(args[2]), Convert.ToSingle(args[3]));
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + ", " + args[2] + ", " + args[3] + "'");
			return Vector3.zero;
		}
		return v;

	}


	/// <summary>
	/// 
	/// </summary>
	private static Vector2 ArgsToVector2(string[] args)
	{

		Vector2 v;

		if ((args.Length - 1) != 2)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return Vector2.zero;
		}

		try
		{
			v = new Vector2(Convert.ToSingle(args[1]), Convert.ToSingle(args[2]));
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + ", " + args[2] + "'");
			return Vector2.zero;
		}
		return v;

	}


	/// <summary>
	/// 
	/// </summary>
	private static float ArgsToFloat(string[] args)
	{

		float f;

		if ((args.Length - 1) != 1)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return 0.0f;
		}

		try
		{
			f = Convert.ToSingle(args[1]);
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + "'");
			return 0.0f;
		}
		return f;

	}


	/// <summary>
	/// 
	/// </summary>
	private static int ArgsToInt(string[] args)
	{

		int i;

		if ((args.Length - 1) != 1)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return 0;
		}

		try
		{
			i = Convert.ToInt32(args[1]);
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + "'");
			return 0;
		}
		return i;

	}


	/// <summary>
	/// 
	/// </summary>
	private static bool ArgsToBool(string[] args)
	{

		if ((args.Length - 1) != 1)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return false;
		}

		if (args[1].ToLower() == "true")
			return true;
		else if (args[1].ToLower() == "false")
			return false;

		PresetError("Illegal value: '" + args[1] + "'");
		return false;

	}


	/// <summary>
	/// 
	/// </summary>
	private static string ArgsToString(string[] args)
	{

		string s = "";

		// put all arguments, with spaces inbetween,
		// into a single string
		for(int v = 1; v<args.Length; v++)
		{
			s += args[v];
			if (v < args.Length-1)
				s += " ";
		}

		return s;

	}


	/// <summary>
	/// returns the type of a certain field based on its name.
	/// NOTE: not for high performance use
	/// </summary>
	public Type GetFieldType(string fieldName)
	{

		Type type = null;

		foreach (Field f in m_Fields)
		{
			FieldInfo fi = FieldInfo.GetFieldFromHandle(f.FieldHandle);
			if (fi.Name == fieldName)
				type = fi.FieldType;
		}

		return type;

	}


	/// <summary>
	/// returns the value of a certain field based on its name
	/// NOTE: not for high performance use
	/// </summary>
	public object GetFieldValue(string fieldName)
	{

		object value = null;

		foreach (Field f in m_Fields)
		{
			FieldInfo fi = FieldInfo.GetFieldFromHandle(f.FieldHandle);
			if (fi.Name == fieldName)
				value = f.Args;
		}

		return value;

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
	/// logs a preset error to the console. called during the
	/// phase when a text file is read into memory as a preset
	/// </summary>
	private static void PresetError(string message)
	{
		if (!LogErrors)
			return;
		UnityEngine.Debug.LogError("Preset Error: " + m_FullPath + " (at " + m_LineNumber + ") " + message);
	}


	/// <summary>
	/// logs a regular error to the console. called in any phase
	/// </summary>
	private static void Error(string message)
	{
		if (!LogErrors)
			return;
		UnityEngine.Debug.LogError("Error: " + message);
	}


}






