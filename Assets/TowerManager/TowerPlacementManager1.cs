using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerPlacementManager : MonoBehaviourPunCallbacks
{
    public GameObject baseTowerPrefab;
    public GameObject slowingTowerPrefab;
    public GameObject poisonTowerPrefab;
    public GameObject bombTowerPrefab;
    public GameObject markTowerPrefab;
    public GameObject warmupTowerPrefab;
    public GameObject bouncingTowerPrefab;

    public bool isBaseTowerSpawned = false;
    public bool isSlowingTowerSpawned = false;
    public bool isPoisonTowerSpawned = false;
    public bool isBombTowerSpawned = false;
    public bool isMarkTowerSpawned = false;
    public bool isWarmupTowerSpawned = false;
    public bool isBouncingTowerSpawned = false;

    private int nextTowerID = 0;

    private GameObject selectedTower;

    public List<Vector3> occupiedPositions = new List<Vector3>();

    public new PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit2D.collider != null)
            {
                if (hit2D.collider.gameObject.CompareTag("TowerPlacement"))
                {
                    Vector3 placementPosition = hit2D.collider.transform.position;
                    placementPosition.y += 1.3f;

                    if (!occupiedPositions.Contains(placementPosition))
                    {
                        if (selectedTower != null)
                        {
                            photonView.RPC("PlaceTower", RpcTarget.All, selectedTower.name, placementPosition);
                            GameObject turnManager = GameObject.Find("TurnManager");
                            if (selectedTower == baseTowerPrefab)
                            {
                                isBaseTowerSpawned = true;
                                Transform buttonToDisable = turnManager.GetComponent<TurnManager>().defenderButtons.transform.Find("BaseTower");
                                buttonToDisable.gameObject.SetActive(false);
                            }
                            else if (selectedTower == slowingTowerPrefab)
                            {
                                isSlowingTowerSpawned = true;
                                Transform buttonToDisable = turnManager.GetComponent<TurnManager>().defenderButtons.transform.Find("SlowingTower");
                                buttonToDisable.gameObject.SetActive(false);
                            }
                            else if (selectedTower == poisonTowerPrefab)
                            {
                                isPoisonTowerSpawned = true;
                                Transform buttonToDisable = turnManager.GetComponent<TurnManager>().defenderButtons.transform.Find("PoisonTower");
                                buttonToDisable.gameObject.SetActive(false);
                            }
                            else if (selectedTower == bombTowerPrefab)
                            {
                                isBombTowerSpawned = true;
                                Transform buttonToDisable = turnManager.GetComponent<TurnManager>().defenderButtons.transform.Find("BombTower");
                                buttonToDisable.gameObject.SetActive(false);
                            }
                            else if (selectedTower == markTowerPrefab)
                            {
                                isMarkTowerSpawned = true;
                                Transform buttonToDisable = turnManager.GetComponent<TurnManager>().defenderButtons.transform.Find("MarkTower");
                                buttonToDisable.gameObject.SetActive(false);
                            }
                            else if (selectedTower == warmupTowerPrefab)
                            {
                                isWarmupTowerSpawned = true;
                                Transform buttonToDisable = turnManager.GetComponent<TurnManager>().defenderButtons.transform.Find("WarmupTower");
                                buttonToDisable.gameObject.SetActive(false);
                            }
                            else if (selectedTower == bouncingTowerPrefab)
                            {
                                isBouncingTowerSpawned = true;
                                Transform buttonToDisable = turnManager.GetComponent<TurnManager>().defenderButtons.transform.Find("BouncingTower");
                                buttonToDisable.gameObject.SetActive(false);
                            }
                            FindObjectOfType<TurnManager>().EndDefenderTurn();
                        }
                    }
                    else
                    {
                        Debug.Log("TowerPlacement point is already occupied.");
                        if (selectedTower == baseTowerPrefab)
                        {
                            isBaseTowerSpawned = false;
                        }
                        else if (selectedTower == slowingTowerPrefab)
                        {
                            isSlowingTowerSpawned = false;
                        }
                        else if (selectedTower == poisonTowerPrefab)
                        {
                            isPoisonTowerSpawned = false;
                        }
                        else if (selectedTower == bombTowerPrefab)
                        {
                            isBombTowerSpawned = false;
                        }
                        else if (selectedTower == markTowerPrefab)
                        {
                            isMarkTowerSpawned = false;
                        }
                        else if (selectedTower == warmupTowerPrefab)
                        {
                            isWarmupTowerSpawned = false;
                        }
                        else if (selectedTower == bouncingTowerPrefab)
                        {
                            isBouncingTowerSpawned = false;
                        }
                    }
                    selectedTower = null;
                }
            }
        }
    }
    public void SelectBaseTower()
    {
        if (!isBaseTowerSpawned)
        {
            selectedTower = baseTowerPrefab;
        }
    }

    public void SelectSlowingTower()
    {

        if (!isSlowingTowerSpawned)
        {
            selectedTower = slowingTowerPrefab;
        }
    }

    public void SelectPoisonTower()
    {
        if (!isPoisonTowerSpawned)
        {
            selectedTower = poisonTowerPrefab;
        }
    }

    public void SelectBombTower()
    {
        if(!isBombTowerSpawned)
        {
            selectedTower = bombTowerPrefab;
        }
    }

    public void SelectMarkTower()
    {
        if (!isMarkTowerSpawned)
        {
            selectedTower = markTowerPrefab;
        }
    }

    public void SelectWarmupTower()
    {
        if (!isWarmupTowerSpawned)
        {
            selectedTower = warmupTowerPrefab;
        }
    }

    public void SelectBouncingTower()
    {
        if (!isBouncingTowerSpawned)
        {
            selectedTower = bouncingTowerPrefab;
        }
    }

    [PunRPC]
    void PlaceTower(string towerName, Vector3 position)
    {
        GameObject towerPrefab;
        switch (towerName)
        {
            case "BaseTower": towerPrefab = baseTowerPrefab; break;
            case "SlowingTower": towerPrefab = slowingTowerPrefab; break;
            case "PoisonTower": towerPrefab = poisonTowerPrefab; break;
            case "BombTower": towerPrefab = bombTowerPrefab; break;
            case "MarkTower": towerPrefab = markTowerPrefab; break;
            case "WarmupTower": towerPrefab = warmupTowerPrefab; break;
            case "BouncingTower": towerPrefab = bouncingTowerPrefab; break;
            default:
                Debug.LogError("Unknown tower name: " + towerName);
                return;
        }

        GameObject newTower = Instantiate(towerPrefab, position, Quaternion.identity);
        newTower.GetComponent<BaseTower>().towerID = nextTowerID;
        GenerateNextTowerID();
        DisplayManager displayManager = GameObject.Find("DisplayManager").GetComponent<DisplayManager>();
        displayManager.towerLookup.Add(newTower.GetComponent<BaseTower>().towerID, newTower.GetComponent<BaseTower>());
        occupiedPositions.Add(position);
    }

    void GenerateNextTowerID()
    {
        nextTowerID++;
    }
}