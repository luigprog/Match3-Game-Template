using UnityEngine;

public class ParticlesManager : MonoBehaviour
{
    private static ParticlesManager instance;
    private const int QUANTITY_IN_POOL = 20;

    [SerializeField]
    private GameObject tileDestructionParticlePrefab;

    [SerializeField]
    private Transform tileDestructionParticlesHolder;

    private ParticleSystem[] tileDestructionParticles;
    private int tileDestructionParticlesCursor;

    public static ParticlesManager Instance { get { return instance; } }

    private void Awake()
    {
        instance = this;

        tileDestructionParticles = new ParticleSystem[QUANTITY_IN_POOL];
        for (int i = 0; i < QUANTITY_IN_POOL; i++)
        {
            GameObject particleObject = Instantiate(tileDestructionParticlePrefab);
            particleObject.transform.parent = tileDestructionParticlesHolder;
            particleObject.transform.localPosition = Vector3.zero;
            tileDestructionParticles[i] = particleObject.GetComponent<ParticleSystem>();
        }
    }

    public void PlayTileDestructionParticle(Color ofColor, Vector3 atPosition)
    {
        tileDestructionParticles[tileDestructionParticlesCursor].gameObject.transform.position = atPosition;
        tileDestructionParticles[tileDestructionParticlesCursor].startColor = ofColor;
        tileDestructionParticles[tileDestructionParticlesCursor].Stop();
        tileDestructionParticles[tileDestructionParticlesCursor].Play();
        tileDestructionParticlesCursor++;
        if (tileDestructionParticlesCursor == QUANTITY_IN_POOL)
        {
            tileDestructionParticlesCursor = 0;
        }
    }
}