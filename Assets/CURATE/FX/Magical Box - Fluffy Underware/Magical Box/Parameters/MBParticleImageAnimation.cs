// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Image Animation parameter
/// </summary>
/// <remarks>
/// See also: \ref paramimageanim "ImageAnimation parameter reference"
/// </remarks>
[MBParameterInfo(Menu = "Image Animation",
                 CanAnimateBirth = MBParameterAnimationMode.Optional,
                 CanAnimateLife = MBParameterAnimationMode.Optional)]
public class MBParticleImageAnimation : MBParameter
{
    /// <summary>
    /// Starting frame
    /// </summary>
    public int StartFrame=0;
    
    /// <summary>
    /// Birth animation mode
    /// </summary>
    public MBImageAnimationMode BirthAnimMode;
    /// <summary>
    /// Birth animation repeating mode
    /// </summary>
    public MBImageAnimationRepeat BirthAnimRepeat;
    /// <summary>
    /// Birth animation timing mode
    /// </summary>
    public MBImageAnimationTimingMode BirthAnimTiming;
    /// <summary>
    /// Birth animation speed
    /// </summary>
    public float BirthAnimSpeed;
    /// <summary>
    /// Birth animation direction
    /// </summary>
    public MBImageAnimationDirection BirthAnimDirection;
    /// <summary>
    /// Lifetime animation mode
    /// </summary>
    public MBImageAnimationMode LifetimeAnimMode;
    /// <summary>
    /// Lifetime animation repeating mode
    /// </summary>
    public MBImageAnimationRepeat LifetimeAnimRepeat;
    /// <summary>
    /// Lifetime animation timing mode
    /// </summary>
    public MBImageAnimationTimingMode LifetimeAnimTiming;
    /// <summary>
    /// Lifetime animation speed
    /// </summary>
    public float LifetimeAnimSpeed;
    /// <summary>
    /// Lifetime animation direction
    /// </summary>
    public MBImageAnimationDirection LifetimeAnimDirection;

    /// <summary>
    /// Gets the number of available frames
    /// </summary>
    public int FramesCount
    {
        get { return ParentEmitter.FramesUV.Length; }
    }

    float mBirthStepTime;
    int mBirthFrame;
    float mBirthLastTime;
    int mBirthDir;

    float mLifetimeStepTime;
    int mLifeLastTimeID;
    int mLifeDirID;

     public override void OnPlay()
    {
        base.OnPlay();
        mBirthFrame = StartFrame;
        if (AnimatedBirth) {
            switch (BirthAnimTiming) {
                case MBImageAnimationTimingMode.FramesPerSecond:
                    mBirthStepTime = (BirthAnimSpeed > 0) ? 1f / BirthAnimSpeed : 0;
                    break;
                case MBImageAnimationTimingMode.FramesPerDuration:
                    mBirthStepTime = (BirthAnimSpeed > 0) ? ParentEmitter.Duration / BirthAnimSpeed : 0;
                    break;
            }
            mBirthLastTime = ParticleSystem.GlobalTime;
            mBirthDir = (int)BirthAnimDirection;
        }
        if (AnimatedLife) {
            mLifeLastTimeID = ParentEmitter.RegisterParticleUserData("ImageAnimLastTime" + GetInstanceID());
            mLifeDirID = ParentEmitter.RegisterParticleUserData("ImageAnimDir" + GetInstanceID());

            switch (LifetimeAnimTiming) {
                case MBImageAnimationTimingMode.FramesPerSecond:
                    mLifetimeStepTime = (LifetimeAnimSpeed > 0) ? 1f / LifetimeAnimSpeed : 0;
                    break;
                case MBImageAnimationTimingMode.FramesPerDuration:
                    mLifetimeStepTime = (LifetimeAnimSpeed > 0) ? ParentEmitter.Duration / LifetimeAnimSpeed : 0;
                    break;
            }
            
        }
    }

