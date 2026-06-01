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
        private void Awake()
        {
            InputManager.CreateSingleton();
            InputManager.Initialize();

            CameraController.Initialize(Player.transform, PlayerMovement);
            PlayerMovement.Initialize(Camera.main, GroundDetector, NormalProjector, PlayerRigidbody);
            GroundDetector.Initialize(PlayerMovement);
            DebugPanel.Initialize(GroundDetector, PlayerRigidbody);
        }
        private void OnDisable()
        {
            CameraController.HandleDisable();
            PlayerMovement.HandleDisable();
            GroundDetector.HandleDisable();
            DebugPanel.HandleDisable();

            InputManager.HandleDisable();
        }
    }
}

