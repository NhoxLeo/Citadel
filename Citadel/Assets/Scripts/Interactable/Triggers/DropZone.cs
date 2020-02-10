using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VHS;

public class DropZone : ActivationTrigger
{
    #region Variables & Inspector Options
    [Header("Setup Options")]
    #region Setup Options
    [Tooltip("Select if the player only needs to bring one of the possible objects within the list to this dropzone to trigger")]
    public bool allowSingleObjectFromList = false;

    [Tooltip("Select if the dropzone should continue to react forever without being used up (Number Of Uses must be at least 1)")]
    [ConditionalHide("numberOfUses", true)]
    public bool isInfinite = false;

    [Tooltip("Number of uses before this dropzone will be used up and will no longer trigger")]
    public int numberOfUses = 1;
    #endregion

    [Space(10)]
    [Header("Color Settings")]
    #region Color Settings
    [Tooltip("Color that will be displayed when no objects are interacting")]
    public Color defaultColor;
    [Tooltip("Color that will be displayed when a correct object has interacted")]
    public Color validColor;
    [Tooltip("Color that will be displayed when an incorrect object has interacted")]
    public Color invalidColor;
    #endregion

    [Space(10)]
    [Header("Compatible Objects")]
    #region Compatible Objects   
    public List<ObjectCount> objectList;
    #endregion    

    #region Stored Data
    private Dictionary<string, StorageCount> storage = new Dictionary<string, StorageCount>();
    private bool activated;
    public override bool Activated { get { return activated; } }
    private float progress;
    public override float Progress { get { return progress; } }
    private Material mat;
    private List<GameObject> invalidObjects = new List<GameObject>();
    private bool invalid, triggered;
    #endregion
    #endregion

    // Use this for initialization
    void Start ()
    {
        mat = GetComponent<MeshRenderer>().material;
        foreach (ObjectCount o in objectList)
        {
            storage.Add(o.objectID, new StorageCount(o.count));
        }
    }

    #region Listener Events
    private void OnTriggerEnter(Collider other)
    {
        RemoveDestroyedObjects();
        DropZoneObject d = other.GetComponent<DropZoneObject>();
            if (d == null)
            {
                return;
            }

            if (AddObject(d))
            {
                Destroy(d.gameObject);

                SetMaterialColor(validColor);
                SetMaterialColor(defaultColor, 2f);
                SetScale(3);
                SetScale(1, 3);
            }
            else
            {
                invalidObjects.Add(d.gameObject);
                SetMaterialColor(invalidColor, 0);
            }
    }

    private void OnTriggerExit(Collider other)
    {
        RemoveDestroyedObjects();
        invalidObjects.Remove(other.gameObject);

        if (invalidObjects.Count == 0)
        {
            SetMaterialColor(defaultColor, 0.5f);
        }
    }
    #endregion

    /// <summary>
    /// Event trigger that handles all the trigger properties before the RPC
    /// </summary>
    public void ActivateDropZone()
    {
        triggered = true;
        SetMaterialColor(validColor);

        Trigger(0);

        if (!isInfinite)
        {
            numberOfUses--;
        }

        if (numberOfUses > 0)
        {
            ResetDropZone();
        }
    }

    /// <summary>
    /// Resets the drop zone back to its OnStart state
    /// </summary>
    public void ResetDropZone()
    {
        //Debug.Log("Reset");
        invalidObjects.Clear();
        storage.Clear();
        progress = 0;
        foreach (ObjectCount o in objectList)
        {
            storage.Add(o.objectID, new StorageCount(o.count));
        }
        activated = false;
        triggered = false;
    }

    #region Helper Methods & Coroutines
    /// <summary>
    /// Remove objects that are destroyed before exiting trigger
    /// </summary>
    private void RemoveDestroyedObjects()
    {
        foreach (GameObject g in invalidObjects)
        {
            if (g == null)
            {
                invalidObjects.Remove(g);
            }
        }
    }

