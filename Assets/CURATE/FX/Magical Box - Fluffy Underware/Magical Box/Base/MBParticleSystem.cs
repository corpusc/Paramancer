// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================

using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// Main ParticleSystem class
/// </summary>
/// <remarks>
/// \sa \ref particlesysref
/// </remarks>
/// <para>(c) Fluffy Underware</para>
[ExecuteInEditMode]
public class MBParticleSystem : MBObject
{
    public const string VERSION = "1.22";

    #region ### Inspector Fields ###
    /// <summary>
    /// Whether this particle system renders particles?
    /// </summary>
    /// <remarks>
    /// Unlike a muted system all emitters will be calculated and only the rendering will be suppressed.
    /// </remarks>
    public bool Invisible;
    /// <summary>
    /// The camera used by this ParticleSystem
    /// </summary>
    public Camera Camera;
    /// <summary>
    /// Whether this particle system is persistent between scene changes
    /// </summary>
    public bool Persistent;
    /// <summary>
    /// Whether this particle system will start playing automatically
    /// </summary>
    public bool AutoPlay=true;
    /// <summary>
    /// Whether this ParticleSystem runs once or repeats playing
    /// </summary>
    public bool SingleShot;
    /// <summary>
    /// Whether this ParticleSystem has infinite duration
    /// </summary>
    /// <remarks>
    /// This is useful if you play single emitter instead of the whole ParticleSystem. 
    /// \note Note that in Infinite mode AgePercent is always 0.5 while Age is undefined.
    /// </remarks>
    public bool Infinite;
    /// <summary>
    /// The duration in seconds one loop lasts
    /// </summary>
    public float Duration;
    /// <summary>
    /// Amount of time Advance the particle system should advance without rendering when starting to play
    /// </summary>
    public float TimeWarp;
    /// <summary>
    /// Timestep used for TimeWarp.
    /// </summary>
    /// <remarks>Using a larger timestep is faster but may lead to incorrect results</remarks>
    public float TimeWarpStepSize = 0.1f;
    /// <summary>
    /// Pause ParticleSystem after time warping?
    /// </summary>
    /// <remarks>Useful to precalculate complex Particlesystems' timewarping. Call Play() again to continue playing</remarks>
    public bool PauseAfterTimeWarp;
    
    /// <summary>
    /// Initial particle treshold
    /// </summary>
    public int ParticleThreshold = 100;
    /// <summary>
    /// How many Particles are created at once when the threshold exceeds
    /// </summary>
    public int ParticleBlocksize = 20;
    
    /// <summary>
    /// Whether the texture atlas is persistent or generated at Start()
    /// </summary>
    public bool PersistentAtlas;

    /// <summary>
    /// Texture padding used when packing textures
    /// </summary>
    /// <remarks>If you see glitches around your particles, increase texture padding!</remarks>
    public int TexturePadding = 0;

    [HideInInspector]
    public List<Texture2D> TextureMapKeys = new List<Texture2D>();  // mTextureUVMap-Serialization
    [HideInInspector]
    public List<Rect> TextureMapValues = new List<Rect>(); // mTextureUVMap-Serialization

    /// <summary>
    /// The path where TextureAtlas should be stored
    /// </summary>
    [HideInInspector]
    public string TextureAtlasPath;

    #endregion

    #region ### Public Properties ###
    
    /// <summary>
    /// Gets the current Runtime since Play() in seconds
    /// </summary>
    public float Age { get; private set; }
    /// <summary>
    /// Gets the current Runtime since Play() as percentage (0..1) in relation to Duration
    /// </summary>
    public float AgePercent { get; private set; }
        /// <summary>
    /// Gets the active camera for this ParticleSystem
    /// </summary>
    /// <remarks>
    /// While not playing, this will be either scene view camera or game view camera. 
    /// At runtime this will either return the Camera set in the inspector, the current/main camera or null</remarks>
    public Camera ActiveCamera
    {
        get
        {
            if (!Application.isPlaying && mbEditorCamera)
                return mbEditorCamera;
            else {
                if (!Camera)
                    Camera = Camera.current ? Camera.current : Camera.main;
                return Camera;
            }
        }
    }
    /// <summary>
    /// Gets or sets the delta time for this ParticleSystem
    /// </summary>
    /// <remarks>This will either be Time.deltaTime (Runtime: set by LateUpdate() or a calculated value (Edit-Mode)</remarks>
    public float DeltaTime { get; set; }
    /// <summary>
    /// Gets the first layer or null
    /// </summary>
    public MBLayer DefaultLayer
    {
        get
        {
            return (mLayers != null && mLayers.Count > 0) ? mLayers[0] : null;
        }
    }

