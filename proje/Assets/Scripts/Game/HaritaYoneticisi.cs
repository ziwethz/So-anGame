using UnityEngine;
using System.Collections.Generic;

public class HaritaYoneticisi : MonoBehaviour
{
    [System.Serializable]
    public class OzelBolge
    {
        public string bolgeAdi;
        public Transform bolgeTransform;
        public float etkiAlani;
        public BolgeTipi tip;
    }
    
    public enum BolgeTipi
    {
        ParfumBolgesi,
        SogukHavaBolgesi,
        RuzgarBolgesi,
        MaskeBolgesi
    }
    
    [Header("Harita Ayarları")]
    [SerializeField] private List<OzelBolge> ozelBolgeler = new List<OzelBolge>();
    [SerializeField] private float ruzgarDegisimSuresi = 10f;
    [SerializeField] private float ruzgarGucu = 2f;
    
    [Header("Harita Oluşturma")]
    [SerializeField] private GameObject zeminPrefab;
    [SerializeField] private GameObject duvarPrefab;
    [SerializeField] private GameObject tezgahPrefab;
    [SerializeField] private GameObject cadirPrefab;
    [SerializeField] private GameObject kutuPrefab;
    [SerializeField] private GameObject cicekPrefab;
    [SerializeField] private GameObject buzPrefab;
    [SerializeField] private GameObject bayrakPrefab;
    
    [Header("Harita Boyutları")]
    [SerializeField] private int haritaGenislik = 50;
    [SerializeField] private int haritaYukseklik = 30;
    [SerializeField] private float platformYukseklikFarki = 2f;
    
    [Header("Eşya Spawn Ayarları")]
    [SerializeField] private GameObject parfumPrefab;
    [SerializeField] private GameObject sogukHavaPrefab;
    [SerializeField] private GameObject maskePrefab;
    [SerializeField] private float esyaSpawnSuresi = 30f;
    
    private Vector2 ruzgarYonu;
    private float sonRuzgarDegisimZamani;
    private float sonEsyaSpawnZamani;
    private List<Vector3> platformPozisyonlari = new List<Vector3>();
    
    private void Start()
    {
        HaritayiOlustur();
        RuzgarYonunuGuncelle();
    }
    
    private void HaritayiOlustur()
    {
        // Ana zemin
        GameObject anaZemin = Instantiate(zeminPrefab, Vector3.zero, Quaternion.identity);
        anaZemin.transform.localScale = new Vector3(haritaGenislik, 1, 1);
        
        // Platformları oluştur
        PlatformlariOlustur();
        
        // Tezgahları yerleştir
        TezgahlariYerlestir();
        
        // Çadırları yerleştir
        CadirlariYerlestir();
        
        // Kutuları yerleştir
        KutulariYerlestir();
        
        // Özel bölgeleri oluştur
        OzelBolgeleriOlustur();
    }
    
    private void PlatformlariOlustur()
    {
        int platformSayisi = Random.Range(3, 6);
        float platformGenislik = haritaGenislik / (platformSayisi + 1);
        
        for (int i = 0; i < platformSayisi; i++)
        {
            float xPoz = (i + 1) * platformGenislik - haritaGenislik / 2;
            float yPoz = Random.Range(1f, 5f) * platformYukseklikFarki;
            
            GameObject platform = Instantiate(zeminPrefab, new Vector3(xPoz, yPoz, 0), Quaternion.identity);
            platform.transform.localScale = new Vector3(platformGenislik * 0.8f, 1, 1);
            
            platformPozisyonlari.Add(platform.transform.position);
        }
    }
    
    private void TezgahlariYerlestir()
    {
        foreach (Vector3 platformPoz in platformPozisyonlari)
        {
            int tezgahSayisi = Random.Range(2, 4);
            float tezgahAralik = 3f;
            
            for (int i = 0; i < tezgahSayisi; i++)
            {
                float xPoz = platformPoz.x - (tezgahSayisi * tezgahAralik) / 2 + i * tezgahAralik;
                Vector3 tezgahPoz = new Vector3(xPoz, platformPoz.y + 1f, 0);
                
                GameObject tezgah = Instantiate(tezgahPrefab, tezgahPoz, Quaternion.identity);
                
                // Tezgahın altına özel bölge ekle
                if (Random.value < 0.3f) // %30 ihtimalle
                {
                    OzelBolge yeniBolge = new OzelBolge
                    {
                        bolgeAdi = "Tezgah_" + i,
                        bolgeTransform = tezgah.transform,
                        etkiAlani = 2f,
                        tip = (BolgeTipi)Random.Range(0, 4)
                    };
                    ozelBolgeler.Add(yeniBolge);
                }
            }
        }
    }
    
    private void CadirlariYerlestir()
    {
        int cadirSayisi = Random.Range(2, 4);
        float cadirAralik = haritaGenislik / (cadirSayisi + 1);
        
        for (int i = 0; i < cadirSayisi; i++)
        {
            float xPoz = (i + 1) * cadirAralik - haritaGenislik / 2;
            float yPoz = 0.5f; // Zemin üzerinde
            
            GameObject cadir = Instantiate(cadirPrefab, new Vector3(xPoz, yPoz, 0), Quaternion.identity);
            
            // Çadırın içine özel bölge ekle
            OzelBolge yeniBolge = new OzelBolge
            {
                bolgeAdi = "Cadir_" + i,
                bolgeTransform = cadir.transform,
                etkiAlani = 3f,
                tip = (BolgeTipi)Random.Range(0, 4)
            };
            ozelBolgeler.Add(yeniBolge);
        }
    }
    
