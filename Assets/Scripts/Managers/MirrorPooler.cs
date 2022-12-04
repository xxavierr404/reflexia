using System.Collections.Generic;
using UnityEngine;

public class MirrorPooler : MonoBehaviour
{
    private static List<Mirror> _pool;
    
    private void Awake()
    {
        _pool = new List<Mirror>();
    }

    public static void AddMirror(Mirror mirror)
    {
        _pool.Add(mirror);
    }

    public static List<Mirror> GetMirrorPool()
    {
        return _pool;
    }
}