    /// <summary>
    /// Gets a list of (enabled) child emitters
    /// </summary>
    public List<MBEmitter> Emitter
    {
        get
        {
            if (mEmitters == null) {
                mEmitters = new List<MBEmitter>(MBUtility.GetChildren<MBEmitter>(transform, true, true));
                mEmitters.Sort();
            }
            return mEmitters;
        }
    }
    /// <summary>
    /// Gets a list of layers of this ParticleSystem
    /// </summary>
    public List<MBLayer> Layer
    {
        get
        {
            if (mLayers == null) {
                mLayers = new List<MBLayer>(MBUtility.GetChildren<MBLayer>(transform, false, true));
                mLayers.Sort();
            }
            return mLayers;
        }
    }
    
    /// <summary>
    /// Gets or sets the global time for this ParticleSystem
    /// </summary>
    /// <remarks>This will either be Time.time (Runtime: set by LateUpdate()) or a calculated value (Edit-Mode)</remarks>
    public float GlobalTime { get; set; }

    /// <summary>
    /// Gets whether this ParticleSystem is fully initialized
    /// </summary>
    /// <remarks>
    /// Often you want to setup some properties in your Start() function. Because of Unity's random intialization order you'll need to wait until the whole
    /// ParticleSystem is fully initialized. Here's an example:
    /// \code
    /// IEnumerator Start()
    /// {
    ///     // wait until the ParticleSystem is fully loaded
    ///     while (!MyParticleSystem.IsInitialized)
    ///        yield return new WaitForEndOfFrame();
    ///     // Now the system is fully loaded, it's safe to access all classes now
    /// }       
    /// \endcode
    /// </remarks>
    public bool IsInitialized { get; protected set; }

    /// <summary>
    /// Gets whether this ParticleSystem is currently paused
    /// </summary>
    public bool Paused { get; private set; }
    /// <summary>
    /// Gets the number of particles currently idle
    /// </summary>
    /// <remarks>You can call Purge() to clear buffers</remarks>
    public int ParticleCountIdle { get { return mIdleParticles.Count; } }
    /// <summary>
    /// Gets whether this ParticleSystem is currently playing
    /// </summary>
    public bool Playing { get; private set; }

    /// <summary>
    /// Gets the number of particles currently processed by this particle system's emitters
    /// </summary>
    public int ParticleCount { get; private set; }
    /// <summary>
    /// Gets the number of particles currently been rendered by this particle system's layers
    /// </summary>
    public int ParticlesRendered { get; private set; }

    /// <summary>
    /// Gets the maximum number of particles rendered since Play()
    /// </summary>
    /// <remarks>Calling Play() will reset this value. This is useful for determining a good treshold value for your particle system</remarks>
    public int ParticlesRenderedMax { get; private set; }

    public override MBParticleSystem ParticleSystem
    {
        get
        {
            mParticleSystem = this;
            return this;
        }
    }

    /// <summary>
    /// Gets whether this particle system is about to stop.
    /// </summary>
    /// <remarks>This is needed to differentiate between Stop() (=continue rendering) and halted state</remarks>
    public bool Stopping { get; private set; }

    /// <summary>
    /// Gets the generated texture atlas
    /// </summary>
    public Texture2D TextureAtlas { get; private set; }

    /// <summary>
    /// Gets all textures used by emitters of this ParticleSystem
    /// </summary>
    public List<Texture2D> Textures
    {
        get
        {
            MBEmitter[] emitters = GetComponentsInChildren<MBEmitter>();
            List<Texture2D> res = new List<Texture2D>();
            foreach (MBEmitter em in emitters)
                foreach (Texture2D tex in em.Textures)
                    if (!res.Contains(tex))
                        res.Add(tex);
            return res;
        }
    }
    

    /// <summary>
    /// Whether GlobalTime/DeltaTime is set by user or engine
    /// </summary>
    public bool UserTiming { get; set; }

    #endregion
  
    #region ### Private Fields ###

