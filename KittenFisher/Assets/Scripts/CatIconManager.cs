using UnityEngine;
using UnityEngine.UI;

public class CatIconManager : MonoBehaviour
{
    public static CatIconManager Instance;
    private Image imagemGato;

    [Header("Expressões do Gato")]
    public Sprite gatoNormal;
    public Sprite gatoFeliz;
    public Sprite gatoEnvenenado;
    public Sprite gatoCatnip;

    [Tooltip("Tempo em segundos para a carinha feliz sumir")]
    [SerializeField] private float tempoExpressaoFeliz = 2f;

    private bool estaEnvenenado = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        imagemGato = GetComponent<Image>();
    }

    void Start()
    {
        DefinirExpressao(gatoNormal);
    }

    public void DefinirExpressao(Sprite novaExpressao)
    {
        if (imagemGato != null && novaExpressao != null)
        {
            imagemGato.sprite = novaExpressao;
        }
    }

    // Chamado pelo Inventário ao coletar/comer peixe bom
    public void ExpressaoFeliz()
    {
        if (estaEnvenenado) return; // Se está passando mal, não fica feliz!

        CancelInvoke(nameof(ResetarNormal));
        DefinirExpressao(gatoFeliz);
        Invoke(nameof(ResetarNormal), tempoExpressaoFeliz);
    }

    // Chamado pelo Inventário ao comer peixe estragado
    public void ExpressaoEnvenenado()
    {
        estaEnvenenado = true;
        CancelInvoke(nameof(ResetarNormal));
        DefinirExpressao(gatoEnvenenado);
    }

    // Chamado pelo Baú do Catnip no início do efeito
    public void ExpressaoCatnipAtivar()
    {
        CancelInvoke(nameof(ResetarNormal));
        DefinirExpressao(gatoCatnip);
    }

    // Chamado pelo Baú do Catnip no fim do efeito
    public void ExpressaoCatnipDesativar()
    {
        // Se ele já estava envenenado antes do Catnip, volta para a cara de envenenado
        if (estaEnvenenado)
        {
            DefinirExpressao(gatoEnvenenado);
        }
        else
        {
            DefinirExpressao(gatoNormal);
        }
    }

    // Use esta função para curar o gatinho quando ele passar no seu Skill Check!
    public void ResetarNormal()
    {
        estaEnvenenado = false;
        DefinirExpressao(gatoNormal);
    }
}