

using UnityEngine;

public abstract class BaseAnimationController: MonoBehaviour
{
    protected float baseAnimSpeed = 1;

    protected string Join(string a, string b)
    {
        return AnimNames.Combine(a, b);
    }

    protected string Join(string a, string b, string c)
    {
        return AnimNames.Combine(a, b, c);
    }
}