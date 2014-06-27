// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.Collections;

/// <summary>
/// Main editor class.
/// </summary>
/// <remarks>You usually don't need to do changes here. Instead create a class that inherites from
/// MBEditorEmitterTypeHandler or MBEditorParameterHandler to create your custom GUI
/// </remarks>
public class MBEditor : EditorWindow {
    static MBEditor _instance;
    static bool _NeedRepaint;
    static double _LastGUIUpdateTime;
    public static bool _ImportReadableTextures = false;

    public static MBEditor Instance
    {
        get
        {
            if (!_instance)
                ShowWindow();
            return _instance;
        }
    }

    public static bool EditorIsOpen { get { return _instance != null; } }

    /// <summary>
    /// The selected MBObject (if any)
    /// </summary>
    public static MBObject SelectedObject {get;set;}
    
    /// <summary>
    /// The ParticleSystem SelectedObject belongs to. Could be Null, SelectedObject itself or one of its parents
    /// </summary>
    public static MBParticleSystem SelectedParticleSystem
    {
        get { return (SelectedObject) ? SelectedObject.ParticleSystem : null; }
    }

    public static MBEmitter SelectedEmitter 
    { 
        get 
        {
            if (SelectedObject is MBEmitter)
                return (MBEmitter)SelectedObject;
            else if (SelectedObject is MBParameter)
                return (MBEmitter)SelectedObject.Parent;
            else
                return null;
        }
    }

    /// <summary>
    /// The selected overview object. As Parameters aren't shown there, this will return the parent emitter
    /// </summary>
    public static MBObject SelectedOverviewObject
    {
        get
        {
            if (SelectedObject) {
                MBObject selem = SelectedEmitter;
                return (selem) ? selem : SelectedObject;
            }
            else
                return null;
        }
    }


    /// <summary>
    /// To calculate ParticleSystem's DeltaTime in editor
    /// </summary>
    public static float LastEditorFrameTime;
    /// <summary>
    /// To handle slow motion
    /// </summary>
    static float mRealTime;

    public static EditorWindow GameView;

    // Overview / Toolbar / Other settings
    Vector2 mOverviewScroll;
    List<MBObject> mObjects = new List<MBObject>();
    List<int> mIndent = new List<int>();
    List<MBEmitter> mEmitters = new List<MBEmitter>();  // subset of mObjects only containing emitters
    Dictionary<System.Type, Texture> mObjectIcons = new Dictionary<System.Type, Texture>(); // stores symbols of MBObject-Types
    Dictionary<string, bool> mSectionToggleState = new Dictionary<string, bool>(); // stores toggle state of sections
    bool mParametersToggleState=true;    // used by "Toggle all Params"-Button
    bool mAutoSelect; // sync Selection
    bool mParametersAutoToggleSelected;
    static float mSlowMotion = 1.0f;
    
    MBGizmoFlags mGizmos;
    MBGizmoFlags mGizmosSelected;
    float mEditorUpdateInterval;
    bool mListeningForGUIChanges; // for Undo

    // Details
    Vector2 mDetailScroll;
    bool[] mEMGuiButton = new bool[3] { true, false,false };
    Vector2 mImageScroll;
    int mSelectedFrameIndex;
    

    /// <summary>
    /// Maps Handler-Type to Parameter/Emitter-Type
    /// </summary>
    Dictionary<System.Type, System.Type> mHandlerMap = new Dictionary<System.Type, System.Type>();
    // loaded Emittertype handler
    MBEditorEmitterTypeHandler mEmitterTypeHandler;
    
    /// <summary>
    /// List of instantiated handlers for the current emitter
    /// </summary>
    List<MBEditorParameterHandler> mHandler = new List<MBEditorParameterHandler>();
    

    // Styles / Colors
    GUIStyle mStyleHeaderStyle;
    GUIStyle mStyleBoldCentered;
    GUIStyle mStyleEmptyBox;
    Color mColDefault;
    Color mColOverviewSelected = new Color(0.5f, 0.9f, 0.5f);
    Color mColOverview = new Color(0.8f, 0.8f, 0.9f);
    Color mColHeader = new Color(0.7f, 0.7f, 0.8f);
    Color mColParameterInfo = new Color(0.8f, 0.8f, 0.9f);
    Color mColParameterHeader = new Color(0.9f, 0.9f, 1f);
    Color mColParameterHeaderSelected = new Color(0.5f, 0.9f, 0.5f);
    Color mColParameter = GUI.backgroundColor;
    Color mColParameterSelected = new Color(0.9f, 1f, 0.9f);
    Texture mTexLifeAnimated;
    Texture mTexBirthAnimated;
    Texture mTexClose;
    Texture mTexPlay;
    Texture mTexStop;
    Texture mTexHalt;
    Texture mTexSynchronize;
    Texture mTexToggleZoom;
    Texture mTexEMTrail;
    Texture mTexMuted;
    Texture mTexUnMuted;
    Texture mTexHourglass;
    Texture mTexAutoSelect;
    Texture mTexUpdateInterval;
    Texture mTexParamOrderUp;
    Texture mTexParamOrderDown;
    

    #region ### Unity Callbacks ###

    void Update()
    {
        if (_NeedRepaint && EditorApplication.timeSinceStartup-_LastGUIUpdateTime>mEditorUpdateInterval) {
            Repaint();
            _LastGUIUpdateTime = EditorApplication.timeSinceStartup;
            _NeedRepaint = false;
        }
        
    }


    void OnEnable()
    {
        _instance = this;
        autoRepaintOnSceneChange = true;
        
        mGizmos = (MBGizmoFlags)System.Enum.Parse(typeof(MBGizmoFlags), EditorPrefs.GetInt("MagicalBox_DrawGizmos").ToString());
        mGizmosSelected = (MBGizmoFlags)System.Enum.Parse(typeof(MBGizmoFlags), EditorPrefs.GetInt("MagicalBox_DrawGizmosSelected").ToString());
        mAutoSelect=EditorPrefs.GetBool("MagicalBox_AutoSelect");
        mEditorUpdateInterval = EditorPrefs.GetFloat("MagicalBox_EditorUpdateInterval");
        mParametersAutoToggleSelected = EditorPrefs.GetBool("MagicalBox_AutoToggleParams");
        mTexLifeAnimated = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/LifeAnim.png", typeof(Texture)) as Texture;
        mTexBirthAnimated = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/BirthAnim.png", typeof(Texture)) as Texture;
        mTexClose = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/Close.png", typeof(Texture)) as Texture;
        mTexPlay = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/Play.png", typeof(Texture)) as Texture;
        mTexStop = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/Stop.png", typeof(Texture)) as Texture;
        mTexHalt = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/Halt.png", typeof(Texture)) as Texture;
        mTexSynchronize = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/Synchronize.png", typeof(Texture)) as Texture;
        mTexToggleZoom = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/ToggleZoom.png", typeof(Texture)) as Texture;
        mTexEMTrail = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/MBEmitterTrail.png", typeof(Texture)) as Texture;
        mTexMuted = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/Mute.png", typeof(Texture)) as Texture;
        mTexUnMuted = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/UnMute.png", typeof(Texture)) as Texture;
        mTexHourglass = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/Hourglass.png", typeof(Texture)) as Texture;
        mTexAutoSelect = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/AutoSelect.png", typeof(Texture)) as Texture;
        mTexUpdateInterval = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/UpdateInterval.png", typeof(Texture)) as Texture;
        mTexParamOrderUp = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/ParamOrderUp.png", typeof(Texture)) as Texture;
        mTexParamOrderDown = Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/ParamOrderDown.png", typeof(Texture)) as Texture;

        // Get a list of classes that derive from MBObject
        Assembly asm = Assembly.GetAssembly(typeof(MBParameter));
        System.Type[] types = asm.GetTypes();

        foreach (System.Type T in types) {
            if (T.BaseType == typeof(MBObject)) {
                Texture tex = Resources.LoadAssetAtPath("Assets/Gizmos/" + T.Name + ".png", typeof(Texture)) as Texture;
                if (tex)
                    mObjectIcons.Add(T, tex);
            }
        }
        Initialize();
        
    }

    void OnDisable()
    {
        _instance = null;
        
        EditorPrefs.SetInt("MagicalBox_DrawGizmos", (int)mGizmos);
        EditorPrefs.SetInt("MagicalBox_DrawGizmosSelected", (int)mGizmosSelected);
        EditorPrefs.SetBool("MagicalBox_AutoSelect", mAutoSelect);
        EditorPrefs.SetFloat("MagicalBox_EditorUpdateInterval", mEditorUpdateInterval);
        EditorPrefs.SetBool("MagicalBox_AutoToggleParams", mParametersAutoToggleSelected);
    }
    
