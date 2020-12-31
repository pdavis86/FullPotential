using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rigidBody;
    public float forwardForce = 2000;
    public float sidewaysForce = 500;

    bool goLeft;
    bool goRight;

    // Start is called before the first frame update
    //void Start()
    //{

    //}

    // Update is called once per frame
    void Update()
    {
        goLeft = Input.GetKey("a");
        goRight = Input.GetKey("d");
    }

    // Use for physics
    void FixedUpdate()
    {
        rigidBody.AddForce(0, 0, forwardForce * Time.deltaTime);

        if (goRight)
        {
            rigidBody.AddForce(sidewaysForce * Time.deltaTime, 0, 0);
        }

        if (goLeft)
        {
            rigidBody.AddForce(-sidewaysForce * Time.deltaTime, 0, 0);
        }
    }

}
