using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Event", menuName = "Scriptables/Event")]
public class SO_GenericEvent : ScriptableObject
{
    #region Event

    private event Action _action = delegate { };

    #endregion

    #region Methods

    public void Invoke()
    {
        _action.Invoke();
    }

    public void AddListener(Action listener)
    {
        _action -= listener;
        _action += listener;
    }

    public void RemoveListener(Action listener)
    {
        _action -= listener;
    }

    #endregion
}