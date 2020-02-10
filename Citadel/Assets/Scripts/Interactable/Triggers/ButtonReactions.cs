using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHS
{
    public class ButtonReactions : InteractableBase
    {
        #region Variables & Inspector Options
        [Header("Reaction Modes")]
        #region Reaction Modes
        [Tooltip("Select if you wish to play a particle on use")]
        public bool useParticle;

        [Tooltip("Select if you wish for audio to play on use")]
        public bool useAudio;

        [Tooltip("Select if you wish for an event to trigger on use")]
        public bool useRelocation;

        [Tooltip("Select if you wish for an animation to trigger on use")]
        public bool useAnimation;

        [Tooltip("Select if you wish for an gameObject to be instantiated on use")]
        public bool useInstantiation;
        #endregion

        [Space(10)]
        [Header("Setup Info")]
        #region Setup Info

        [Tooltip("Animator that will handle the triggering of various conditions")]
        [ConditionalHide("useAnimation", true)]
        public Animator animator;

        [Tooltip("Select if this reaction has infinite or limited uses")]
        public ReactionLimitMode reactionLimitMode = ReactionLimitMode.Infinite;

        [Tooltip("Number of times this button can be used (Leave at one if infinite)")]
        [ConditionalHide("isLimited", true)]
        public int numberOfUses = 1;

        [Tooltip("Select if this object should destroy itself after delayBeforeObjectReset Finishes (Must not be infinite, will only destroy after all uses finish)")]
        public bool destroySelfAfterUse;

        [HideInInspector]
        public ReactionState reactionState = ReactionState.Ready;
        #endregion

        [Space(10)]
        [Header("Particle Content")]
        #region Particle Content
        [Tooltip("Select the mode that this particle will be used in")]
        [ConditionalHide("useParticle", true)]
        public ContentMode particleMode = ContentMode.IndexIncrement;

        [Tooltip("The delay between each particle being played if playParticlesAtOnce is false")]
        [ConditionalHide("useParticle", true)]
        public float particleDelay;

        [Space(10)]
        #region Particle Content Debug
        [Tooltip("Select if you wish to have the particle gizmos visible")]
        [ConditionalHide("useParticle", true)]
        public bool showParticlePoints = true;
        #endregion

        [Tooltip("List of available particles (Default will play all particles")]
        [ConditionalHide("useParticle", true)]
        [Space(5)]
        public InstantiationList particles;
        #endregion

        [Space(10)]
        [Header("Audio Content")]
        #region Audio Content
        [Tooltip("Select the mode that this audio will be used in")]
        [ConditionalHide("useAudio", true)]
        public ContentMode audioMode = ContentMode.IndexIncrement;

        [Tooltip("The delay between each sound effect being played if playAudioAtOnce is false")]
        [ConditionalHide("useAudio", true)]
        public float audioDelay;

        [Tooltip("List of available sound effects (Default will play all sound effects")]
        [ConditionalHide("useAudio", true)]
        [Space(5)]
        public AudioList audio;
        #endregion

        [Space(20)]
        [Header("Animation Content")]
        #region Animation Content
        [Tooltip("Select the mode that this animation will be used in")]
        [ConditionalHide("useAnimation", true)]
        public ContentMode animationMode = ContentMode.IndexIncrement;

        [Tooltip("Please add the total time this script should wait before considering the animation sequence finished (Leave at -1 if an external source will control this with FinishedAnimation)")]
        [ConditionalHide("useAnimation", true)]
        public float animationLength = -1;

        [Tooltip("The delay between each animation condition being toggled if toggleAnimationAtOnce is false")]
        [ConditionalHide("useAnimation", true)]
        public float animationDelay;

        [Tooltip("List of available animation conditions (Default will toggle all animations")]
        [ConditionalHide("useAnimation", true)]
        [Space(5)]
        public AnimationConditionList animationConditions;
        #endregion

        [Space(20)]
        [Header("Instantiation Content")]
        #region Instantiation Content
        [Tooltip("Select the mode that this instantiation will be used in")]
        [ConditionalHide("useInstantiation", true)]
        public ContentMode instantiationMode = ContentMode.IndexIncrement;

        [Tooltip("The delay between each gameObject being instantiated if allInstantiationAtOnce is false")]
        [ConditionalHide("useInstantiation", true)]
        public float instantiationDelay;

        [Tooltip("Total number of instantiations this script can do before deleting the first instantiated object")]
        [ConditionalHide("useInstantiation", true)]
        public int ObjectCountMax = 5;

        [Space(10)]
        #region Instantiation Content Debug
        [Tooltip("Master cashedID for this networked RPC")]
        [ConditionalHide("useInstantiation", true)]
        public int cachedID;

        [Tooltip("Select if you wish to have the Instantiation gizmos visible")]
        [ConditionalHide("useInstantiation", true)]
        public bool showInstantiationPoints = true;
        #endregion

        [Tooltip("List of available gameObjects to be instantiated (Default will instantiate all gameObjects")]
        [ConditionalHide("useInstantiation", true)]
        [Space(5)]
        public InstantiationList instantiations;
        #endregion

        [Space(10)]
        [Header("Relocation Content")]
        #region Relocation Content
        [Tooltip("The delay between activation of the button and the relocation happening")]
        [ConditionalHide("useRelocation", true)]
        public float relocationDelay;

        [Tooltip("Select if you wish for it to choose a random relocation point from the list")]
        [ConditionalHide("useRelocation", true)]
        public bool isRelocationRandom;

        [Tooltip("Select if you wish for the next relocation point within the list to reset to the first index if the last index is reached")]
        [ConditionalHide("useRelocation", true)]
        public bool restartRelocationPointsAfterFinish;

        [Tooltip("Select if you wish to have the position gizmos visible")]
        [ConditionalHide("useRelocation", true)]
        public bool showRelocationPoints = true;

        [Tooltip("Color that the gizmos at each relocation point will be")]
        [ConditionalHide("useRelocation", true)]
        public Color relocationPointGizmoColor = Color.red;

        [Tooltip("Available positions to relocate to after use")]
        [ConditionalHide("useRelocation", true)]
        [Space(5)]
        public RelocationList relocationPoints;
        #endregion

        #region Stored Data
        public enum ReactionState { Active, Ready, Finished }
        public enum ReactionLimitMode { Infinite, Limited }
        public enum ContentMode { IndexIncrement, RandomChoice, AllAtOnce };
        private int indexOfParticleList, indexOfAudioList, indexOfAnimationList, indexOfInstantionList, indexOfRelocationList;
        private bool particlesReady = true, audioReady = true, animationReady = true, instantiationReady = true;
        private List<GameObject> activeParticles;
        private List<GameObject> instantiatedObjects;
        private bool willDestroy;
        private float defaultVolume;
        private List<AnimationCondition> activatedAnimationConditions;
        private IEnumerator lerpVectorCoroutine;
        [HideInInspector]
        public bool isLimited;
        #endregion
        #endregion

        /// <summary>
        /// Use this for initialization
        /// </summary>
        void Start()
        {
            activeParticles = new List<GameObject>();
            instantiatedObjects = new List<GameObject>();
            activatedAnimationConditions = new List<AnimationCondition>();
            indexOfRelocationList = relocationPoints.relocationList.Count - 1;
            ResetValues();
        }

        private void OnValidate()
        {
            if (reactionLimitMode == ReactionLimitMode.Infinite)
            {
                isLimited = false;
                if (numberOfUses < 1)
                {
                    numberOfUses = 1;
                }
            }
            else
            {
                if (!isLimited)
                {
                    isLimited = true;
                }
            }

            if (destroySelfAfterUse && reactionLimitMode == ReactionLimitMode.Infinite)
            {
                reactionLimitMode = ReactionLimitMode.Limited;
            }

            if (useAudio && (audio != null) && (audio.audioList != null) && audio.audioList.Count > 0)
            {
                for (int i = 0; i < audio.audioList.Count; i++)
                {
                    if (audio.audioList[i].volume == 0.0f && audio.audioList[i].priority == 0 && audio.audioList[i].pitch == 0) //Default the values
                    {
                        audio.audioList[i].volume = 1.0f;
                        audio.audioList[i].priority = 128;
                        audio.audioList[i].pitch = 1;
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (useInstantiation && showInstantiationPoints && instantiations != null && instantiations.instantiationList != null)
            {
                if (instantiations.instantiationList.Count > 0)
                {
                    //Helper Gizmos For Inspector Use
                    for (int i = 0; i < instantiations.instantiationList.Count; i++)
                    {
                        Quaternion newRotation = Quaternion.identity;
                        Vector3 newPosition = Vector3.zero;

                        if (instantiations.instantiationList[i].instantiationReferenceObject == null)
                        {
                            if (instantiations.instantiationList[i].instantiatableMode == InstantiatableGameObject.InstantiatableVelocityMode.Local)
                            {
                                newRotation = GetGameObjectQuaterion(transform.forward + instantiations.instantiationList[i].instantiationRotation);
                                newPosition = transform.position + instantiations.instantiationList[i].instantiationLocation;
                            }
                            else
                            {
                                newRotation = GetGameObjectQuaterion(instantiations.instantiationList[i].instantiationRotation);
                                newPosition = instantiations.instantiationList[i].instantiationLocation;
                            }
                        }
                        else
                        {
                            newRotation = instantiations.instantiationList[i].instantiationReferenceObject.rotation;
                            newPosition = instantiations.instantiationList[i].instantiationReferenceObject.position;
                        }

                        //Debug.Log("drawing");
                        Gizmos.color = instantiations.instantiationList[i].debugColor;

                        if (instantiations.instantiationList[i].instantiatableGameObject && instantiations.instantiationList[i].instantiatableGameObject.GetComponent<MeshFilter>())
                        {
                            Gizmos.DrawMesh(instantiations.instantiationList[i].instantiatableGameObject.GetComponent<MeshFilter>().sharedMesh, newPosition, newRotation, instantiations.instantiationList[i].instantiationScale);
                        }
                        else
                        {
                            Gizmos.DrawCube(newPosition, instantiations.instantiationList[i].instantiationScale);
                        }

                        if (instantiations.instantiationList[i] != null && instantiations.instantiationList[i].initialVelocity != Vector3.zero)
                        {
                            if (instantiations.instantiationList[i].instantiationReferenceObject == null)
                            {
                                Gizmos.DrawLine(newPosition, newPosition + instantiations.instantiationList[i].initialVelocity / 25);
                            }
                            else
                            {
                                Gizmos.DrawLine(newPosition, newPosition + (instantiations.instantiationList[i].instantiationReferenceObject.forward * (instantiations.instantiationList[i].initialVelocity / 25).magnitude));
                            }
                        }
                    }
                }
            }

            if (useParticle && showParticlePoints && particles != null && particles.instantiationList != null)
            {
                if (particles.instantiationList.Count > 0)
                {
                    //Helper Gizmos For Inspector Use
                    for (int i = 0; i < particles.instantiationList.Count; i++)
                    {
                        Quaternion newRotation = GetGameObjectQuaterion(particles.instantiationList[i].instantiationRotation);

                        //Debug.Log("drawing");
                        Gizmos.color = particles.instantiationList[i].debugColor;
                        Gizmos.DrawSphere(transform.position + particles.instantiationList[i].instantiationLocation, particles.instantiationList[i].instantiationScale.magnitude);
                        Gizmos.DrawLine(transform.position + particles.instantiationList[i].instantiationLocation, transform.position + particles.instantiationList[i].instantiationLocation + (particles.instantiationList[i].instantiationRotation.normalized));
                    }
                }
            }

            if (useRelocation && showRelocationPoints && relocationPoints != null)
            {
                if (relocationPoints.relocationList.Count > 0)
                {
                    for (int i = 0; i < relocationPoints.relocationList.Count; i++)
                    {
                        //Copy given values from a reference transform (probably an empty object) for inspector control
                        if (relocationPoints.relocationList[i].referenceTransform != null)
                        {
                            relocationPoints.relocationList[i].relocationLocation = relocationPoints.relocationList[i].referenceTransform.position;
                            relocationPoints.relocationList[i].relocationRotation = relocationPoints.relocationList[i].referenceTransform.rotation.eulerAngles;
                            relocationPoints.relocationList[i].relocationScale = relocationPoints.relocationList[i].referenceTransform.localScale;
                        }

                        //Helper Gizmos For Inspector Use
                        Gizmos.color = relocationPointGizmoColor;
                        Quaternion newRotation = GetGameObjectQuaterion(relocationPoints.relocationList[i].relocationRotation);

                        if (gameObject.GetComponent<MeshFilter>())
                        {
                            if (relocationPoints.relocationList[i].relocationRotation != Vector3.zero)
                            {
                                Gizmos.DrawMesh(gameObject.GetComponent<MeshFilter>().sharedMesh, relocationPoints.relocationList[i].relocationLocation, newRotation, relocationPoints.relocationList[i].relocationScale);
                            }
                            else
                            {
                                Gizmos.DrawMesh(gameObject.GetComponent<MeshFilter>().sharedMesh, relocationPoints.relocationList[i].relocationLocation, Quaternion.identity, relocationPoints.relocationList[i].relocationScale);
                            }
                        }
                        else
                        {
                            Gizmos.DrawCube(relocationPoints.relocationList[i].relocationLocation, relocationPoints.relocationList[i].relocationScale);
                        }

                        //    Quaternion rotation = Quaternion.LookRotation(relocationPoints.relocationList[i].relocationLocation, Vector3.up);
                        //    Gizmos.color = Color.blue;
                        //    Gizmos.DrawLine(relocationPoints.relocationList[i].relocationLocation,  relocationPoints.relocationList[i].relocationLocation + (transform.forward + rotation.eulerAngles));
                    }
                }
            }
        }

        /// <summary>
        /// Triggerable method for event calls to use in inspector
        /// </summary>
        public void ActivateReaction()
        {
            OnInteract();
        }

        /// <summary>
        /// Depending on the Reaction Mode(s) selected, this will trigger all reactions
        /// </summary>
        public override void OnInteract(Vector3 contactPoint = new Vector3(), Transform playerGrip = null)
        {
            //Debug.Log("ActivateButton | useParticle: "+useParticle+" | useAudio: "+useAudio+" | useAnimation: "+useAnimation);
            if (reactionState == ReactionState.Ready)
            {
                if (numberOfUses > 0)
                {
                    reactionState = ReactionState.Active;

                    #region Uses & Reuse
                    if (isLimited)
                    {
                        numberOfUses--;
                        if (numberOfUses == 0)
                        {
                            if (destroySelfAfterUse)
                            {
                                willDestroy = true;
                            }

                            reactionState = ReactionState.Finished;
                            SpecialReactions specialReactions = GetComponent<SpecialReactions>();
                            if (specialReactions)
                            {
                                specialReactions.reactionState = ReactionState.Finished;
                                //specialReactions.combinationInteractionManager.UpdateHighlightedObjects(null);
                                if (specialReactions.currentReactant)
                                {
                                    //specialReactions.combinationInteractionManager.UpdateHighlightedObjects(specialReactions.currentReactant);
                                }
                            }
                        }
                    }
                    #endregion

                    #region Ready Setup
                    if (useParticle)
                    {
                        particlesReady = false;
                    }
                    if (useAudio)
                    {
                        audioReady = false;
                    }
                    if (useAnimation)
                    {
                        animationReady = false;
                    }
                    if (useInstantiation)
                    {
                        instantiationReady = false;
                    }
                    #endregion

                    #region Particle Activation
                    if (useParticle)
                    {
                        if (particles != null && particles.instantiationList.Count > 0)
                        {
                            if (particleMode == ContentMode.RandomChoice)
                            {
                                int chosenParticleIndex = Random.Range(0, particles.instantiationList.Count);
                                SingleObjectInstantiation(chosenParticleIndex, true);
                                particlesReady = true;
                            }
                            else
                            {
                                CreateParticles();
                            }
                        }
                        else
                        {
                            Debug.Log("[WARNING] useParticle is enabled but its list is empty");
                        }
                    }
                    #endregion

                    #region Audio Activation
                    if (useAudio)
                    {
                        if (audio != null && audio.audioList.Count > 0)
                        {
                            if (audioMode == ContentMode.RandomChoice)
                            {
                                int chosenAudioIndex = Random.Range(0, audio.audioList.Count);
                                SingleAudioPlay(audio.audioList[chosenAudioIndex]);
                                audioReady = true;
                            }
                            else
                            {
                                PlayAudio();
                            }
                        }
                        else if (useAudio)
                        {
                            Debug.Log("[WARNING] useAudio is enabled but its list is empty");
                        }
                    }
                    #endregion

                    #region Animation Toggle
                    if (useAnimation)
                    {
                        if (animationConditions != null && animationConditions.animationConditionList != null && animationConditions.animationConditionList.Count > 0)
                        {
                            if (animationMode == ContentMode.RandomChoice)
                            {
                                int chosenAnimationIndex = Random.Range(0, animationConditions.animationConditionList.Count);

                                SingleAnimationToggle(animationConditions.animationConditionList[chosenAnimationIndex]);

                                if (!animationConditions.animationConditionList[chosenAnimationIndex].dontReturnToDefaultAfterAnimationLength)
                                {
                                    activatedAnimationConditions.Add(animationConditions.animationConditionList[chosenAnimationIndex]);
                                }
                                animationReady = true;
                            }
                            else
                            {
                                ToggleAnimation();
                                if (animationLength > -1)
                                {
                                    Invoke("FinishedAnimation", animationLength);
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("[WARNING] useAnimation is enabled but its list is empty");
                        }
                    }
                    #endregion

                    #region GameObject Instantiation
                    if (useInstantiation)
                    {
                        if (instantiations != null && instantiations.instantiationList.Count > 0)
                        {
                            if (instantiationMode == ContentMode.RandomChoice)
                            {
                                int chosenInstantiationIndex = Random.Range(0, instantiations.instantiationList.Count);
                                SingleObjectInstantiation(chosenInstantiationIndex, false);
                                instantiationReady = true;
                            }
                            else
                            {
                                InstantiateGameObjects();
                            }
                        }
                    }
                    #endregion

                    #region Relocation Activation
                    if (useRelocation)
                    {
                        if (relocationPoints != null && relocationPoints.relocationList.Count > 0)
                        {
                            if (isRelocationRandom)
                            {
                                int chosenRelocationIndex = Random.Range(0, relocationPoints.relocationList.Count);
                                SingleObjectRelocation(chosenRelocationIndex);
                            }
                            else
                            {
                                RelocateGameObject();
                            }
                        }
                        else
                        {
                            Debug.Log("[WARNING] useRelocation is enabled but its list is empty");
                        }
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// Will reset values on this button to be used again
        /// </summary>
        public void ResetValues()
        {
            //Debug.Log("Reset Values called");
            if (audioReady && particlesReady && animationReady && instantiationReady)
            {
                indexOfAnimationList = animationConditions.animationConditionList.Count - 1;
                indexOfParticleList = particles.instantiationList.Count - 1;
                indexOfInstantionList = instantiations.instantiationList.Count - 1;

                if (activatedAnimationConditions.Count > 0)
                {
                    foreach (AnimationCondition animationCondition in activatedAnimationConditions)
                    {
                        SingleAnimationToggle(animationCondition);
                    }
                    activatedAnimationConditions.Clear();
                }

                if (reactionState != ReactionState.Finished && !destroySelfAfterUse)
                {
                    reactionState = ReactionState.Ready;
                    SpecialReactions specialReactions = GetComponent<SpecialReactions>();
                    if (specialReactions)
                    {
                        specialReactions.reactionState = ReactionState.Ready;
                    }
                }
                else if (willDestroy)
                {
                    Destroy(gameObject); ;
                }
            }
        }

        /// <summary>
        /// Gives the reset values method the all clear that all animations sequences have finished 
        /// </summary>
        public void FinishedAnimation()
        {
            animationReady = true;
            ResetValues();
        }

        #region Extended Reaction Methods
        /// <summary>
        /// Depending on the particle settings, will create all particles in various configurations
        /// </summary>
        private void CreateParticles()
        {
            if (indexOfParticleList >= 0 && particleMode != ContentMode.AllAtOnce)
            {
                activeParticles.Add(SingleObjectInstantiation(indexOfParticleList, true));
                indexOfParticleList--;
                if (indexOfParticleList > -1)
                {
                    Invoke("CreateParticles", particleDelay);
                }
                else
                {
                    particlesReady = true;
                    ResetValues();
                }
            }
            else if (particleMode == ContentMode.AllAtOnce)
            {
                for (int i = 0; i < particles.instantiationList.Count; i++)
                {
                    SingleObjectInstantiation(i, true);
                }
            }
        }

        /// <summary>
        /// Depending on the audio settings, will play all audio in various configurations
        /// </summary>
        private void PlayAudio()
        {
            if (indexOfAudioList >= 0 && audioMode != ContentMode.AllAtOnce)
            {
                audioReady = false;
                SingleAudioPlay(audio.audioList[indexOfAudioList]);
                indexOfAudioList--;

                if (indexOfAudioList > -1)
                {
                    audioReady = true;
                    ResetValues();
                }
                else
                {
                    indexOfAudioList = audio.audioList.Count - 1;
                    audioReady = true;
                    ResetValues();
                }
            }
            else if (audioMode == ContentMode.AllAtOnce)
            {
                foreach (ReactionAudio reactionAudio in audio.audioList)
                {
                    GameVars.instance.audioManager.PlaySFX(reactionAudio.audioClip, reactionAudio.volume, transform.position);
                }

                audioReady = true;
                ResetValues();
            }
        }

        /// <summary>
        /// Depending on the animation settings, will toggle all animations conditions in various configurations
        /// </summary>
        private void ToggleAnimation()
        {
            if (indexOfAnimationList >= 0 && animationMode != ContentMode.AllAtOnce)
            {
                animationReady = false;

                SingleAnimationToggle(animationConditions.animationConditionList[indexOfAnimationList]);
                if (!animationConditions.animationConditionList[indexOfAnimationList].dontReturnToDefaultAfterAnimationLength)
                {
                    activatedAnimationConditions.Add(animationConditions.animationConditionList[indexOfAnimationList]);
                }

                indexOfAnimationList--;
                if (indexOfAnimationList > -1)
                {
                    Invoke("ToggleAnimation", animationDelay);
                }
            }
            else if (animationMode == ContentMode.AllAtOnce)
            {
                foreach (AnimationCondition animationCondition in animationConditions.animationConditionList)
                {
                    SingleAnimationToggle(animationCondition);
                    if (!animationCondition.dontReturnToDefaultAfterAnimationLength)
                    {
                        activatedAnimationConditions.Add(animationCondition);
                    }
                }
            }
        }
        /// <summary>
        /// Depending on the relocation settings, will relocate this gameObject in various configurations
        /// </summary>
        private void RelocateGameObject()
        {
            if (indexOfRelocationList >= 0)
            {
                //Debug.Log("Relocated Within Method");
                SingleObjectRelocation(indexOfRelocationList);
                if (indexOfRelocationList > -1)
                {
                    indexOfRelocationList--;
                    ResetValues();
                }

                if (restartRelocationPointsAfterFinish)
                {
                    if (indexOfRelocationList == -1)
                    {
                        indexOfRelocationList = relocationPoints.relocationList.Count - 1;
                        ResetValues();
                    }
                }
            }
        }

        /// <summary>
        /// Depending on the instantiation settings, will instantiate all gameObjects in various configurations
        /// </summary>
        private void InstantiateGameObjects()
        {
            if (indexOfInstantionList >= 0 && instantiationMode != ContentMode.AllAtOnce)
            {
                instantiationReady = false;
                SingleObjectInstantiation(indexOfInstantionList, false);

                indexOfInstantionList--;
                if (indexOfInstantionList > -1)
                {
                    Invoke("InstantiateGameObjects", instantiationDelay);
                }
                else
                {
                    instantiationReady = true;
                    ResetValues();
                }
            }
            else if (instantiationMode == ContentMode.AllAtOnce)
            {
                for (int i = 0; i < instantiations.instantiationList.Count; i++)
                {
                    SingleObjectInstantiation(i, false);
                }
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Plays a single audio clip from a given reaction audio with its included parameters
        /// </summary>
        /// <param name="reactionAudio"></param>
        private void SingleAudioPlay(ReactionAudio reactionAudio)
        {
            if (reactionAudio.audioClip)
            {
                GameVars.instance.audioManager.PlaySFX(reactionAudio.audioClip, reactionAudio.volume, transform.position);
            }
            else
            {
                Debug.Log("[WARNING] No audioclip found for this reactionaudio");
            }
        }

        /// <summary>
        /// Instantiates a single gameObject from the instantiationList at a given index
        /// </summary>
        /// <param name="listIndex"></param>
        private GameObject SingleObjectInstantiation(int listIndex, bool isParticle)
        {
            InstantiationList chosenList = null;

            if (instantiatedObjects.Count == ObjectCountMax) // Clears the latest object added to the list to ensure that there are only the max amount of objects in the room
            {
                GameObject OldRemove = instantiatedObjects[0].gameObject;
                instantiatedObjects.Remove(OldRemove);
                Destroy(OldRemove);
            }

            if (!isParticle)
            {
                chosenList = instantiations;
            }
            else
            {
                if (particles != null)
                {
                    chosenList = particles;
                }
                else
                {
                    Debug.Log("[WARNING] Attempting to use particles without a valid particle list");
                }
            }

            if (chosenList != null && chosenList.instantiationList != null && chosenList.instantiationList[listIndex] != null && chosenList.instantiationList[listIndex].instantiatableGameObject != null)
            {
                GameObject instantiatedObject = null;
                if (chosenList.instantiationList[listIndex].instantiationReferenceObject == null)
                {
                    if (chosenList.instantiationList[listIndex].instantiatableMode == InstantiatableGameObject.InstantiatableVelocityMode.Local)
                    {
                        instantiatedObject = Instantiate(chosenList.instantiationList[listIndex].instantiatableGameObject, transform.position + chosenList.instantiationList[listIndex].instantiationLocation, GetGameObjectQuaterion(transform.forward + chosenList.instantiationList[listIndex].instantiationRotation));
                    }
                    else if (chosenList.instantiationList[listIndex].instantiatableMode == InstantiatableGameObject.InstantiatableVelocityMode.Global)
                    {
                        instantiatedObject = Instantiate(chosenList.instantiationList[listIndex].instantiatableGameObject, chosenList.instantiationList[listIndex].instantiationLocation, GetGameObjectQuaterion(chosenList.instantiationList[listIndex].instantiationRotation));
                    }
                }
                else
                {
                    instantiatedObject = Instantiate(chosenList.instantiationList[listIndex].instantiatableGameObject, chosenList.instantiationList[listIndex].instantiationReferenceObject.position, chosenList.instantiationList[listIndex].instantiationReferenceObject.rotation);
                }

                if (instantiatedObject != null)
                {
                    instantiatedObjects.Add(instantiatedObject); //Adds this Object to the Object List
                    instantiatedObject.name = chosenList.instantiationList[listIndex].instantiatableGameObject.name;
                    instantiatedObject.transform.localScale = chosenList.instantiationList[listIndex].instantiationScale;
                    if (chosenList.instantiationList[listIndex].initialVelocity != Vector3.zero)
                    {
                        if (instantiatedObject && instantiatedObject.GetComponent<Rigidbody>())
                        {
                            if (chosenList.instantiationList[listIndex].instantiationReferenceObject == null)
                            {
                                if (chosenList.instantiationList[listIndex].instantiatableMode == InstantiatableGameObject.InstantiatableVelocityMode.Local)
                                {
                                    instantiatedObject.GetComponent<Rigidbody>().AddForce(transform.forward + chosenList.instantiationList[listIndex].initialVelocity);
                                }
                                else
                                {
                                    instantiatedObject.GetComponent<Rigidbody>().AddForce(chosenList.instantiationList[listIndex].initialVelocity);
                                }
                            }
                            else
                            {
                                instantiatedObject.GetComponent<Rigidbody>().AddForce(chosenList.instantiationList[listIndex].instantiationReferenceObject.forward * (chosenList.instantiationList[listIndex].initialVelocity).magnitude);
                            }
                        }
                    }

                    if (chosenList.instantiationList[listIndex].instantiateInHand)
                    {
                        StartCoroutine(DelayGrabInHand(chosenList, listIndex, instantiatedObject));
                    }
                }
                return instantiatedObject;
            }
            else
            {
                Debug.Log("[WARNING] Invalid List");
            }
            return null;
        }

        /// <summary>
        /// Forces play to grab newly instantiated object on both VR or Desktop
        /// </summary>
        /// <param name="chosenList"></param>
        /// <param name="listIndex"></param>
        /// <param name="instantiatedObject"></param>
        /// <returns></returns>
        IEnumerator DelayGrabInHand(InstantiationList chosenList, int listIndex, GameObject instantiatedObject)
        {
            yield return new WaitForSeconds(.1f);
            if (chosenList.instantiationList[listIndex].instantiateInHand)
            {
                /*
                GrabObject grabObject = instantiatedObject.GetComponent<GrabObject>();
                if (grabObject)
                {
                    grabObject.Grab(1f);
                }
                */
            }
        }

        /// <summary>
        /// Relocates a single gameObject from the relocationList at a given index
        /// </summary>
        /// <param name="listIndex"></param>
        private void SingleObjectRelocation(int listIndex)
        {
            bool[] toLerp = new bool[3];
            if (relocationPoints.relocationList[listIndex].lerpLocation)
            {
                toLerp[0] = true;
            }
            else
            {
                toLerp[0] = false;
                transform.position = relocationPoints.relocationList[listIndex].relocationLocation;
            }

            if (relocationPoints.relocationList[listIndex].lerpRotation)
            {
                toLerp[1] = true;
            }
            else
            {
                toLerp[1] = false;
                transform.rotation = GetGameObjectQuaterion(relocationPoints.relocationList[listIndex].relocationRotation);
            }

            if (relocationPoints.relocationList[listIndex].lerpScale)
            {
                toLerp[2] = true;
            }
            else
            {
                toLerp[2] = false;
                transform.localScale = transform.localScale;
            }

            if (lerpVectorCoroutine != null)
            {
                StopCoroutine(lerpVectorCoroutine);
            }

            if (toLerp[0] == true || toLerp[1] == true || toLerp[2] == true)
            {
                StartCoroutine(lerpVectorCoroutine = LerpVector(relocationPoints.relocationList[listIndex], relocationPoints.relocationList[listIndex].lerpTime, toLerp));
            }
        }

        /// <summary>
        /// Handles the lerping of the given relocation object's position, rotation, and scale
        /// </summary>
        /// <param name="relocationGameObject"></param>
        /// <param name="duration"></param>
        /// <param name="toLerp"></param>
        /// <returns></returns>
        private IEnumerator LerpVector(RelocationGameObject relocationGameObject, float duration, bool[] toLerp)
        {
            bool[] allConditions = toLerp;
            bool wasAllMet = false;
            Quaternion rotationToMatch = GetGameObjectQuaterion(relocationGameObject.relocationRotation);
            float t = 0f;

            while (wasAllMet == false)
            {
                t += Time.deltaTime / duration;
                if (allConditions[0] == true) //Looking for position
                {
                    transform.position = Vector3.Lerp(transform.position, relocationGameObject.relocationLocation, t);
                }

                if (allConditions[1] == true) //Looking for rotation
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotationToMatch, t);
                }

                if (allConditions[2] == true) //Looking for scale
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, relocationGameObject.relocationScale, t);
                }

                //Check if conditions met
                if (allConditions[0] == true)
                {
                    if (transform.position == relocationGameObject.relocationLocation)
                    {
                        allConditions[0] = false;
                    }
                }

                if (allConditions[1] == true)
                {
                    if (transform.rotation == rotationToMatch)
                    {
                        transform.rotation = GetGameObjectQuaterion(relocationGameObject.relocationRotation);
                        allConditions[1] = false;
                    }
                }

                if (allConditions[2] == true)
                {
                    if (transform.localScale == relocationGameObject.relocationScale)
                    {
                        allConditions[2] = false;
                    }
                }

                if (allConditions[0] == false && allConditions[1] == false && allConditions[2] == false)
                {
                    wasAllMet = true;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Toggles a single animation conditional from the animationConditionList at a given index
        /// </summary>
        /// <param name="animationCondition"></param>
        private void SingleAnimationToggle(AnimationCondition animationCondition)
        {
            Animator animatorToUse = animator;
            SpecialReactions specialReactions = null;
            if (animationCondition.conditionState == AnimationCondition.ConditionState.Reactant)
            {
                specialReactions = GetComponent<SpecialReactions>();
                if (specialReactions)
                {
                    if (specialReactions.currentReactant && specialReactions.currentReactant.GetComponent<Animator>())
                    {
                        animatorToUse = specialReactions.currentReactant.GetComponent<Animator>();
                    }
                    else
                    {
                        Debug.Log("[WARNING] Cannot activate animation on reactant without animator present on reactant");
                    }
                }
                else
                {
                    Debug.Log("[WARNING] Cannot activate animation on reactant without Special Reactions script");
                }
            }

            if (animatorToUse != null)
            {
                if (animatorToUse.GetBool(animationCondition.condition))
                {
                    animatorToUse.SetBool(animationCondition.condition, false);
                }
                else
                {
                    animatorToUse.SetBool(animationCondition.condition, true);
                }

                if (specialReactions != null)
                {
                    //specialReactions.combinationInteractionManager.UpdateHighlightedObjects(null);
                    //specialReactions.combinationInteractionManager.UpdateHighlightedObjects(specialReactions.currentReactant);
                }
            }
            else
            {
                Debug.Log("[WARNING] No Valid Animator On " + gameObject.name + " Found When Attempting To Trigger " + animationCondition);
            }
        }

        /// <summary>
        /// Returns a qaternion from a given vector3 for better usage
        /// </summary>
        /// <param name="vectorToConvert"></param>
        /// <returns></returns>
        private Quaternion GetGameObjectQuaterion(Vector3 vectorToConvert)
        {
            return Quaternion.Euler(vectorToConvert.x, vectorToConvert.y, vectorToConvert.z);
        }
        #endregion

        #region Classes
        [System.Serializable]
        public class AudioList
        {
            public List<ReactionAudio> audioList;
        }

        [System.Serializable]
        public class ReactionAudio
        {
            [Header("Audio Source Settings")]
            #region Audio Source Settings
            [Tooltip("The audioclip to be played")]
            public AudioClip audioClip;
            [Tooltip("The volume this audio will be played at (Default 1.0f)")]
            [Range(0.0f, 1.0f)]
            public float volume = 1.0f;
            [Tooltip("The priority this audio will be played at (Default 128)")]
            [Range(0, 256)]
            public int priority = 128;
            [Tooltip("The pitch this audio will be played at (Default 1.0f)")]
            [Range(-3.0f, 3.0f)]
            public float pitch = 1.0f;
            #endregion
        }

        [System.Serializable]
        public class InstantiationList
        {
            public List<InstantiatableGameObject> instantiationList;
        }

        [System.Serializable]
        public class InstantiatableGameObject
        {
            [Header("Instantiation Settings")]
            #region Instantiation Settings
            [Tooltip("The gameObject to be instantiated")]
            public GameObject instantiatableGameObject;
            [Tooltip("The location at which this gameObject will be instantiated")]
            public Vector3 instantiationLocation;
            [Tooltip("The rotation of this gameObject upon instantiation")]
            public Vector3 instantiationRotation;
            [Tooltip("The scale at which this gameObject will become once instantiated")]
            public Vector3 instantiationScale;
            [Tooltip("The speed & direction at which this gameObject will travel once instantiated")]
            public Vector3 initialVelocity;
            [Tooltip("The relative velocity direction the gameObject will travel once instantiated")]
            public InstantiatableVelocityMode instantiatableMode;
            [Tooltip("Select if the instantiated item should use local or global references")]
            [HideInInspector]
            public bool instantiateInHand;
            #endregion

            [Header("Instantiation Debug")]
            #region Instantiation Debug
            [Tooltip("The transform that will serve as the reference for all paremeters of spawn")]
            public Transform instantiationReferenceObject;
            [Tooltip("The color this mesh gizmo will appear as to represent the given gameObject")]
            public Color debugColor = Color.blue;
            [Tooltip("If the item needs to be grabbable after instantiation")] // ADDED BY DYLAN 190708
            [HideInInspector]
            public bool NetworkedItem = true;
            #endregion

            #region Stored Data
            public enum InstantiatableVelocityMode { Local, Global };
            #endregion
        }

        [System.Serializable]
        public class AnimationConditionList
        {
            public List<AnimationCondition> animationConditionList;
        }

        [System.Serializable]
        public class AnimationCondition
        {
            [Header("Condition Settings")]
            #region Condition  Settings
            [Tooltip("The animation conditional to be toggles on the reactor or reactant depending on settings")]
            public string condition;
            [Tooltip("Select if this condition is for the reactant instead of the reactor")]
            public ConditionState conditionState = ConditionState.Reactor;
            [Tooltip("Select if this condition will not reset along with rest of this object")]
            public bool dontReturnToDefaultAfterAnimationLength;
            #endregion

            #region StoredData
            public enum ConditionState { Reactor, Reactant }
            #endregion
        }

        [System.Serializable]
        public class RelocationList
        {
            public List<RelocationGameObject> relocationList;
        }

        [System.Serializable]
        public class RelocationGameObject
        {
            [Header("Relocation Settings")]
            #region Relocation GameObject
            [Tooltip("The gameObjects new position upon relocation")]
            public Vector3 relocationLocation;
            [Tooltip("The gameObjects new rotation upon relocation")]
            public Vector3 relocationRotation;
            [Tooltip("The gameObjects new scale upon relocation")]
            public Vector3 relocationScale;
            #endregion

            [Header("Relocation Debug")]
            #region Relocation GameObject
            [Tooltip("Add a gameObject transform here to copy its transform to the above settings")]
            public Transform referenceTransform;
            public float lerpTime = 1.0f;
            public bool lerpLocation;
            public bool lerpRotation;
            public bool lerpScale;
            #endregion
        }
        #endregion
    }
}
