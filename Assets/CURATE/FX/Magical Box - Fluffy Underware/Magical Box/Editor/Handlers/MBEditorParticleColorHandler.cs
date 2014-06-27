// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;


public class MBEditorParticleColorHandler : MBEditorParameterHandler
{
    Texture2D mGradient;
    bool mDragging;
    ColorKey mDragKey;
    float mDragOffset;
   
    public override void  OnDestroy()
    {
 	    base.OnDestroy();
        if (mGradient) 
            GameObject.DestroyImmediate(mGradient, true);
    }
    
    public override void OnBirthGUI()
    {
 	    base.OnBirthGUI();
    }

    public override void OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        MBParticleColorBase P = Target as MBParticleColorBase;

        P.DieOnAlpha = MBGUI.DoToggle("Die On Alpha", "Die when Alpha<=0 ?", P.DieOnAlpha);
        EditorGUILayout.BeginHorizontal();
        P.AutoFade = MBGUI.DoToggle("AutoFade", "Easy fade in/out", P.AutoFade);
        if (P.AutoFade) {
            P.FadeIn = MBGUI.DoFloatSlider("Fade In", "Fade in to first color's alpha automatically", P.FadeIn, 0, 1);
            P.FadeOut = MBGUI.DoFloatSlider("Fade Out", "Fade out from last color's alpha automatically", P.FadeOut, 0, 1);
        }
        EditorGUILayout.EndHorizontal();
    }

    public void DoGradientGUI(MBParticleColorBase P)
    {
        bool lmbd = Event.current.type == EventType.MouseDown && Event.current.button == 0;
        bool lmbu = Event.current.type == EventType.MouseUp && Event.current.button == 0;
        bool drg = Event.current.type==EventType.MouseDrag;
        bool dbl = lmbd && Event.current.clickCount > 1;
        bool rmb = Event.current.type == EventType.MouseDown && Event.current.button == 1;
        
        Vector2 mp= Event.current.mousePosition;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.MinHeight(24), GUILayout.ExpandWidth(true) });

        Rect r = GUILayoutUtility.GetLastRect();
        r = new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4);
        if (!mGradient && Event.current.type == EventType.Repaint){
            mGradient=new Texture2D((int)r.width,(int)r.height, TextureFormat.RGB24, false);
            CreateColorGradient(mGradient, P);
        }
        
        if (mGradient) {
            EditorGUI.DrawPreviewTexture(r, mGradient);
            
            for (int k = 0; k < P.Colors.Count; k++) {
                Rect HandleRect = new Rect(r.x + P.Colors[k].t * r.width, r.y, 40, r.height);
                
                // Delete Key?
                if (rmb && HandleRect.Contains(mp) && P.Colors.Count>2 && k>0 && k<P.Colors.Count-1) {
                    P.Colors.Remove(P.Colors[k]);
                    CreateColorGradient(mGradient, P);
                    Event.current.Use();
                }
                
                // Begin Drag?
                if (drg && !mDragging && k>0 && k<P.Colors.Count-1 && HandleRect.Contains(mp)) {
                    mDragging = true;
                    mDragKey = P.Colors[k];
                    mDragOffset = mp.x - HandleRect.x;
                    Event.current.Use();
                }
                
                // Drag?
                if (Event.current.type==EventType.Repaint && mDragging && mDragKey==P.Colors[k]) {
                     mDragKey.t = (mp.x - r.x-mDragOffset) / r.width;
                     HandleRect = new Rect(r.x + mDragKey.t * r.width, r.y, 40, r.height);
                     NeedRepaint = true;
                }
                
                // Eat Down-Event to allow drag without opening Color Dialog
                if (lmbd && HandleRect.Contains(mp))
                    Event.current.Use();
                
                // End Drag or turn Up-Event into Down to trigger Color Dialog
                if (lmbu) {
                    if (mDragging && mDragKey==P.Colors[k]) {
                        mDragging = false;
                        mDragKey = null;
                        CreateColorGradient(mGradient, P);
                        Event.current.Use();
                    }
                    else if (HandleRect.Contains(mp)) {
                        Event.current.type = EventType.MouseDown;
                    }
                }
                
                Color c = P.Colors[k].Color;
                P.Colors[k].Color = EditorGUI.ColorField(HandleRect, P.Colors[k].Color);
                if (c != P.Colors[k].Color)
                    CreateColorGradient(mGradient, P);
                
            }
            // New Key?
            if (dbl && r.Contains(mp)) {
                P.AddColorKey((mp.x-r.x)/r.width,mGradient.GetPixel((int)(mp.x-r.x), 4));
                CreateColorGradient(mGradient,P);
                NeedRepaint = true;
                Event.current.Use();
            }
            
        }
         
        GUILayout.Box("", GUIStyle.none, new GUILayoutOption[] { GUILayout.MinWidth(40), GUILayout.ExpandWidth(false) });
        EditorGUILayout.EndHorizontal();
        
    }

    public void CreateColorGradient (Texture2D tex, MBParticleColorBase p)
    {
        Color col;
        for (int x = 0; x < tex.width; x++) {
            col = p.GetGradientColor(x / (float)tex.width,false);
            for (int y=0;y<tex.height;y++)
                tex.SetPixel(x,y,col);
        }
        tex.Apply();
    }
    
}

[MBParameterHandler(typeof(MBParticleColorFixed))]
public class MBEditorParticleColorFixedHandler : MBEditorParticleColorHandler
{
    public override void OnBirthGUI()
    {
        base.OnBirthGUI();
        MBParticleColorFixed P = Target as MBParticleColorFixed;            

        EditorGUILayout.BeginHorizontal();
        P.Colors[0].Color = MBGUI.DoColorField("Base", "", P.Colors[0].Color);
        P.ColorMode = (MBFixedColorMode)MBGUI.DoEnumField("Mode", "Color Mode", P.ColorMode);
        EditorGUILayout.EndHorizontal();
        if (P.ColorMode != MBFixedColorMode.Fixed) {
            P.AnimatedBirth = true;
            DoGradientGUI(Target as MBParticleColorBase);
        }
        else
            P.AnimatedBirth = false;
    }
    
}

[MBParameterHandler(typeof(MBParticleColorTimeline))]
public class MBEditorParticleColorTimelineHandler : MBEditorParticleColorHandler
{
    public MBEditorParticleColorTimelineHandler() 
    {
        HideBirthGUI = true;
    }

    public override void OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        DoGradientGUI(Target as MBParticleColorBase);
        GUILayout.Label("(DoubleClick: Create Key, RMB: Delete Key, Drag to move)");
    }
}