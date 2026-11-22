
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPicareta : MonoBehaviour
{
    [Header("Referência ao Diálogo")]
    public SceneIntroDialogue scriptDialogo;

    void OnMouseDown()
    {
        if (EsconderijosManager.Instance != null &&
            EsconderijosManager.Instance.interacaoBloqueada)
            return;

        ColetarPicareta();
    }

    void ColetarPicareta()
    {
        if (GerenciadorInventario.Instance != null)
        {
            GerenciadorInventario.Instance.temPicareta = true;
        }

        if (scriptDialogo == null)
        {
            scriptDialogo = FindFirstObjectByType<SceneIntroDialogue>();
        }

        if (scriptDialogo != null)
        {
            scriptDialogo.IniciarDialogoComFalas(
                new List<string>
                {
                    "Caraca! Que sorte! Achei uma picareta, e agora posso finalmente quebrar essas pedras!"
                }
            );
        }

        gameObject.SetActive(false);
    }
}
