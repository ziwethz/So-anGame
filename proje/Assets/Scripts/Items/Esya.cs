using UnityEngine;
using Mirror;

public class Esya : NetworkBehaviour
{
    public enum EsyaTipi
    {
        Parfum,
        SogukHava,
        Maske
    }
    
    [Header("Eşya Ayarları")]
    [SerializeField] private EsyaTipi tip;
    [SerializeField] private float etkiSuresi = 10f;
    [SerializeField] private float donmeHizi = 100f;
    [SerializeField] private float yukariAsagiHareketHizi = 1f;
    [SerializeField] private float yukariAsagiHareketMesafesi = 0.5f;
    
    private Vector3 baslangicPozisyonu;
    private float zaman;
    
    private void Start()
    {
        baslangicPozisyonu = transform.position;
    }
    
    private void Update()
    {
        if (!isServer) return;
        
        // Eşyayı döndür ve yukarı-aşağı hareket ettir
        zaman += Time.deltaTime;
        transform.Rotate(Vector3.up * donmeHizi * Time.deltaTime);
        
        float yukariAsagiOffset = Mathf.Sin(zaman * yukariAsagiHareketHizi) * yukariAsagiHareketMesafesi;
        transform.position = baslangicPozisyonu + new Vector3(0f, yukariAsagiOffset, 0f);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer) return;
        
        PlayerController oyuncu = other.GetComponent<PlayerController>();
        if (oyuncu != null)
        {
            // Eşyayı oyuncuya ver
            EsyayiKullan(oyuncu);
            
            // Eşyayı yok et
            NetworkServer.Destroy(gameObject);
        }
    }
    
    private void EsyayiKullan(PlayerController oyuncu)
    {
        switch (tip)
        {
            case EsyaTipi.Parfum:
                oyuncu.SendMessage("ParfumKullan", etkiSuresi, SendMessageOptions.DontRequireReceiver);
                break;
            case EsyaTipi.SogukHava:
                oyuncu.SendMessage("SogukHavaKullan", etkiSuresi, SendMessageOptions.DontRequireReceiver);
                break;
            case EsyaTipi.Maske:
                oyuncu.SendMessage("MaskeKullan", etkiSuresi, SendMessageOptions.DontRequireReceiver);
                break;
        }
    }
} 