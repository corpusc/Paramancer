// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Menus and other commands used by the editor.
/// </summary>
public class MBEditorCommands
{
    public static MBObject ClipboardObject;
    public static string ClipboardActionPending; // "cut","copy"

    #region ### Magical Box ###
   
    [MenuItem("GameObject/Create Other/Magical Box ParticleSystem",false)]
    [MenuItem("Window/Magical Box/Create ParticleSystem",false,1)]
    public static void AddParticleSystem()
    {
        MBEditor.SnapshotScene("Add ParticleSystem");
        MBParticleSystem sys = CreateParticleSystem("ParticleSystem");
        if (sys) {
            if (Selection.activeGameObject && !Selection.activeGameObject.GetComponent<MBObject>())
                sys.transform.parent = Selection.activeGameObject.transform;
            Selection.activeObject = sys.gameObject;
        }
    }

    [MenuItem("Window/Magical Box/Show Editor", false, 6)]
    public static void ShowMBEditor()
    {
        MBEditor.ShowWindow();
    }

   
    [MenuItem("Window/Magical Box/Publish selected Prefabs", false, 8)]
    public static void PublishPrefabs()
    {
        string packagepath = EditorUtility.SaveFilePanel("Save prefab(s) as package", "", "","unitypackage");
        if (!string.IsNullOrEmpty(packagepath)) {
            Object[] depends=EditorUtility.CollectDependencies(Selection.objects);
            List<string>paths=new List<string>();
            
            foreach (Object obj in depends) {
                string path = AssetDatabase.GetAssetPath(obj);
                if (path.StartsWith("Assets/Magical Box/User EmitterTypes") ||
                    path.StartsWith("Assets/Magical Box/User Parameters") ||
                    path.StartsWith("Assets/Magical Box/Scripts") || 
                    !path.StartsWith("Assets/Magical Box/"))
                    paths.Add(path);
            }
            AssetDatabase.ExportPackage(paths.ToArray(), packagepath, ExportPackageOptions.Default);
            EditorUtility.DisplayDialog("Exporting to package", "Package successfully exported!", "OK");
        }
    }

    [MenuItem("Window/Magical Box/Publish selected Prefabs", true)]
    public static bool CanPublishPrefabs()
    {
        foreach (Object obj in Selection.objects) {
            if (PrefabUtility.GetPrefabType(obj) != PrefabType.Prefab)
                return false;

            return true;
        }
        return false;
    }

    #endregion

    #region ### Magical Box/Object ###

    // PLAY
    [MenuItem("Window/Magical Box/Object/Play", false, 55)]
    public static void PlayObject()
    {
        if (MBEditor.SelectedObject is MBParticleSystem)
            ((MBParticleSystem)MBEditor.SelectedObject).Play();
        else
            MBEditor.SelectedEmitter.Play();
    }
    [MenuItem("Window/Magical Box/Object/Play", true)]
    public static bool CanPlayObject() 
    {
        return (MBEditor.SelectedObject && MBEditor.SelectedObject.Matches(typeof(MBEmitter), typeof(MBParticleSystem),typeof(MBParameter)));
    }

    // STOP
    [MenuItem("Window/Magical Box/Object/Stop", false, 56)]
    public static void StopObject()
    {
        MBObject obj = MBEditor.SelectedObject;
        if (MBEditor.SelectedObject is MBParticleSystem)
            ((MBParticleSystem)obj).Stop();
        else
            MBEditor.SelectedEmitter.Stop();
    }

    [MenuItem("Window/Magical Box/Object/Stop", true)]
    public static bool CanStopObject() 
    {
        MBObject obj = MBEditor.SelectedObject;
        return (obj && (
                (obj is MBEmitter && ((MBEmitter)obj).Playing) ||
                (obj is MBParameter && ((MBParameter)obj).ParentEmitter.Playing) ||
                (obj is MBParticleSystem && ((MBParticleSystem)obj).Playing))
                );
    }

