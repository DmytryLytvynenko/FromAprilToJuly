using UnityEngine;

namespace SimpleDependencyManagement
{
    public class SingleEntryPoint : MonoBehaviour
    {
        [SerializeField] private bool LockCursor = false;
        [field: SerializeField] public Player Player { get; private set; }
        [field: SerializeField] public CameraController CameraController { get; private set; }
        [field: SerializeField] public PlayerMovement PlayerMovement { get; private set; }
        [field: SerializeField] public Rigidbody PlayerRigidbody { get; private set; }
        [field: SerializeField] public InputManager InputManager { get; private set; }
        [field: SerializeField] public GroundDetector GroundDetector { get; private set; }
        [field: SerializeField] public NormalProjector NormalProjector { get; private set; }
        [field: SerializeField] public DebugPanel DebugPanel { get; private set; }
        [field: SerializeField] public Interact Interact { get; private set; }
        [field: SerializeField] public ChargeIndicator ChargeIndicator { get; private set; }
        [field: SerializeField] public PlayerAnimator PlayerAnimator { get; private set; }
        private void Awake()
        {
            if (LockCursor)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
            InputManager.CreateSingleton();
            InputManager.Initialize();

            CameraController?.Initialize(Player.transform, GroundDetector);
            PlayerMovement?.Initialize(Camera.main, GroundDetector, NormalProjector, PlayerRigidbody);
            GroundDetector?.Initialize(PlayerMovement, NormalProjector);
            DebugPanel?.Initialize(GroundDetector, PlayerRigidbody);
            Interact?.Initialize(Camera.main);
            ChargeIndicator?.Initialize(Interact);
            PlayerAnimator?.Initialize(PlayerMovement, GroundDetector);
        }
        private void OnDisable()
        {
            CameraController?.HandleDisable();
            PlayerMovement?.HandleDisable();
            GroundDetector?.HandleDisable();
            DebugPanel?.HandleDisable();
            Interact?.HandleDisable();
            ChargeIndicator?.HandleDisable();
            PlayerAnimator?.HandleDisable();

            InputManager.HandleDisable();
        }
    }
}

