using System;
using UnityEngine;

public class NormalProjector : MonoBehaviour
{
    [SerializeField] float _acceptableVerticalAngle = 40f; 
    [SerializeField] float _collisionHightThreshold = .8f; 

    private Vector3 normal = Vector3.up;

    public Vector3 CurrentNormal => normal;

    public Vector3 Project(Vector3 moveVector)
    {
        if (moveVector.magnitude < 0.01f)
            return moveVector;

        float slopeAngle = Vector3.Angle(normal, Vector3.up);

        if (slopeAngle > _acceptableVerticalAngle)
        {
            Vector3 projectedMove = moveVector - Vector3.Dot(moveVector, normal) * normal;

            if (Vector3.Dot(moveVector, -normal) > 0) 
            {
                //return Vector3.zero; 
                return new Vector3 (projectedMove.x * .15f, moveVector.y, projectedMove.z * .15f); 
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
        if (collision.thisCollider.gameObject.CompareTag("ItemMover"))
        {
            return;
        }
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

        // Рисуем нормаль поверхности
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + normal * 2);

        // Рисуем спроецированное движение
        Gizmos.color = Color.green;
        Vector3 projected = Project(transform.forward);
        if (projected.magnitude > 0.01f)
        {
            Gizmos.DrawLine(transform.position, transform.position + projected * 2);
        }

        // Рисуем информацию о склоне
        float slopeAngle = Vector3.Angle(normal, Vector3.up);
        Gizmos.color = slopeAngle > _acceptableVerticalAngle ? Color.red : Color.cyan;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f, Vector3.one * 0.3f);
    }
}