    // HALT
    [MenuItem("Window/Magical Box/Object/Halt", false, 57)]
    public static void HaltObject()
    {
        MBObject obj = MBEditor.SelectedObject;
        if (MBEditor.SelectedObject is MBParticleSystem)
            ((MBParticleSystem)obj).Halt();
        else
            MBEditor.SelectedEmitter.Halt();
    }
    [MenuItem("Window/Magical Box/Object/Halt", true)]
    public static bool CanHaltObject() 
    {
        MBObject obj = MBEditor.SelectedObject;
        return (obj && (
                (obj is MBEmitter && ((MBEmitter)obj).Playing) ||
                (obj is MBParameter && ((MBParameter)obj).ParentEmitter.Playing) ||
                (obj is MBParticleSystem && ((MBParticleSystem)obj).Playing))
                );
    }

    #region ### Magical Box/Object/Add ###

    [MenuItem("Window/Magical Box/Object/Add/Layer", false, 70)]
    public static void AddLayer()
    {
        MBEditor.SnapshotScene("Add Layer");
        Material mat = CreateMaterialQuestion(MBEditor.SelectedParticleSystem.name+"Mat");
        if (mat) 
            MBEditor.SelectedParticleSystem.AddLayer(mat);
        SyncHierarchy();
    }

    [MenuItem("Window/Magical Box/Object/Add/Layer", true)]
    public static bool CanAddLayer()
    {
        return (MBEditor.SelectedObject is MBParticleSystem);
    }

    [MenuItem("Window/Magical Box/Object/Add/Emitter", false, 71)]
    public static void AddEmitter()
    {
        MBEditor.SnapshotScene("Add Emitter");
        MBObject obj = MBEditor.SelectedObject;
        MBEmitter em=obj.ParticleSystem.AddEmitter(obj);
        Texture2D deftex = Resources.LoadAssetAtPath("Assets/Magical Box/Base/default.png", typeof(Texture2D)) as Texture2D;
        if (em && deftex) {
            em.SetTexture(deftex);
        }
        if (em) {
            SyncHierarchy();
            MBEditor.Select(em);
        }
    }

    [MenuItem("Window/Magical Box/Object/Add/Emitter", true)]
    public static bool CanAddEmitter()
    {
        MBObject sel = MBEditor.SelectedObject;
        if (!sel) return false;
        if (sel is MBEmitter && !((MBEmitter)sel).IsTrail)
            return true;
        else
            return sel.Matches(typeof(MBParticleSystem), typeof(MBAnchor));
    }

    [MenuItem("Window/Magical Box/Object/Add/Anchor", false, 72)]
    public static void AddAnchor()
    {
        MBEditor.SnapshotScene("Add Anchor");
        MBObject obj = MBEditor.SelectedObject;
        MBAnchor anc=obj.ParticleSystem.AddAnchor(obj);
        SyncHierarchy();
        MBEditor.Select(anc);
    }

    [MenuItem("Window/Magical Box/Object/Add/Anchor", true)]
    public static bool CanAddAnchor()
    {
        MBObject sel = MBEditor.SelectedObject;
        if (!sel) return false;
        if (sel is MBEmitter && !((MBEmitter)sel).IsTrail)
            return true;
        else
            return sel.Matches(typeof(MBParticleSystem), typeof(MBAnchor));
    }

    #endregion

    [MenuItem("Window/Magical Box/Object/Cut", false, 110)]
    public static void CutObject()
    {
        ClipboardObject = MBEditor.SelectedObject;
        ClipboardActionPending = "cut";
    }

    [MenuItem("Window/Magical Box/Object/Cut", true)]
    public static bool CanCutObject()
    {
        return MBEditor.SelectedObject && !(MBEditor.SelectedObject is MBParticleSystem);
    }

    [MenuItem("Window/Magical Box/Object/Copy", false, 111)]
    public static void CopyObject()
    {
        ClipboardObject = MBEditor.SelectedObject;
        ClipboardActionPending = "copy";
    }

    [MenuItem("Window/Magical Box/Object/Copy", true)]
    public static bool CanCopyObject()
    {
        return MBEditor.SelectedObject && !(MBEditor.SelectedObject is MBParticleSystem);
    }


