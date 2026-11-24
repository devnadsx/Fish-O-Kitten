using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private const string CHAVE_PAXTON = "PaxtonAcompanha";

    public bool paxtonAcompanha = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CriarGameManager()
    {
        if (Instance == null)
        {
            GameObject objeto = new GameObject("GameManager");
            objeto.AddComponent<GameManager>();
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (PlayerPrefs.HasKey(CHAVE_PAXTON))
        {
            paxtonAcompanha = PlayerPrefs.GetInt(CHAVE_PAXTON) == 1;
        }

        Debug.Log("GameManager criado! Paxton acompanha: " + paxtonAcompanha);
    }

    public void PaxtonVaiAcompanhar()
    {
        DefinirEscolha(true);
    }

    public void PaxtonVaiFicar()
    {
        DefinirEscolha(false);
    }

    void DefinirEscolha(bool escolha)
    {
        paxtonAcompanha = escolha;

        PlayerPrefs.SetInt(CHAVE_PAXTON, escolha ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Escolha salva! Paxton acompanha: " + paxtonAcompanha);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}