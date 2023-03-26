using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public partial class Extension
{
    public static void Enable(this Behaviour input)
    {
        if (input != null)
        {
            input.enabled = true;
        }
    }

    public static void Disable(this Behaviour input)
    {
        if (input != null)
        {
            input.enabled = false;
        }
    }
}

