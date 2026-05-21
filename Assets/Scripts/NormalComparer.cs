using System;
using UnityEngine;

public class NormalComparer : MonoBehaviour
{
    [SerializeField] float acceptableVerticalAngle = 40f; 

    private Vector3 normal = Vector3.up;

    public Vector3 CurrentNormal => normal;

    public Vector3 Project(Vector3 moveVector)
    {
        if (moveVector.magnitude < 0.01f)
            return moveVector;

        // Проверяем угол наклона между нормалью и вертикалью (up)
        // Угол 0° = горизонтальная поверхность, 90° = вертикальная стена
        float slopeAngle = Vector3.Angle(normal, Vector3.up);

        // Ограничиваем движение, если угол больше допустимого
        if (slopeAngle > acceptableVerticalAngle)
        {
            // Если склон слишком крутой, не позволяем двигаться вверх по нему
            // но позволяем двигаться вниз
            Vector3 projectedMove = moveVector - Vector3.Dot(moveVector, normal) * normal;

            // Проверяем, движемся ли мы вверх по склону
            if (Vector3.Dot(moveVector, -normal) > 0) // движемся против нормали (вверх по склону)
            {
                return Vector3.zero; // Блокируем движение вверх по крутому склону
            }
        }

        // Проецируем вектор движения на поверхность
        // Вычитаем компоненту движения вдоль нормали
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

    private void UpdateNormal(Collision collision)
    {
        if (collision.contactCount > 0)
        {
            Vector3 newNormal = collision.contacts[0].normal;

            // Проверяем, смотрит ли нормаль вверх (от поверхности к персонажу)
            if (Vector3.Dot(newNormal, Vector3.up) > -0.1f)
            {
                normal = newNormal;
            }
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
        Gizmos.color = slopeAngle > acceptableVerticalAngle ? Color.red : Color.cyan;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f, Vector3.one * 0.3f);
    }
}