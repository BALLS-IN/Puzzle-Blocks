using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class LevelSO : ScriptableObject
{
    [SerializeField] private int maxTries;
    [SerializeField] private List<Cube> cubesPlacement;
    [SerializeField] private List<Vector2> cubesModel;
    [SerializeField] private int WidthBoard;
    [SerializeField] private int HeightBoard;

    public int GetMaxtries()
    {
        return maxTries;
    }

    public List<Cube> GetCubesPlacement() {
        return cubesPlacement;
    }

    public List<Vector2> GetCubesModel() {
        return cubesModel;
    }

    public int GetWidthBoard() {
        return WidthBoard;
    }

    public int GetHeightBoard() {
        return HeightBoard;
    }
}
