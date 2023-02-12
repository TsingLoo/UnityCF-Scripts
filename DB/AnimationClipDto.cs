using MiniExcelLibs.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class AnimationClipDto
{
    public string AnimName { get; set; }

    [ExcelIgnore]
    [JsonIgnore]
    public AnimationClip AnimClip { get; set; }
    public float RealTime { get; set; }
    public float FrameRate { get; set; }
    public float Length { get; set; }
    public float Speed { get; set; }

    #region From QC
    public int TotalFrame { get; set; }
    // frames per second
    public int FPS { get; set; }
    #endregion

    // add events in code
    // https://docs.unity3d.com/ScriptReference/AnimationClip.AddEvent.html
    [ExcelIgnore]
    public List<AnimationEventDto> AnimEventDtos { get; set; } = new();

    #region help funcs

    public float GetTime()
    {
        if (Speed == 0)
        {
            Speed = 1;
        }

        return AnimClip.length / Speed;
    }

    public string GetNameAffix()
    {
        return AnimClip.name.Split("_").LastOrDefault();
    }

    internal int GetTotalFrame()
    {
        if(AnimClip != null)
        {
           return (int)(AnimClip.frameRate * AnimClip.length);
        }

        return 0;
    }
    #endregion
}