    Dictionary<Texture2D, Rect> mTextureUVMap = new Dictionary<Texture2D, Rect>();
    List<MBParticle> mIdleParticles = new List<MBParticle>();
    
    float mStartTime;
    
    float mTimeToWarp;
    // hierarchy cache
    List<MBEmitter> mEmitters;
    List<MBLayer> mLayers;
    #endregion

    #region ### Unity Callbacks ###

    protected override void OnEnable()
    {
        IsInitialized = false;
        base.OnEnable();

        if (Persistent)
            DontDestroyOnLoad(this);

        // Prepare Buffers
        mbResetBuffers();

        mStartTime = 0;
        Playing = false;
    }

    protected override void OnDisable()
    {
        if (!this.enabled)
            Halt();
        base.OnDisable();
        IsInitialized = false;
        if (!PersistentAtlas)
            TextureAtlas = null;
        
        mbEditorEnabledScripts.Clear();
    }

    void Start()
    {
        if (!ActiveCamera)
            Debug.LogWarning("Magical Box: ParticleSystem '" + name + "' is missing a camera to use!");

        mbReloadHierarchy();
        // Do we need to create a texture atlas?
        if (!PersistentAtlas)
            GenerateTextureMap(true);
        else {
            TextureAtlas = Resources.LoadAssetAtPath(TextureAtlasPath, typeof(Texture2D)) as Texture2D;
            mbBuildDictionary(false);
        }

        // Get storyboard timing        
        mbCalculateRuntime();
        IsInitialized = true;
        
        if (Application.isPlaying && AutoPlay) 
                Play();
    }

    void Update()
    {
        // Editor rendering
        if (!Application.isPlaying) {
            foreach (MBLayer lyr in Layer)
                lyr.mbEditorRender();
        }
    }
    

    void LateUpdate()
    {
        // Runtime rendering
        if (Application.isPlaying) {
            if (!UserTiming) {
                GlobalTime = Time.time;
                DeltaTime = Time.smoothDeltaTime;
            }
            mbUpdate();
            mbRender();
        }
    }

    public override void Reset()
    {
        base.Reset();
    }


    #endregion

    #region ### Public Methods ###

    /// <summary>
    /// Creates a new empty anchor
    /// </summary>
    /// <returns></returns>
    public MBAnchor AddAnchor() { return AddAnchor(this); }
    /// <summary>
    /// Creates a new empty anchor as a child of parent
    /// </summary>
    /// <param name="parent">the parent of the new anchor</param>
    /// <returns>the new anchor</returns>
    public MBAnchor AddAnchor(MBObject parent) 
    {
        if (!parent.Matches(typeof(MBParticleSystem),typeof(MBEmitter),typeof(MBAnchor)))
            return null;
        if (parent is MBEmitter && ((MBEmitter)parent).IsTrail) return null;

        GameObject go = new GameObject("Anchor");
        MBAnchor anc = go.AddComponent<MBAnchor>();
        // initialize hierarchy
        anc.SetParent(parent);
        anc.mbReloadHierarchy();
        anc.Transform.position = parent.Transform.position;

        return anc;
    }
    /// <summary>
    /// Creates a clone of an existing anchor and adds it as a child of source
    /// </summary>
    /// <param name="source">the anchor to be cloned</param>
    /// <param name="parent">the new parent of the clone</param>
    /// <returns>the new anchor</returns>
    public MBAnchor AddAnchor(MBAnchor source, MBObject parent)
    {
        if (!parent.Matches(typeof(MBParticleSystem), typeof(MBEmitter), typeof(MBAnchor)))
            return null;
        if (parent is MBEmitter && ((MBEmitter)parent).IsTrail) return null;

        if (source) {
            MBAnchor anc = (MBAnchor)Object.Instantiate(source);
            // initialize hierarchy
            anc.SetParent(parent);
            anc.mbReloadHierarchy();
            anc.Transform.position = parent.Transform.position;
            return anc;
        }
        else
            return null;
    }

    /// <summary>
    /// Creates a new empty child emitter
    /// </summary>
    /// <returns>the new emitter</returns>
    public MBEmitter AddEmitter() { return AddEmitter(this); }

