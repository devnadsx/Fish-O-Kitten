using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public int foundedFish;
    [Tooltip("Defina a quantidade de peixes necessários no Inspector")]
    public int FishNumber = 3; // Valor padrão, mas você altera no Inspector como quiser!
    public UnityEvent OnVictory;

    [Header("Gerenciador do Gatinho")]
    public IconManager iconManager;

    [Header("Efeitos Sonoros")]
    public AudioSource audioSource;
    public AudioClip somColetaPeixe;

    void Start()
    {
        // Linha removida! Agora a variável FishNumber NÃO será mais sobrescrita automaticamente.
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