using UnityEngine;

namespace SimpleDependencyManagement
{
    public class SingleEntryPoint : MonoBehaviour
    {
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
        private void Awake()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            InputManager.CreateSingleton();
            InputManager.Initialize();

            ChargeIndicator.Initialize(Interact);
            CameraController.Initialize(Player.transform, GroundDetector);
            PlayerMovement.Initialize(Camera.main, GroundDetector, NormalProjector, PlayerRigidbody);
            GroundDetector.Initialize(PlayerMovement, NormalProjector);
            DebugPanel.Initialize(GroundDetector, PlayerRigidbody);
            Interact.Initialize(Camera.main);
        }
        private void OnDisable()
        {
            CameraController.HandleDisable();
            PlayerMovement.HandleDisable();
            GroundDetector.HandleDisable();
            DebugPanel.HandleDisable();
            Interact.HandleDisable();
            ChargeIndicator.HandleDisable();

            InputManager.HandleDisable();
        }
    }
}

