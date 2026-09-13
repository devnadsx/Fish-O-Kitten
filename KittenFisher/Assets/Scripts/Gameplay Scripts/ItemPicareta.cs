using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPicareta : MonoBehaviour
{
    [Header("Referência ao Diálogo")]
    public SceneIntroDialogue scriptDialogo; // Arraste o objeto com o SceneIntroDialogue no Inspector

    void OnMouseDown()
    {
        if (EsconderijosManager.Instance != null && EsconderijosManager.Instance.interacaoBloqueada) return;

        ColetarPicareta();
    }

    void ColetarPicareta()
    {
        // 1. Marca no inventário que possui a picareta
        if (GerenciadorInventario.Instance != null)
        {
            GerenciadorInventario.Instance.temPicareta = true;
        }

        // 2. Busca usando a sintaxe atualizada da Unity (elimina o aviso CS0618)
        if (scriptDialogo == null)
        {
            scriptDialogo = FindFirstObjectByType<SceneIntroDialogue>();
        }

        // 3. Injeta a fala e ativa o diálogo
        if (scriptDialogo != null)
        {
            scriptDialogo.falasDoGato.Clear();
            scriptDialogo.falasDoGato.Add("Great! I found a pickaxe! Now I can break those big rocks in my way!");

            scriptDialogo.gameObject.SetActive(true);
            scriptDialogo.IniciarNovoDialogoExterno();
        }

        // 4. Desativa a picareta do cenário
        gameObject.SetActive(false);
    }
}