using UnityEngine;
using NaughtyAttributes;

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
            if (!GameVars.instance.isPaused)
            {
                interactionInputData.InteractedClicked = Input.GetKeyDown(KeyCode.E);
                interactionInputData.InteractedReleased = Input.GetKeyUp(KeyCode.E);
            }
        }

        void GetCameraInput()
        {
            if (!GameVars.instance.isPaused)
            {
                cameraInputData.InputVectorX = Input.GetAxis("Mouse X");
                cameraInputData.InputVectorY = Input.GetAxis("Mouse Y");

                if (InteractionController.instance.hasPlayerDied == false)
                {
                    cameraInputData.ZoomClicked = Input.GetMouseButtonDown(0);
                }
                cameraInputData.ZoomReleased = Input.GetMouseButtonUp(0);

                if (InteractionController.instance.hasPlayerDied == false)
                {
                    cameraInputData.ReloadClicked = Input.GetKeyDown(KeyCode.R);
                }
                cameraInputData.ReloadReleased = Input.GetKeyUp(KeyCode.R);
            }
        }

        void GetMovementInputData()
        {
            if (InteractionController.instance.hasPlayerDied == false && !GameVars.instance.isPaused)
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
                movementInputData.InputVectorX = Input.GetAxisRaw("Horizontal");
                movementInputData.InputVectorY = Input.GetAxisRaw("Vertical");

                movementInputData.RunClicked = Input.GetKey(KeyCode.LeftShift);
                movementInputData.RunReleased = Input.GetKeyUp(KeyCode.LeftShift);

                if (movementInputData.RunClicked && fpsController.m_isGrounded && !movementInputData.IsCrouching)
                    movementInputData.IsRunning = true;

                if (movementInputData.RunReleased || movementInputData.IsCrouching)
                    movementInputData.IsRunning = false;

                movementInputData.JumpClicked = Input.GetKeyDown(KeyCode.Space);
                movementInputData.CrouchClicked = Input.GetKeyDown(KeyCode.C);
            }
        }
        #endregion
    }
}