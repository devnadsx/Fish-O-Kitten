using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour
{
    public int foundedFish;
    public int FishNumber;
    public UnityEvent OnVictory;

    [Header("Gerenciador do Gatinho")]
    public IconManager iconManager; // Arraste o objeto do gato aqui no Inspector!

    [Header("Efeitos Sonoros")]
    public AudioSource audioSource;
    public AudioClip somColetaPeixe;

    void Start()
    {
        FishNumber = transform.childCount;
    }

    public void FoundFish()
    {
        foundedFish += 1;

        // 1. Toca o som do peixe
        if (audioSource != null && somColetaPeixe != null)
        {
            audioSource.PlayOneShot(somColetaPeixe);
        }

        // 2. Faz o gato ficar feliz diretamente!
        if (iconManager != null)
        {
            iconManager.MudarParaFeliz();
        }

        // Checa Vitória
        if (foundedFish >= FishNumber)
        {
            OnVictory.Invoke();
        }
    }
}