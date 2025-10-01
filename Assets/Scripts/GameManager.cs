using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Thirdweb.Unity;
using System.Threading.Tasks;
using Thirdweb;

public class GameManager : MonoBehaviour
{
    private GameState gameState;

    [SerializeField] private List<LevelSO> levels;
    private LevelSO currentLevelSO;
    [SerializeField] private GameObject PlayingUI;
    [SerializeField] private GameObject GameOverUI;
    [SerializeField] private GameObject GameWinUI;
    [SerializeField] private Board spawnBoard;
    [SerializeField] private GameObject TriesUI;
    [SerializeField] private GameObject BaseGridUI;
    [SerializeField] private GameObject ModelGridUI;

    [SerializeField] private List<Sprite> BaseGridImages;
    [SerializeField] private List<Sprite> ModelGridImages;

    [SerializeField] private List<GameObject> ProgressBars;
    [SerializeField] private Transform ProgressContainer;
    [SerializeField] private ParticleSystem ConfettiPrefab;

    // Partie Thirdweb
    [SerializeField] private string contractAddress; // Ton adresse de contrat
    [SerializeField] private int tokenId = 0; // ID du NFT universel
    [SerializeField] private Sprite featureToUnlock; // Skin/Booster à activer
    [SerializeField] private TextMeshProUGUI walletAddressText;
    [SerializeField] private GameObject HeartUI;
    [SerializeField] private Sprite defaultIcon; // Skin/Booster à activer

    private int chainId = 11155111;
    private string wallet_address;


    [SerializeField] private Camera mainCamera; // Ta caméra
    [SerializeField] private float baseHeight = 2.5f; // Hauteur de base de la caméra
    [SerializeField] private float baseFOV = 60f; // FOV de base de la caméra
    [SerializeField] private GameObject TutorialText;


    private int indexLvl;

    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Playing,
        GameWin,
        GameOver
    }

    private void Awake()
    {
        Instance = this;
        OnEnterGamePlaying();
    }

    private void Start()
    {
        AdjustCameraForGrid(currentLevelSO.GetWidthBoard(), currentLevelSO.GetHeightBoard());
    }
    private void Update()
    {
        //gere les differents etats du jeu 
        switch (gameState)
        {
            // partie lancer
            case GameState.Playing:
                GetTriesUI();
                break;
            // partie perdu
            case GameState.GameOver:
                PlayingUI.SetActive(false);
                GameOverUI.SetActive(true);
                break;
            // partie gagne
            case GameState.GameWin:
                PlayingUI.SetActive(false);
                GameWinUI.SetActive(true);
                break;
        }
    }

    private void Initialise()
    {
        spawnBoard.SetLvlValue(
            currentLevelSO.GetWidthBoard(),
            currentLevelSO.GetHeightBoard(),
            currentLevelSO.GetCubesPlacement(),
            currentLevelSO.GetCubesModel(),
            currentLevelSO.GetMaxtries()
            );
    }

    //récupérer les essais pour les mettre dans l'UI
    public void GetTriesUI()
    {
        TriesUI.GetComponent<TextMeshProUGUI>().SetText(spawnBoard.GetTries().ToString());
    }

    public void ResetLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    private void SetImages(int index)
    {
        BaseGridUI.GetComponent<Image>().sprite = BaseGridImages[index];
        ModelGridUI.GetComponent<Image>().sprite = ModelGridImages[index];
    }

    public void AdjustCameraForGrid(int gridWidth, int gridHeight)
    {

        // Ajuster la position de la caméra en fonction de la taille de la grille
        Vector3 newPosition = new Vector3((gridWidth - 1) / 2f, baseHeight, gridHeight * -2.0375f);
        mainCamera.transform.position = newPosition;
    }

    private void LoadLevel()
    {
        int levelI;

        levelI = PlayerPrefs.GetInt("levelIndex", 0);
        levelI = levelI % levels.Count;

        // initialise le level, en mettant à jour les différentes valeurs (voir levelSO)
        currentLevelSO = levels[levelI];
        SetImages(levelI);
        Instantiate(ProgressBars[levelI], ProgressContainer);
        Initialise();
    }

    // passe au niveau suivant
    public void NextLevel()
    {
        indexLvl = PlayerPrefs.GetInt("levelIndex");

        indexLvl++;

        PlayerPrefs.SetInt("levelIndex", indexLvl);
        PlayerPrefs.Save();


        // Reinitialise la scène
        ResetLevel();
    }

    public void OnEnterGameWin()
    {
        gameState = GameState.GameWin;
        ShowConfetti();
    }

    public void OnEnterGamePlaying() {
        gameState = GameState.Playing;
        LoadLevel();
    }

    public void OnEnterGameOver()
    {
        gameState = GameState.GameOver;
    }

    public void HideTutoText()
    {
        TutorialText.SetActive(false);
    }

    public void ShowConfetti()
    {
        // Ajuster la position de la caméra en fonction de la taille de la grille
        Vector3 newPosition = new Vector3((currentLevelSO.GetWidthBoard() - 1) / 2f, baseHeight + 1, currentLevelSO.GetHeightBoard() * -1.5f);
        ConfettiPrefab.transform.position = newPosition;

        Quaternion spawnRotation = Quaternion.identity; // Default rotation
        ConfettiPrefab.transform.localScale = Vector3.one;

        Instantiate(ConfettiPrefab, newPosition,spawnRotation);
    }

    private async Task CheckOwnership()
    {
        try
        {
            // 1. Récupère le contrat
            var contract = await ThirdwebManager.Instance.GetContract(contractAddress, chainId);
            var result = await contract.Read<string>("name");


            // 2. Appelle ownerOf
            var owner = await contract.Read<string>("ownerOf", tokenId);
            Debug.Log($"Propriétaire du token {tokenId} : {owner}");

            if (wallet_address != null)
            {
                if (owner == wallet_address)
                {
                    Debug.Log($"{wallet_address} possède le token {tokenId} ");
                    walletAddressText.text = "FEATURE : ON";
                    HeartUI.GetComponent<Image>().sprite = featureToUnlock;
                }
                else
                {
                    Debug.Log($"{wallet_address} ne possède pas le token {tokenId} ");
                    walletAddressText.text = "FEATURE : OFF";
                    HeartUI.GetComponent<Image>().sprite = defaultIcon;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur : {e.Message}");
            //featureToUnlock.SetActive(false);
        }
    }

    public async void ConnectWallet()
    {
        try
        {
            var options = new WalletOptions(provider: WalletProvider.WalletConnectWallet, chainId: 1);
            var wallet = await ThirdwebManager.Instance.ConnectWallet(options);
            wallet_address = await wallet.GetAddress();

            ThirdwebManager.Instance.SetActiveWallet(wallet);

            await CheckOwnership();

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur : {e.Message}");
        }
    }
}