    [MenuItem("Window/Magical Box/Object/Paste", false, 112)]
    public static MBObject PasteObject()
    {
        MBEditor.SnapshotScene("Paste Object");
        MBObject obj=ClipboardObject;
        MBEmitter em = (obj is MBEmitter) ? (MBEmitter)obj : null;
        MBLayer emLayer = null;

        if (em) {
            // Handle Layers:
            // if Particlesystem changes, look for a layer with matching material
            // if found, let the emitter use this one.
            // otherwise, ask to create a new layer using the same material
            emLayer = em.Layer;
            if (em.ParticleSystem != MBEditor.SelectedParticleSystem && em.Layer) {
                Material layerMat=em.Layer.Material;
                emLayer=MBEditor.SelectedParticleSystem.FindLayer(layerMat);
                if (!emLayer) {
                    if (EditorUtility.DisplayDialog("Assign Layer", "The material that this emitter's layer is using, isn't used by this particle system. Do you want to create a layer with that material?", "Yes", "No")) 
                        emLayer = MBEditor.SelectedParticleSystem.AddLayer(layerMat);
                }
            }
        }

        if (ClipboardActionPending == "cut") {
            if (em)
                em.Layer = emLayer;
            ClipboardObject.SetParent(MBEditor.SelectedObject);
        }
        else { // copy
            Vector3 localPos = ClipboardObject.transform.localPosition;
            obj = (MBObject)Object.Instantiate(ClipboardObject);
            obj.name = ClipboardObject.name;
            MBObject tgt = (MBEditor.SelectedObject is MBParameter) ? MBEditor.SelectedObject.Parent : MBEditor.SelectedObject;
            if (em)
                ((MBEmitter)obj).Layer = emLayer;
            obj.SetParent(tgt);
            obj.transform.localPosition = localPos;
        }

        SyncHierarchy();
        ClipboardObject = null;
        ClipboardActionPending = "";
        return obj;
    }

    [MenuItem("Window/Magical Box/Object/Paste", true)]
    public static bool CanPasteObject()
    {
        MBObject P=MBEditor.SelectedObject;
        if (P && ClipboardObject && !string.IsNullOrEmpty(ClipboardActionPending)) {
            switch (ClipboardObject.GetType().Name) {
                case "MBEmitter":
                case "MBAnchor":
                    return P.Matches(typeof(MBParticleSystem),typeof(MBEmitter),typeof(MBAnchor));
                case "MBLayer":
                    return P.Matches(typeof(MBParticleSystem));
                default: //MBParameter
                    return (ClipboardObject is MBParameter && P.Matches(typeof(MBEmitter),typeof(MBParameter)));
            }
        }
        return false;
    }

    [MenuItem("Window/Magical Box/Object/Paste Attributes", false, 114)]
    public static void PasteObjectAttributes()
    {
        MBEditor.SnapshotScene("Paste Attributes");
        // This is actually a Paste and a Delete
        MBParameter toDelete = (MBParameter)MBEditor.SelectedObject;
        PasteObject().name = toDelete.name;
        MBEditor.SelectedObject = toDelete;
        DeleteObject();
    }

    [MenuItem("Window/Magical Box/Object/Paste Attributes", true)]
    public static bool CanPasteObjectAttributes()
    {
        return MBEditor.SelectedObject is MBParameter && ClipboardObject && MBEditor.SelectedObject.GetType() == ClipboardObject.GetType();
    }

    [MenuItem("Window/Magical Box/Object/Duplicate", false, 130)]
    public static void DuplicateObject()
    {
        MBEditor.SnapshotScene("Duplicate Object");
        
        MBObject obj = (MBObject)Object.Instantiate(MBEditor.SelectedObject);
        obj.transform.parent = MBEditor.SelectedObject.transform.parent;
        obj.transform.position = MBEditor.SelectedObject.transform.position;
        obj.name = MBEditor.SelectedObject.name + " Copy";
        if (MBEditor.SelectedObject is MBLayer) {
            //((MBLayer)MBEditor.SelectedObject).GenerateTextureMap(true);
            //MBEditorUtility.WriteMaterialTexture((MBLayer)MBEditor.SelectedObject);
        }
            
        SyncHierarchy();
    }

