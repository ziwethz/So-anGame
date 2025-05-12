using UnityEngine;
using System.Collections.Generic;

public class EngelYoneticisi : MonoBehaviour
{
    [Header("Engel Prefabları")]
    [SerializeField] private GameObject duvarPrefab;
    [SerializeField] private GameObject engelPrefab;
    [SerializeField] private GameObject gizliYolPrefab;
    
    [Header("Engel Ayarları")]
    [SerializeField] private int engelSayisi = 10;
    [SerializeField] private float engelYukseklik = 2f;
    [SerializeField] private float gizliYolOlasiligi = 0.3f;
    
    private List<GameObject> engeller = new List<GameObject>();
    private List<GameObject> gizliYollar = new List<GameObject>();
    
    public void EngelleriOlustur(Vector3[] platformPozisyonlari)
    {
        // Haritanın kenarlarına duvarlar ekle
        DuvarEkle(new Vector3(-25f, 0f, 0f), new Vector3(1f, 30f, 1f)); // Sol duvar
        DuvarEkle(new Vector3(25f, 0f, 0f), new Vector3(1f, 30f, 1f));  // Sağ duvar
        
        // Platformlar arası engeller
        for (int i = 0; i < platformPozisyonlari.Length - 1; i++)
        {
            Vector3 platform1 = platformPozisyonlari[i];
            Vector3 platform2 = platformPozisyonlari[i + 1];
            
            // Platformlar arası mesafe
            float mesafe = Vector3.Distance(platform1, platform2);
            
            // Engel sayısını belirle
            int bolgeEngelSayisi = Mathf.CeilToInt(mesafe / 5f);
            
            for (int j = 0; j < bolgeEngelSayisi; j++)
            {
                float t = (j + 1f) / (bolgeEngelSayisi + 1f);
                Vector3 engelPoz = Vector3.Lerp(platform1, platform2, t);
                
                // Engel yüksekliğini platformlar arası yüksekliğe göre ayarla
                float yukseklik = Mathf.Abs(platform2.y - platform1.y) * 0.5f;
                yukseklik = Mathf.Max(yukseklik, engelYukseklik);
                
                // Engel oluştur
                GameObject engel = Instantiate(engelPrefab, engelPoz, Quaternion.identity);
                engel.transform.localScale = new Vector3(1f, yukseklik, 1f);
                engeller.Add(engel);
                
                // Gizli yol olasılığı
                if (Random.value < gizliYolOlasiligi)
                {
                    GameObject gizliYol = Instantiate(gizliYolPrefab, engelPoz, Quaternion.identity);
                    gizliYol.transform.localScale = new Vector3(1f, yukseklik * 0.5f, 1f);
                    gizliYollar.Add(gizliYol);
                }
            }
        }
    }
    
    private void DuvarEkle(Vector3 pozisyon, Vector3 boyut)
    {
        GameObject duvar = Instantiate(duvarPrefab, pozisyon, Quaternion.identity);
        duvar.transform.localScale = boyut;
        engeller.Add(duvar);
    }
    
    public void EngelleriTemizle()
    {
        foreach (var engel in engeller)
        {
            if (engel != null)
            {
                Destroy(engel);
            }
        }
        
        foreach (var gizliYol in gizliYollar)
        {
            if (gizliYol != null)
            {
                Destroy(gizliYol);
            }
        }
        
        engeller.Clear();
        gizliYollar.Clear();
    }
    
    public bool GizliYolVarMi(Vector3 pozisyon)
    {
        foreach (var gizliYol in gizliYollar)
        {
            if (gizliYol != null)
            {
                float mesafe = Vector3.Distance(pozisyon, gizliYol.transform.position);
                if (mesafe < 1f)
                {
                    return true;
                }
            }
        }
        return false;
    }
} 