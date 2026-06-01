using UnityEngine;
using UnityEngine.Events;
public class GameController : MonoBehaviour
{
    public int foundedFish;
    public int FishNumber;
    public UnityEvent OnVictory;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FishNumber = transform.childCount;


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
        if (foundedFish >= FishNumber)
        {
            OnVictory.Invoke();
        }
        //foundedCats++;
    }
}