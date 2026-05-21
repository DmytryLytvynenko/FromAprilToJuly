using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhisicItemMover : MonoBehaviour
{
    private Player Player;
    private Rigidbody rb;
    public void Initialize(Player player)
    {
        Player = player;
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        rb.position = (Player.transform.position);
    }
}