    [MenuItem("Window/Magical Box/Object/Duplicate", true)]
    public static bool CanDuplicateObject()
    {
        return MBEditor.SelectedObject;
    }

    [MenuItem("Window/Magical Box/Object/Delete", false, 131)]
    public static void DeleteObject()
    {
        MBEditor.SnapshotScene("Delete Object");
        MBObject obj=MBEditor.SelectedObject;
        if (MBEditor.SelectedObject is MBParameter) {
            MBEmitter em = MBEditor.SelectedEmitter;
            em.RemoveParameter((MBParameter)MBEditor.SelectedObject);
            MBEditor.Select(em);
            MBEditor.Instance.SelectParameterTab();
        } else
            if (EditorUtility.DisplayDialog("Confirm","Delete '"+obj.name+"' ?","Yes","No")) {
                MBObject parent = obj.Parent;
                if (parent && Selection.activeGameObject==obj.gameObject)
                    Selection.activeGameObject = parent.gameObject;
                GameObject.DestroyImmediate(obj.gameObject);
                if (parent)
                    MBEditor.Select(parent);
                else
                    MBEditor.Select(null);
            }
    }

    [MenuItem("Window/Magical Box/Object/Delete", true)]
    public static bool CanDeleteObject()
    {
        return MBEditor.SelectedObject;
    }

    [MenuItem("Window/Magical Box/Object/Reset", false, 150)]
    public static void ResetObject()
    {
        MBEditor.SnapshotScene("Reset Object");
        MBEditor.SelectedObject.Reset();
        SyncHierarchy();
    }
    [MenuItem("Window/Magical Box/Object/Reset", true)]
    public static bool CanResetObject()
    {
        return MBEditor.SelectedObject;
    }


    [MenuItem("Window/Magical Box/Object/Purge", false, 152)]
    public static void PurgeObject()
    {
        MBEditor.SelectedObject.Purge();
        Debug.Log("Magical Box: '" + MBEditor.SelectedObject.name + "' and all subordinated objects successfully purged!");
    }

    [MenuItem("Window/Magical Box/Object/Purge", true)]
    public static bool CanPurgeObject()
    {
        return MBEditor.SelectedObject;
    }

    [MenuItem("Window/Magical Box/Object/Toggle Recorder", true)]
    public static bool CanToggleRecorder()
    {
        return MBEditor.SelectedObject && MBEditor.SelectedObject.Matches(typeof(MBParticleSystem), typeof(MBEmitter));
    }

    [MenuItem("Window/Magical Box/Object/Create Prefab", false, 170)]
    public static Object ExportPrefab()
    {
        MBObject obj = MBEditor.SelectedObject;
        Object prefab = null;
        obj.ParticleSystem.Halt();
        obj.Purge();
        string path = "";
        if (obj.Matches(typeof(MBEmitter),typeof(MBAnchor)))
            path = EditorUtility.SaveFilePanelInProject("Save Emitter as Prefab", obj.name + "EM.prefab", "prefab", "Please enter a prefab name to save the Emitter to");
        else
            path = EditorUtility.SaveFilePanelInProject("Save ParticleSystem as Prefab", obj.name + ".prefab", "prefab", "Please enter a prefab name to save the ParticleSystem to");
        if (path.Length > 0) {
            prefab = PrefabUtility.CreateEmptyPrefab(path);
            PrefabUtility.ReplacePrefab(obj.gameObject, prefab);
            AssetDatabase.Refresh();
        }
        EditorApplication.Beep();
        return prefab;
    }

    [MenuItem("Window/Magical Box/Object/Create Prefab", true)]
    public static bool CanExportPrefab()
    {
        return MBEditor.SelectedObject && MBEditor.SelectedObject.Matches(typeof(MBParticleSystem), typeof(MBEmitter), typeof(MBAnchor));
    }

    


