using UnityEngine;

public class AnimationEventForwarder : MonoBehaviour
{
    [SerializeField] private SFXActor _SFXActor;

    public void PlaySound(string SFX)
    {
        _SFXActor.PlaySound_BUTTON(SFX);
    }
}