    /// <summary>
    /// Creates a new empty emitter as a child of parent
    /// </summary>
    /// <param name="parent">the parent of the new emitter</param>
    /// <returns>the new emitter</returns>
    public MBEmitter AddEmitter(MBObject parent)
    {
        if (!parent.Matches(typeof(MBParticleSystem), typeof(MBAnchor), typeof(MBEmitter))) 
            return null;
        if (parent is MBEmitter && ((MBEmitter)parent).IsTrail) return null;
            
        GameObject go = new GameObject("New Emitter");
        MBEmitter em = go.AddComponent<MBEmitter>();
        // initialize em hierarchy
        em.SetParent(parent);
        em.mbReloadHierarchy();
        em.Transform.position = parent.Transform.position;
        em.Layer = ParticleSystem.Layer[0];
        
        return em;
    }
    /// <summary>
    /// Creates a clone of an existing emitter and adds it as a child of source
    /// </summary>
    /// <param name="source">the emitter to be cloned</param>
    /// <param name="parent">the parent of the new emitter</param>
    /// <returns>the new emitter</returns>
    public MBEmitter AddEmitter(MBEmitter source, MBObject parent)
    {
        MBEmitter em = null;
        if (source && parent.Matches(typeof(MBParticleSystem),typeof(MBAnchor),typeof(MBEmitter))) {
            if (parent is MBEmitter && ((MBEmitter)parent).IsTrail) return null;
            em = (MBEmitter)Object.Instantiate(source);
            // initialize em hierarchy
            em.SetParent(parent);
            em.mbReloadHierarchy();
            em.Transform.position = parent.Transform.position;
        }
        return em;
    }

    public MBLayer AddLayer(Material mat)
    {
        GameObject go = new GameObject("Layer");
        MBLayer lyr=go.AddComponent<MBLayer>();
        lyr.name = mat.name;
        lyr.SetParent(this);
        lyr.Material = mat;
        lyr.mbReloadHierarchy();
        Layer.Add(lyr);
        Layer.Sort();
        return lyr;
    }
    
    /// <summary>
    /// Clears all particles
    /// </summary>
    public void Clear()
    {
        mStartTime = 0;
        Age = 0;
        AgePercent = 0;
        ParticleCount = 0;
        mbResetBuffers();
        foreach (MBLayer lyr in Layer)
            lyr.mbResetBuffers();
    }

    /// <summary>
    /// Return a layer with matching material or null
    /// </summary>
    /// <param name="forMaterial">the Material the layer needs to use</param>
    /// <returns>the found layer or null</returns>
    public MBLayer FindLayer(Material forMaterial)
    {
        foreach (MBLayer lyr in Layer)
            if (lyr.Material == forMaterial)
                return lyr;

        return null;
    }
    /// <summary>
    /// Return a layer with matching shader
    /// </summary>
    /// <param name="shaderName">the shader the layer needs to use</param>
    /// <returns>the found layer or null</returns>
    public MBLayer FindLayer(string shaderName)
    {
        foreach (MBLayer lyr in Layer)
            if (lyr.Material.shader.name == shaderName)
                return lyr;

        return null;
    }

    /// <summary>
    /// Generate a texture atlas containing all textures our emitters need
    /// </summary>
    public void GenerateTextureMap() { GenerateTextureMap(false); }
    /// <summary>
    /// Generate a texture atlas containing all textures our emitters need
    /// </summary>
    /// <param name="forceRebuild">if set to false, atlas will only be built if a texture is missing</param>
    public void GenerateTextureMap(bool forceRebuild)
    {
        List<Texture2D> texlist = Textures;
        
        // only pack textures if neccessary
        bool dirty = false;
        if (!forceRebuild && texlist.Count == TextureMapKeys.Count) {
            foreach (Texture2D tex in texlist)
                if (!TextureMapKeys.Contains(tex)) {
                    dirty = true;
                    break;
                }
        }
        else
            dirty = true;
        
        if (dirty && !string.IsNullOrEmpty(TextureAtlasPath)) {
            // create the texture atlas
        
            //if (texlist.Count > 0) {
            
                Texture2D tex = new Texture2D(2048, 2048,TextureFormat.ARGB32,true);

                TextureMapKeys = new List<Texture2D>(texlist);
                TextureMapValues = new List<Rect>(tex.PackTextures(TextureMapKeys.ToArray(), TexturePadding, 2048));
                TextureAtlas=MBUtility.SaveTexture(tex, TextureAtlasPath);
                //TextureAtlas = Resources.LoadAssetAtPath(TextureAtlasPath, typeof(Texture2D)) as Texture2D;
                
                foreach (MBLayer lyr in Layer)
                    lyr.UpdateMaterial();
            //}
            //else
            //    TextureAtlas = null;
            // build the dictionary
            mbBuildDictionary(true);
            
        }
        else
            mbBuildDictionary(false);
    }