    #endregion

    [MenuItem("Window/Magical Box/Create Custom Emitter Type", false, 230)]
    public static void CreateCustomEMType()
    {
        MBEditorCreateCustom.Open(0);
    }

    [MenuItem("Window/Magical Box/Create Custom Parameter", false, 231)]
    public static void CreateCustomParameter()
    {
        MBEditorCreateCustom.Open(1);
    }

    [MenuItem("Window/Magical Box/Create Editor Enabled Script", false, 232)]
    public static void CreateEditorEnabledScript()
    {
        MBEditorCreateCustom.Open(2);
    }

    #region ### Help Menu ###
    [MenuItem("Window/Magical Box/Help/About Magical Box", false, 252)]
    public static void ShowAbout()
    {
        MBEditorAbout.Open();
    }

    [MenuItem("Window/Magical Box/Help/Online Manual", false, 253)]
    public static void OpenHelp()
    {
        Application.OpenURL("http://docs.fluffyunderware.com/magicalbox");
    }

    [MenuItem("Window/Magical Box/Help/Support Forum", false, 254)]
    public static void OpenForum()
    {
        Application.OpenURL("http://forum.fluffyunderware.com");
    }

    #endregion

    

    #region ### Other Commands ###

    public static Material CreateMaterialAsset(string defaultName)
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Layer Material as", defaultName, "mat", "Please enter the name of the new material");
        if (path.Length > 0) {
            Material mat = new Material(Shader.Find("Particles/Additive"));
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }
        return null;
    }

    public static MBParticleSystem CreateParticleSystem(string defaultName)
    {
        MBParticleSystem sys = null;
        // Ask for atlas file
        string atlasPath=EditorUtility.SaveFilePanelInProject("Each ParticleSystem needs a texture atlas", defaultName + ".png", "png", "Please enter the name of the texture atlas");
        if (string.IsNullOrEmpty(atlasPath)) return null;

        Material mat = CreateMaterialQuestion(System.IO.Path.GetFileNameWithoutExtension(atlasPath));

        if (mat) {
            sys = new GameObject("ParticleSystem", new System.Type[] { typeof(MBParticleSystem) }).GetComponent<MBParticleSystem>();
            sys.AddLayer(mat);
            // Add a default emitter
            MBEmitter em=sys.AddEmitter();
            Texture2D deftex=Resources.LoadAssetAtPath("Assets/Magical Box/Base/default.png",typeof(Texture2D)) as Texture2D;
            if (em && deftex) {
                em.SetTexture(deftex);
            }
            sys.SetTextureAtlas(atlasPath);
            AssetDatabase.Refresh();
        }
        return sys;
    }
    
    public static Material CreateMaterialQuestion(string defaultName)
    {
        Material mat = null;
        int dlg = EditorUtility.DisplayDialogComplex("Each Layer needs a material", "Do you want to use an existing material or create a new one?", "Load", "Create", "Cancel");
        if (dlg == 0) {
            string matfile = EditorUtility.OpenFilePanel("Select Particle Material", "Assets", "mat");
            if (matfile.Length > 0) {
                mat = (Material)AssetDatabase.LoadAssetAtPath(matfile.Replace(Application.dataPath, "assets"), typeof(Material));
            }
        }
        else if (dlg == 1)
            mat = CreateMaterialAsset(defaultName + ".mat");
        return mat;
    }
   
    public static void SyncHierarchy()
    {
        if (MBEditor.SelectedParticleSystem) {
            MBEditor.SelectedParticleSystem.mbReloadHierarchy();

            MBEditor.SelectedParticleSystem.GenerateTextureMap(true);
           
            if (MBEditor.EditorIsOpen) {
                MBEditor.Instance.InitializeTree();
                // if an emitter or parameter is selected, reload parameters
                if (MBEditor.SelectedObject.Matches(typeof(MBEmitter), typeof(MBParameter)))
                    MBEditor.Instance.InstantiateParameterHandler();
            }
        }
    }

    #endregion




}