    void OnHierarchyChange()
    {
       //MBEditorCommands.SyncHierarchy();
    }

    
    void OnGUI()
    {
        if (!_instance) return;
        
        // Check if window was open on recompile. If so, we need to reinitialize
        if (mStyleHeaderStyle == null) {
            mStyleHeaderStyle = new GUIStyle(GUI.skin.GetStyle("button"));
            mStyleHeaderStyle.fontStyle = FontStyle.Bold;

            mStyleBoldCentered = new GUIStyle(GUI.skin.GetStyle("label"));
            mStyleBoldCentered.fontStyle = FontStyle.Bold;
            mStyleBoldCentered.alignment = TextAnchor.MiddleCenter;
            mStyleBoldCentered.stretchWidth = true;

            mStyleEmptyBox = new GUIStyle(GUI.skin.GetStyle("box"));
            mStyleEmptyBox.margin = new RectOffset(0, 0, 0, 0);
            mStyleEmptyBox.padding = new RectOffset(0, 0, 0, 0);
        }

        if (!SelectedParticleSystem) return;

        mColDefault = GUI.backgroundColor;
        Event e = Event.current;
        Object[] deephierarchy=new Object[0];
        if ((e.type == EventType.MouseDown && e.button == 0) || (e.type == EventType.KeyUp && (e.keyCode == KeyCode.Tab))) {
            deephierarchy=EditorUtility.CollectDeepHierarchy(new Object[]{SelectedObject});
            Undo.SetSnapshotTarget(deephierarchy, "Magical Box value change");
            Undo.CreateSnapshot();
            Undo.ClearSnapshotTarget();
            mListeningForGUIChanges = true;
        }
        // Store mouse position for editor enabled scripts
        if (!Application.isPlaying) {
            MBEditorEnabledScript.MousePosition = new Vector3(
                e.mousePosition.x, -e.mousePosition.y+25, 0);
        }
        
        // Top row holds Toolbar
        EditorGUILayout.BeginHorizontal();
        DoToolbarGUI();
        EditorGUILayout.EndHorizontal();
        // Left window side shows the overview
        GUILayout.BeginArea(new Rect(0, 26, 204, position.height - 26), GUI.skin.GetStyle("Box"));
        mOverviewScroll = EditorGUILayout.BeginScrollView(mOverviewScroll);
        DoOverviewGUI();
        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
        
        // Right window side shows details of selection
        GUI.backgroundColor = mColDefault;
        GUILayout.BeginArea(new Rect(205, 26, position.width - 205, position.height - 26), GUI.skin.GetStyle("Box"));
        if (SelectedObject.Matches(typeof(MBEmitter), typeof(MBParameter)))
            DoEmitterGUI();
        else if (SelectedObject is MBParticleSystem)
            DoSystemGUI();
        else if (SelectedObject is MBAnchor)
            DoAnchorGUI();
        else if (SelectedObject is MBLayer)
            DoLayerGUI();

        GUILayout.EndArea();
        
        // Control Undo
        if (mListeningForGUIChanges && GUI.changed) {
            Undo.SetSnapshotTarget(deephierarchy, "Magical Box value change");
            Undo.RegisterSnapshot();
            Undo.ClearSnapshotTarget();
            mListeningForGUIChanges = false;
        }

        GUI.backgroundColor = mColDefault;
    }

    void OnSelectionChange()
    {
        MBObject obj = (Selection.activeGameObject) ? Selection.activeGameObject.GetComponent<MBObject>() : null;
        
        if (SelectedObject is MBParameter)
            Select(Selection.activeGameObject);
        else if (obj is MBObject)
            Select(obj);
    }

    #endregion

    #region ### Editor Hooks & Hierarchy updates ###

    public static void Select(Object obj)
    {
        // I'm sure this function can be written with fewer lines of code, but we want to keep this clearly layouted
        GameObject newGO = obj as GameObject;
        MBObject newObj = (newGO) ? newGO.GetComponent<MBObject>() : obj != null ? (MBObject)obj : null;
        // Null Prefabs
        if (newObj && PrefabUtility.GetPrefabType(newObj) == PrefabType.Prefab)
            newObj = null;
        
        if (newObj == SelectedObject) return; // nothing changed, exit!        

        MBParticleSystem newSys = (newObj) ? newObj.ParticleSystem : null;

        bool InitAll = true;
        bool InitEM = false;
        
        // Case I: Leaving a system to a non MB object 
        if (SelectedParticleSystem && !newSys) {
            if (!Application.isPlaying)
                SelectedParticleSystem.Halt();
            // detach hook
            EditorApplication.update -= EditorUpdate;
            //EditorApplication.playmodeStateChanged -= OnPlaymodeChange;
        } else
        // Case II: Entering a system from a non MB object
        if (!SelectedParticleSystem && newSys) {
            if (!Application.isPlaying){
                // attach hook
                
                EditorApplication.update -= EditorUpdate;
                EditorApplication.update += EditorUpdate;
                EditorApplication.playmodeStateChanged -= OnPlaymodeChange;
                EditorApplication.playmodeStateChanged += OnPlaymodeChange;
                // Load System
                newSys.mbReloadHierarchy();
                newSys.GenerateTextureMap(true);
                // Initialize Timing
                mRealTime = (float)EditorApplication.timeSinceStartup;
                LastEditorFrameTime = mRealTime;
                newSys.GlobalTime = mRealTime;
            }
            
        } else
        // Case III: Switching to another system
        if (SelectedParticleSystem && newSys && SelectedParticleSystem != newSys) {
            if (!Application.isPlaying)
                SelectedParticleSystem.Halt();
            
            // Load System
            newSys.mbReloadHierarchy();
            newSys.GenerateTextureMap(true);
            // Initialize Timing
            newSys.GlobalTime = mRealTime;
        }
        else {
            // CASE IV: Selecting within system
            InitAll = false;
            InitEM = (newObj is MBEmitter || (newObj is MBParameter && newObj.Parent != SelectedEmitter));
        }

        SelectedObject = newObj;
        
        if (_instance) {
            if (InitAll)
                _instance.Initialize();
            if (InitEM)
                _instance.InitializeEmitter();
            if (_instance.mAutoSelect && SelectedObject)
                Selection.activeGameObject = SelectedObject.gameObject;
            
            if (SelectedObject is MBParameter)
                _instance.SelectParameterTab();

            RepaintEditor(false);
        }
            
    }

    public static void EditorUpdate()
    {
        if (EditorApplication.isCompiling) {
            Select(null);
            if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<MBObject>())
                Selection.activeGameObject = null;
        }

        if (EditorApplication.isPlaying || !SelectedParticleSystem) return;

        // Time.time & Time.deltatime won't run in editor, so set them here
        if (!SelectedParticleSystem.Warping) {
            float elapsedRealtime = (float)EditorApplication.timeSinceStartup-mRealTime;
            mRealTime = (float)EditorApplication.timeSinceStartup;
            SelectedParticleSystem.GlobalTime += elapsedRealtime * MBEditor.mSlowMotion;
            SelectedParticleSystem.DeltaTime = SelectedParticleSystem.GlobalTime - LastEditorFrameTime;//Mathf.Min(SelectedParticleSystem.GlobalTime - LastEditorFrameTime, 1f);
            LastEditorFrameTime = SelectedParticleSystem.GlobalTime; // last GT
            //Old timing w/o slomotion:
            //SelectedParticleSystem.GlobalTime = (float)EditorApplication.timeSinceStartup;
            //SelectedParticleSystem.DeltaTime = Mathf.Min(SelectedParticleSystem.GlobalTime - LastEditorFrameTime,2f);
            //LastEditorFrameTime = SelectedParticleSystem.GlobalTime;
        }

        if (SelectedParticleSystem.AutoPlay && !SelectedParticleSystem.Playing) 
            SelectedParticleSystem.Play();
        

        SelectedParticleSystem.mbUpdate();
        SelectedParticleSystem.mbRender();

        if (!GameView)
            GameView = MBEditorUtility.GetGameViewWindow();
        
        if (GameView)
           GameView.Repaint();
        
