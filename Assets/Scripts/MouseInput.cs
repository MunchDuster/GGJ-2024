using UnityEngine;

public class MouseInput : Inputter
{
    private void Update()
    {
        move = new Vector2(
            GetAxis(Input.mousePosition.x, Screen.width),
            GetAxis(Input.mousePosition.y, Screen.height)
            );
    }

    private float GetAxis(float position, int size) => (position / ((float)size / 2)) - 0.5f;
}