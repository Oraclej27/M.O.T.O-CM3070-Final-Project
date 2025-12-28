using Unity.VisualScripting;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField]
    float movementSpeed;
    [SerializeField]
    float rotationSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //local variable 
        float movement = Input.GetAxisRaw("Vertical");
        float turn = Input.GetAxisRaw("Horizontal");

        transform.Translate(new Vector3(0,0, movement) * movementSpeed * Time.deltaTime);
        transform.Rotate(new Vector3(0, turn, 0) * rotationSpeed * Time.deltaTime);

        //if (Input.GetButtonDown("Jump"))
        //{
        //    transform.Translate(new Vector3(0, 1f, 0));
        //}
    }
}
