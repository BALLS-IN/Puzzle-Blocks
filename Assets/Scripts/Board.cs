using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Board : MonoBehaviour
{
    [SerializeField]private int widthBoard;
    [SerializeField]private int heightBoard;

    [SerializeField] private List<Cube> cubePrefabs;
    private int tries;

    private int[,] targetGrid;
    private Cube[,] cubes;
    private int[,] grid;
    private bool cubeMoving = false;
    int newX;
    int newY;

    private void Start()
    {
        grid = new int[widthBoard, heightBoard];
        cubes = new Cube[widthBoard, heightBoard];
        GenerateBoard();
    }

    public void SetLvlValue(int width, int height, List<Cube> cubesPlacement, List<Vector2> modelPlacement, int maxTries)
    {
        widthBoard = width;
        heightBoard = height;
        cubePrefabs = cubesPlacement;
        tries = maxTries;

        targetGrid = new int[widthBoard, heightBoard];

        foreach (Vector2 pos in modelPlacement)
        {
            targetGrid[(int)pos.x, (int)pos.y] = 1; // Marque les positions occupées dans le modèle
        }
    }


    void GenerateBoard()
    {
        int i = 0;

        // G?n?rer la grille
        for (int y = 0; y < heightBoard; y++)
        {
            for (int x = 0; x < widthBoard; x++)
            {

                // Ne spawn pas de block sur la case vide 
                if (cubePrefabs[i] == null)
                {
                    grid[x, y] = 0;
                    cubes[x, y] = null;
                }

                else
                {
                    Cube spawnedCube = Instantiate(cubePrefabs[i], new Vector3(x, y), Quaternion.identity, transform);
                    grid[x, y] = 1;
                    cubes[x, y] = spawnedCube;

                    spawnedCube.OnCubeTouched += CheckIfVoidOnSide;
                }
                i++;
            }
        }
    }

    private void CheckIfVoidOnSide(Cube cube,Vector3 hit)
    {
        if(tries != 0)
        {
            if (!cubeMoving)
            {
                GameManager.Instance.HideTutoText();
                int cubePositionX = (int)cube.transform.position.x;
                int cubePositionY = (int)cube.transform.position.y;

                Debug.Log("Cube: " + cube.transform.position);
                Debug.Log("hit: "+ hit);

                newY = cubePositionY;
                newX = cubePositionX;

                float diffX = Mathf.Abs(hit.x - cube.transform.position.x);
                float diffY = Mathf.Abs(hit.y - cube.transform.position.y);

                if (diffX > diffY)
                {
                    if (hit.x > cube.transform.position.x)
                    {
                        //Debug.Log("Swiping Right");
                        newX = cubePositionX + 1;
                        newY = cubePositionY;
                    }
                    if (hit.x < cube.transform.position.x)
                    {
                        //Debug.Log("Swiping Left");
                        newX = cubePositionX - 1;
                        newY = cubePositionY;

                    }
                }
                else
                {
                    if (hit.y > cube.transform.position.y)
                    {
                        //Debug.Log("Swiping Up");
                        newY = cubePositionY + 1;
                        newX = cubePositionX;
                    }
                    if (hit.y < cube.transform.position.y)
                    {
                        //Debug.Log("Swiping down");
                        newY = cubePositionY - 1;
                        newX = cubePositionX;
                    }
                }

                if (IsTileOccupied(newX, newY))
                {
                    MoveCube(cube, cubePositionX, cubePositionY, newX, newY);
                }
            }
        }
    }

    public bool IsTileOccupied(int x, int y)
    {
        // V?rifie si les coordonn?es sont dans les limites de la grille
        if (x < 0 || x >= widthBoard || y < 0 || y >= heightBoard)
        {
            /*Debug.LogWarning("Coordonnees hors limites !");*/
            return false;
        }

        // Retourne l'etat de la case (1 = occup?e, 0 = vide)
        return grid[x, y] == 0;
    }

    private void MoveCube(Cube cube, int oldX, int oldY, int newX, int newY)
    {
        cubeMoving = true;
        UpdateGrid(oldX, oldY, newX, newY);
        cube.Move(newX, newY);
        cube.OnCubePlaced += cubePlaced;
    }

    private void UpdateGrid(int oldX, int oldY, int newX, int newY)
    {
        grid[oldX, oldY] = 0; // Marque l'ancienne position comme vide
        cubes[newX, newY] = cubes[oldX, oldY]; // Deplace la piece dans le tableau
        cubes[oldX, oldY] = null;
        grid[newX, newY] = 1; // Marque la nouvelle position comme occupe
        tries -= 1;
        if (IsGridMatching())
        {
            Debug.Log(" Bravo ! La grille correspond au modèle !");
            // Ici, tu peux déclencher une animation, un effet sonore ou passer au niveau suivant
            StartCoroutine(WaitGameWin());
        }
        else if(tries == 0)
        {
            Debug.Log("You LOSE, NO TRIES LEFT");
            StartCoroutine(WaitGameOver());
        }
    }

    private IEnumerator WaitGameOver()
    {
        yield return new WaitForSeconds(0.5f);

        GameManager.Instance.OnEnterGameOver();
    }

    private IEnumerator WaitGameWin()
    {
        yield return new WaitForSeconds(0.5f);

        GameManager.Instance.OnEnterGameWin();
    }

    private void cubePlaced(Cube cube)
    {
        cube.OnCubePlaced -= cubePlaced;
        cubeMoving = false;   
    }


    private bool IsGridMatching()
    {
        for (int x = 0; x < widthBoard; x++)
        {
            for (int y = 0; y < heightBoard; y++)
            {
                if (grid[x, y] != targetGrid[x, y])
                {
                    Debug.Log(grid[x, y] + " " + targetGrid[x, y] + " ne correspondent pas");
                    return false; // Dès qu'une case ne correspond pas, ce n'est pas bon
                }
            }
        }
        Debug.Log("toutes les cases correspondent");
        return true; // Si toutes les cases correspondent, la grille est correcte
    }

    public int GetTries()
    {
        return tries;
    }
};
