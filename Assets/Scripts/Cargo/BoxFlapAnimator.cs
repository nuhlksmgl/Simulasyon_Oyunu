using UnityEngine;
using System.Collections;

public class BoxFlapAnimator : MonoBehaviour
{
    [Header("Animasyon Ayarlarý")]
    [Tooltip("Animasyonun tamamlanacaðý saniye cinsinden süre.")]
    [SerializeField] private float animationDuration = 0.5f;
    [Tooltip("Kapanacak olan ilk kapak.")]
    [SerializeField] private Transform flap1;
    [Tooltip("Kapanacak olan ikinci kapak.")]
    [SerializeField] private Transform flap2;

    // Kapaklarýn açýk ve kapalý haldeki yerel rotasyonlarý
    private Quaternion flap1_OpenRotation;
    private Quaternion flap2_OpenRotation;
    private Quaternion flap1_ClosedRotation;
    private Quaternion flap2_ClosedRotation;

    private bool isAnimating = false;

    void Start()
    {
        // Baþlangýçtaki (açýk) rotasyonlarýný kaydet
        if (flap1 != null) flap1_OpenRotation = flap1.localRotation;
        if (flap2 != null) flap2_OpenRotation = flap2.localRotation;

        // Kapalý rotasyonlarý belirle (genellikle X ekseninde 90 derece içe doðru)
        // Not: Eðer kapaklar farklý eksende dönüyorsa buradaki Euler açýlarýný deðiþtirmen gerekebilir.
        // Örneðin, Z ekseninde dönüyorsa Quaternion.Euler(0, 0, 90) gibi.
        flap1_ClosedRotation = Quaternion.Euler(90, 0, 0);
        flap2_ClosedRotation = Quaternion.Euler(90, 0, 0);
    }

    /// <summary>
    /// Kutuyu kapatma animasyonunu baþlatýr.
    /// </summary>
    public void CloseBox()
    {
        if (!isAnimating)
        {
            StartCoroutine(AnimateFlaps(flap1_OpenRotation, flap1_ClosedRotation, flap2_OpenRotation, flap2_ClosedRotation));
        }
    }

    /// <summary>
    /// Kutuyu açma animasyonunu baþlatýr.
    /// </summary>
    public void OpenBox()
    {
        if (!isAnimating)
        {
            StartCoroutine(AnimateFlaps(flap1_ClosedRotation, flap1_OpenRotation, flap2_ClosedRotation, flap2_OpenRotation));
        }
    }

    private IEnumerator AnimateFlaps(Quaternion start1, Quaternion end1, Quaternion start2, Quaternion end2)
    {
        isAnimating = true;
        float time = 0;

        while (time < animationDuration)
        {
            // Geçen süreye göre animasyonun ne kadarýnýn tamamlandýðýný hesapla (0 ile 1 arasýnda)
            float t = time / animationDuration;

            // Kapaklarýn localRotation'ýný yumuþakça (Slerp) baþlangýçtan hedefe doðru deðiþtir
            if (flap1 != null) flap1.localRotation = Quaternion.Slerp(start1, end1, t);
            if (flap2 != null) flap2.localRotation = Quaternion.Slerp(start2, end2, t);

            time += Time.deltaTime;
            yield return null; // Bir sonraki frame'e kadar bekle
        }

        // Animasyon bitince tam olarak hedef rotasyonda olduklarýndan emin ol
        if (flap1 != null) flap1.localRotation = end1;
        if (flap2 != null) flap2.localRotation = end2;

        isAnimating = false;
    }
}