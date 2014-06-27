// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;
using System.Reflection;


/// <summary>
/// Utilities used by the editor
/// </summary>
public class MBEditorUtility
{

    public static EditorWindow GetGameViewWindow()
    {
            Assembly asm = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            System.Type type = asm.GetType("UnityEditor.GameView");
            if (type != null)
                return EditorWindow.GetWindow(type, false, null, false);
            else
                return null;
    }

    /// <summary>
    /// Called by Inspector classes to catch scene camera
    /// </summary>
    public static void OnEditorSceneGUI()
    {
        if (MBEditor.SelectedParticleSystem){
            MBEditor.SelectedParticleSystem.mbEditorCamera = Camera.current;
            
        }
    }
  
    public static void MakeTexturesReadable(params Texture2D[] textures)
    {
        
        foreach (Texture2D tex in textures) {
            string path=AssetDatabase.GetAssetPath(tex);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (!importer) {
                Debug.Log("Magical Box: Unable to import texture!");
                return;
            }
            if (!importer.isReadable) {
                importer.isReadable = true;
                importer.textureFormat = TextureImporterFormat.ARGB32;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
             
            
        }
    }

    
    /// <summary>
    /// Saves TextureAtlas of a particle system to file
    /// </summary>
    public static void SaveTextureAtlas(MBParticleSystem sys)
    {
        string path=EditorUtility.SaveFilePanelInProject("Save texture atlas", sys.name + "_atlas.png", "png", "Save the current texture atlas at...");
        MBUtility.SaveTexture(sys.TextureAtlas, path);
    }

    

    
}