        if (GameView && SceneView.focusedWindow == GameView)
            SelectedParticleSystem.mbEditorCamera = null;
        if (SceneView.lastActiveSceneView)
            SceneView.lastActiveSceneView.Repaint();
        
    }

    public static void OnPlaymodeChange()
    {
        // Unselect Editor object on playmode change
            Selection.activeObject = null;
            Select(null);
        /*
        if (!Application.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode) {
            // EDITMODE: about to start playing
                if (SelectedEmitter)
                    Select(SelectedParticleSystem);
        } else 
        if (!Application.isPlaying) {
            if (SelectedObject) {
                Selection.activeObject = null;
                Select(null);
            }
        }
         */
    }

    #endregion

    #region ### Public Methods and Properties ###

    public static void SnapshotScene(string name)
    {
        //Will be working in the next version. Right now reverting a snapshot may cause errors here and there...
        //Undo.RegisterSceneUndo(name);
    }

    public static void ShowWindow()
    {
        MBEditor win=GetWindow<MBEditor>(false, "Magical Box", false);
        win.maxSize = new Vector2(1400, 1400);
    }

    
    #endregion

    #region ### Privates ###

    Texture ObjectIcon (System.Type type)
    {
        return (mObjectIcons.ContainsKey(type)) ? mObjectIcons[type] : null;
    }

    public void Initialize()
    {
        mObjects.Clear();
        mEmitters.Clear();
        mIndent.Clear();
        ClearHandlers();
        mSectionToggleState.Clear();
        mEmitterTypeHandler = null;
        
        if (SelectedParticleSystem) {
            SelectedParticleSystem.mbDrawGizmos = mGizmos;
            SelectedParticleSystem.mbDrawGizmosSelected = mGizmosSelected;

            InitializeHandlerMap();
            
            InitializeTree();
            InitializeEmitter();
        }

        RepaintEditor(true);
    }

    void InitializeEmitter()
    {
        InstantiateEMTypeHandler();
        InstantiateParameterHandler();
        RepaintEditor(false);
    }

    void InitializeHandlerMap()
    {
        mHandlerMap.Clear();
        // get a list of all existing Parameter/EmitterType-Handlers
        Assembly asm = Assembly.GetAssembly(typeof(MBEditorParameterHandler));
        foreach (System.Type T in asm.GetTypes()) {
            System.Attribute pa = System.Attribute.GetCustomAttribute(T, typeof(MBParameterHandler));
            if (pa != null)
                mHandlerMap.Add(((MBParameterHandler)pa).ParameterType, T);

            System.Attribute ea = System.Attribute.GetCustomAttribute(T, typeof(MBEmitterTypeHandler));
            if (ea != null)
                mHandlerMap.Add(((MBEmitterTypeHandler)ea).EmitterType, T);

        }
    }

    public void InitializeTree()
    {
        if (SelectedParticleSystem) {
            mObjects.Clear();
            mEmitters.Clear();
            mIndent.Clear();
            FetchTree(SelectedParticleSystem, 0);
            RepaintEditor(true);
        }
    }

    public static void RepaintEditor(bool immediately)
    {
        _NeedRepaint = true;

        if (immediately)
            _LastGUIUpdateTime = 0;
    }

    void FetchTree(MBObject obj, int indent)
    {
        // skip Parameters and EmitterTypes!
        if (obj.Matches(typeof(MBParameter),typeof(MBEmitterType))) return;
        // Add self
        mObjects.Add(obj);
        mIndent.Add(indent);

        // store Emitters in a additional list for easier access
        if (obj is MBEmitter)
            mEmitters.Add((MBEmitter)obj);
        // Get list of smart childs
        List<MBObject> childs = MBUtility.GetChildren<MBObject>(obj.transform, true, true);
        // Sort them in a nice order
        childs.Sort();
        // Recursive call children
        foreach (MBObject sub in childs)
            FetchTree(sub, indent + 1);
    }
    
    void DoToolbarGUI()
    {
        if (MBGUI.DoButton(new GUIContent(mTexClose,"Close Magical Box editor"), true, false))
            Close();
        GUILayout.Space(10);
        if (MBGUI.DoButton(new GUIContent(mTexPlay, "Play ParticleSystem"), SelectedParticleSystem != null, false)) {
            if (!SelectedParticleSystem.Paused)
                SelectedParticleSystem.Halt();
            SelectedParticleSystem.Play();
        }
        if (MBGUI.DoButton(new GUIContent(mTexStop, "Stop ParticleSystem"), SelectedParticleSystem != null && SelectedParticleSystem.Playing, false))
            SelectedParticleSystem.Stop();
        if (MBGUI.DoButton(new GUIContent(mTexHalt, "Halt ParticleSystem"), SelectedParticleSystem != null && SelectedParticleSystem.Playing, false))
            SelectedParticleSystem.Halt();
        GUILayout.Space(10);
        if (MBGUI.DoButton(new GUIContent(mTexPlay, "Play Selected"), MBEditorCommands.CanPlayObject(), false)) 
            MBEditorCommands.PlayObject();

        if (MBGUI.DoButton(new GUIContent(mTexStop, "Stop Selected"), MBEditorCommands.CanStopObject(), false))
            MBEditorCommands.StopObject();
        if (MBGUI.DoButton(new GUIContent(mTexHalt, "Halt Selected"), MBEditorCommands.CanHaltObject(), false))
            MBEditorCommands.HaltObject();
        GUILayout.Space(10);
        if (MBGUI.DoButton(new GUIContent(mTexSynchronize, "Resync"), SelectedParticleSystem != null, false)) 
            MBEditorCommands.SyncHierarchy();

        

        // DrawGizmo-Flags
        MBGUI.DoLabel("Gizmos:");
        bool sys = (mGizmos & MBGizmoFlags.MBParticleSystem) == MBGizmoFlags.MBParticleSystem;
        bool anc = (mGizmos & MBGizmoFlags.MBAnchor) == MBGizmoFlags.MBAnchor;
        bool em = (mGizmos & MBGizmoFlags.MBEmitter) == MBGizmoFlags.MBEmitter;
        bool par = (mGizmos & MBGizmoFlags.MBParameter) == MBGizmoFlags.MBParameter;
        sys = MBGUI.DoToggleButton(new GUIContent(ObjectIcon(typeof(MBParticleSystem)), "All ParticleSystems"), sys);
        anc = MBGUI.DoToggleButton(new GUIContent(ObjectIcon(typeof(MBAnchor)), "All Anchors"), anc);
        em = MBGUI.DoToggleButton(new GUIContent(ObjectIcon(typeof(MBEmitter)), "All Emitters"), em);
        par = MBGUI.DoToggleButton(new GUIContent(ObjectIcon(typeof(MBParameter)), "All Parameters"), par);
        if (sys)
            mGizmos |= MBGizmoFlags.MBParticleSystem;
        else
            mGizmos &= ~MBGizmoFlags.MBParticleSystem;
        if (anc)
            mGizmos |= MBGizmoFlags.MBAnchor;
        else
            mGizmos &= ~MBGizmoFlags.MBAnchor;
        if (em)
            mGizmos |= MBGizmoFlags.MBEmitter;
        else
            mGizmos &= ~MBGizmoFlags.MBEmitter;
        if (par)
            mGizmos |= MBGizmoFlags.MBParameter;
        else
            mGizmos &= ~MBGizmoFlags.MBParameter;
        
        // DrawGizmoSelected-Flags
        MBGUI.DoLabel("Selected:");
        sys = (mGizmosSelected & MBGizmoFlags.MBParticleSystem) == MBGizmoFlags.MBParticleSystem;
        anc = (mGizmosSelected & MBGizmoFlags.MBAnchor) == MBGizmoFlags.MBAnchor;
        em = (mGizmosSelected & MBGizmoFlags.MBEmitter) == MBGizmoFlags.MBEmitter;
        par = (mGizmosSelected & MBGizmoFlags.MBParameter) == MBGizmoFlags.MBParameter;
        sys = MBGUI.DoToggleButton(new GUIContent(ObjectIcon(typeof(MBParticleSystem)), "Selected ParticleSystem"), sys);
        anc = MBGUI.DoToggleButton(new GUIContent(ObjectIcon(typeof(MBAnchor)), "Selected Anchor"), anc);
        em = MBGUI.DoToggleButton(new GUIContent(ObjectIcon(typeof(MBEmitter)), "Selected Emitter"), em);
        par = MBGUI.DoToggleButton(new GUIContent(ObjectIcon(typeof(MBParameter)), "Selected Parameter"), par);
        if (sys)
            mGizmosSelected |= MBGizmoFlags.MBParticleSystem;
        else
            mGizmosSelected &= ~MBGizmoFlags.MBParticleSystem;
        if (anc)
            mGizmosSelected |= MBGizmoFlags.MBAnchor;
        else
            mGizmosSelected &= ~MBGizmoFlags.MBAnchor;
        if (em)
            mGizmosSelected |= MBGizmoFlags.MBEmitter;
        else
            mGizmosSelected &= ~MBGizmoFlags.MBEmitter;
        if (par)
            mGizmosSelected |= MBGizmoFlags.MBParameter;
        else
            mGizmosSelected &= ~MBGizmoFlags.MBParameter;

        GUILayout.Space(10);
        mAutoSelect = MBGUI.DoToggleButton(new GUIContent(mTexAutoSelect, "Synchronize Hierarchy when using the Overview"), mAutoSelect);
        GUILayout.Space(10);
        mSlowMotion = MBGUI.DoFloatSliderSmall(new GUIContent(mTexHourglass, "Slow Motion %"), mSlowMotion, 0f, 1f);
        mEditorUpdateInterval = MBGUI.DoFloatFieldSmall(new GUIContent(mTexUpdateInterval, "Editor update interval in seconds (0=instant)"), mEditorUpdateInterval,true);

        SelectedParticleSystem.mbDrawGizmos = mGizmos;
        SelectedParticleSystem.mbDrawGizmosSelected = mGizmosSelected;
    }

    void DoOverviewGUI()
    {
        if (!SelectedOverviewObject) return;

        DoSectionHeader("Overview", mColHeader,false);
        Event evt=Event.current;
        //Draw a tree like button list holding the current system and all emitters

        for (int i = 0; i < mObjects.Count; i++) {
            if (mObjects[i] != null) {
                if (mObjects[i] == SelectedOverviewObject) {
                    GUI.backgroundColor = mColOverviewSelected;
                    EditorGUILayout.BeginHorizontal(mStyleEmptyBox, GUILayout.ExpandWidth(true));
                }
                else {
                    GUI.backgroundColor = mColOverview;
                    EditorGUILayout.BeginHorizontal();
                }

                GUILayout.Space(mIndent[i] * 20);
                GUIContent content;
                if (mObjectIcons.ContainsKey(mObjects[i].GetType()))
                    if (mObjects[i] is MBEmitter && ((MBEmitter)mObjects[i]).IsTrail)
                        content = new GUIContent(mObjects[i].name, mTexEMTrail);
                    else
                        content = new GUIContent(mObjects[i].name, ObjectIcon(mObjects[i].GetType()));
                else
                    content = new GUIContent(mObjects[i].name);
                if (GUILayout.Button(content, EditorStyles.label, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.Width(173 - mIndent[i] * 20) })) {
                    Select(mObjects[i]);
                    if (evt.button == 1)
                        EditorUtility.DisplayPopupMenu(new Rect(evt.mousePosition.x, evt.mousePosition.y, 100, 100), "Window/Magical Box/Object", null);
                }
                if (mObjects[i].Matches(typeof(MBEmitter), typeof(MBLayer)) && GUILayout.Button(new GUIContent(mObjects[i].Muted ? mTexMuted : mTexUnMuted, "Mute/Unmute"), "label", GUILayout.Width(16))) {
                    mObjects[i].Muted = !mObjects[i].Muted;
                }
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = mColDefault;
            }
        }
    }

    void DoAnchorGUI()
    {
        MBAnchor anc = SelectedObject as MBAnchor;
        
        mDetailScroll = EditorGUILayout.BeginScrollView(mDetailScroll);
        // GENERAL SECTION
        EditorGUILayout.BeginVertical(GUI.skin.GetStyle("Box"));
        if (DoSectionHeader("General Settings", mColHeader, true)) {
            EditorGUILayout.BeginHorizontal();
            string tempname = MBGUI.DoTextField("Name", "", SelectedObject.name);
            if (MBGUI.HasChanged) SelectedObject.name = tempname;
            EditorGUILayout.EndHorizontal();
            anc.PoolingEnabled = MBGUI.DoToggle("Enable Pooling", "Use this anchor as an pooling manager?", anc.PoolingEnabled);
            if (anc.PoolingEnabled) {
                if (DoSectionHeader("Pooling Manager", mColHeader, true)) {
                    MBGUI.DoLabel("NOTE: The first child is used as the pool source, so it must be an emitter!", 400, true);
                    EditorGUILayout.BeginHorizontal();
                        anc.MinPoolSize = MBGUI.DoIntField("Min Pool Size", "Minimum stock size", anc.MinPoolSize);
                        anc.MaxPoolSize = MBGUI.DoIntField("Max Pool Size", "Max stock size", anc.MaxPoolSize);
                        anc.OnMaxPoolSize = (MBAnchorPoolExceededMode)MBGUI.DoEnumField("On Max PoolSize", "Behaviour if max. pool size exceeds", anc.OnMaxPoolSize);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                        anc.AllocationBlockSize = MBGUI.DoIntField("Alloc Block Size", "Items processed at once when the pool needs to grow/shrink", anc.AllocationBlockSize);
                        anc.AutoDespawn = MBGUI.DoToggle("Auto Despawn", "Despawn emitters automatically when they stops playing", anc.AutoDespawn);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                        anc.AutoCull = MBGUI.DoToggle("Auto Culling", "Cull items automatically", anc.AutoCull);
                        anc.CullingSpeed = MBGUI.DoFloatField("Culling Speed", "Time between culls in seconds", anc.CullingSpeed);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    void DoLayerGUI()
    {
        mDetailScroll = EditorGUILayout.BeginScrollView(mDetailScroll);
        EditorGUILayout.BeginVertical(GUI.skin.GetStyle("Box"));
        MBLayer lyr=SelectedObject as MBLayer;
        if (DoSectionHeader(string.Format("Layer: {0} ({1})",lyr.name,lyr.Material.shader.name), mColHeader, true)) {
            EditorGUILayout.BeginHorizontal();
            string tempname = MBGUI.DoTextField("Name", "", SelectedObject.name);
            if (MBGUI.HasChanged) SelectedObject.name = tempname;
            lyr.RenderQueue=MBGUI.DoIntField("RenderQueue", "Rendering order", lyr.RenderQueue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
                int oldth = lyr.LayerThreshold;
                lyr.LayerThreshold = MBGUI.DoIntField("Threshold", "Quad Buffer Size", lyr.LayerThreshold);
                if (oldth != lyr.LayerThreshold)
                    lyr.Purge();
                lyr.LayerBlocksize = MBGUI.DoIntField("Blocksize", "Blocksize when adding", lyr.LayerBlocksize);
            EditorGUILayout.EndHorizontal();
            lyr.FreezeWhenCulled = MBGUI.DoToggle("Freeze when culled?", "Freeze emitters when mesh is invisible?", lyr.FreezeWhenCulled);
        }
        if (DoSectionHeader("Used in Emitters",mColHeader,true)){
            foreach (MBEmitter em in lyr.Emitter) {
                MBGUI.DoLabelButton(new GUIContent(em.name, (em.IsTrail) ? mTexEMTrail : ObjectIcon(typeof(MBEmitter))), true);
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    void DoSystemGUI()
    {
            mDetailScroll = EditorGUILayout.BeginScrollView(mDetailScroll);
            // GENERAL SECTION
            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("Box"));
            if (DoSectionHeader("General Settings", mColHeader, true)) {
                EditorGUILayout.BeginHorizontal();
                string tempname = MBGUI.DoTextField("Name", "", SelectedParticleSystem.name);
                if (MBGUI.HasChanged) SelectedParticleSystem.name = tempname;
                int oldth = SelectedParticleSystem.ParticleThreshold;
                SelectedParticleSystem.ParticleThreshold = MBGUI.DoIntField("Threshold", "Particle Buffer Size", SelectedParticleSystem.ParticleThreshold);
                if (oldth != SelectedParticleSystem.ParticleThreshold)
                    SelectedParticleSystem.Purge();
                SelectedParticleSystem.ParticleBlocksize = MBGUI.DoIntField("Blocksize", "Blocksize when adding", SelectedParticleSystem.ParticleBlocksize);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                SelectedParticleSystem.AutoPlay = MBGUI.DoToggle("AutoPlay", "Start playing when GameObject becomes enabled?", SelectedParticleSystem.AutoPlay);
                SelectedParticleSystem.SingleShot = MBGUI.DoToggle("SingleShot", "Play only once?", SelectedParticleSystem.SingleShot);
                SelectedParticleSystem.Infinite = MBGUI.DoToggle("Infinite", "Ignore Duration?", SelectedParticleSystem.Infinite);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();

                int oldp = SelectedParticleSystem.TexturePadding;
                SelectedParticleSystem.TexturePadding = Mathf.Max(0, MBGUI.DoIntField("Padding", "Texture padding", SelectedParticleSystem.TexturePadding));
                if (oldp != SelectedParticleSystem.TexturePadding)
                    SelectedParticleSystem.GenerateTextureMap(true);
                SelectedParticleSystem.PersistentAtlas = MBGUI.DoToggle("Persistent Atlas", "Don't build atlas at start?", SelectedParticleSystem.PersistentAtlas);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            // TIMING
            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("Box"));
            if (DoSectionHeader("Timing", mColHeader, true)) {
                EditorGUILayout.BeginHorizontal();
                SelectedParticleSystem.TimeWarp = Mathf.Max(0, MBGUI.DoFloatField("Timewarp", "Advance without rendering", SelectedParticleSystem.TimeWarp, true));
                SelectedParticleSystem.TimeWarpStepSize = MBGUI.DoFloatField("StepSize", "Timewarp step size", SelectedParticleSystem.TimeWarpStepSize, true);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(155);
                GUILayout.HorizontalSlider(SelectedParticleSystem.AgePercent, 0, 1, new GUILayoutOption[] { GUILayout.Width(300), GUILayout.ExpandWidth(false) });
                MBGUI.DoLabel(((int)(SelectedParticleSystem.Age * 10) / 10.0f).ToString());
                EditorGUILayout.EndHorizontal();
                // TIMELINE-Editor
                //Draw a tree like button list holding the current system and all emitters

                EditorGUILayout.BeginHorizontal();
                MBGUI.DoLabelButton(new GUIContent(SelectedParticleSystem.name, ObjectIcon(typeof(MBParticleSystem)), ""), 150, true);
                float dummy = 0, dummy2 = 1;
                EditorGUILayout.MinMaxSlider(ref dummy, ref dummy2, 0, 1, GUILayout.Width(300));
                GUILayout.Space(125);
                //SYS-Duration
                float dur = SelectedParticleSystem.Duration;
                SelectedParticleSystem.Duration = MBGUI.DoFloatFieldSmall(new GUIContent("Duration", "Total Loop-Time"), SelectedParticleSystem.Duration, false);
                if (dur != SelectedParticleSystem.Duration)
                    SelectedParticleSystem.mbCalculateRuntime();
                if (MBGUI.DoButton("Auto", "Calculate time from emitters", true)) {
                    SelectedParticleSystem.Duration = 0;
                    SelectedParticleSystem.mbCalculateRuntime();
                }

                EditorGUILayout.EndHorizontal();

                for (int e = 0; e < mEmitters.Count; e++) {
                    MBEmitter em = mEmitters[e];
                    if (em) {
                        EditorGUILayout.BeginHorizontal();
                        MBGUI.DoLabelButton(new GUIContent(em.name, (em.IsTrail) ? mTexEMTrail : ObjectIcon(typeof(MBEmitter))), 150, true);
                        dur = em.Duration;
                        float combined = em.Delay + em.Duration;
                        EditorGUILayout.MinMaxSlider(ref em.Delay, ref combined, 0, SelectedParticleSystem.Duration, GUILayout.Width(300));
                        em.Duration = combined - em.Delay;
                        em.Delay = MBGUI.DoFloatFieldSmall(new GUIContent("Delay", "Time until playing begins"), em.Delay, false);
                        em.Duration = MBGUI.DoFloatFieldSmall(new GUIContent("Duration", "Emitting time"), em.Duration, false);
                        if (dur != em.Duration)
                            SelectedParticleSystem.mbCalculateRuntime();
                        if (MBGUI.DoButton("Max", "Set duration to match ParticleSystem's runtime", true))
                            em.Duration = SelectedParticleSystem.Duration - em.Delay;
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUILayout.EndVertical();
            if (SelectedParticleSystem.Playing)
                RepaintEditor(false);
            // STATISTICS
            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("Box"));
            if (DoSectionHeader("Statistics", mColHeader, true)) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(150);
                MBGUI.DoLabel("Active", 50, true);
                MBGUI.DoLabel("Idle", 50, true);
                MBGUI.DoLabel("Max Rndr", 85, true);
                MBGUI.DoLabel("Death: Age", 85, true);
                MBGUI.DoLabel("Color", 50, true);
                MBGUI.DoLabel("Size", 50, true);
                MBGUI.DoLabel("Zone", 50, true);
                MBGUI.DoLabel("User", 50, true);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                MBGUI.DoLabelButton(new GUIContent(SelectedParticleSystem.name, ObjectIcon(typeof(MBParticleSystem)), ""), 170, true);
                MBGUI.DoLabel(SelectedParticleSystem.ParticleCount.ToString(), 30, false);
                MBGUI.DoLabel(SelectedParticleSystem.ParticleCountIdle.ToString(), 70, false);
                MBGUI.DoLabel(SelectedParticleSystem.ParticlesRenderedMax.ToString(), 85, false);
                GUILayout.Space(265);
                if (MBGUI.DoButton("Max->Threshold", "Set Particle treshold to max. Rendered", true)) {
                    SelectedParticleSystem.ParticleThreshold = SelectedParticleSystem.ParticlesRenderedMax;
                    SelectedParticleSystem.Purge();
                }
                EditorGUILayout.EndHorizontal();
                for (int l = 0; l < SelectedParticleSystem.Layer.Count; l++) {
                    MBLayer lyr = SelectedParticleSystem.Layer[l];
                    if (lyr) {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        MBGUI.DoLabelButton(new GUIContent(lyr.name, ObjectIcon(typeof(MBLayer)), ""), 150, true);
                        MBGUI.DoLabel(lyr.ParticlesRendered.ToString(), 50, false);
                        GUILayout.Space(55);
                        MBGUI.DoLabel(lyr.ParticlesRenderedMax.ToString(), 50, false);
                        GUILayout.Space(300);
                        if (MBGUI.DoButton("Max->Threshold", "Set layer treshold to max. Rendered", true)) {
                            lyr.LayerThreshold = lyr.ParticlesRenderedMax;
                            lyr.Purge();
                        }
                        EditorGUILayout.EndHorizontal();
                        for (int e = 0; e < lyr.Emitter.Count; e++) {
                            MBEmitter em = lyr.Emitter[e];
                            if (em) {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(40);
                                MBGUI.DoLabelButton(new GUIContent(em.name, (em.IsTrail) ? mTexEMTrail : ObjectIcon(typeof(MBEmitter))), MBGUI._DefaultWidth - 40, true);
                                MBGUI.DoLabel(em.ParticleCount.ToString(), 30, false);
                                GUILayout.Space(50);
                                MBGUI.DoLabel(em.DeathByAge.ToString(), 70, false);
                                MBGUI.DoLabel(em.DeathByColor.ToString(), 50, false);
                                MBGUI.DoLabel(em.DeathBySize.ToString(), 50, false);
                                MBGUI.DoLabel(em.DeathByZone.ToString(), 50, false);
                                MBGUI.DoLabel(em.DeathByUser.ToString(), 50, false);
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
    }

    public void SelectParameterTab()
    {
        mEMGuiButton[1] = true;
        mEMGuiButton[0] = !mEMGuiButton[1];
        mEMGuiButton[2] = !mEMGuiButton[1];
    }

    public void SelectEmitterTypeTab()
    {
        mEMGuiButton[0] = true;
        mEMGuiButton[1] = !mEMGuiButton[0];
        mEMGuiButton[2] = !mEMGuiButton[0];
        Select(SelectedEmitter);
    }

    public void SelectDebuggingTab()
    {
        mEMGuiButton[2] = true;
        mEMGuiButton[0] = !mEMGuiButton[2];
        mEMGuiButton[1] = !mEMGuiButton[2];
    }

    

    void DoEmitterGUI()
    {
        MBEmitter em = (SelectedObject is MBEmitter) ? SelectedObject as MBEmitter: (SelectedObject as MBParameter).ParentEmitter;
        if (!em) return;

        if (mEmitterTypeHandler == null)
            InstantiateEMTypeHandler();
        EditorGUILayout.BeginHorizontal();

                GUI.backgroundColor = mEMGuiButton[0] ? mColOverviewSelected : mColDefault;
                if (GUILayout.Button(new GUIContent("Emittertype/Image"))) {
                        SelectEmitterTypeTab();
                }
                GUI.backgroundColor = mEMGuiButton[1] ? mColOverviewSelected : mColDefault;
                if (GUILayout.Button(new GUIContent("Parameters"))) {
                    SelectParameterTab();
                }
                GUI.backgroundColor = mEMGuiButton[2] ? mColOverviewSelected : mColDefault;
                if (GUILayout.Button(new GUIContent("Debugging"))) {
                    SelectDebuggingTab();
                }
                GUI.backgroundColor = mColDefault;
        EditorGUILayout.EndHorizontal();


        if (mEMGuiButton[1])
            DoParameterGUI(em);
        else if (mEMGuiButton[2])
            DoDebuggingGUI(em);
        else if (mEmitterTypeHandler != null) {
            #region Emitter
            // Emitter
            mDetailScroll = EditorGUILayout.BeginScrollView(mDetailScroll);
            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("Box"));
            if (DoSectionHeader(string.Format("Emittertype: ({0})", mEmitterTypeHandler.EmitterTypeInfo.Menu), mColHeader, true)) {
                #region EmitterType
                EditorGUILayout.BeginHorizontal();
                string tempname = MBGUI.DoTextField("Name", "", em.name);
                if (MBGUI.HasChanged) em.name = tempname;

                if (MBGUI.DoButton("Change Type", "Replace the current EmitterType", true))
                    AddEmitterTypeMenu();
                string lyrname = (em.Layer) ? em.Layer.name : "NONE";
                if (MBGUI.DoButton("Layer: " + lyrname, "Change the current layer", true))
                    LayerSelectMenu();

                // Only sub-emitters can be trails
                bool trail = em.IsTrail;
                em.IsTrail = (em.Parent is MBEmitter) ? MBGUI.DoToggle("Is Trail?", "Use this emitter as trail of its parent?", em.IsTrail) : false;
                if (trail != em.IsTrail)
                    em.Position = Vector3.zero;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                em.AutoPlay = MBGUI.DoToggle("AutoPlay", "Play when Particlesystem starts playing?", em.AutoPlay);
                em.AutoRepeat = MBGUI.DoToggle("AutoRepeat", "Loop emitter and ignore ParticleSystem's duration", em.AutoRepeat);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                em.Billboard = (MBBillboard)MBGUI.DoEnumField("Billboard", "Set billboarding method", em.Billboard);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                em.LaunchRateAnimated = MBGUI.DoToggle("Animate", "Animate launch rate?", em.LaunchRateAnimated);
                if (em.LaunchRateAnimated) {
                    em.LaunchRateCurve = MBGUI.DoCurve("Launch Rate", "Launch Rate over age", em.LaunchRateCurve, em.LaunchRate, em.LaunchRate);
                    MBGUI.LimitCurveValue(ref em.LaunchRateCurve, 0, 99999999);
                }
                else {
                    em.LaunchRate = Mathf.Max(0, MBGUI.DoFloatField("Launch Rate", "Emission Rate in Particles/s", em.LaunchRate));
                    em.LaunchRandom = MBGUI.DoFloatSlider("Random %", "Random deviation from Launch rate", em.LaunchRandom, 0, 1);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                em.InstantLaunch = Mathf.Max(0, MBGUI.DoIntField("Instant Launch", "Instant Launch", em.InstantLaunch));
                em.InstantLaunchRepeat = MBGUI.DoToggle("Repeat", "Use on AutoRepeat?", em.InstantLaunchRepeat);
                em.LaunchBuffer = MBGUI.DoIntField("Buffer", "Particles spawn once LaunchRate fills the buffer", em.LaunchBuffer);
                EditorGUILayout.EndHorizontal();

                // EmitterType-GUI
                mEmitterTypeHandler.OnGUI();
                mEmitterTypeHandler.Target.Validate();
                #endregion
            }
            EditorGUILayout.EndVertical();

            if (DoSectionHeader("Texture(s)", mColHeader, true)) {
                EditorGUILayout.BeginHorizontal();
                Vector2 oldPivot = em.ImagePivot;
                em.ImagePivot.x = MBGUI.DoFloatSlider("Image-Pivot X", "Image Handle Offset", em.ImagePivot.x, -1, 1);
                em.ImagePivot.y = MBGUI.DoFloatSlider("Image-Pivot Y", "Image Handle Offset", em.ImagePivot.y, -1, 1);
                if ((em.ImagePivot - oldPivot).sqrMagnitude > 0)
                    em.mbInitializeQuad();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                MBGUI.DoLabel(em.Textures.Count + " Frame(s)");

                if (MBGUI.DoButton("Add Frame(s)", "Add selected textures from your Project to this emitter", Selection.activeObject is Texture)) {
                    List<Texture2D> files = MBUtility.GetAll<Texture2D>(Selection.objects);
                    files.Sort(delegate(Texture2D a, Texture2D b) { return a.name.CompareTo(b.name); });
                    MBEditorUtility.MakeTexturesReadable(files.ToArray());
                    em.Textures.AddRange(files);
                    SelectedParticleSystem.GenerateTextureMap(false);
                }
                if (MBGUI.DoButton("Remove", "Remove selected frame", em.Textures.Count > 0)) {
                    em.Textures.RemoveAt(mSelectedFrameIndex);
                    SelectedParticleSystem.GenerateTextureMap(false);
                }
                if (MBGUI.DoButton("Clear", "Clear all frames", em.Textures.Count > 0)) {
                    em.Textures.Clear();
                    SelectedParticleSystem.GenerateTextureMap(false);
                }
                EditorGUILayout.EndHorizontal();

                if (em.Textures.Count < 2) {
                    Texture2D tex = (em.Textures.Count > 0) ? em.Textures[0] : null;


                    tex = EditorGUILayout.ObjectField(tex, typeof(Texture2D), false, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.Height(100), GUILayout.Width(100) }) as Texture2D;

                    if (tex && (em.Textures.Count == 0 || tex != em.Textures[0])) {
                        MBEditorUtility.MakeTexturesReadable(tex);
                        em.Textures = new List<Texture2D>(new Texture2D[] { tex });
                        SelectedParticleSystem.GenerateTextureMap(false);
                    }

                }
                else {
                    #region Frames
                    //EditorGUILayout.BeginHorizontal(GUILayout.Height(200));
                    mImageScroll = EditorGUILayout.BeginScrollView(mImageScroll, GUILayout.Height(140));
                    GUILayout.Box("", GUIStyle.none, GUILayout.Width(em.Textures.Count * 110)); // force scrolling
                    Rect r = new Rect(10, 0, 100, 100);
                    for (int i = 0; i < em.Textures.Count; i++) {
                        Texture2D frame = em.Textures[i];
                        if (frame == null) {
                            em.Textures.RemoveAt(i);
                        }
                        else {
                            EditorGUI.DrawPreviewTexture(r, frame);

                            if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition)) {
                                mSelectedFrameIndex = i;
                                RepaintEditor(true);
                            }

                            if (i == mSelectedFrameIndex) {
                                Color c = GUI.backgroundColor;
                                GUI.backgroundColor = new Color(1, 0, 1, 0.5f);
                                GUI.Box(new Rect(r.x - 3, r.y - 3, r.width + 6, r.height + 6), new GUIContent("", string.Format("{0} ({1}x{2})", frame.name, frame.width, frame.height)));
                                GUI.backgroundColor = c;
                            }

                            r.x += 105;
                        }
                    }

                    EditorGUILayout.EndScrollView();
                    //EditorGUILayout.EndHorizontal();

                    #endregion
                }
            }
            // EVENTS
            if (DoSectionHeader("Events", mColHeader, true)) {
                em.ParticleBirthSM = MBGUI.DoEditorEvent("Particle Birth", "Particle Birth Event Target", em.ParticleBirthSM);
                em.ParticleDeathSM = MBGUI.DoEditorEvent("Particle Death", "Particle Death Event Target", em.ParticleDeathSM);
                em.EmitterStopsPlayingSM = MBGUI.DoEditorEvent("Emitter stops playing", "Emitter Stops Playing Event Target", em.EmitterStopsPlayingSM);
            }
            // DESCRIPTION

            if (DoSectionHeader("Description", mColHeader, true)) {
                EditorGUILayout.BeginVertical(GUI.skin.GetStyle("Box"));
                if (string.IsNullOrEmpty(em.Description))
                    em.Description = "";
                em.Description = EditorGUILayout.TextArea(em.Description, GUILayout.Height(100));
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
            #endregion
        }
        
    }

    void DoParameterGUI(MBEmitter em)
    {
        // Header-Block
            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("Box"));
            if (DoSectionHeader(em.name+": Parameters", mColHeader,true)) {
                
                #region Parameters Toolbar
                EditorGUILayout.BeginHorizontal();
                if (MBGUI.DoButton("Add", "Add a parameter", true)) {
                    AddParameterMenu();
                }
                if (MBGUI.DoButton("Delete", "Delete selected parameter", SelectedObject is MBParameter)) {
                    MBEditorCommands.DeleteObject();
                }
                if (MBGUI.DoButton("Clear", "Clear all parameters except Image", SelectedEmitter.Parameters.Count>0)) {
                    if (EditorUtility.DisplayDialog("Confirm", "Really strip all parameters?", "Yes", "No")) {
                        SnapshotScene("Clear parameters");
                        List<MBParameter> pars = MBUtility.GetChildren<MBParameter>(em.transform, true, false);
                        em.Parameters.Clear();
                        foreach (MBParameter p in pars) {
                            GameObject.DestroyImmediate(p.gameObject);
                        }
                        InstantiateParameterHandler();
                        Select(SelectedEmitter);
                    }
                }
                 GUILayout.Space(10);
                 if (MBGUI.DoButton("Toggle","Toggle Parameters ViewState",true)){
                     mParametersToggleState = !mParametersToggleState;
                     if (mParametersToggleState) {
                         // show all
                         foreach (MBEditorParameterHandler handler in mHandler) {
                             string label = ParameterCaption(handler);
                             if (mSectionToggleState.ContainsKey(label))
                                 mSectionToggleState.Remove(label);
                         }
                     }
                     else {
                         // hide all
                         foreach (MBEditorParameterHandler handler in mHandler) {
                             string label = ParameterCaption(handler);
                             if (mSectionToggleState.ContainsKey(label))
                                 mSectionToggleState[label] = false;
                             else
                                 mSectionToggleState.Add(label, false);
                         }
                     }
                 }
                 mParametersAutoToggleSelected = MBGUI.DoToggle("Auto-Fold", "Fold unselected Parameters", mParametersAutoToggleSelected);
             
             EditorGUILayout.EndHorizontal();
                #endregion
                
             mDetailScroll = EditorGUILayout.BeginScrollView(mDetailScroll);
                
                    // Call custom node handlers
                MBEditorParameterHandler[] handlers = mHandler.ToArray();
                    foreach (MBEditorParameterHandler handler in handlers) {
                        if (handler.Target != null) {
                            GUI.backgroundColor = (handler.Target == SelectedObject) ? mColParameterSelected : mColParameter;
                            EditorGUILayout.BeginVertical(mStyleHeaderStyle);
                            GUI.backgroundColor = (handler.Target == SelectedObject) ? mColParameterHeaderSelected : mColParameterHeader;
                            if (DoParameterHeader(handler)) {
                                // Needs or Excludes?
                                DoParameterInfo(handler.ParameterInfo);
                                GUI.backgroundColor = (handler.Target == SelectedObject) ? mColParameterSelected : mColParameter;
                                // OnBirthGUI
                                if (!handler.HideBirthGUI) {
                                    EditorGUILayout.BeginVertical(mStyleHeaderStyle);
                                    handler.OnBirthGUI();
                                    EditorGUILayout.EndVertical();
                                }
                                // OnLifeGUI, if animated
                                if (handler.Target.AnimatedLife) {
                                    EditorGUILayout.BeginVertical(mStyleHeaderStyle);
                                    handler.OnLifetimeGUI();
                                    EditorGUILayout.EndVertical();
                                }
                                handler.Target.Validate();
                                _NeedRepaint |= handler.NeedRepaint;
                                handler.NeedRepaint = false;
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                 
             EditorGUILayout.EndScrollView();  
            }
        EditorGUILayout.EndVertical();
    }

    void DoDebuggingGUI(MBEmitter em)
    {
        EditorGUILayout.BeginVertical(GUI.skin.GetStyle("Box"));
        if (DoSectionHeader("General", mColHeader, true)) {
            EditorGUILayout.BeginHorizontal();
                em.mbDebugging = MBGUI.DoToggle("Enable", "Enable visualization", em.mbDebugging);
                em.mbDebugRate = MBGUI.DoFloatSlider("Rate", "Visualization amount", em.mbDebugRate, 0f, 1f);
            EditorGUILayout.EndHorizontal();
        }
        if (DoSectionHeader("Options", mColHeader, true)) {
            EditorGUILayout.BeginHorizontal();
                em.mbDebugSize = MBGUI.DoToggle("Size", "Show size gizmo", em.mbDebugSize);
                em.mbDebugSizeCol = MBGUI.DoColorField("Color", "Gizmo color", em.mbDebugSizeCol);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
                em.mbDebugOrientation = MBGUI.DoToggle("Orientation", "Show orientation gizmo", em.mbDebugOrientation);
                em.mbDebugOrientationCol = MBGUI.DoColorField("Color", "Gizmo color", em.mbDebugOrientationCol);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
                em.mbDebugSpeed = MBGUI.DoToggle("Speed", "Show speed gizmo", em.mbDebugSpeed);
                em.mbDebugSpeedCol = MBGUI.DoColorField("Color", "Gizmo color", em.mbDebugSpeedCol);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
                em.mbDebugHeading = MBGUI.DoToggle("Heading", "Show heading gizmo", em.mbDebugHeading);
                em.mbDebugHeadingCol = MBGUI.DoColorField("Color", "Gizmo color", em.mbDebugHeadingCol);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            em.mbDebugForces = MBGUI.DoToggle("Forces", "Show forces gizmo", em.mbDebugForces);
            em.mbDebugForcesCol = MBGUI.DoColorField("Color", "Gizmo color", em.mbDebugForcesCol);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            em.mbDebugZones = MBGUI.DoToggle("Zones", "Color particles affected by zones", em.mbDebugZones);
            em.mbDebugZonesCol = MBGUI.DoColorField("Color", "Gizmo color", em.mbDebugZonesCol);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
                em.mbDebugAge = MBGUI.DoToggle("Age", "Show age gizmo", em.mbDebugAge);
                em.mbDebugAgeCol = MBGUI.DoColorField("Color", "Gizmo color", em.mbDebugAgeCol);
            EditorGUILayout.EndHorizontal();
            
        }
    }

    public void InstantiateParameterHandler()
    {
        ClearHandlers();
        MBEmitter em = SelectedEmitter;
        
        if (em) {
            foreach (MBParameter param in em.Parameters) {
                if (mHandlerMap.ContainsKey(param.GetType())) {
                    System.Type T = mHandlerMap[param.GetType()];
                    MBEditorParameterHandler pHnd = System.Activator.CreateInstance(T) as MBEditorParameterHandler;
                    pHnd.Target = param;
                    pHnd.ParameterInfo = param.ParameterInfo; // cache reflection call
                    mHandler.Add(pHnd);
                }
                else
                    Debug.LogError("Missing Handler for Parameter '"+param.GetType().Name+"'!");
            }
        }
    }

    void InstantiateEMTypeHandler()
    {
        MBEmitter em = (SelectedObject as MBEmitter);
        if (em) {
            MBEmitterType emtype = em.EmitterType;
            if (mHandlerMap.ContainsKey(emtype.GetType())) {
                System.Type T = mHandlerMap[emtype.GetType()];
                mEmitterTypeHandler = System.Activator.CreateInstance(T) as MBEditorEmitterTypeHandler;
                mEmitterTypeHandler.Target = emtype;
                mEmitterTypeHandler.EmitterTypeInfo = emtype.EmitterTypeInfo;
            }
            else if (!EditorApplication.isPlaying)
                Debug.LogError("Missing Handler for EmitterType '" + em.EmitterType.GetType().Name + "'!");

        }
    }

    /// <summary>
    /// Creates a custom menu for adding parameters
    /// </summary>
    void AddParameterMenu()
    {
        List<GUIContent> ParamMenu = new List<GUIContent>();
        Dictionary<string, System.Type> ParamTypes = new Dictionary<string, System.Type>();

        // Get a list of classes that derive from MBParameter
        Assembly asm = Assembly.GetAssembly(typeof(MBParameter));
        System.Type[] types = asm.GetTypes();
        foreach (System.Type T in types) {
            if (T.IsSubclassOf(typeof(MBParameter))) {
                // Get MBParameterInfo-Attribute
                System.Attribute info = System.Attribute.GetCustomAttribute(T, typeof(MBParameterInfo));
                if (info != null) {
                    // Add Menu entry
                    ParamMenu.Add(new GUIContent(((MBParameterInfo)info).Menu));
                    // Link Menu entry to class (for OnAddParameterMenu)
                    ParamTypes.Add(((MBParameterInfo)info).Menu, T);
                }
            }
        }
        EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 100, 100),
                                        ParamMenu.ToArray(), -1, OnAddParameterMenu,ParamTypes);
    }

    /// <summary>
    /// Called when the user selects an entry from the Parameter-Menu
    /// </summary>
    void OnAddParameterMenu(object userData, string[] options, int selected)
    {
        if (selected > -1) {
            System.Type paramtype = ((Dictionary<string, System.Type>)userData)[options[selected]];
            MBParameter pm;
            SnapshotScene("Add "+paramtype.ToString());
            if (SelectedObject is MBEmitter)
                pm=((MBEmitter)SelectedObject).AddParameter(paramtype);
            else
                pm=((MBParameter)SelectedObject).ParentEmitter.AddParameter(paramtype);

            InstantiateParameterHandler();
            Select(pm);
        }
    }

    /// <summary>
    /// Creates a custom menu for replacing emittertype
    /// </summary>
    void AddEmitterTypeMenu()
    {
        List<GUIContent> Menu = new List<GUIContent>();
        Dictionary<string, System.Type> EMTypes = new Dictionary<string, System.Type>();
        
        // Get a list of classes that derive from MBEmitterType
        Assembly asm = Assembly.GetAssembly(typeof(MBEmitterType));
        System.Type[] types = asm.GetTypes();
        foreach (System.Type T in types) {
            // Get MBEmitterTypeInfo-Attribute
            System.Attribute info = System.Attribute.GetCustomAttribute(T, typeof(MBEmitterTypeInfo));
            if (info != null) {
                // Add menu entry
                Menu.Add(new GUIContent(((MBEmitterTypeInfo)info).Menu));
                // Link menu entry to class 
                EMTypes.Add(((MBEmitterTypeInfo)info).Menu, T);
            }
        }
        EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 100, 100),
                                        Menu.ToArray(), -1, OnAddEmitterTypeMenu, EMTypes);    
    }

    void OnAddEmitterTypeMenu(object userData, string[] options, int selected)
    {
        if (selected > -1) {
            System.Type emtype = ((Dictionary<string, System.Type>)userData)[options[selected]];
            if (SelectedEmitter) {
                SelectedEmitter.SetEmitterType(emtype);
                InstantiateEMTypeHandler();
            }
        }
    }

    void LayerSelectMenu()
    {
        List<GUIContent> Menu = new List<GUIContent>();
        List<string> names = new List<string>();
        foreach (MBLayer lyr in SelectedParticleSystem.Layer){
            string n = lyr.name;
            while (names.Contains(n)) {
                n += " ";
            }
            Menu.Add(new GUIContent(n));
            names.Add(n);
        }
        EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 100, 100),
                                        Menu.ToArray(), -1, OnLayerSelectMenu, null);    
    }

    void OnLayerSelectMenu(object userData, string[] options, int selected)
    {
        if (selected > -1) {
            SelectedEmitter.Layer = SelectedParticleSystem.Layer[selected] as MBLayer;
        }
    }

    void ClearHandlers()
    {
        foreach (MBEditorParameterHandler h in mHandler)
            h.OnDestroy();
        mHandler.Clear();
    }

    #endregion

    #region ### GUI-Helpers ###

    public bool DoSectionHeader(string caption, Color backgroundColor, bool canToggle)
    {
        bool state = true;
        
        if (canToggle) {
            if (!mSectionToggleState.ContainsKey(caption))
                mSectionToggleState.Add(caption, true);
            state = mSectionToggleState[caption];
            GUILayout.BeginHorizontal();
        }

        GUI.backgroundColor = backgroundColor;
        GUILayout.Label(caption, mStyleHeaderStyle);
        Rect r = GUILayoutUtility.GetLastRect();
        if (canToggle) {
            if (GUI.Button(new Rect(r.xMax-20,r.y,20,20),new GUIContent(mTexToggleZoom, "Show/Hide Section"),GUI.skin.label)) {
                state = !state;
                mSectionToggleState[caption] = state;
            }
            GUILayout.EndHorizontal();
        }
        GUI.backgroundColor = mColDefault;
        return state;
    }

    public bool DoParameterHeader(MBEditorParameterHandler handler)
    {
        bool dirty = false;
        bool state = true;

        EditorGUILayout.BeginHorizontal(mStyleHeaderStyle);
        
        // Birth Animation
        GUI.enabled = handler.ParameterInfo.CanAnimateBirth == MBParameterAnimationMode.Optional;
        handler.Target.AnimatedBirth = MBGUI.DoToggle(mTexBirthAnimated, "Are parameter's initial values animated?", handler.Target.AnimatedBirth, out dirty);
        if (dirty) {
            SelectedEmitter.mbReloadHierarchy();
        }
        
        // Life Animation
        GUI.enabled = handler.ParameterInfo.CanAnimateLife == MBParameterAnimationMode.Optional;
        handler.Target.AnimatedLife = MBGUI.DoToggle(mTexLifeAnimated, "Is this parameter animated over lifetime?", handler.Target.AnimatedLife, out dirty);
        GUI.enabled = true;
        if (dirty) {
            SelectedEmitter.mbReloadHierarchy();
        }
        string label = ParameterCaption(handler);
        if (GUILayout.Button(label, mStyleBoldCentered)) {
            Select(handler.Target);
            if (Event.current.button == 1)
                EditorUtility.DisplayPopupMenu(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 100, 100), "Window/Magical Box/Object", null);
        }
        // Mute
        if (GUILayout.Button(new GUIContent(handler.Target.Muted ? mTexMuted : mTexUnMuted,"Mute/Unmute"), GUI.skin.label, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MaxHeight(18) }))
            handler.Target.Muted = !handler.Target.Muted;
        // Toggle Section
        if (!mSectionToggleState.ContainsKey(label))
            mSectionToggleState.Add(label, true);
        state = mSectionToggleState[label];
        if (GUILayout.Button(new GUIContent(mTexToggleZoom, "Show/Hide Section"), GUI.skin.label,new GUILayoutOption[]{GUILayout.ExpandWidth(false),GUILayout.MaxHeight(18)})) {
            state = !state;
            mSectionToggleState[label] = state;
        }
        if (GUILayout.Button(new GUIContent(mTexParamOrderUp, "Move Up (calculate earlier)"), GUI.skin.label, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MaxHeight(18) })) {
            MBParameter prev=handler.Target.PreviousParameter;
            if (prev) {
                int ord=prev.Order;
                prev.Order=handler.Target.Order;
                handler.Target.Order = ord;
                handler.Target.ParentEmitter.mbSortParameters();
                InstantiateParameterHandler();
            }
        }
        if (GUILayout.Button(new GUIContent(mTexParamOrderDown, "Move Down (calculate later)"), GUI.skin.label, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MaxHeight(18) })) {
            MBParameter next = handler.Target.NextParameter;
            if (next) {
                int ord = next.Order;
                next.Order = handler.Target.Order;
                handler.Target.Order = ord;
                handler.Target.ParentEmitter.mbSortParameters();
                InstantiateParameterHandler();
            }
        }
        EditorGUILayout.EndHorizontal();

        return (mParametersAutoToggleSelected) ? handler.Target==SelectedObject: state;
    }

    string ParameterCaption(MBEditorParameterHandler handler)
    {
        return (handler.Target.name != handler.Target.GetType().Name) ? handler.ParameterInfo.Menu + " (" + handler.Target.name + ")" : handler.ParameterInfo.Menu;
    }

    public void DoParameterInfo(MBParameterInfo info)
    {
        if (string.IsNullOrEmpty(info.Excludes + info.Needs)) return;

        GUI.backgroundColor = mColParameterInfo;
        StringBuilder s = new StringBuilder();
        if (!string.IsNullOrEmpty(info.Needs))
            s.AppendFormat("Needs: {0}", info.Needs);
        if (!string.IsNullOrEmpty(info.Excludes))
            s.AppendFormat("   Excludes: {0}", info.Excludes);
        if (!string.IsNullOrEmpty(info.Note))
            s.AppendFormat("   Notes: {0}", info.Note);
            
        GUILayout.Label(s.ToString(), GUI.skin.GetStyle("box"),GUILayout.ExpandWidth(true));
        GUI.backgroundColor = mColDefault;
    }

    #endregion

}

