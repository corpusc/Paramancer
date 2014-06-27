Magical Box v1.22
=================

Product homepage: http://fluffyunderware.com/pages/unity-plugins/magical-box.php
Documentation: http://docs.fluffyunderware.com/magicalbox
Support Forum: http://forum.fluffyunderware.com/

Getting started
===============
Please watch the workshop videos available at the product website and read the online documentation

Version History
===============
Version:
	1.22
		New:
			Added SpawnObject() and DespawnObject() to MBParticleTransformConnector, check the docs for usage
			Particle Debugger now visualize zones
			Added MBEmitter.TransformPointToParticleSystem()
			Added MBEmitter.LaunchBuffer
			Added SupportsWorldSpace to Zone Parameters
			Added Note property to MBParameterInfo
		Changes:
			Several Performance optimizations
			Changed editor window max. width
			Rearranged some Parameters' GUI to better fit editor window
		Fixes:
			Fixed trail's heading modes not working properly
			Fixed typo in editor template
			Fixed MBParticleMass randomness
			Loading Layers now opens file dialog in the Assets folder
			MBEmitterType.FixedHeadingIsGlobal working properly now
			Fixed MBParticleRectForce.WorldSpace
			Zone forces now affected by Mass properly
			Fixed link to support forum
	1.21
		New:
			Added MBEmitter.InstantLaunchRepeat
			Added MBEmitterType.FixedHeadingIsGlobal
			Added wizard for creating Editor Enabled Scripts
			Special/MathOperator: Added DistanceToCenter as Target
			Parameter Order now swapable in the editor
		Changes:
			Reorganized folder structure. Now everything's going into <root>/Magical Box
			Gizmo icons will now be drawn without scaling
			Removed MBLayer.UpdateBounds
		Fixes:
			Fixed spawned emitters may miss parameters
			Rendering in editor now uses the gameObject's layer
	1.20
		New:
			Unity 4 ready
			Added OnParticleEntering and OnParticleLeave events to all zone parameters
			Added MBParticleZoneMode.EventsOnly allowing zones to only generate events
			Added playMaker Integration
			Added pooling functionality to MBAnchor
			Added MBSphereEmitter.Arc to produce half spheres
			Added MBEmitter.CameraAware to simulate frustum culling, see Culling in the manual
			Added MBParticleSphereForce
			Added MBParticleCollider.BounceCombine and MBParticleCollider.RestBelowVelocity
			Added wizards for creating custom emitter types and parameters
			Mute now works for layers, too
		Changes:
			Reworked Add-methods (AddAnchor,AddEmitter etc.): added error checks and some more functions
			Marked Add-methods of MBEmitter and MBAnchor as obsolete. They will be removed with the next update
			Reworked Image Animation Parameter
		Fixes:
			Removed big texture preview in ParticleSystem inspector to save some space and prevent layout hiccup
			Fixed ParticleSystem not scaling correctly in edit mode GameView
			Fixed Radial Zone reflection when particles collide from the inside
			Fixed editor timing bug when selecting a ParticleSystem after scene loading
			Some minor bug fixes
	1.15
        New:
            Magical Box now working on mobiles! Just replace material shaders with the respective "Mobile/Particles" ones and you're done.
			Moved the Magical Box menu to "Window" as needed by the new guidelines
        Changes:
            Removed MBEmitterType.FitScreen and replaced it with MBEmitterType.FitScreenWidth and MBEmitterType.FitScreenHeight. These parameters are now enabled for all emitter types.
        Fixes:
            Fixed Debugging size not calculated correctly
            Fixed error when pasting emitters
			Fixed a bug that infrequently caused an error when showing the "About" window

Version:
    1.13
        New:
            Debugging Particle Properties
            Slow motion in the editor
            Math Operator
            MBEditorEnabledScript
            MBSyncToCursor2D script
            MBRotateAxis script
            Copy&Paste for all curves
            Added behaviour mode (attract,freeze,kill,reflect) to zones
            Added arc range limitation to EmitterType's Random2D heading mode
            Added MBParticle.HasUserData and MBParticle.HasUserDataValue
            MBUtility.DrawGizmoArc now supporting optional radius parameter
            MBUtility.Get
            Now supporting scale on Magical Box GameObjects
        Changes:
            UI overhaul
            MBParticleSystem.AutoPlay now defaults to true
            Zone parameters now skip calculation when Attraction==0
        Fixes:
            Several minor fixes and code improvements


Version:
    1.12
        New:
            Added MBLayer.RenderQueue
        Fixes:
			Fixed Prefab publishing
            several minor fixes and code improvements

Version:
    1.10
        New:
            Layers !!! See here for updating instructions!
            Added to MBParticleSystem:
                MBParticleSystem.AddLayer()
                MBParticleSystem.FindLayer()
                MBParticleSystem.Layer
            Added MBEmitter.LayerShaderName
            Added menu command "Publish selected prefabs"
            Added MBParticleImageAnimation.MBImageAnimationAdvanceMode
            Added vertex normal support (see Using shaders that require vertex normals)
        Fixes:
            MBParticleImageAnimation: Random mode ignored the last frame
            Reworked some documentation (e.g. added an engine scheme)
            Changed to to prevent warnings when using Unity 3.4.x
            Changed type of Mesh parameter of MBMeshVertexEmitter, MBMeshSurfaceEmitter and MBMeshEdgeEmitter from Mesh to MeshFilter

Version:
    1.02

        New:
            Editor Event Binding: MBSendMessageTarget. See Events
            Vector Emitter types : Line, Polygon
            Mesh Emitter types : Vertex, Edge, Surface
            Additional heading modes: Random2D, Random3D, MeshNormal
        Fixes:
            Several minor code improvements and fixes

Version:
    1.01

        New:
            MBParticleSystem.Invisible, MBEmitter.Invisible to suppress rendering
            MBParticle.UserData
            MBBoxEmitter, MBHollowBoxEmitter
            MBParticleSize3
            MBParticleTransformConnector
        Fixes:
            MBParticleSystem.Muted not working
            Halt() not firing ParticleDeath event

Version:
    1.00 Initial release 

