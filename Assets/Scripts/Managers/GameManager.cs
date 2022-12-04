using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    private static List<Mirror> _mirrorPool;

    private void Awake()
    {
        _instance = this;
        _mirrorPool = new List<Mirror>();
        PlayerPrefs.SetString("Level", SceneManager.GetActiveScene().name);
    }

    public static GameManager GetInstance()
    {
        return _instance;
    }

    public void AddMirrorToPool(Mirror mirror)
    {
        _mirrorPool.Add(mirror);
    }

    public List<Mirror> GetMirrorsPool()
    {
        return _mirrorPool;
    }
}