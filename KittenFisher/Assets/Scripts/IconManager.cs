using UnityEngine;
using UnityEngine.UI;

public class IconManager : MonoBehaviour
{
    [Header("Componente de Imagem do Gato")]
    public Image imagemGatoUI;

    [Header("Sprites das Expressões")]
    public Sprite gatoNormal;
    public Sprite gatoFeliz;
    public Sprite gatoEnvenenado;
    public Sprite gatoCatnip;

    [Header("Configurações")]
    public float tempoExpressaoFeliz = 2f;

    private bool estaEnvenenado = false;

    void Start()
    {
        // Se você esqueceu de arrastar no Inspector, ele tenta pegar o componente do próprio objeto
        if (imagemGatoUI == null)
        {
            imagemGatoUI = GetComponent<Image>();
        }

        MudarParaNormal();
    }

    // --- FUNÇÕES PÚBLICAS PARA OUTROS SCRIPTS CHAMAR ---

    public void MudarParaFeliz()
    {
        if (estaEnvenenado) return; // Se está mal do estômago, não fica feliz

        CancelInvoke(nameof(MudarParaNormal));
        MudarSprite(gatoFeliz);
        Debug.Log("🐱 Gatinho ficou FELIZ!");

        // Volta para o normal depois de alguns segundos
        Invoke(nameof(MudarParaNormal), tempoExpressaoFeliz);
    }

    public void MudarParaEnvenenado()
    {
        estaEnvenenado = true;
        CancelInvoke(nameof(MudarParaNormal));
        MudarSprite(gatoEnvenenado);
        Debug.Log("🐱 Gatinho ficou ENVENENADO!");
    }

    public void MudarParaCatnip()
    {
        CancelInvoke(nameof(MudarParaNormal));
        MudarSprite(gatoCatnip);
        Debug.Log("🐱 Gatinho ficou DOIDÃO DE CATNIP!");
    }

    public void MudarParaNormal()
    {
        estaEnvenenado = false;
        MudarSprite(gatoNormal);
        Debug.Log("🐱 Gatinho voltou ao NORMAL.");
    }

    // Função interna que troca a imagem de fato
    private void MudarSprite(Sprite novoSprite)
    {
        if (imagemGatoUI != null && novoSprite != null)
        {
            imagemGatoUI.sprite = novoSprite;
        }
        else
        {
            Debug.LogWarning("⚠️ IconManager: A imagem da UI ou o Sprite do gato está VAZIO no Inspector!");
        }
    }
    
}