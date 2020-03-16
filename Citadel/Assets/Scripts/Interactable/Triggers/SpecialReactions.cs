 using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace VHS
{
    public class SpecialReactions : InteractableBase
    {
        #region Variables & Inspector Options
        [Header("Setup Options")]
        #region Setup Options
        public bool triggerAllEventsAtOnce = true;
        public bool restartListOnComplete = false;
        [Tooltip("Delays between the triggering of events")]
        public float eventsStartDelay, eventsBetweenDelay;
        #endregion

        [Space(10)]
        [Header("Events")]
        #region Events
        [Tooltip("Add event scripts to be triggered like ButtonReactions")]
        public List<UnityEvent> reactionEvents;
        #endregion

        #region Stored Data
        [HideInInspector]
        private int currentEventIndex;
        #endregion
        #endregion

        #region Listener Events
        /// <summary>
        /// If interacted with, begin triggering events
        /// </summary>         public override void OnInteract(Vector3 contactPoint = new Vector3(), Transform playerGrip = null)
        {
            StartCoroutine(EventsHandler());
        }
        #endregion

        #region Event Trigger/Handler
        /// <summary>
        /// Depending on whether the user is in VR or on Desktop or if they are online or offline, this will call the trigger method with correct parameters
        /// </summary>
        private IEnumerator EventsHandler()
        {
            yield return new WaitForSeconds(eventsStartDelay);
            if (reactionEvents != null)
            {
                StartCoroutine(TriggerEvent());
            }
        }

        /// <summary>
        /// Handles the triggering of all elements within the events list with specified delay
        /// </summary>
        private IEnumerator TriggerEvent()
        {
            reactionEvents[currentEventIndex].Invoke();
            if (currentEventIndex < reactionEvents.Count - 1)
            {
                currentEventIndex++;
                if (triggerAllEventsAtOnce)
                {
                    yield return new WaitForSeconds(eventsBetweenDelay);
                    StartCoroutine(TriggerEvent());
                }
            }
            else
            {
                if (restartListOnComplete)
                {
                    currentEventIndex = 0;
                }
            }
        }
        #endregion
    }
}
