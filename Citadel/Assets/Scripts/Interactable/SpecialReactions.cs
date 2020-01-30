using System.Collections.Generic;
using UnityEngine; using UnityEngine.EventSystems;
using UnityEngine.Events;
using VHS;

public class SpecialReactions : MonoBehaviour
{
    #region Variables & Inspector Options
    [Header("Setup Options")]
    #region Setup Options
    //[Tooltip("Manager needed to get what reactions are within this scene")]
    //public CombinationInteractionManager combinationInteractionManager;

    [Tooltip("Game Object that will be highlighted when a correct reactant for this reactor is picked up (Requires Mesh Collider and Highlight Effect script)")]
    public GameObject gameObjectToHighlight;

    [Tooltip("Select which reaction mode should be looked for to trigger this reaction in VR")]
    public VRReactionMode vrReactionMode = VRReactionMode.OnCollide;

    [Tooltip("Delays between the triggering of events")]
    public float eventsStartDelay, eventsBetweenDelay;

    [Tooltip("Select if the object held will be destroyed after valid use")]
    public bool destroyGrabbedObjectOnUse;

    [Tooltip("Index within the CombInE Manager that this reactor is used (Automatically Set)")]
    [HideInInspector]
    public int myInteractionIndex = 0;
    #endregion

    [Space(10)]
    [Header("Events")]
    #region Events
    [Tooltip("Add event scripts to be triggered like ButtonReactions")]
    public List<UnityEvent> reactionEvents;

    [Tooltip("Current state of this script that will only activate when Ready. (Use a reaction script like ButtonReact to reset this value after use)")]
    [HideInInspector]
    public ButtonReactions.ReactionState reactionState = ButtonReactions.ReactionState.Ready;
    #endregion

    #region Stored Data
    [HideInInspector]
    public GameObject currentReactant;
    private Vector3 MousePosition;
    private GrabIndicator GrabIndicator;
    private int currentEventIndex;   
    private HighlightPlus.HighlightEffect highlightEffect;
    public enum VRReactionMode { OnCollide, OnTrigger };
    #endregion
    #endregion

    // Use this for initialization
    void Start ()
    {
        GrabIndicator = GetComponent<GrabIndicator>();

        if (gameObjectToHighlight == null)
        {
            gameObjectToHighlight = gameObject;
            Debug.Log("[WARNING] GameObjectToHighlight is missing! Attempting to populate with self.");
        }

        highlightEffect = gameObjectToHighlight.GetComponent<HighlightPlus.HighlightEffect>();
        if (highlightEffect == null)
        {
            Debug.Log("[WARNING] GameObjectToHighlight is missing a Highlight Effect script!");
        }

    }

    private void OnDrawGizmos()
    {
        /*
        if(combinationInteractionManager == null)
        {
            combinationInteractionManager = GameObject.FindObjectOfType<CombinationInteractionManager>();
        }
        
        if(GetComponent<SpecialReactionsObjectSetup>())
        {
            if(GetComponent<SpecialReactionsObjectSetup>().readyToDestroy)
            {
                DestroyImmediate(GetComponent<SpecialReactionsObjectSetup>());
            }            
        }
        */
    }

    /// <summary>
    /// Toggles the highlight effect located on GameObjectToBeHighlighted
    /// </summary>
    public void ToggleHighlight(bool toggle)
    {
        if (highlightEffect != null)
        {
            if (reactionState != ButtonReactions.ReactionState.Finished)
            {
                if (toggle == false)
                {
                    highlightEffect.SetHighlighted(false);
                }
                else
                {
                    highlightEffect.SetHighlighted(true);
                }
            }
            else
            {
                highlightEffect.SetHighlighted(false);
            }
        }
    }

    #region Listener Events
    private void OnMouseEnter()     {         if (GrabIndicator)
        {             GrabIndicator.GrowIndicator();
        }     }      private void OnMouseExit()     {         if (GrabIndicator)
        {             GrabIndicator.ShrinkIndicator();
        }     }      private void OnMouseDown()     {         MousePosition = Input.mousePosition;     }  

    /// <summary>
    /// Verifies if the correct gameObject is currently held and the correct gameObject is clicked to trigger a valid reaction from the manager
    /// </summary>     private void OnMouseUp()     {         if (reactionState == ButtonReactions.ReactionState.Ready && MousePosition == Input.mousePosition && !EventSystem.current.IsPointerOverGameObject() && ((GrabIndicator && GrabIndicator.GrabSprite.gameObject.activeSelf) || !GrabIndicator))         {
            /*
            if(combinationInteractionManager.CheckInteraction(RealPlayerManager.Instance.MyAvatar.gameObject.GetComponent<TrainerGrab>().grabbedObject, gameObject))
            {
                currentReactant = RealPlayerManager.Instance.MyAvatar.gameObject.GetComponent<TrainerGrab>().grabbedObject;
                reactionState = ButtonReactions.ReactionState.Active;
                EventsHandler();
            }             */         }     }

    #endregion

    #region Event Trigger/Handler
    /// <summary>
    /// Depending on whether the user is in VR or on Desktop or if they are online or offline, this will call the trigger method with correct parameters
    /// 
    /// </summary>
    private void EventsHandler()
    {
        if(reactionEvents != null & reactionEvents.Count > 0 && currentEventIndex == 0)
        {
            if (destroyGrabbedObjectOnUse)
            {
                //combinationInteractionManager.UpdateHighlightedObjects(null);
                //RealPlayerManager.Instance.DestroyObjects();
            }
            
            Trigger(0);
        }
    }

    /// <summary>
    /// Intermediary function used for networking the call of the recursive function TriggerEvent
    /// </summary>
    private void Trigger(int id)
    {
        ButtonReactions buttonReactions = GetComponent<ButtonReactions>();
        if(buttonReactions)
        {
            buttonReactions.cachedID = id;
        }

        Invoke("TriggerEvent", eventsStartDelay);
    }
     
    /// <summary>
    /// Recursive function that handles the triggering of all elements within the events list with specified delay
    /// </summary>
    private void TriggerEvent()
    {
        //Debug.Log("TriggerEvent");
        reactionEvents[currentEventIndex].Invoke();
        if (currentEventIndex < reactionEvents.Count-1)
        {
            currentEventIndex++;
            Invoke("TriggerEvent", eventsBetweenDelay);
        }
    }
    #endregion


}
