using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ActivationTrigger : MonoBehaviour {
    public abstract bool Activated { get; }
    public abstract float Progress { get; }

    [SerializeField]
    protected UnityEvent OnActivated = new UnityEvent();
    [SerializeField]
    protected UnityEvent OnDisabled = new UnityEvent();
}