    private void KutulariYerlestir()
    {
        foreach (Vector3 platformPoz in platformPozisyonlari)
        {
            int kutuSayisi = Random.Range(1, 3);
            
            for (int i = 0; i < kutuSayisi; i++)
            {
                float xPoz = platformPoz.x + Random.Range(-2f, 2f);
                Vector3 kutuPoz = new Vector3(xPoz, platformPoz.y + 0.5f, 0);
                
                Instantiate(kutuPrefab, kutuPoz, Quaternion.identity);
            }
        }
    }
    
    private void OzelBolgeleriOlustur()
    {
        // Parfüm bölgeleri için çiçekler
        foreach (var bolge in ozelBolgeler)
        {
            if (bolge.tip == BolgeTipi.ParfumBolgesi)
            {
                Instantiate(cicekPrefab, bolge.bolgeTransform.position, Quaternion.identity);
            }
            else if (bolge.tip == BolgeTipi.SogukHavaBolgesi)
            {
                Instantiate(buzPrefab, bolge.bolgeTransform.position, Quaternion.identity);
            }
            else if (bolge.tip == BolgeTipi.RuzgarBolgesi)
            {
                Instantiate(bayrakPrefab, bolge.bolgeTransform.position, Quaternion.identity);
            }
        }
    }
    
    private void Update()
    {
        // Rüzgar yönünü periyodik olarak güncelle
        if (Time.time >= sonRuzgarDegisimZamani + ruzgarDegisimSuresi)
        {
            RuzgarYonunuGuncelle();
            sonRuzgarDegisimZamani = Time.time;
        }
        
        // Eşyaları periyodik olarak spawn et
        if (Time.time >= sonEsyaSpawnZamani + esyaSpawnSuresi)
        {
            EsyaSpawnEt();
            sonEsyaSpawnZamani = Time.time;
        }
    }
    
    private void RuzgarYonunuGuncelle()
    {
        float rastgeleAci = Random.Range(0f, 360f);
        ruzgarYonu = new Vector2(
            Mathf.Cos(rastgeleAci * Mathf.Deg2Rad),
            Mathf.Sin(rastgeleAci * Mathf.Deg2Rad)
        ).normalized;
        
        Debug.Log($"Rüzgar yönü güncellendi: {ruzgarYonu}");
    }
    
    private void EsyaSpawnEt()
    {
        // Rastgele bir özel bölge seç
        if (ozelBolgeler.Count == 0) return;
        
        OzelBolge hedefBolge = ozelBolgeler[Random.Range(0, ozelBolgeler.Count)];
        Vector3 spawnPozisyonu = hedefBolge.bolgeTransform.position;
        
        // Bölge tipine göre eşya spawn et
        GameObject spawnEdilecekEsya = null;
        switch (hedefBolge.tip)
        {
            case BolgeTipi.ParfumBolgesi:
                spawnEdilecekEsya = parfumPrefab;
                break;
            case BolgeTipi.SogukHavaBolgesi:
                spawnEdilecekEsya = sogukHavaPrefab;
                break;
            case BolgeTipi.MaskeBolgesi:
                spawnEdilecekEsya = maskePrefab;
                break;
        }
        
        if (spawnEdilecekEsya != null)
        {
            Instantiate(spawnEdilecekEsya, spawnPozisyonu, Quaternion.identity);
        }
    }
    
    public Vector2 RuzgarYonunuAl()
    {
        return ruzgarYonu;
    }
    
    public float RuzgarGucunuAl()
    {
        return ruzgarGucu;
    }
    
    public bool OyuncuOzelBolgedeMi(Vector3 oyuncuPozisyonu, out BolgeTipi bolgeTipi)
    {
        bolgeTipi = BolgeTipi.ParfumBolgesi; // Varsayılan değer
        
        foreach (var bolge in ozelBolgeler)
        {
            float mesafe = Vector3.Distance(oyuncuPozisyonu, bolge.bolgeTransform.position);
            if (mesafe <= bolge.etkiAlani)
            {
                bolgeTipi = bolge.tip;
                return true;
            }
        }
        
        return false;
    }
    
    private void OnDrawGizmos()
    {
        // Özel bölgeleri Unity editöründe görselleştir
        foreach (var bolge in ozelBolgeler)
        {
            if (bolge.bolgeTransform == null) continue;
            
            switch (bolge.tip)
            {
                case BolgeTipi.ParfumBolgesi:
                    Gizmos.color = Color.magenta;
                    break;
                case BolgeTipi.SogukHavaBolgesi:
                    Gizmos.color = Color.cyan;
                    break;
                case BolgeTipi.RuzgarBolgesi:
                    Gizmos.color = Color.blue;
                    break;
                case BolgeTipi.MaskeBolgesi:
                    Gizmos.color = Color.yellow;
                    break;
            }
            
            Gizmos.DrawWireSphere(bolge.bolgeTransform.position, bolge.etkiAlani);
        }
    }
} 