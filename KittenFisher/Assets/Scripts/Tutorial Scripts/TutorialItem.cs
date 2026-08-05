using UnityEngine;

public class TutorialItem : MonoBehaviour
{
    // Digite exatamente: "Lupa", "Aquario" ou "Tesoura" no Inspector
    public string nomeDoItem;
    public AudioSource somColeta;

    void OnMouseDown()
    {
        // Se o tutorial estiver ativo, verifica se PODE clicar neste item agora
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