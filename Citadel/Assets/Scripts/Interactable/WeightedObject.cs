using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightedObject : MonoBehaviour {

    [SerializeField]
    private int weight = 1;
    public int Weight { get { return weight; } }
}
