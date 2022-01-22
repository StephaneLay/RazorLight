using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager gm;
    public int score;
    
    public Camera mainCamera;
    public PlayerController player;
    public Transform obstaclesContainer;
    public List<ObstacleComponent> obstaclesList;
    public Transform bulletContainer;
    public List<PhaseSO> phaseList;
    private int enemyLeftToSpawn;
    public AudioSource audioSource;
    public bool isGameOver;
    public CanvasGroup gameOverCanvas;
    

    [Space]
    [Header("Particles")]
    [Space]

    public Transform particlesContainer;
    public GameObject explosionParticles;



    [Space]
    [Header("Spawn")]
    [Space]
    
    public Transform spawnerContainer;
    public Transform enemyContainer;
    
    public GameObject enemyprefab;
    public float timeBetweenWaves;
    
    public List<GameObject> activeSpawners;
    public List<GameObject> availableSpawners;
        
    private void Awake()
    {
        gm = this;

        audioSource = GetComponent<AudioSource>();
        
        foreach (Transform child in obstaclesContainer)
        {
             obstaclesList.Add(child.gameObject.GetComponent<ObstacleComponent>());
        }
        
        foreach (Transform child in spawnerContainer)
        {
            activeSpawners.Add(child.gameObject);
        }
    }

    public void StartGame() 
    {
        EnablePhase(0);
    }

   
    public void Retry()
    {
        foreach (Transform child in bulletContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in enemyContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in particlesContainer)
        {
            Destroy(child.gameObject);
        }
        
        score = 0;
        player.currenthealth = 3;
        player.ship.SetActive(true);
        player.leftShipSection.SetActive(true);
        player.rightShipSection.SetActive(true);
        player.isLeftFireEnabled = false;
        player.isRightFireEnabled = false;
        gameOverCanvas.alpha = 0;
        Time.timeScale = 1;
        EnablePhase(0);
        isGameOver = false;
    }

    public void EnablePhase(int _phasenumber)
    {
        
         enemyLeftToSpawn = phaseList[_phasenumber].enemyQty;
         Debug.Log("Phase " + _phasenumber + " � d�but�");
        SetArenaPattern(_phasenumber);
         Debug.Log(phaseList[_phasenumber].track);
        AudioManager.am.Mute(phaseList[_phasenumber].track, false);
        AddPowerUp(_phasenumber);
        StartCoroutine(SpawnPhaseWaves(_phasenumber));
    }
    private void SetArenaPattern(int _phasenumber)
    {
        for (int i = 0; i < phaseList[_phasenumber].arenaPattern.activatedObsacles.Length-1 ; i++)
        {
            obstaclesList[i].SetObstacleActive(phaseList[_phasenumber].arenaPattern.activatedObsacles[i]);
        }
    }
    private void AddPowerUp(int _phasenumber)
    {
        if (phaseList[_phasenumber].powerUpToAdd == PhaseSO.powerUp.None)
        {
            return;
        }
        if (phaseList[_phasenumber].powerUpToAdd == PhaseSO.powerUp.addBounce)
        {
            player.maxBulletBounces++;
        }
        else
        {
            if (player.isLeftFireEnabled)
            {
                player.isRightFireEnabled = true;
            }
            else
            {
                player.isLeftFireEnabled = true;
            }
        }
        
    }
    public IEnumerator SpawnPhaseWaves(int _phasenumber)
    {
        for (int i = 0; i < phaseList[_phasenumber].spawnProgression.Count; i++)
        {
            if (isGameOver)
                break;
            
            UpdateSpawnerList();
            SpawnRandomly(phaseList[_phasenumber].spawnProgression[i],_phasenumber);
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }
    
    public void UpdateSpawnerList()
    {
        availableSpawners.Clear();
        for (int i = 0; i < activeSpawners.Count; i++)
        {
            if (activeSpawners[i].GetComponent<SpawnerComponent>().isAvailable())
            {
                availableSpawners.Add(activeSpawners[i]);
            }
        }
    }
    public void SpawnRandomly(int _waveQty,int _phasenumber)
    {
        if (isGameOver)
            return;
        if (_waveQty<= availableSpawners.Count)
        {
            List<int> randomizator = new List<int>();
            while (randomizator.Count < _waveQty)
            {
                int r = Random.Range(0, availableSpawners.Count);
                if (randomizator.Contains(r))
                {
                    continue;
                }
                else
                {
                    randomizator.Add(r);
                }
            }
            for (int i = 0; i < _waveQty; i++)
            {
                Instantiate(enemyprefab, availableSpawners[randomizator[i]].transform.position, Quaternion.identity, enemyContainer);
                enemyLeftToSpawn--;
                if (enemyLeftToSpawn == 0 )
                {
                    EnablePhase(_phasenumber + 1);
                }
            }
            
        }
        else
        {
            
            int spawnsToDelay = _waveQty - availableSpawners.Count;
            phaseList[_phasenumber].spawnProgression.Add(spawnsToDelay);

            for (int i = 0; i < availableSpawners.Count; i++)
            {
                
                Instantiate(enemyprefab, availableSpawners[i].transform.position, Quaternion.identity, enemyContainer);
                enemyLeftToSpawn--;
                if (enemyLeftToSpawn == 0 )
                {
                    EnablePhase(_phasenumber + 1);
                }
            }
            
        }
    }
}
