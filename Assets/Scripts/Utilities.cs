using System.Collections;
using UnityEngine;

public static class Utilities
{
    public static Vector3 DampVelocity;
    public const short FramesDelay = 5;

    public static IEnumerator Focus(Transform from, Transform to)
    {
        while (true)
        {
            var targetPos = to.TransformPoint(Vector3.back * 12);
            from.position = Vector3.SmoothDamp(from.position, targetPos, ref DampVelocity, 0.3f);
            yield return null;
        }
    }

    public static IEnumerator LerpPosition(Transform from, Vector3 targetPos, float duration)
    {
        while (Vector3.Distance(from.position, targetPos) > 0.1f)
        {
            from.position = Vector3.Lerp(from.position, targetPos, Time.deltaTime / duration);
            yield return null;
        }
    }

    public static IEnumerator LerpRotation(Transform from, Transform to, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            from.rotation = Quaternion.Lerp(from.rotation, to.rotation, time);
            time += Time.deltaTime;
            yield return null;
        }
    }

    public static IEnumerator LerpRotation(Transform from, Vector3 euler, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            from.rotation = Quaternion.Lerp(from.rotation, Quaternion.Euler(euler), time);
            time += Time.deltaTime;
            yield return null;
        }
    }

    public static Texture2D ToTexture2D(RenderTexture rTex)
    {
        var tex = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    public static bool IsVisible(Transform visibleTransform, Camera source, float tolerancy)
    {
        var pos = source.WorldToViewportPoint(visibleTransform.position);
        return pos.x < 1f + tolerancy && pos.x > -tolerancy && pos.y < 1f + tolerancy && pos.y > -tolerancy &&
               pos.z > 0;
    }

    public static GameObject FindNearestMirror(Transform origin, float maxDistance)
    {
        var mirrors = GameObject.FindGameObjectsWithTag("Mirror");
        if (mirrors.Length == 0) return null;
        var minDistance = Mathf.Infinity;
        GameObject nearestMirror = null;
        foreach (var checkMirror in mirrors)
        {
            var currentDistance = (origin.position - checkMirror.transform.position).magnitude;
            if (currentDistance <= maxDistance && currentDistance <= minDistance)
            {
                minDistance = currentDistance;
                nearestMirror = checkMirror;
            }
        }

        return nearestMirror;
    }

    public static bool IsGrounded(Transform objectToCheck)
    {
        return Physics.Raycast(objectToCheck.position, -Vector3.up, 1.05f, ~LayerMask.GetMask("PlayerReflection"));
    }

    public static bool IsTriggeredByPlayer(Collider collider)
    {
        return collider.transform.CompareTag("Player");
    }

    public static bool IsCollidedWithMovable(Collision collision)
    {
        return collision.gameObject.layer == 8;
    }
}