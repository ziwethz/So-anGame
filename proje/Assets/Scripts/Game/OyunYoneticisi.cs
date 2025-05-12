using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;

public class OyunYoneticisi : NetworkBehaviour
{
    [Header("Oyun Ayarları")]
    [SerializeField] private float macSuresi = 90f;
    [SerializeField] private int minimumOyuncuSayisi = 4;
    [SerializeField] private int maksimumOyuncuSayisi = 8;
    
    [Header("Prefablar")]
    [SerializeField] private GameObject oyuncuPrefab;
    [SerializeField] private GameObject soğanRuhuPrefab;
    
    private float kalanSure;
    private bool oyunBasladi;
    private List<PlayerController> aktifOyuncular = new List<PlayerController>();
    private List<PlayerController> soğanRuhlari = new List<PlayerController>();
    
    public override void OnStartServer()
    {
        NetworkManager.singleton.OnServerConnect += OyuncuBaglandi;
        NetworkManager.singleton.OnServerDisconnect += OyuncuAyrildi;
    }
    
    private void OyuncuBaglandi(NetworkConnection conn)
    {
        // Yeni oyuncuyu oluştur
        GameObject oyuncuObj = Instantiate(oyuncuPrefab);
        NetworkServer.AddPlayerForConnection(conn, oyuncuObj);
        
        // Oyuncu listesine ekle
        PlayerController yeniOyuncu = oyuncuObj.GetComponent<PlayerController>();
        aktifOyuncular.Add(yeniOyuncu);
        
        // Yeterli oyuncu varsa oyunu başlat
        if (aktifOyuncular.Count >= minimumOyuncuSayisi && !oyunBasladi)
        {
            OyunuBaslat();
        }
    }
    
    private void OyuncuAyrildi(NetworkConnection conn)
    {
        // Oyuncuyu listeden çıkar
        PlayerController ayrilanOyuncu = aktifOyuncular.Find(p => p.connectionToClient == conn);
        if (ayrilanOyuncu != null)
        {
            aktifOyuncular.Remove(ayrilanOyuncu);
            soğanRuhlari.Remove(ayrilanOyuncu);
        }
    }
    
    [Server]
    private void OyunuBaslat()
    {
        oyunBasladi = true;
        kalanSure = macSuresi;
        
        // Rastgele bir soğan ruhu seç
        int soğanIndex = Random.Range(0, aktifOyuncular.Count);
        PlayerController soğanRuhu = aktifOyuncular[soğanIndex];
        soğanRuhlari.Add(soğanRuhu);
        
        // Tüm oyunculara oyunun başladığını bildir
        RpcOyunBasladi();
    }
    
    [ClientRpc]
    private void RpcOyunBasladi()
    {
        Debug.Log("Oyun başladı!");
        // UI güncellemeleri ve diğer başlangıç işlemleri
    }
    
    private void Update()
    {
        if (!isServer || !oyunBasladi) return;
        
        kalanSure -= Time.deltaTime;
        
        // Oyun süresi bitti mi kontrol et
        if (kalanSure <= 0)
        {
            OyunuBitir();
        }
        
        // Kazanan var mı kontrol et
        if (soğanRuhlari.Count == aktifOyuncular.Count - 1)
        {
            PlayerController kazanan = aktifOyuncular.FirstOrDefault(p => !soğanRuhlari.Contains(p));
            if (kazanan != null)
            {
                OyunuBitir(kazanan);
            }
        }
    }
    
    [Server]
    private void OyunuBitir(PlayerController kazanan = null)
    {
        oyunBasladi = false;
        
        if (kazanan != null)
        {
            RpcOyunBitti(kazanan.netIdentity);
        }
        else
        {
            RpcOyunBitti(null);
        }
    }
    
    [ClientRpc]
    private void RpcOyunBitti(NetworkIdentity kazanan)
    {
        if (kazanan != null)
        {
            Debug.Log($"Oyun bitti! Kazanan: {kazanan.name}");
        }
        else
        {
            Debug.Log("Oyun bitti! Süre doldu!");
        }
        
        // UI güncellemeleri ve diğer bitiş işlemleri
    }
} 