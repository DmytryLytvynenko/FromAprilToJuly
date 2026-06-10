using UnityEngine;

public class NormalProjector : MonoBehaviour
{
    [SerializeField] private float _acceptableVerticalAngle = 40f; 
    [SerializeField] private int _ignoreSlopeHigherThan = 85; 
    [SerializeField] private float _collisionHightThreshold = .8f; 
    [SerializeField] private Transform _visual; 

    private Vector3 normal = Vector3.up;

    public Vector3 CurrentNormal => normal;
    public bool SlopeIsWalkable { get { return Vector3.Angle(normal, Vector3.up) < _acceptableVerticalAngle; } }

    public Vector3 Project(Vector3 moveVector, out bool CanMove)
    {
        CanMove = true;
        if (moveVector.magnitude < 0.01f)
            return moveVector;

        float slopeAngle = Vector3.Angle(normal, Vector3.up);

        if ((int)slopeAngle >= _ignoreSlopeHigherThan)
            return moveVector;

        if (slopeAngle > _acceptableVerticalAngle)
        {
            Vector3 projectedMove = moveVector - Vector3.Dot(moveVector, normal) * normal;

            if (Vector3.Dot(moveVector, -normal) > 0) 
            {
                CanMove = false;
                return Vector3.zero; 
            }
        }
        Vector3 projectedMovement = moveVector - Vector3.Dot(moveVector, normal) * normal;

        return projectedMovement.normalized * moveVector.magnitude;
    }
    public Vector3 Project(Vector3 moveVector)
    {
        if (moveVector.magnitude < 0.01f)
            return moveVector;

        float slopeAngle = Vector3.Angle(normal, Vector3.up);

        if ((int)slopeAngle >= _ignoreSlopeHigherThan)
            return moveVector;

        if (slopeAngle > _acceptableVerticalAngle)
        {
            Vector3 projectedMove = moveVector - Vector3.Dot(moveVector, normal) * normal;

            if (Vector3.Dot(moveVector, -normal) > 0)
            {
                return Vector3.zero;
            }
        }
        Vector3 projectedMovement = moveVector - Vector3.Dot(moveVector, normal) * normal;

        return projectedMovement.normalized * moveVector.magnitude;
    }
    private void OnCollisionEnter(Collision collision)
    {
        UpdateNormal(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        UpdateNormal(collision);
    }
/*    private void OnCollisionExit(Collision collision)
    {
        UpdateNormal(collision);
    }*/

    private void UpdateNormal(Collision collision)
    {
        if (collision.gameObject.CompareTag("Stairs")) return;
        if (collision.contactCount > 0)
        {
            if (transform.position.y - collision.contacts[0].point.y < _collisionHightThreshold)
                return;
            Vector3 newNormal = collision.contacts[0].normal;

            if (Vector3.Dot(newNormal, Vector3.up) > -0.1f)
            {
                normal = newNormal;
            }
        }
        else
        {
            normal = Vector3.up;
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (!_visual) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + normal * 2);

        Gizmos.color = Color.green;
        Vector3 projected = Project(_visual.forward);
        if (projected.magnitude > 0.01f)
        {
            Gizmos.DrawLine(transform.position, transform.position + projected * 2);
        }

        float slopeAngle = Vector3.Angle(normal, Vector3.up);
        Gizmos.color = slopeAngle > _acceptableVerticalAngle ? Color.red : Color.cyan;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f, Vector3.one * 0.3f);
    }
}