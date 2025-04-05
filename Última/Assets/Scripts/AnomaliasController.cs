using UnityEngine;
using System.Collections;

public class AnomaliasController : MonoBehaviour
{
    [Header("Configuración de Anomalías")]
    [SerializeField] private GameObject panelLuces;
    [SerializeField] private float intervaloParpadeo = 0.5f;
    [SerializeField] private int numeroParpadeos = 5;
    [SerializeField] private float delayInicial = 1f;

    private void Start()
    {
        StartCoroutine(VerificarAnomalias());
    }

    IEnumerator VerificarAnomalias()
    {
        yield return new WaitForSeconds(delayInicial);

        // Usamos la misma clave que en MainMenuController ("PreviousScene")
        if (PlayerPrefs.HasKey("PreviousScene"))
        {
            string escenaAnterior = PlayerPrefs.GetString("PreviousScene");
            
            Debug.Log($"Escena anterior: {escenaAnterior}");

            // Ignorar MainMenu y EndGame
            if (escenaAnterior == "MainMenu" || escenaAnterior == "EndGame")
            {
                PlayerPrefs.DeleteKey("PreviousScene");
                yield break;
            }

            // Solo activar si el jugador PERDIÓ (minijuego no completado)
            if (!GameManager.Instance.IsMinigameCompleted(escenaAnterior))
            {
                StartCoroutine(EjecutarLucesParpadeantes());
            }

            PlayerPrefs.DeleteKey("PreviousScene");
        }
    }

    IEnumerator EjecutarLucesParpadeantes()
    {
        if (panelLuces == null) yield break;

        for (int i = 0; i < numeroParpadeos; i++)
        {
            panelLuces.SetActive(true);
            yield return new WaitForSeconds(intervaloParpadeo);
            panelLuces.SetActive(false);
            yield return new WaitForSeconds(intervaloParpadeo);
        }
    }
}