using UnityEngine;

public class WeaponRunTime : MonoBehaviour
{
    public WeaponData data;
    public int currentAmmo;

    public WeaponRunTime(WeaponData data)
    {
        this.data = data;
        if (data.useAmmo)
            currentAmmo = data.magazineSize;
        else
            currentAmmo = 0;
    }

    public bool HasAmmo()
    {
        if (!data.useAmmo)
            return true;
        return currentAmmo > 0;
    }

    public void ConsumeAmmo()
    {
        if(!data.useAmmo) return;
        currentAmmo--;
    }

    public void Reload()
    {
        if(!data.useAmmo) return;
        currentAmmo = data.magazineSize;
    }
}
