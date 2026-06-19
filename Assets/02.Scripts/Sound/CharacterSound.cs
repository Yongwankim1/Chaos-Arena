using UnityEngine;

public class CharacterSound : MonoBehaviour
{
    [SerializeField]
    private CharacterSoundLibrary library;

    private void Play(SoundEntry entry)
    {
        if (entry == null)
            return;

        SoundManager.Instance.Play3D(
            entry,
            transform.position);
    }

    public void PlayFootStep() => Play(library.FootStep);

    public void PlayJump() => Play(library.Jump);

    public void PlayLand() => Play(library.Land);

    public void PlayDeath() => Play(library.Death);

    public void PlayAttack1() => Play(library.Attack1);

    public void PlayAttack2() => Play(library.Attack2);

    public void PlayAttack3() => Play(library.Attack3);

    public void PlayAttack4() => Play(library.Attack4);

    public void PlayAttack5() => Play(library.Attack5);

    public void PlayDash() => Play(library.Dash);

    public void PlaySkillQ() => Play(library.SkillQ);

    public void PlaySkillE() => Play(library.SkillE);

    public void PlaySkillR() => Play(library.SkillR);
}