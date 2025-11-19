using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Quest
{
    public string name;
    public string id;
    public Transform target;
    public bool complete;
    public bool runImmediately;

    public Quest(string _name, string _id, Transform _target, bool _complete, bool _runImmediately)
    {
        name = _name;
        id = _id;
        target = _target;
        complete = _complete;
        runImmediately = _runImmediately;
    }
}
