using UnityEngine;

public class PlayerDance : MonoBehaviour
{
    [SerializeField] Animator m_Animator;
    [SerializeField] PlayerInputReader m_PlayerInputReader;
    [SerializeField] string danceTriggerName = "Dance";
    private int danceHash;

    private void Awake()
    {
        if (m_Animator == null)
            m_Animator = GetComponent<Animator>();
        if(m_PlayerInputReader == null)
            m_PlayerInputReader = GetComponent<PlayerInputReader>();
        danceHash = Animator.StringToHash(danceTriggerName);
    }

    private void Update()
    {
        if (m_PlayerInputReader.IsDancePerformedThisFrame)
        {
            PlayDance();
        }
    }
    void PlayDance()
    {
        m_Animator.SetTrigger(danceHash);
    }
}
