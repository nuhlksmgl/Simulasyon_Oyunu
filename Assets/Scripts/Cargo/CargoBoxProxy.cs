using UnityEngine;

public class CargoBoxProxy : MonoBehaviour
{
    [Tooltip("Hiyerarþide CargoBox.cs script'ini içeren asýl objeyi buraya sürükleyin.")]
    public CargoBox RealCargoBox;
    private bool isFollowing = false;

    void Awake()
    {
        if (RealCargoBox == null) RealCargoBox = GetComponentInChildren<CargoBox>();
    }

    void LateUpdate()
    {
        if (isFollowing && RealCargoBox != null)
        {
            transform.position = RealCargoBox.transform.position;
            transform.rotation = RealCargoBox.transform.rotation;
        }
    }

    public void StartFollowing()
    {
        if (RealCargoBox != null) RealCargoBox.transform.SetParent(null, true);
        isFollowing = true;
    }

    public void StopFollowingAndReparent()
    {
        isFollowing = false;
        if (RealCargoBox != null) RealCargoBox.transform.SetParent(this.transform, true);
    }
}