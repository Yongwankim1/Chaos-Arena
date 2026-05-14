using System.Collections.Generic;
using UnityEngine;

public class WeaponTester : MonoBehaviour
{
    [SerializeField] List<WeaponData> weaponDatas = new List<WeaponData>();

    void Start()
    {
        for (int i = 0; i < weaponDatas.Count; i++)
        {
            string jobs = string.Empty;
            for (int j = 0; j < weaponDatas[i].jobs.Count; j++)
            {
                jobs += weaponDatas[i].jobs[j].ToString() + " ";
            }
            Debug.Log($"무기 이름 : {weaponDatas[i].weaponName}\n" +
                $"공격력 : {weaponDatas[i].damage}\n" +
                $"사거리 : {weaponDatas[i].attackRange}\n" +
                $"공격 속도 : {weaponDatas[i].attackRate}\n" +
                $"탄약 사용 여부 : {weaponDatas[i].useAmmo}\n" +
                $"근접 무기 여부 : {weaponDatas[i].isMelee}\n" +
                $"착용 가능 직업 : {jobs}");

        }
    }

}
