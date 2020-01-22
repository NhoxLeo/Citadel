using UnityEngine;
using NaughtyAttributes;

namespace VHS
{    
    public class InputHandler : MonoBehaviour
    {
        #region Data
            [Space,Header("Input Data")]
            [SerializeField] private CameraInputData cameraInputData = null;
            [SerializeField] private MovementInputData movementInputData = null;
            [SerializeField] private InteractionInputData interactionInputData = null;
        public string previousInputs;
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
                interactionInputData.InteractedClicked = Input.GetKeyDown(KeyCode.E);
                interactionInputData.InteractedReleased = Input.GetKeyUp(KeyCode.E);
            }

            void GetCameraInput()
            {
                cameraInputData.InputVectorX = Input.GetAxis("Mouse X");
                cameraInputData.InputVectorY = Input.GetAxis("Mouse Y");

                //cameraInputData.ZoomClicked = Input.GetMouseButtonDown(1);
                //cameraInputData.ZoomReleased = Input.GetMouseButtonUp(1);
            }

            void GetMovementInputData()
            {
                if (Input.inputString == "")
                {
                    previousInputs += "#";
                }
                else
                {
                    previousInputs += Input.inputString;
                }
                
                if(previousInputs.Length > maxPreviousFrames)
                {
                    previousInputs = previousInputs.Remove(0,1);
                }
                movementInputData.InputVectorX = Input.GetAxisRaw("Horizontal");
                movementInputData.InputVectorY = Input.GetAxisRaw("Vertical");

                movementInputData.RunClicked = Input.GetKeyDown(KeyCode.LeftShift);
                movementInputData.RunReleased = Input.GetKeyUp(KeyCode.LeftShift);

                if(movementInputData.RunClicked)
                    movementInputData.IsRunning = true;

                if(movementInputData.RunReleased)
                    movementInputData.IsRunning = false;

                movementInputData.JumpClicked = Input.GetKeyDown(KeyCode.Space);
                movementInputData.CrouchClicked = Input.GetKeyDown(KeyCode.C);
            }
        #endregion
    }
}