    /// <summary>
    /// Attempt to add an object to storage.
    /// </summary>
    /// <param name="other">Object to add</param>
    /// <returns>true if addition was successful, false otherwise.</returns>
    private bool AddObject(DropZoneObject other)
    {
        if (!storage.ContainsKey(other.ObjectID) || storage[other.ObjectID].Full)
        {
            return false;
        }

        // increment the object's counter
        storage[other.ObjectID].Add();
        // recalculate progress to completion
        progress = (float)storage.Values.Sum((x) => x.Count) / storage.Values.Sum((x) => x.Capacity);
        // activate if full
        if (!allowSingleObjectFromList)
        {
            if (activated = storage.Values.All((x) => x.Full) && !triggered)
            {
                ActivateDropZone();
            }
        }
        else
        {
            if (storage.Values.Count > 0 && !triggered)
            {
                ActivateDropZone();
            }
        }
        return true;
    }

    /// <summary>
    /// Intermediary function used for networking the call of OnActivated
    /// </summary>
    private void Trigger(int id)
    {
        ButtonReactions buttonReactions = GetComponent<ButtonReactions>();
        if (buttonReactions)
        {
            buttonReactions.cachedID = id;
        }

        OnActivated.Invoke();
    }

    /// <summary>
    /// Sets the material Color
    /// </summary>
    /// <param name="color">Color to use.</param>
    private void SetMaterialColor(Color color, float lerpDuration = 0)
    {
        if (lerpDuration == 0)
        {
            mat.color = color;
        }
        else
        {
            if (lerpColorCoroutine != null)
            {
                StopCoroutine(lerpColorCoroutine);
            }
            StartCoroutine(lerpColorCoroutine = LerpColor(color, lerpDuration));
        }
    }

    /// <summary>
    /// Sets the material scale
    /// </summary>
    /// <param name="scale">value to use.</param>
    private void SetScale(float scale, float rate = 0)
    {
        if (rate == 0)
        {
            mat.SetFloat("_Scale", scale);
        }
        else
        {
            if (smoothScaleCoroutine != null)
            {
                StopCoroutine(smoothScaleCoroutine);
            }
            StartCoroutine(smoothScaleCoroutine = SmoothScale(scale, rate));
        }
    }
    #endregion

    #region Coroutines
    /// <summary>
    /// Coroutine that handles the valid object color shifting
    /// </summary>
    private IEnumerator lerpColorCoroutine;

    /// <summary>
    /// Lerps to another color from current over a length of duration
    /// </summary>
    /// <param name="color"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator LerpColor(Color color, float duration)
    {
        while (mat.color != color)
        {
            mat.color = Vector4.MoveTowards(mat.color, color, 1 / duration * Time.deltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// Coroutine that handles smoothly scaling
    /// </summary>
    private IEnumerator smoothScaleCoroutine;

    /// <summary>
    /// Scales based on given parameter rate
    /// </summary>
    /// <param name="scale"></param>
    /// <param name="rate"></param>
    /// <returns></returns>
    private IEnumerator SmoothScale(float scale, float rate)
    {
        float currScale = mat.GetFloat("_Scale");
        while (currScale != scale)
        {
            currScale = Mathf.MoveTowards(currScale, scale, rate * Time.deltaTime);
            mat.SetFloat("_Scale", currScale);
            yield return null;
        }
    }
    #endregion

    #region Classes & Structs
    /// <summary>
    /// Class used within the disctionary of stored objects to compare IDs with
    /// </summary>
    private class StorageCount
    {
        public int Capacity { get; private set; }
        public int Count { get; private set; }
        public bool Full { get { return Count >= Capacity; } }

        public StorageCount(int capacity)
        {
            Capacity = capacity;
        }

        public void Add()
        {
            ++Count;
        }
    }

    /// <summary>
    /// Stored Object with an ID and the total number needed to trigger this drop zone
    /// </summary>
    [System.Serializable]
    public class ObjectCount
    {
        [Header("Interactable Object")]
        [Tooltip("ID string that will represent an interactable object found on the drop zone object script")]
        public string objectID;
        [Tooltip("The total number of this object that must interact with the drop zone to trigger")]
        public int count;    
    }
    #endregion
}
