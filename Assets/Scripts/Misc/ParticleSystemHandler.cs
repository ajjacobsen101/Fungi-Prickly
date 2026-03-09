using UnityEngine;

public class ParticleSystemHandler : MonoBehaviour
{
    [SerializeField] ParticleSystem particle;
    void Awake()
    {
        particle = GetComponentInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
