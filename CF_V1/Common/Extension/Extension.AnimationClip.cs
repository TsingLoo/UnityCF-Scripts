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
            return input.length;
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
}
