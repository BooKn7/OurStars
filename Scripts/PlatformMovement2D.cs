using System.Collections.Generic;
using UnityEngine;

public class PlatformMovement2D : MonoBehaviour
{
    public List<Vector2> points = new();

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
        // 暂时先用MoveTowards，也许以后该改成用DOTween来做平滑移动
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, point, step);

        if ((Vector2)transform.position == point)
        {
            currentPointIndex = (currentPointIndex + 1) % points.Count;
        }
    }
}
