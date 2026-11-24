
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool paxtonAcompanha = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("GameManager criado. Instância: " + Instance);
    }
    public void PaxtonVaiAcompanhar()
    {
        paxtonAcompanha = true;
        Debug.Log("Paxton vai acompanhar Myke!");
    }

    public void PaxtonVaiFicar()
    {
        paxtonAcompanha = false;
        Debug.Log("Paxton vai ficar para trás!");
    }
    void OnDestroy()
    {
        if (Instance == this)
        {
            Debug.LogWarning("O GameManager principal foi destruído!");
            Instance = null;
        }
    }
}
