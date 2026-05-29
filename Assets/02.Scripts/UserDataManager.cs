using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance;
    [SerializeField]
    private UserData userData;
    public UserData UserData => userData;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void CreateUserData(string nickName, int gold = 0, int level = 1, int exp = 0)
    {
        UserData userData = new UserData();
        userData.UserName = nickName;
        userData.Level = level;
        userData.Exp = exp;
        userData.Gold = gold;
        this.userData = userData;
    }
}
