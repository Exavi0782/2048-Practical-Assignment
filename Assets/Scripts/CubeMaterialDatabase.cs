using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CubeMaterialDatabase", menuName = "2048/Cube Material Database")]
public class CubeMaterialDatabase : ScriptableObject
{
    [SerializeField] private List<Material> materials;

    public Material GetMaterial(int level)
    {
        if (level < 0 || level >= materials.Count)
            return materials[0];

        return materials[level];
    }

    public int GetLevel(Material mat)
    {
        return materials.IndexOf(mat);
    }

    public int MaxLevel => materials.Count - 1;
}
