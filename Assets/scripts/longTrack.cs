using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class longTrack : MonoBehaviour
{
    public Camera farCamera;
    public Camera followCamera;
    public GameObject carPrefab;
    private network champNetwork;
    private carNetwork car;

    private int[] layers = new int[] { 3, 4, 5, 4, 3, 2 }; // how many layers/neurons in each layer(no bias neurons)
    private bool created = false;
    private Quaternion iniRot;
    // Start is called before the first frame update
    void Start()
    {
        farCamera.enabled = false;
        followCamera.enabled = true;

        iniRot = followCamera.transform.rotation;
        if (File.Exists(Application.dataPath + "\\previousCars/savedWeights.txt"))
        {
            Debug.Log("file found!");
        }
        else
            Debug.Log("file not found!");
    }

    // Update is called once per frame
    void Update()
    {
        if(!created)
        {
            created = true;
            createNetwork();
            car = ((GameObject)Instantiate(carPrefab, transform.position, transform.rotation)).GetComponent<carNetwork>();
            car.Init(champNetwork);
        }
        if(car.lapGateTime < 10)
        {
            car.lapGateTime = 10000000;
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            farCamera.enabled = !farCamera.enabled;
            followCamera.enabled = !followCamera.enabled;
        }
        followCamera.transform.rotation = iniRot;
        followCamera.transform.position = car.transform.position + new Vector3(0,10,0);
    }

    void createNetwork()
    {
        champNetwork = new network(layers);
        readText();
    }

    private void readText()
    {
        //changes the first network to the loaded champion
        string path = Application.dataPath + "\\previousCars/savedWeights.txt";// path for the file
        StreamReader reader = new StreamReader(path);
        string text;
        for (int i = 0; i < champNetwork.weights.Length; i++)
        {
            for (int j = 0; j < champNetwork.weights[i].Length; j++)
            {
                for (int k = 0; k < champNetwork.weights[i][j].Length; k++)
                {
                    text = reader.ReadLine();
                    float loadedWeight = float.Parse(text);
                    champNetwork.weights[i][j][k] = loadedWeight;

                }
            }
        }
    }

    public void returnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
