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

    public Quest(string _name, string _id, Transform _target, bool _complete)
    {
        name = _name;
        id = _id;
        target = _target;
        complete = _complete;
    }
}
