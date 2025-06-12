using UnityEngine;

/// <summary>
/// Bu script, bir prefab'ýn root objesine konulur.
/// Görevi, hiyerarþinin daha derinlerindeki asýl CargoBox script'ine bir referans tutmaktýr.
/// Diðer script'ler bu proxy üzerinden asýl CargoBox'a ulaþýr.
/// </summary>
public class CargoBoxProxy : MonoBehaviour
{
    [Tooltip("Hiyerarþide CargoBox.cs script'ini içeren asýl objeyi buraya sürükleyin.")]
    public CargoBox RealCargoBox;

    void Awake()
    {
        // Inspector'dan atama yapýlmamýþsa, çocuklarda aramayý dene. Bu bir güvenlik önlemidir.
        if (RealCargoBox == null)
        {
            RealCargoBox = GetComponentInChildren<CargoBox>();
            if (RealCargoBox == null)
            {
                Debug.LogError($"CargoBoxProxy ({gameObject.name}): 'RealCargoBox' referansý atanmamýþ ve çocuk objelerde de CargoBox script'i bulunamadý!", this);
            }
        }
    }
}