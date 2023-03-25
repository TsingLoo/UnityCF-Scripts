using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.SceneTemplate;
using UnityEngine;

public partial class Extension
{
    public static string GetNameAffix(this AnimationClip input)
    {
        return input.name.Split("_").LastOrDefault();
    }

    public static float GetSpeedByTime
        (this AnimationClip input, float time)
    {
        if(input == null)
        {
            return 1f;
        }

        if(time == 0)
        {
            return 1;
        }

        return input.length / time;
    }

    public static float GetTimeBySpeed
        (this AnimationClip input, float speed)
    {
        if(speed == 0) 
        {
            speed = 1;
        }
        return input.length / speed;
    }

    //public static void AddEvents(this AnimationClip input, 
    //    AnimationEvent[] list)
    //{
    //    foreach (var animEvent in list)
    //    {
    //        input.AddEvent(animEvent);
    //    }
    //}
}
