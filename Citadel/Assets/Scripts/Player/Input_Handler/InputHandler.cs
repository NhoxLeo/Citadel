using UnityEngine;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;

namespace VHS
{
    public class InputHandler : MonoBehaviour
    {
        #region Data
        [Space, Header("Input Data")]
        [SerializeField] private CameraInputData cameraInputData = null;
        [SerializeField] private MovementInputData movementInputData = null;
        [SerializeField] private InteractionInputData interactionInputData = null;

        public FirstPersonController fpsController;
        [HideInInspector]
        public string previousInputs;
        [HideInInspector]
        public int maxPreviousFrames;
        [HideInInspector]
        public bool canMove = true;
        #endregion

        #region BuiltIn Methods
        void Start()
        {
            cameraInputData.ResetInput();
            movementInputData.ResetInput();
            interactionInputData.ResetInput();
        }

        void Update()
        {
            GetCameraInput();
            GetMovementInputData();
            GetInteractionInputData();
        }
        #endregion

        #region Custom Methods
        void GetInteractionInputData()
        {
            if (InteractionController.instance.hasPlayerDied == false && !GameVars.instance.isPaused)
            {
                interactionInputData.InteractedClicked = Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_USE));
                interactionInputData.InteractedReleased = Input.GetKeyUp((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_USE));
            }
            else
            {
                interactionInputData.InteractedReleased = true;
            }
        }

        public IEnumerator HaultMove(float delay)
        {
            canMove = false;
            yield return new WaitForSeconds(delay);
            canMove = true;
        }

        void GetCameraInput()
        {
            if (InteractionController.instance.hasPlayerDied == false && !GameVars.instance.isPaused)
            {
                cameraInputData.InputVectorX = Input.GetAxis("Mouse X");
                cameraInputData.InputVectorY = Input.GetAxis("Mouse Y");

                if (InteractionController.instance.hasPlayerDied == false)
                {
                    cameraInputData.ZoomClicked = Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_SHOOT));
                    cameraInputData.ZoomHeld = Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_SHOOT));
                }
                cameraInputData.ZoomReleased = Input.GetKeyUp((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_SHOOT));

                if (InteractionController.instance.hasPlayerDied == false)
                {
                    cameraInputData.ReloadClicked = Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_RELOAD));
                }
                cameraInputData.ReloadReleased = Input.GetKeyUp((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_RELOAD));
            }
            else
            {
                cameraInputData.ZoomReleased = true;
                cameraInputData.ReloadReleased = true;
                cameraInputData.InputVectorX = 0;
                cameraInputData.InputVectorY = 0;
            }
        }

        void GetMovementInputData()
        {
            if (InteractionController.instance.hasPlayerDied == false && !GameVars.instance.isPaused && canMove)
            {
                if (Input.inputString == "")
                {
                    previousInputs += "#";
                }
                else
                {
                    previousInputs += Input.inputString;
                }

                if (previousInputs.Length > maxPreviousFrames)
                {
                    previousInputs = previousInputs.Remove(0, 1);
                }

                if(Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_FORWARD)))
                {
                    movementInputData.InputVectorY = 1;
                }
                else if (Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_BACKWARDS)))
                {
                    movementInputData.InputVectorY = -1;
                }
                else
                {
                    movementInputData.InputVectorY = 0;
                }

                if (Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_RIGHT)))
                {
                    movementInputData.InputVectorX = 1;
                }
                else if (Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_LEFT)))
                {
                    movementInputData.InputVectorX = -1;
                }
                else
                {
                    movementInputData.InputVectorX = 0;
                }

                movementInputData.RunClicked = Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_SPRINT));
                movementInputData.RunReleased = Input.GetKeyUp((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_SPRINT));

                if (movementInputData.RunClicked && fpsController.m_isGrounded && !movementInputData.IsCrouching)
                    movementInputData.IsRunning = true;

                if (movementInputData.RunReleased || movementInputData.IsCrouching)
                    movementInputData.IsRunning = false;

                movementInputData.JumpClicked = Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_JUMP));
                movementInputData.CrouchClicked = Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_CROUCH));

                movementInputData.MapClicked = Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_MAP));

            }
            else
            {
                movementInputData.InputVectorX = 0;
                movementInputData.InputVectorY = 0;
                movementInputData.RunReleased = true;
                movementInputData.JumpClicked = false;
                previousInputs += "####";
            }
        }
        #endregion
    }
}