    /// <summary>
    /// Gets the UV of the corresponding texture
    /// </summary>
    /// <param name="texture">a Texture from MBImage</param>
    /// <returns>the UV rect on the particle system's mainTexture for this texture</returns>
    public Rect GetTextureUV(Texture2D texture)
    {
        if (texture && mTextureUVMap.ContainsKey(texture))
            return mTextureUVMap[texture];
        else {
            return new Rect(0, 0, 1, 1);
        }
    }

            
    /// <summary>
    /// Stops all emitters and destroys all particles immediately
    /// </summary>
    public void Halt()
    {
        Stop();
        Playing = false;
        foreach (MBEmitter em in Emitter)
            em.Halt(true);
        Clear();
        // Clear mesh
        mbRender();
    }

    /// <summary>
    /// Freezes the particle system
    /// </summary>
    public void Pause()
    {
        Paused = true;
    }

    /// <summary>
    /// Starts playing
    /// </summary>
    public void Play()
    {
        Play(false);
    }
    /// <summary>
    /// Starts playing
    /// </summary>
    /// <param name="pauseAfterTimewarp">true to play until Timewarp and then pause</param>
    public void Play(bool pauseAfterTimewarp)
    {
        if (Paused) {
            Paused = false;
        }
        else {
            PauseAfterTimeWarp = pauseAfterTimewarp;
            if (!Playing) { // only warp when not playing before
                mTimeToWarp = TimeWarp;
                GlobalTime -= mTimeToWarp;
                ParticlesRenderedMax = 0;
                foreach (MBLayer lyr in Layer)
                    lyr.mbOnBeginPlay();
            }
            mStartTime = GlobalTime;
            ParticlesRenderedMax = 0;
            TimeWarpStepSize = Mathf.Max(0.001f, TimeWarpStepSize); // to prevent endless loop in mbUpdateStep()
            Stopping = false;
            Playing = true;
            
            foreach (MBEmitter em in Emitter) {
                if (em.AutoPlay)
                    em.Play(true);
            }
        }
        
    }

    public override void Purge()
    {
        base.Purge();
        mbResetBuffers();
    }

    /// <summary>
    /// Set the filename that this particle system uses to write it's texture atlas
    /// </summary>
    /// <param name="path">path to a .png file, e.g. "assets/myparticlesys.png"</param>
    public void SetTextureAtlas(string path)
    {
        if (string.IsNullOrEmpty(path))
            TextureAtlas=null;
        else {
            TextureAtlasPath = path;
            GenerateTextureMap(true);
        }
    }

    /// <summary>
    /// Stops all emitters, but continues rendering until all particles died
    /// </summary>
    public void Stop()
    {
        Stopping = true;
        foreach (MBEmitter em in Emitter)
            em.Stop(true);
    }


    /// <summary>
    /// Gets whether this Particlesystem is currently in timewarp mode
    /// </summary>
    public bool Warping
    {
        get { return Playing && mTimeToWarp > 0; }
    }

    #endregion

    #region ### Internal Publics. Don't use them unless you know what you're doing ###
    /*! @name Internal Public
     * Don't use them unless you know what you're doing
     */
    //@{

    public void mbBuildDictionary(bool reload)
    {
        if (mTextureUVMap == null || TextureMapKeys == null || TextureMapValues == null) return;
        if (mTextureUVMap.Count == 0 || reload) {
            mTextureUVMap.Clear();
            for (int i = 0; i < TextureMapKeys.Count; i++) {
                mTextureUVMap.Add(TextureMapKeys[i], TextureMapValues[i]);
            }
        }
        // Let all emitters fetch their texture UVs
        foreach (MBEmitter em in Emitter)
            em.mbInitializeTextures();
    }

