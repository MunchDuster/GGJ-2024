using UnityEngine;

public class KeyboardInputter : Inputter
{
    [SerializeField] private KeyCode forward;
    [SerializeField] private KeyCode back;
    [SerializeField] private KeyCode right;
    [SerializeField] private KeyCode left;

    private void Update()
    {
        move = new Vector2(
            GetKey(right) - GetKey(left),
            GetKey(forward) - GetKey(back)
            );
    }

    private int GetKey(KeyCode keyCode) => Input.GetKey(keyCode) ? 1 : 0;
}