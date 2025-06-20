using UnityEngine;

[CreateAssetMenu()]
public class SlimeSummonSO : ScriptableObject
{

    [SerializeField] private string slimeName;
    [SerializeField] private Transform slimeSummonPrefab;

    public string SlimeName => slimeName;
    public Transform SlimeSummonPrefab => slimeSummonPrefab;
}