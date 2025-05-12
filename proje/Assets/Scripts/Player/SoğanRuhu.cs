using UnityEngine;
using Mirror;
using System.Collections;

public class SoğanRuhu : NetworkBehaviour
{
    [Header("Koku Ayarları")]
    [SerializeField] private float kokuYayilmaHizi = 2f;
    [SerializeField] private float kokuEtkiAlani = 3f;
    [SerializeField] private float kokuOlusturmaSuresi = 0.5f;
    
    [Header("Görsel Efektler")]
    [SerializeField] private GameObject kokuBulutuPrefab;
    [SerializeField] private ParticleSystem kokuParticle;
    
    private bool kokuYayiyor;
    private float sonKokuZamani;
    
    private void Start()
    {
        if (!isLocalPlayer) return;
        
        // Soğan ruhu görünümünü aktifleştir
        GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.8f, 0.2f, 1f);
    }
    
    private void Update()
    {
        if (!isLocalPlayer) return;
        
        // Koku yayma kontrolü
        if (Input.GetKey(KeyCode.Space) && Time.time >= sonKokuZamani + kokuOlusturmaSuresi)
        {
            CmdKokuYay();
            sonKokuZamani = Time.time;
        }
    }
    
    [Command]
    private void CmdKokuYay()
    {
        // Koku bulutunu oluştur
        GameObject kokuBulutu = Instantiate(kokuBulutuPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(kokuBulutu);
        
        // Koku efektlerini başlat
        RpcKokuEfektleriniBaslat();
    }
    
    [ClientRpc]
    private void RpcKokuEfektleriniBaslat()
    {
        if (kokuParticle != null)
        {
            kokuParticle.Play();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer) return;
        
        // Diğer oyuncularla temas kontrolü
        PlayerController oyuncu = other.GetComponent<PlayerController>();
        if (oyuncu != null)
        {
            // Oyuncunun ağlama seviyesini artır
            oyuncu.SendMessage("AglamaSeviyesiniArtir", SendMessageOptions.DontRequireReceiver);
        }
    }
} 