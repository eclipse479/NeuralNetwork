using UnityEngine;
using UnityEngine.UI;

public class carNetwork : MonoBehaviour
{
    public GameObject startPoint; //spawn point for the cars
    //renderer of the object
    private Renderer rend;
    //the neural network the car will have
    private network neuralNetwork;
    //speed and rotation values
    public float acceleration;
    public float maxSpeed;
    public float turnSpeed;
    public float maxTurnSpeed;

    // input that the car will use to drive
    private float forwardDistance;
    private float leftDistance;
    private float rightDistance;
    private float[] inputs;

    //the outputs the car will manipulate
    private float velocity = 0;
    private float rotation = 0;

    //the fitness score of the car
    private float distanceTravelled;

    //used for distance traveled
    private Vector3 distance;
    private Vector3 lastPos;

    //has the car hit a wall
    private bool crashed;
    //allows the cars raycast to not hit other cars
    int layerMask = 1 << 8;
    //allows the colour of the car to change depending on the last generation
    private bool champion = false;
    private bool randomStart = false;
    private bool wasLoaded = false;
    //lap gate timer, how often the car needs to hit a lapgate in order to not die
    public float maxLapGateTimer;
    [HideInInspector]
    public float lapGateTime;
    private int lapGatesPassed = 0;
    private int lapMultiplier = 1;


    void Start()
    {
        lapGateTime = maxLapGateTimer;
        rend = gameObject.GetComponent<Renderer>();
        crashed = false;
        lastPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!crashed)
        {
            if (randomStart)
            {
                rend.material.color = new Color(1, 0, 0);
                randomStart = false;
                transform.Translate(new Vector3(0, 0.01f, 0));
            }
            if (champion)
            {
                rend.material.color = new Color(0, 1, 0);
                champion = false;
                transform.Translate(new Vector3(0, 0.015f, 0));
            }
            if (wasLoaded)
            {
                rend.material.color = new Color(0, 0, 1);
                wasLoaded = false;
                transform.Translate(new Vector3(0, 0.02f, 0));
            }
            // must hit a lap gate at least every 10 seconds or die
            lapGateTime -= Time.deltaTime;
            if(lapGateTime < 0)
            {
                wallHit();
            }

            //gets the left and right of the car
            var right45 = Quaternion.Euler(0, 45, 0) * transform.forward;
            var left45 = Quaternion.Euler(0, -45, 0) * transform.forward;

            ///raycasts to check how far the nearest wall is to the car in the direction
            RaycastHit forwardRay;
            //forward raycast
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out forwardRay, 10, layerMask))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * forwardRay.distance, Color.red);
                forwardDistance = forwardRay.distance;
            }
            else
            {
                forwardDistance = 10.0f;
            }
            //left raycast
            RaycastHit leftRay;
            if (Physics.Raycast(transform.position, left45, out leftRay, 10, layerMask))
            {
                Debug.DrawRay(transform.position, left45 * leftRay.distance, Color.blue);
                leftDistance = leftRay.distance;
            }
            else
            {
                leftDistance = 10.0f;
            }
            //right raycast
            RaycastHit rightRay;
            if (Physics.Raycast(transform.position, right45, out rightRay, 10, layerMask))
            {
                Debug.DrawRay(transform.position, right45 * rightRay.distance, Color.blue);
                rightDistance = rightRay.distance;
            }
            else
            {
                rightDistance = 10.0f;
            }

            //---puts the raycast lengths between 0 and 1
            forwardDistance /= 10;
            leftDistance /= 10;
            rightDistance /= 10;

            //  ------------------------    Put Neural network below this   ------------------------
            float[] inputs = new float[3];
            //sets the inputs
            inputs[0] = forwardDistance;
            inputs[1] = leftDistance;
            inputs[2] = rightDistance;
            //runs the inputs through the network
            float[] outputs = neuralNetwork.FeedForward(inputs);

            //  ------------------------    Put Neural network above this   ------------------------
            //gets the outputs from the neural network
            outputs[0] *= turnSpeed;
            outputs[1] *= acceleration;

            //add the outputs to the related variable
            rotation += outputs[0];
            velocity = outputs[1];
            //movement speed clamps
            if (velocity > maxSpeed)
                velocity = maxSpeed;
            if (velocity < -maxSpeed)
                velocity = -maxSpeed;
            // rotation speed clamps
            if (rotation > maxTurnSpeed)
                rotation = maxTurnSpeed;
            if (rotation < -maxTurnSpeed)
                rotation = -maxTurnSpeed;

            //moves the car according to the outputs received 
            transform.Rotate(new Vector3(0, outputs[0], 0) * Time.deltaTime);
            transform.Translate(new Vector3(0, 0, outputs[1]) * Time.deltaTime);


            //adds the distance traveled by the car to the fitness score
            distance = transform.position - lastPos;
            distanceTravelled = Vector3.Magnitude(distance);
            lastPos = transform.position;
            // add the fitness score to the neural network -> the further it moves the higher the fitness score becomes
            increaseFitness();


        }
    }
    /// <summary>
    /// used for getting points for lapgates and killing on wall collision
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "wall")
        {
            wallHit();
        }
        if (other.gameObject.tag == "lapGate")
        {
            if (velocity > 0)
            {
                lapGatesPassed++;
                neuralNetwork.addFitness((10.0f) * lapMultiplier); // add fitness when passing a lapgate
                lapGateTime = maxLapGateTimer;
            }
        }
        if (other.gameObject.tag == "finishLine")
        {
            if (velocity > 0 && lapGatesPassed == 21)
            {
                lapGatesPassed = 0;
                lapMultiplier++;
                neuralNetwork.addFitness((10.0f) * lapMultiplier); // add fitness when passing a lapgate
                lapGateTime = maxLapGateTimer;
            }
        }
    }
    /// <summary>
    /// kill self when it hits a wall
    /// </summary>
    private void wallHit()
    {
        crashed = true;
        neuralNetwork.hasCrashed = true;
        rend.material.color = new Color(0, 0, 0);
        velocity = 0;
        rotation = 0;
    }
    /// <summary>
    /// asigns a neural network to the car for use 
    /// </summary>
    /// <param name="net"></param>
    public void Init(network net)
    {
        neuralNetwork = net;
        crashed = false;
        if (net.champ)
        {
            champion = true;
        }
        if (net.randomCar)
        {
            randomStart = true;
        }
        if (net.loaded)
        {
            wasLoaded = true;
        }
    }
    /// <summary>
    /// decrease score if car is driving backwards
    /// </summary>
    private void increaseFitness()
    {
        if (velocity <= 0)
            neuralNetwork.addFitness(velocity * 100); // severly punish moving backwards (should stop revesing over a lap gate constantly)

    }
}
