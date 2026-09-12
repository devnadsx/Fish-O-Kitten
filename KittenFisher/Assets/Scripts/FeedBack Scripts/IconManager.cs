using UnityEngine;
using UnityEngine.UI;

public class IconManager : MonoBehaviour
{
    [Header("Componente de Imagem do Gato")]
    public Image imagemGatoUI;

    [Header("Sprites das Expressões")]
    public Sprite gatoNormal;
    public Sprite gatoFeliz;
    public Sprite gatoCatnip;
    public Sprite gatoFelizCatnip; // 👈 Novo campo adicionado!

    [Header("Configurações")]
    public float tempoExpressaoFeliz = 2f;

    // Propriedade para controlar se o catnip está ativo
    public bool EstaSobEfeitoCatnip { get; private set; } = false;

    void Start()
    {
        if (imagemGatoUI == null)
        {
            imagemGatoUI = GetComponent<Image>();
        }

        MudarParaNormal();
    }

    // --- MÉTODOS DE CONTROLE ---

    public void MudarParaFeliz()
    {
        CancelInvoke(nameof(RetornarEstadoAposFeliz));

        // Se estiver sob efeito do Catnip, mostra a carinha especial (Feliz + Catnip)
        if (EstaSobEfeitoCatnip)
        {
            MudarSprite(gatoFelizCatnip != null ? gatoFelizCatnip : gatoFeliz);
            Debug.Log("🐱 Gatinho ficou FELIZ com CATNIP!");
        }
        else
        {
            MudarSprite(gatoFeliz);
            Debug.Log("🐱 Gatinho ficou FELIZ!");
        }

        // Volta ao estado normal/catnip depois do tempo configurado
        Invoke(nameof(RetornarEstadoAposFeliz), tempoExpressaoFeliz);
    }

    public void MudarParaCatnip()
    {
        EstaSobEfeitoCatnip = true;
        CancelInvoke(nameof(RetornarEstadoAposFeliz));
        MudarSprite(gatoCatnip);
        Debug.Log("🐱 Gatinho ficou DOIDÃO DE CATNIP!");
    }

    public void MudarParaNormal()
    {
        EstaSobEfeitoCatnip = false;
        CancelInvoke(nameof(RetornarEstadoAposFeliz));
        MudarSprite(gatoNormal);
        Debug.Log("🐱 Gatinho voltou ao NORMAL.");
    }

    private void RetornarEstadoAposFeliz()
    {
        if (EstaSobEfeitoCatnip)
        {
            MudarSprite(gatoCatnip);
            Debug.Log("🐱 Gatinho voltou para a carinha normal de Catnip!");
        }
        else
        {
            MudarSprite(gatoNormal);
            Debug.Log("🐱 Gatinho voltou ao Normal.");
        }
    }

    private void MudarSprite(Sprite novoSprite)
    {
        if (imagemGatoUI != null && novoSprite != null)
        {
            imagemGatoUI.sprite = novoSprite;
        }
        else if (novoSprite == null)
        {
            Debug.LogWarning("⚠️ IconManager: O Sprite que você tentou carregar não está atribuído no Inspector!");
        }
    }
}