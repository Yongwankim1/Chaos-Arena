using Fusion;
using UnityEngine;

public class MageDash : NetworkBehaviour, IDash
{



    public bool IsDashing => throw new System.NotImplementedException();

    public Vector3 DashDirection => throw new System.NotImplementedException();

    public void Dash()
    {

    }

    public float GetMoveThisTick()
    {
        return 1f;
    }

}
