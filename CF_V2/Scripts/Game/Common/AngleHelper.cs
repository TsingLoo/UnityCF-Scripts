using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class AngleHelper
{
    public static float ClampAngle360(float angleIn,
        float minAngle = float.MinValue,
        float maxAngle = float.MaxValue)
    {
        if (angleIn < -360f)
        {
            angleIn += 360f;
        }

        if (angleIn > 360f)
        {
            angleIn -= 360f;
        }

        return Mathf.Clamp(angleIn, minAngle, maxAngle);
    }

}
