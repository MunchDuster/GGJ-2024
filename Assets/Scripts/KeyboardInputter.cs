using UnityEngine;

public class KeyboardInputter : Inputter
{
    [SerializeField] private string forward;
    [SerializeField] private string back;
    [SerializeField] private string right;
    [SerializeField] private string left;

    private void Update()
    {
        move = new Vector2(
            GetKey(right) - GetKey(left),
            GetKey(forward) - GetKey(back)
            );
    }

    private int GetKey(string keyCode) => Input.GetKey(keyCode) ? 1 : 0;
}