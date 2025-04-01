using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class DialogManager : MonoBehaviour
{
    [SerializeField] private GameObject playerDialogPrefab;
    [SerializeField] private GameObject SecondPlayerDialogPrefab;
    [SerializeField] private GameObject bartenderDialogPrefab;
    [SerializeField] private GameObject SecondBartenderDialogPrefab;
    [SerializeField] private GameObject choiceDialogPrefab;

    private GameObject currentDialogInstance;
    private Button clearButton;
    private int currentLineIndex = 0;
    private bool isPostRuleta = false;

    private struct DialogLine
    {
        public string text;
        public GameObject dialogPrefab;
    }

    private DialogLine[] dialogLines;

    private void Start()
    {
        int playerState = GameManager.instance != null ? GameManager.instance.GetPlayerState() : 0;

        if (playerState > 0)
        {
            InitializeNewPlayerDialog();
        }
        else if (PlayerPrefs.GetString("PreviousScene") == "MainMenu")
        {
            InitializeIntroDialog();
        }
        else if (GameManager.instance != null && GameManager.instance.IsMinigameCompleted("Ruleta_Rusa") &&
                 !GameManager.instance.HasSeenPostRuletaDialogs())
        {
            InitializePostRuletaDialog();
        }

        if (dialogLines != null && dialogLines.Length > 0)
        {
            PlayerMovement player = FindObjectOfType<PlayerMovement>();
            if (player != null)
            {
                player.canMove = false;
            }

            ShowNextDialog();
        }
    }

    private void InitializeIntroDialog()
    {
        dialogLines = new DialogLine[]
        {
            new DialogLine { text = "¿Dónde estoy...? Todo esto se siente extraño...", dialogPrefab = playerDialogPrefab },
            new DialogLine { text = "Recuerdo haber muerto... pero no sé cómo sucedió...", dialogPrefab = playerDialogPrefab },
            new DialogLine { text = "¿Qué está pasando aquí?", dialogPrefab = playerDialogPrefab },
            new DialogLine { text = "Efectivamente, has muerto.", dialogPrefab = bartenderDialogPrefab },
            new DialogLine { text = "Y ahora estás aquí, en este lugar entre la vida y la muerte.", dialogPrefab = bartenderDialogPrefab },
            new DialogLine { text = "Para poder seguir adelante, tendrás que demostrar tu valía en una serie de juegos.", dialogPrefab = bartenderDialogPrefab },
            new DialogLine { text = "Cada juego es una prueba. Si las superas, quizás encuentres respuestas.", dialogPrefab = bartenderDialogPrefab },
            new DialogLine { text = "Si fallas... bueno, mejor ni hablemos de eso.", dialogPrefab = bartenderDialogPrefab },
            new DialogLine { text = "¿Listo para empezar?", dialogPrefab = bartenderDialogPrefab }
        };
    }

    private void InitializePostRuletaDialog()
    {
        isPostRuleta = true;
        dialogLines = new DialogLine[]
        {
            new DialogLine { text = "¿Creías que había muerto?", dialogPrefab = bartenderDialogPrefab },
            new DialogLine { text = "En este lugar se podría decir que soy como un juez, una bala no me matará", dialogPrefab = bartenderDialogPrefab },
            new DialogLine { text = "Aún así me has ganado", dialogPrefab = bartenderDialogPrefab },
            new DialogLine { text = "¿Qué harás ahora?", dialogPrefab = bartenderDialogPrefab },
            new DialogLine { text = "¿Qué haré? ¿Qué opciones tengo?", dialogPrefab = playerDialogPrefab },
            new DialogLine { text = "Bueno, puedes reencarnar, olvidarás todo esto y podrás llevar una nueva vida", dialogPrefab = bartenderDialogPrefab },
            new DialogLine { text = "O puedes liberarme y tomar mi lugar", dialogPrefab = bartenderDialogPrefab },
            new DialogLine { text = "Ser el nuevo juez", dialogPrefab = bartenderDialogPrefab },
            new DialogLine { text = "¿Qué prefieres?", dialogPrefab = bartenderDialogPrefab }
        };
    }
        
    private void InitializeNewPlayerDialog()
    {
        int playerState = GameManager.instance != null ? GameManager.instance.GetPlayerState() : 0;
        bool isOriginalBartender = GameManager.instance != null && !GameManager.instance.IsPlayerJudge();

        // Primera línea del jugador (siempre la misma)
        List<DialogLine> lines = new List<DialogLine>
        {
            new DialogLine { text = "¿Dónde estoy?", dialogPrefab = SecondPlayerDialogPrefab }
        };

        // Respuesta del bartender (depende del tipo)
        if (isOriginalBartender)
        {
            lines.Add(new DialogLine { 
                text = "Se podría decir que en el limbo, haz muerto", 
                dialogPrefab = bartenderDialogPrefab 
            });
        }
        else
        {
            lines.Add(new DialogLine { 
                text = "Oh, al fin llegó alguien... estás en el limbo", 
                dialogPrefab = SecondBartenderDialogPrefab 
            });
        }

        // Segunda línea del jugador
        lines.Add(new DialogLine { 
            text = "¿El limbo? ¿Cómo llegué aquí?", 
            dialogPrefab = SecondPlayerDialogPrefab 
        });

        // Respuesta del bartender sobre la muerte
        if (isOriginalBartender)
        {
            lines.Add(new DialogLine { 
                text = "Eso es obvio, haz muerto", 
                dialogPrefab = bartenderDialogPrefab 
            });
        }
        else
        {
            lines.Add(new DialogLine { 
                text = "Bueno, lamentablemente haz muerto", 
                dialogPrefab = SecondBartenderDialogPrefab 
            });
        }

        // Tercera línea del jugador
        lines.Add(new DialogLine { 
            text = "¿Muerto? No lo recuerdo...", 
            dialogPrefab = SecondPlayerDialogPrefab 
        });

        // Respuesta del bartender sobre el recuerdo
        if (isOriginalBartender)
        {
            lines.Add(new DialogLine { 
                text = "Quizás lo hagas una vez que demuestres tu valor... supera los juegos aquí establecidos", 
                dialogPrefab = bartenderDialogPrefab 
            });
        }
        else
        {
            lines.Add(new DialogLine { 
                text = "Es normal, yo tampoco lo hice. Lo recordarás a medida que vayas jugando. Si terminas los juegos recordarás y podrás irte", 
                dialogPrefab = SecondBartenderDialogPrefab 
            });
        }

        // Cuarta línea del jugador
        lines.Add(new DialogLine { 
            text = "¿Debo jugar?", 
            dialogPrefab = SecondPlayerDialogPrefab 
        });

        // Respuesta final del bartender
        if (isOriginalBartender)
        {
            lines.Add(new DialogLine { 
                text = "Sí, mejor comienza ya", 
                dialogPrefab = bartenderDialogPrefab 
            });
        }
        else
        {
            lines.Add(new DialogLine { 
                text = "Sí, buena suerte", 
                dialogPrefab = SecondBartenderDialogPrefab 
            });
        }

        dialogLines = lines.ToArray();
    }


    private void ShowNextDialog()
    {
        if (currentLineIndex >= dialogLines.Length)
        {
            if (isPostRuleta)
                ShowFinalChoice();
            else
                EndDialog();

            return;
        }

        if (currentDialogInstance != null)
        {
            Destroy(currentDialogInstance);
        }

        // Crear la nueva instancia del diálogo
        currentDialogInstance = Instantiate(dialogLines[currentLineIndex].dialogPrefab, Vector3.zero, Quaternion.identity);
        currentDialogInstance.transform.SetParent(GameObject.Find("Canvas").transform, false);
        currentDialogInstance.SetActive(true);

        TMP_Text messageText = currentDialogInstance.GetComponentInChildren<TMP_Text>();
        if (messageText != null)
        {
            messageText.text = dialogLines[currentLineIndex].text;
        }

        clearButton = currentDialogInstance.GetComponentInChildren<Button>();
        if (clearButton != null)
        {
            clearButton.onClick.RemoveAllListeners();
            clearButton.onClick.AddListener(() =>
            {
                currentLineIndex++;
                ShowNextDialog();
            });
        }
    }

    private void ShowFinalChoice()
    {
        Destroy(currentDialogInstance);

        currentDialogInstance = Instantiate(choiceDialogPrefab, Vector3.zero, Quaternion.identity);
        currentDialogInstance.transform.SetParent(GameObject.Find("Canvas").transform, false);
        currentDialogInstance.SetActive(true);

        TMP_Text messageText = currentDialogInstance.GetComponentInChildren<TMP_Text>();
        if (messageText != null)
        {
            messageText.text = "¿Qué prefieres?";
        }

        Button reencarnarButton = currentDialogInstance.transform.Find("ReencarnarButton")?.GetComponent<Button>();
        Button juezButton = currentDialogInstance.transform.Find("JuezButton")?.GetComponent<Button>();

        if (reencarnarButton != null)
        {
            reencarnarButton.onClick.AddListener(OnReencarnarClicked);
        }

        if (juezButton != null)
        {
            juezButton.onClick.AddListener(OnJuezClicked);
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.MarkPostRuletaDialogsSeen();
        }
    }

    private void OnReencarnarClicked()
    {
        SceneManager.LoadScene("EndGame");
    }

    private void OnJuezClicked()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.SetPlayerAsJudge();
        }

        BartenderController bartender = FindObjectOfType<BartenderController>();
        if (bartender != null)
        {
            bartender.SetAlternateState();
        }

        SceneManager.LoadScene("EndGame");
    }

    private void EndDialog()
    {
        Destroy(currentDialogInstance);
        PlayerPrefs.DeleteKey("PreviousScene");
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.canMove = true;
        }
    }
}
