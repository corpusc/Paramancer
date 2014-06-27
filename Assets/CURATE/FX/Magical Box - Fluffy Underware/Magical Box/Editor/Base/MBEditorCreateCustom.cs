using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MBEditorCreateCustom : EditorWindow
{
    int mMode;
    bool done = false;
    string mName = "";
    string mDisplayName="";
    MBParameterAnimationMode mAnimBirth=MBParameterAnimationMode.Optional;
    MBParameterAnimationMode mAnimLife = MBParameterAnimationMode.Optional;
    
    List<string>mLog=new List<string>();

    public static void Open(int mode)
    {
        MBEditorCreateCustom win = null;
        switch (mode){
            case 0: win = GetWindow<MBEditorCreateCustom>(true, "Create custom emitter type", true);
                break;
            case 1: win = GetWindow<MBEditorCreateCustom>(true, "Create custom parameter", true);
                break;
            case 2: win = GetWindow<MBEditorCreateCustom>(true, "Create Editor Enabled Script", true);
                break;
        }
        if (win) {
            win.minSize = new Vector2(500, 300);
            win.mMode = mode;
        }
    }

    void OnGUI()
    {
        
        switch (mMode) {
            case 0 : // CUSTOM EMITTER TYPE
                MBGUI.DoLabel("Displayname of your custom emitter (like \"Hollow Box\")");
                mDisplayName = EditorGUILayout.TextField(mDisplayName);
                MBGUI.DoLabel("Name of your custom emitter (like \"HollowBox\")");
                mName=EditorGUILayout.TextField(mName);
                if (MBGUI.DoButton("Create", "", mName.Length > 0 && mDisplayName.Length>0)) {
                    CreateEMTypeFromTemplate();
                }
                break;
            case 1 : // CUSTOM PARAMETER
                MBGUI.DoLabel("Displayname of your custom parameter (like \"Size by Velocity\")");
                mDisplayName = EditorGUILayout.TextField(mDisplayName);
                MBGUI.DoLabel("Name of your custom parameter (like \"SizeVelocity\")");
                mName=EditorGUILayout.TextField(mName);
                mAnimBirth = (MBParameterAnimationMode)MBGUI.DoEnumField("Animated Birth", "Birth animation mode", mAnimBirth);
                mAnimLife = (MBParameterAnimationMode)MBGUI.DoEnumField("Animated Lifetime", "Lifetime animation mode", mAnimLife);
                if (MBGUI.DoButton("Create", "", mName.Length > 0 && mDisplayName.Length>0)) {
                    CreateParameterFromTemplate();
                }
                break;
            case 2: // EDITOR ENABLED SCRIPT
                MBGUI.DoLabel("Name of your editor enabled script (like \"FollowMouse\")");
                mName = EditorGUILayout.TextField(mName);
                if (MBGUI.DoButton("Create", "", mName.Length > 0))
                    CreateEditorEnabledScript();
                break;
        }
            
        foreach (string s in mLog)
            MBGUI.DoLabel(s);

        if (done && MBGUI.DoButton(new GUIContent("Close"), true, true))
            Close();
    }

    void CreateEMTypeFromTemplate()
    {
        if (CreateTemplate(Application.dataPath + "/Magical Box/EmitterTypes/_Template.txt",
                       Application.dataPath + "/Magical Box/User EmitterTypes/MB" + mName + "Emitter.cs",
                       mName, mDisplayName,"",""))
            CreateTemplate(Application.dataPath + "/Magical Box/EmitterTypes/_EditorTemplate.txt",
                       Application.dataPath + "/Magical Box/Editor/User Handlers/MBEditor" + mName + "EmitterHandler.cs",
                       mName, mDisplayName,"","");
            mLog.Add("Done!");
        
        done = true;
    }

    void CreateParameterFromTemplate()
    {
        if (CreateTemplate(Application.dataPath + "/Magical Box/Parameters/_Template.txt",
                       Application.dataPath + "/Magical Box/User Parameters/MBParticle" + mName+".cs",
                       mName, mDisplayName,mAnimBirth.ToString(),mAnimLife.ToString()))
            CreateTemplate(Application.dataPath + "/Magical Box/Parameters/_EditorTemplate.txt",
                       Application.dataPath + "/Magical Box/Editor/User Handlers/MBEditorParticle" + mName + "Handler.cs",
                       mName, mDisplayName, mAnimBirth.ToString(), mAnimLife.ToString());
        mLog.Add("Done!");

        done = true;
    }

    bool CreateTemplate(string src, string dst, string name, string displayname, string animbirth,string animlife)
    {
        if (File.Exists(dst)){
            mLog.Add("File '" + dst + "' already exists!");
            mLog.Add("No files were created!");
            return false;
        }
        string template = File.ReadAllText(src);
        template=template.Replace("%NAME%", name).Replace("%DISPLAYNAME%", displayname);
        template = template.Replace("%ANIMBIRTH%", animbirth).Replace("%ANIMLIFE%", animlife);
        File.WriteAllText(dst, template);
        
        AssetDatabase.ImportAsset(FileUtil.GetProjectRelativePath(dst),ImportAssetOptions.ForceUpdate);
        mLog.Add("File '" + dst + "' created!");
        return true;
    }

    bool CreateEditorEnabledScript()
    {
        done = true;
        string src = Application.dataPath + "/Magical Box/Base/_EditorEnabledScriptTemplate.txt";
        string dst = Application.dataPath + "/Magical Box/Scripts/MB" + mName + ".cs";
        if (File.Exists(dst)) {
            mLog.Add("File '" + dst + "' already exists!");
            mLog.Add("No files were created!");
            return false;
        }
        string template = File.ReadAllText(src);
        template = template.Replace("%NAME%", mName);
        File.WriteAllText(dst, template);
        AssetDatabase.ImportAsset(FileUtil.GetProjectRelativePath(dst), ImportAssetOptions.ForceUpdate);
        mLog.Add("File '" + dst + "' created!");
        return true;
    }

}