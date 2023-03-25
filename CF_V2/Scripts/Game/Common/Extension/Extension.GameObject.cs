using UnityEngine;

public static partial class Extension
{
    public static bool DisablePhysics(this GameObject input)
    {
        var changed = false;

        // rigidbody
        var rigidbody = input.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.isKinematic = true;
            changed = true;
        }

        // colliders
        var colliders = input.GetComponentsInChildren<Collider>();
        if(colliders.HasValue()) 
        {
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            changed = true;
        }

        return changed;
    }

    public static void Show(this GameObject input, bool show)
    {
        if (input != null)
        {
            input.gameObject.SetActive(show);
        }
    }

    public static void Show(this GameObject input)
    {
        if (input != null)
        {
            input.gameObject.SetActive(true);
        }
    }

    public static void Hide(this GameObject input)
    {
        if (input != null)
        {
            input.gameObject.SetActive(false);
        }
    }


    public static GameObject DeepFind(this GameObject gameObject, string name)
    {
        foreach (Transform t in gameObject.GetComponentsInChildren<Transform>())
        {
            if (t.name == name)
                return t.gameObject;
        }

        return null;
    }

    public static void SelfDestroy(this GameObject gameObject, float seconds = 0f)
    {
        Object.Destroy(gameObject, seconds);
    }

}

