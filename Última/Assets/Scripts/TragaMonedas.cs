using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TragaMonedas : MonoBehaviour
{
    [Header("Configuración Visual")]
    [SerializeField] private Sprite machineNormal;
    [SerializeField] private Sprite machineLeverDown;
    [SerializeField] private Sprite[] symbols;
    
    [Header("Componentes UI")]
    [SerializeField] private Image machineImage;
    [SerializeField] private Image[] slots;
    [SerializeField] private Button leverButton;
    [SerializeField] private GameObject winPanel; 
    [SerializeField] private TMP_Text winText;

    [Header("Configuración Tiempos")]
    [SerializeField] private float leverDownTime = 0.5f;
    [SerializeField] private float totalSpinTime = 1.5f;
    [SerializeField] private float initialSpinSpeed = 0.05f;
    [SerializeField] private float finalSpinSpeed = 0.2f;
    [SerializeField] private float winDelay = 0.5f; 

    [Header("Configuración Dificultad")]
    [Range(0, 100)] [SerializeField] private int probabilidadGanar = 15; 

    private bool isSpinning = false;
    private int[] currentResults = new int[3];
    private Coroutine spinCoroutine;

    private void Start()
    {
        leverButton.onClick.AddListener(PullLever);
        ResetGame();
    }

    private void OnDestroy()
    {
        leverButton.onClick.RemoveListener(PullLever);
        if (spinCoroutine != null) StopCoroutine(spinCoroutine);
    }

    private void ResetGame()
    {
        winPanel.SetActive(false); 
        ResetSlots();
    }

    private void ResetSlots()
    {
        foreach (var slot in slots)
        {
            slot.sprite = symbols[0];
        }
    }

    private void PullLever()
    {
        if (!isSpinning)
        {
            if (spinCoroutine != null) StopCoroutine(spinCoroutine);
            spinCoroutine = StartCoroutine(SpinSequence());
        }
    }

    private IEnumerator SpinSequence()
    {
        isSpinning = true;
        machineImage.sprite = machineLeverDown;
        winPanel.SetActive(false); 
        
        var spinSlotsCoroutine = StartCoroutine(SpinSlots());
        yield return new WaitForSeconds(leverDownTime);
        
        machineImage.sprite = machineNormal;
        yield return new WaitForSeconds(totalSpinTime - leverDownTime);
        
        StopCoroutine(spinSlotsCoroutine);
        yield return StartCoroutine(CheckWin()); 
        isSpinning = false;
    }

    private IEnumerator SpinSlots()
    {
        float elapsedTime = 0f;
        bool victoriaPreparada = false;
        int simboloGanador = 0;

        bool esTiradaGanadora = Random.Range(0, 100) < probabilidadGanar;

        if (esTiradaGanadora)
        {
            simboloGanador = Random.Range(0, symbols.Length);
            victoriaPreparada = true;
        }

        while (elapsedTime < totalSpinTime)
        {
            float progress = elapsedTime / totalSpinTime;
            float currentSpeed = Mathf.Lerp(initialSpinSpeed, finalSpinSpeed, progress);
            
            for (int i = 0; i < slots.Length; i++)
            {
                int randomSymbol;
                
                if (progress > 0.85f && victoriaPreparada)
                {
                    randomSymbol = simboloGanador;
                }
                else
                {
                    randomSymbol = Random.Range(0, symbols.Length);
                }

                slots[i].sprite = symbols[randomSymbol];
                currentResults[i] = randomSymbol;
            }
            
            elapsedTime += currentSpeed;
            yield return new WaitForSeconds(currentSpeed);
        }
    }

    private IEnumerator CheckWin()
    {
        bool win = currentResults[0] == currentResults[1] && 
                 currentResults[1] == currentResults[2];
        
        if (win)
        {
            winText.text = "¡GANASTE!";
            winPanel.SetActive(true); 
            
            yield return new WaitForSeconds(winDelay);
            
            GameManager.Instance.CompleteMinigame("Traga_Monedas");
            PlayerPrefs.SetString("LastScene", "Traga_Monedas");
            SceneManager.LoadScene("MainScene");
        }
    }
}