using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class mainMenu1 : MonoBehaviour
{
    public Text population;
    public Text generationLengthTimer;
    // Start is called before the first frame update
    public int pop;
    public int genLength;
    private int lifeTime;
    /// <summary>
    /// makes sure that there will only ever be one main menu gameobject and first sets up the text
    /// </summary>
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        GameObject[] check = GameObject.FindGameObjectsWithTag("menus");
        foreach(GameObject newMenu in check)
        {
            if (newMenu != gameObject) //makes sure that there will only be one
            {
                mainMenu1 other = newMenu.GetComponent<mainMenu1>();
                if (lifeTime < other.lifeTime)
                {
                    Destroy(other.gameObject);
                }
            }
        }
        generationLengthTimer.text = "Generation timer: " + genLength;
        population.text = "Population: " + pop;

    }

    // Update is called once per frame
    private void Update()
    {
        if(lifeTime < 100)
        lifeTime++;
    }
    /// <summary>
    /// starts the run throught from generation 1
    /// </summary>
    public void startbutton()
    {
        SceneManager.LoadScene("raceCourse"); // loads the course
    }
    /// <summary>
    /// quites the application
    /// </summary>
    public void quitButton()
    {
        Application.Quit(); //quits the application
    }
    /// <summary>
    /// increases population size
    /// </summary>
    public void increasePop()
    {
        if (pop < 300)
        {
            pop += 10;
            population.text = "Population: " + pop; //  increases pop size
        }
    }
    /// <summary>
    /// decreases the population size
    /// </summary>
    public void decreasePop()
    {
        if (pop > 20)
        {
            pop -= 10;
            population.text = "Population: " + pop; // decreases pop size
        }
    }
    /// <summary>
    /// increase first generation length
    /// </summary>
    public void increaseGenerationLength()
    {
        if (genLength < 300)
        {
            genLength += 10;
            generationLengthTimer.text = "Generation timer: " + genLength;
        }
    }
    /// <summary>
    /// decrease first generation length
    /// </summary>
    public void decreaseGenerationLength()
    {
        if (genLength > 10)
        {
            genLength -= 10;
            generationLengthTimer.text = "Generation timer: " + genLength;
        }
    }

    /// <summary>
    /// loads teh challenge track
    /// </summary>
    public void loadLongTrack()
    {
        SceneManager.LoadScene("longTrack");
    }
}