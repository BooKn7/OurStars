using System.Collections.Generic;
using UnityEngine;

// プラットフォームを複数のポイント間で巡回移動させる
public class PlatformMovement2D : MonoBehaviour
{
    [Header("移動ポイント")]
    public List<Vector2> points = new();

    [Header("移動速度")]
    public float moveSpeed = 5f;

    private int currentPointIndex = 0;
    private bool isMoving = true;

    private void Start()
    {
        if (points.Count > 0)
        {
            transform.position = points[0];
        }
    }

    private void Update()
    {
        if (isMoving && points.Count > 1)
        {
            MoveToPoint(points[currentPointIndex]);
        }
    }

    private void MoveToPoint(Vector2 point)
    {
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, point, step);

        if ((Vector2)transform.position == point)
        {
            currentPointIndex = (currentPointIndex + 1) % points.Count;
        }
    }
}