     public override void OnBirth(MBParticle PT)
     {
         if (AnimatedBirth) {
             if (ParticleSystem.GlobalTime - mBirthLastTime >= mBirthStepTime) {
                 mBirthFrame = Advance(mBirthFrame, ref mBirthDir, BirthAnimRepeat, BirthAnimMode);
                 mBirthLastTime += mBirthStepTime;
             }
         }
         
         if (AnimatedLife && PT.HasUserData(mLifeLastTimeID) && PT.HasUserData(mLifeDirID))
         {
             PT.UserData[mLifeLastTimeID] = ParticleSystem.GlobalTime;
             PT.UserData[mLifeDirID] = LifetimeAnimDirection;
         }

         PT.ImageFrameIndex = mBirthFrame - 1;
     }

     public override bool OnLifetime(MBParticle PT)
     {
         if (PT.HasUserDataValue(mLifeLastTimeID) && PT.HasUserDataValue(mLifeDirID)) {
             float lastTime = (float)PT.UserData[mLifeLastTimeID];
             int dir = (int)PT.UserData[mLifeDirID];
             if (ParticleSystem.GlobalTime - lastTime >= mLifetimeStepTime) {
                 PT.ImageFrameIndex = Advance(PT.ImageFrameIndex, ref dir, LifetimeAnimRepeat, LifetimeAnimMode);
                 PT.UserData[mLifeLastTimeID] = lastTime + mLifetimeStepTime;
                 PT.UserData[mLifeDirID] = dir;
             }
         }
         return true;
     }

     int Advance(int frame, ref int dir, MBImageAnimationRepeat repeat, MBImageAnimationMode mode)
     {
         if (mode == MBImageAnimationMode.Random)
             return Random.Range(0, FramesCount-1);
         
         int newframe = frame+dir;
         switch (dir) {
             case 1: // Forward
                 if (newframe == FramesCount) {
                     switch (repeat) {
                         case MBImageAnimationRepeat.Loop:
                             return 0;
                         case MBImageAnimationRepeat.PingPong:
                             dir *= -1;
                             return FramesCount - 2;
                     }
                 }
                 return newframe;
             case -1: // Backward
                 if (newframe < 0) {
                     switch (repeat) {
                         case MBImageAnimationRepeat.Loop:
                             return FramesCount-1;
                         case MBImageAnimationRepeat.PingPong:
                             dir *= -1;
                             return 1;
                     }
                 }
                 return newframe;
         }
         return frame;
     }

     public override void Validate()
     {
         StartFrame = Mathf.Clamp(StartFrame, 0, FramesCount-1);
     }

     public override void Reset()
     {
         base.Reset();
         StartFrame = 0;
         AnimatedBirth = true;
         AnimatedLife = false;
         BirthAnimSpeed = 1;
         BirthAnimDirection = MBImageAnimationDirection.Forward;
         BirthAnimTiming = MBImageAnimationTimingMode.FramesPerSecond;
         BirthAnimRepeat = MBImageAnimationRepeat.Loop;
         LifetimeAnimSpeed = 1;
         LifetimeAnimDirection = MBImageAnimationDirection.Forward;
         LifetimeAnimTiming = MBImageAnimationTimingMode.FramesPerSecond;
         LifetimeAnimRepeat = MBImageAnimationRepeat.Loop;
     }

}

public enum MBImageAnimationDirection
{
    Forward = 1,
    Backward = -1
}

public enum MBImageAnimationMode
{
    /// <summary>
    /// Advance to the next frame in order
    /// </summary>
    InOrder = 0,
    /// <summary>
    /// Advance to a random frame
    /// </summary>
    Random = 1
}


public enum MBImageAnimationTimingMode
{
    /// <summary>
    /// Advance images by time
    /// </summary>
    FramesPerSecond = 0,
    /// <summary>
    /// Advance images in relation to emitter's duration
    /// </summary>
    FramesPerDuration = 1
}

/// <summary>
/// Defines image animations repeating mode
/// </summary>
public enum MBImageAnimationRepeat
{
    /// <summary>
    /// Play only once
    /// </summary>
    Once = 0,
    /// <summary>
    /// Loop
    /// </summary>
    Loop = 1,
    /// <summary>
    /// Reverse and loop
    /// </summary>
    PingPong = 2
}

    