using UnityEngine;

/// <summary>
/// Utilitário para localizar a ponta do canhão e converter clique do mouse em coordenadas do mundo.
/// </summary>
public static class CannonFirePoint
{
    public static Transform Find()
    {
        GameObject launcher = GameObject.Find("MissileLauncher");
        if (launcher == null)
            return null;

        Transform tip = launcher.transform.Find("canhao ponta_0");
        if (tip != null)
            return tip;

        foreach (Transform child in launcher.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains("ponta"))
                return child;
        }

        return launcher.transform;
    }

    public static Vector3 GetMouseWorldPosition(Camera camera = null)
    {
        camera ??= Camera.main;
        if (camera == null)
            return Vector3.zero;

        Vector3 mouse = Input.mousePosition;
        mouse.z = Mathf.Abs(camera.transform.position.z);
        Vector3 world = camera.ScreenToWorldPoint(mouse);
        world.z = 0f;
        return world;
    }
}
