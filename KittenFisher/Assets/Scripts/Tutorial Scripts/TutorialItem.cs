using UnityEngine;

public class TutorialItem : MonoBehaviour
{
    // Digite exatamente: "Lupa", "Aquario" ou "Tesoura" no Inspector
    public string nomeDoItem;
    public AudioSource somColeta;

    [Header("Bloqueio de Esconderijo (Opcional)")]
    [Tooltip("Arraste aqui o objeto do cenário (ex: porta do armário) que esconde este item do tutorial.")]
    public GameObject objetoEsconderijo;

    void OnMouseDown()
    {
        // 🔒 1. Checa se o esconderijo existe e se está cobrindo o item (porta fechada)
        if (objetoEsconderijo != null)
        {
            SpriteRenderer spriteEsconderijo = objetoEsconderijo.GetComponent<SpriteRenderer>();
            if (spriteEsconderijo != null && spriteEsconderijo.enabled)
            {
                // A porta do armário ainda está visível/fechada, ignora o clique no item!
                return;
            }
        }

        // 2. Se o tutorial estiver ativo, verifica se PODE clicar neste item agora
        if (TutorialManager.Instance != null)
        {
            if (!TutorialManager.Instance.PodeColetarItem(nomeDoItem))
            {
                Debug.Log($"Você não pode coletar o item {nomeDoItem} agora!");
                return; // Bloqueia a ação
            }

            // Avisa o gerente que este item foi coletado
            TutorialManager.Instance.ColetarItem(nomeDoItem);
        }

        if (somColeta != null) somColeta.Play();

        // Esconde o item coletado da cena
        gameObject.SetActive(false);
    }
}