    public void mbCalculateRuntime()
    {
        float d = 0;
        // consider all emitters ignoring levels
        MBEmitter[] emlist = GetComponentsInChildren<MBEmitter>();
        foreach (MBEmitter em in emlist)
            d = Mathf.Max(em.Delay+em.Duration, d);

        Duration = Mathf.Max(Duration, d);
    }
    /// <summary>
    /// Bitmask to determine the objects that should draw Gizmos
    /// </summary>
    /// <remarks>This is set by the editor and will be stored in editorprefs</remarks>
    public MBGizmoFlags mbDrawGizmos { get; set; }
    /// <summary>
    /// Bitmask to determine the objects that should draw Gizmos while selected
    /// </summary>
    /// <remarks>This is set by the editor and will be stored in editorprefs</remarks>
    public MBGizmoFlags mbDrawGizmosSelected { get; set; }

    /// <summary>
    /// List of scripts that should be called in the editor
    /// </summary>
    [HideInInspector]
    public List<MBEditorEnabledScript> mbEditorEnabledScripts = new List<MBEditorEnabledScript>();

    public MBParticle mbGetParticle()
    {
        // Any idle Particles left?
        if (mIdleParticles.Count > 0) {
            MBParticle PT = mIdleParticles[0];
            mIdleParticles.RemoveAt(0);
            PT.Reset();
            return PT;
        }
        else { // add new
            mbAddIdleParticles(ParticleBlocksize-1);
            return new MBParticle();
        }
    }

    public void mbIdleParticle(MBParticle particle)
    {
        mIdleParticles.Add(particle);
    }

    public void mbIdleParticle(IEnumerable<MBParticle> particles)
    {
        foreach (MBParticle P in particles)
            mbIdleParticle(P);
    }

    /// <summary>
    /// Gets all textures used by an layer of this particlesystem
    /// </summary>
    public List<Texture2D> mbLayerTextures (MBLayer lyr)
    {
            MBEmitter[] emitters = GetComponentsInChildren<MBEmitter>();
            List<Texture2D> res = new List<Texture2D>();
            foreach (MBEmitter em in emitters)
                if (em.Layer == lyr) {
                    foreach (Texture2D tex in em.Textures)
                        if (!res.Contains(tex))
                            res.Add(tex);
                }

            return res;
    }

  
    public void mbRender()
    {
        ParticlesRendered = 0;
        foreach (MBLayer lyr in Layer) {
            lyr.mbRender();
            ParticlesRendered += lyr.ParticlesRendered;
        }
        ParticlesRenderedMax = Mathf.Max(ParticlesRendered, ParticlesRenderedMax);
    }
    
    public override void mbReloadHierarchy()
    {
        base.mbReloadHierarchy();
        mEmitters = null;
        mLayers = null;
        // cache /refetch child emitters now!
        if (Emitter == null) { }
        if (Layer == null) { }
    }

    public Camera mbEditorCamera { get; set; }

    public void mbUpdate()
    {
        if (!Application.isPlaying) {
            foreach (MBEditorEnabledScript scr in mbEditorEnabledScripts)
                scr.mbEditorUpdate();
        }

        if (Playing && !Paused) {
            if (mTimeToWarp > 0) {
                DeltaTime = TimeWarpStepSize;
                while (mTimeToWarp>=0){
                    mbUpdateStep(false);
                    GlobalTime+=Mathf.Min(TimeWarpStepSize,mTimeToWarp);
                    mTimeToWarp -= TimeWarpStepSize;
                }
                mTimeToWarp = 0;
                if (PauseAfterTimeWarp)
                    Pause();
            }
            else
                mbUpdateStep(true);
        }
    }

    //@}

    #endregion

    #region ### Privates ###

    

    void mbUpdateStep(bool render)
    {
        Age = GlobalTime - mStartTime;
        AgePercent = (Infinite) ? 0.5f : Age / Duration;
        if (AgePercent > 1) {
            if (SingleShot)
                Playing = (ParticleCount > 0);
            else if (!Stopping) 
                Play();
            else
                Playing = (ParticleCount > 0);
        }
        
        ParticleCount = 0;
        MBEmitter[] ems = Emitter.ToArray();
        if (!Muted) {
            for (int i=0;i<ems.Length;i++)
                ParticleCount += ems[i].mbUpdateParticles(DeltaTime, render && !Invisible);
        }
    }

    void mbResetBuffers()
    {
        ParticlesRendered = 0;

        ParticleCount = 0;
        mIdleParticles.Clear();
        mbAddIdleParticles(ParticleThreshold);
    }
    
    void mbAddIdleParticles(int count)
    {
        for (int i = 0; i < count; i++)
            mIdleParticles.Add(new MBParticle());
    }

    

    #endregion

}

