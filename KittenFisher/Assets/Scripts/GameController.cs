using UnityEngine;
using UnityEngine.Events;
public class GameController : MonoBehaviour
{
    public int foundedFish;
    public int fishesNumber;
    public UnityEvent OnVictory;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fishesNumber = transform.childCount;


    }

    // Update is called once per frame
    void Update()
    {

    }
    public void FoundFish()
    {
        //Esta linha é apenas para programadores
        //foundedCats = foundedCats + 1;
        foundedFish += 1;
        if (foundedFish >= fishesNumber)
        {
            OnVictory.Invoke();
        }
        //foundedCats++;
    }
}