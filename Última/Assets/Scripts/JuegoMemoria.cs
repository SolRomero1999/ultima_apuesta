using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq; 

public class JuegoMemoria : MonoBehaviour
{
    [Header("Configuración de UI")]
    public GameObject rulesPanel;
    public Button nextButton;
    public TMP_Text rulesText;
    public Image tarotistaImage;
    
    [Header("Configuración del Juego")]
    public GameObject gamePanel;
    public GameObject cartaPrefab; 
    public Transform gridLayout; 
    public Sprite[] diseñosCartas; 
    public Sprite reversoCarta; 
    
    [Header("Diálogos")]
    private string[] introDialogue = new string[]
    {
        "En otro tiempo te hubiera dicho tu fortuna",
        "Pero viendo que es algo obvio solo jugaremos algo simple",
        "Un juego de memoria, con cartas",
        "Básicamente nos iremos turnando para voltear dos cartas",
        "Si formas una pareja te quedas esas cartas",
        "Si no lo son vuelves a voltearlas",
        "Al final gana quien tenga más parejas"
    };
    
    private int currentDialogueIndex = 0;
    private bool hasPlayedBefore = false;
    private Carta[] cartas;
    private Carta primeraCartaSeleccionada;
    private Carta segundaCartaSeleccionada;
    private bool puedeSeleccionar = true;
    private int paresEncontrados = 0;

    void Start()
    {
        hasPlayedBefore = PlayerPrefs.GetInt("HasPlayedMemoriaBefore", 0) == 1;
        gamePanel.SetActive(false);
        rulesPanel.SetActive(true);
        
        if (hasPlayedBefore)
        {
            rulesText.text = "Veamos que te depara el futuro";
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(IniciarJuego);
        }
        else
        {
            rulesText.text = introDialogue[currentDialogueIndex];
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(AdvanceDialogue);
        }
    }

    void AdvanceDialogue()
    {
        currentDialogueIndex++;
        
        if (currentDialogueIndex < introDialogue.Length)
        {
            rulesText.text = introDialogue[currentDialogueIndex];
        }
        else
        {
            PlayerPrefs.SetInt("HasPlayedMemoriaBefore", 1);
            IniciarJuego();
        }
    }

    void IniciarJuego()
    {
        rulesPanel.SetActive(false);
        tarotistaImage.gameObject.SetActive(false);
        gamePanel.SetActive(true);
        
        CrearTablero();
    }

    void CrearTablero()
    {
        foreach (Transform child in gridLayout)
        {
            Destroy(child.gameObject);
        }

        Sprite[] diseñosParaAsignar = new Sprite[18];
        for (int i = 0; i < diseñosCartas.Length; i++)
        {
            diseñosParaAsignar[i * 2] = diseñosCartas[i];
            diseñosParaAsignar[i * 2 + 1] = diseñosCartas[i];
        }

        System.Random rnd = new System.Random();
        diseñosParaAsignar = diseñosParaAsignar.OrderBy(x => rnd.Next()).ToArray();

        cartas = new Carta[18];
        for (int i = 0; i < 18; i++)
        {
            GameObject nuevaCarta = Instantiate(cartaPrefab, gridLayout);
            Carta cartaComponent = nuevaCarta.GetComponent<Carta>();
            cartaComponent.Inicializar(diseñosParaAsignar[i], reversoCarta, i);
            cartaComponent.OnCartaClickeada += ManejarClicCarta;
            cartas[i] = cartaComponent;
        }
    }

    void ManejarClicCarta(int id)
    {
        if (!puedeSeleccionar) return;
        
        Carta cartaSeleccionada = cartas[id];
        
        if (cartaSeleccionada.EstaVolteada || cartaSeleccionada.EstaEmparejada) 
            return;

        cartaSeleccionada.Voltear();

        if (primeraCartaSeleccionada == null)
        {
            primeraCartaSeleccionada = cartaSeleccionada;
        }
        else
        {
            segundaCartaSeleccionada = cartaSeleccionada;
            puedeSeleccionar = false;
            StartCoroutine(VerificarPareja());
        }
    }

    IEnumerator VerificarPareja()
    {
        if (primeraCartaSeleccionada.Diseno == segundaCartaSeleccionada.Diseno)
        {
            primeraCartaSeleccionada.Emparejar();
            segundaCartaSeleccionada.Emparejar();
            paresEncontrados++;
            
            if (paresEncontrados == 9)
            {
                yield return new WaitForSeconds(1f);
                StartCoroutine(EndGame(true));
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);
            primeraCartaSeleccionada.Voltear();
            segundaCartaSeleccionada.Voltear();
        }

        primeraCartaSeleccionada = null;
        segundaCartaSeleccionada = null;
        puedeSeleccionar = true;
    }

    IEnumerator EndGame(bool playerWon)
    {
        if (playerWon)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteMinigame("Juego_Memoria");
            }
        }
        
        PlayerPrefs.SetString("LastScene", "Juego_Memoria");
        SceneManager.LoadScene("MainScene");
        
        yield return null;
    }
}