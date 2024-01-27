using System.Collections;
using UnityEngine;

public class SpringAttack : MonoBehaviour
{
    [SerializeField] private float force;
    [SerializeField] private float longLength;
    [SerializeField] private float time;
    [SerializeField] private WigglyArm[] arms;

    private void Start()
    {
        Screen.SetResolution(640, 480, FullScreenMode.MaximizedWindow, 30);
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //    StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        float originalLength = arms[0].GetLength();

        SetArm(longLength, false);
        yield return new WaitForSeconds(time);
        SetArm(originalLength, true);
    }

    void SetArm(float length, bool enableMovement)
    {
        for (int i = 0; i < arms.Length; i++)
        {
            arms[i].SetLength(length);
            Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = ((Vector2)target - arms[i].hand.position).normalized;
            arms[i].hand.AddForce(direction * force, ForceMode2D.Impulse);
            arms[i].hand.GetComponent<NetworkedMovement>().enabled = enableMovement;
        }
    }
}
