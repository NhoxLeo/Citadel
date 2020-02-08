using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightedButton : ActivationTrigger {

    public int targetWeight = 1;

    private readonly Dictionary<GameObject, int> activeObjects = new Dictionary<GameObject, int>();
    private int totalWeight;

    public override bool Activated { get { return totalWeight >= targetWeight; } }

    public override float Progress { get { return Mathf.Min(totalWeight / targetWeight); } }

    // Update is called once per frame
    void Update () {
        // Remove objects that are destroyed before exiting trigger
        foreach (KeyValuePair<GameObject, int> pair in activeObjects)
            if (pair.Key == null) {
                activeObjects.Remove(pair.Key);
                totalWeight -= pair.Value;
                break;
            }
    }
    
    private void OnTriggerEnter(Collider other) {
        WeightedObject o = other.GetComponent<WeightedObject>();
        if (o == null)
            return;
        bool wasActivated = Activated;
        activeObjects.Add(o.gameObject, o.Weight);
        AddObject(o);
        if (wasActivated != Activated)
            OnActivated.Invoke();
    }

    private void OnTriggerExit(Collider other) {
        WeightedObject o = other.GetComponent<WeightedObject>();
        if (o == null)
            return;
        bool wasActivated = Activated;
        activeObjects.Remove(o.gameObject);
        RemoveObject(o);
        if (wasActivated != Activated)
            OnDisabled.Invoke();
    }
    
    private void AddObject(WeightedObject other) {
        totalWeight += other.Weight;
    }

    private void RemoveObject(WeightedObject other) {
        totalWeight -= other.Weight;
    }
}
