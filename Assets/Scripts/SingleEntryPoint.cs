using UnityEngine;

namespace SimpleDependencyManagement
{
    public class SingleEntryPoint : MonoBehaviour
    {
        [field: SerializeField] public Player Player { get; private set; }
        [field: SerializeField] public CameraController CameraController { get; private set; }
        [field: SerializeField] public PlayerMovement PlayerMovement { get; private set; }
        [field: SerializeField] public InputManager InputManager { get; private set; }
        private void Awake()
        {
            InputManager.CreateSingleton();

            CameraController.Initialize(Player.transform);
            PlayerMovement.Initialize(Camera.main);
        }
    }
}

