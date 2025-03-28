using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ParOImparGame : MonoBehaviour
{
    public GameObject rulesPanel; 
    public Button startButton;   
    public TMP_Text dealerText;   
    public TMP_Text playerTeethText;    
    public Button parButton;      
    public Button imparButton;    
    public GameObject betPanel;   
    public TMP_Text betAmountText; 
    public Button increaseBetButton; 
    public Button decreaseBetButton; 
    public Button betButton;    
    public Image toothImage;     
    public Image backgroundImage; 
    public GameObject blackScreen; 
    public GameObject dialoguePanel; 
    public Sprite imgInicial;        
    public Sprite imgDealerPensando;      
    public Sprite imgDealerApostando;   
    public Sprite imgPlayerApostando;   
    public AudioSource audioSource;     
    public AudioClip RisaClip;   
    public AudioClip QuejidoClip;   

    private int playerTeeth = 10;  
    private int dealerTeeth = 10;  
    private int dealerNumber;      
    private int betAmount = 1;     
    private bool gameOver = false;  
    private bool isFirstTurn = true; 

    void Start()
    {
        rulesPanel.SetActive(true);
        backgroundImage.sprite = imgInicial; 
        dealerText.gameObject.SetActive(false);
        playerTeethText.gameObject.SetActive(false);
        parButton.gameObject.SetActive(false);
        imparButton.gameObject.SetActive(false);
        betPanel.SetActive(false);
        blackScreen.SetActive(false);
        playerTeethText.text = playerTeeth.ToString();
        startButton.onClick.AddListener(StartGame);
        increaseBetButton.onClick.AddListener(IncreaseBet);
        decreaseBetButton.onClick.AddListener(DecreaseBet);
        betButton.onClick.AddListener(() => StartCoroutine(MakeBetCoroutine()));
    }

    void StartGame()
    {
        rulesPanel.SetActive(false); 
        dealerText.gameObject.SetActive(true);
        dialoguePanel.SetActive(true);
        playerTeethText.gameObject.SetActive(true);
        parButton.gameObject.SetActive(false);
        imparButton.gameObject.SetActive(false);
        betPanel.SetActive(false);
        StartCoroutine(DealerTurn());
    }

    IEnumerator DealerTurn()
    {
        if (gameOver) yield break; 

        betPanel.SetActive(false);
        increaseBetButton.gameObject.SetActive(false);
        decreaseBetButton.gameObject.SetActive(false);
        betButton.gameObject.SetActive(false);

        if (isFirstTurn)
        {
            dealerText.text = "¿Los mayores primero, no? Elegiré un número...";
            isFirstTurn = false;
        }
        else
        {
            dealerText.text = "Es mi turno.";
        }
        backgroundImage.sprite = imgDealerPensando; 
        yield return new WaitForSeconds(2f);

        dealerNumber = Random.Range(1, Mathf.Min(dealerTeeth, playerTeeth, 10) + 1);

        dealerText.text = "Ok, ya elegí un número. ¿Crees que es Par o Impar?";
        backgroundImage.sprite = imgDealerApostando; 
        yield return new WaitForSeconds(1f);

        parButton.gameObject.SetActive(true);
        imparButton.gameObject.SetActive(true);

        parButton.onClick.RemoveAllListeners();
        imparButton.onClick.RemoveAllListeners();
        parButton.onClick.AddListener(() => StartCoroutine(PlayerChoiceCoroutine(true)));
        imparButton.onClick.AddListener(() => StartCoroutine(PlayerChoiceCoroutine(false)));
    }

    IEnumerator PlayerChoiceCoroutine(bool chosePar)
    {
        if (gameOver) yield break;

        bool isDealerNumberPar = (dealerNumber % 2 == 0);

        if (chosePar == isDealerNumberPar)
        {        
            backgroundImage.sprite = imgInicial; 
            yield return new WaitForSeconds(2f); 
            //audioSource.PlayOneShot(QuejidoClip);
            dealerText.text = "Tienes suerte...";
            playerTeeth += dealerNumber;
            dealerTeeth -= dealerNumber;
        }
        else
        {
            backgroundImage.sprite = imgInicial; 
            yield return new WaitForSeconds(2f); 
            
            if(dealerNumber == 1)
            {
                //audioSource.PlayOneShot(RisaClip);
                dealerText.text = "¡Ups! Fallaste, era 1 diente.";
            }
            else
            {
                //audioSource.PlayOneShot(RisaClip);
                dealerText.text = $"¡Ups! Fallaste, eran {dealerNumber} dientes.";
            }
            
            playerTeeth -= dealerNumber;
            dealerTeeth += dealerNumber;
        }

        playerTeethText.text = playerTeeth.ToString();

        yield return new WaitForSeconds(2f);

        if (playerTeeth >= 20)
        {
            //audioSource.PlayOneShot(QuejidoClip);
            dealerText.text = "¡Alcanzaste los 20!";
            gameOver = true;
            StartCoroutine(EndGame(true));
        }
        else if (playerTeeth <= 0)
        {
            //audioSource.PlayOneShot(RisaClip);
            dealerText.text = "Te quedaste sin dientes... Que lastima";
            gameOver = true;
            StartCoroutine(EndGame(false));
        }
        else if (dealerTeeth <= 0)
        {
            //audioSource.PlayOneShot(QuejidoClip);
            dealerText.text = "¡Me quede sin dientes!";
            gameOver = true;
            StartCoroutine(EndGame(true));
        }
        else
        {
            StartCoroutine(PlayerTurn());
        }

        parButton.gameObject.SetActive(false);
        imparButton.gameObject.SetActive(false);
    }

    IEnumerator PlayerTurn()
    {
        if (gameOver) yield break;

        dealerText.text = "Es tu turno. Elige cuántos dientes quieres apostar.";
        backgroundImage.sprite = imgInicial; 
        yield return new WaitForSeconds(2f);

        betPanel.SetActive(true);
        betAmount = 1;
        betAmountText.text = betAmount.ToString();

        increaseBetButton.gameObject.SetActive(true);
        decreaseBetButton.gameObject.SetActive(true);
        betButton.gameObject.SetActive(true);
    }

    void IncreaseBet()
    {
        if (betAmount < Mathf.Min(10, playerTeeth, dealerTeeth))
        {
            betAmount++;
            betAmountText.text = betAmount.ToString();
        }
    }

    void DecreaseBet()
    {
        if (betAmount > 1)
        {
            betAmount--;
            betAmountText.text = betAmount.ToString();
        }
    }

    IEnumerator MakeBetCoroutine()
    {
        if (gameOver) yield break;

        betPanel.SetActive(false);
        increaseBetButton.gameObject.SetActive(false);
        decreaseBetButton.gameObject.SetActive(false);
        betButton.gameObject.SetActive(false);
        backgroundImage.sprite = imgPlayerApostando; 

        bool dealerGuess = Random.Range(0, 2) == 0;

        if (playerTeeth == 1 && betAmount == 1)
        {
            dealerGuess = false; 
            //audioSource.PlayOneShot(RisaClip);
            dealerText.text = "Sé que solo te queda 1, apuesto por Impar.";
        }
        else
        {
            dealerText.text = "Creo que el número es " + (dealerGuess ? "Par" : "Impar");
        }
        
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(WaitAndShowResult(dealerGuess));
    }

    IEnumerator WaitAndShowResult(bool dealerGuess)
    {
        yield return new WaitForSeconds(2f);

        bool isBetPar = (betAmount % 2 == 0);

        if (dealerGuess == isBetPar)
        {
            dealerTeeth += betAmount;
            playerTeeth -= betAmount;
            
            if(betAmount == 1)
            {
                //audioSource.PlayOneShot(RisaClip);
                dealerText.text = "¡Adiviné! Dame ese diente.";
            }
            else
            {
                //audioSource.PlayOneShot(RisaClip);
                dealerText.text = $"¡Adiviné! Dame esos dientes.";
            }
        }
        else
        {
            dealerTeeth -= betAmount;
            playerTeeth += betAmount;
            
            if(betAmount == 1)
            {
                //audioSource.PlayOneShot(QuejidoClip);
                dealerText.text = "¡Fallé! Toma 1 diente.";
            }
            else
            {
                //audioSource.PlayOneShot(QuejidoClip);
                dealerText.text = $"¡Fallé! Toma {betAmount} dientes.";
            }
        }

        playerTeethText.text = playerTeeth.ToString();

        yield return new WaitForSeconds(2f);

        if (playerTeeth >= 20)
        {
            dealerText.text = "¡Ganaste!";
            gameOver = true;
            StartCoroutine(EndGame(true));
        }
        else if (playerTeeth <= 0)
        {
            dealerText.text = "¡Perdiste!";
            gameOver = true;
            StartCoroutine(EndGame(false));
        }
        else
        {
            StartCoroutine(DealerTurn());
        }
    }

    IEnumerator EndGame(bool playerWon)
    {
        yield return new WaitForSeconds(2f);
        
        if (playerWon)
        {
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            StartCoroutine(PlayerDeath());
        }
    }

    IEnumerator PlayerDeath()
    {
        blackScreen.SetActive(true);
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("GameOver");
    }
}