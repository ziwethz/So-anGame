using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float hareketHizi = 5f;
    [SerializeField] private float kosmaHizi = 8f;
    [SerializeField] private float ziplamaGucu = 5f;
    
    [Header("Soğan Etkileri")]
    [SerializeField] private float aglamaEslik = 0f;
    [SerializeField] private float maksimumAglama = 100f;
    [SerializeField] private float aglamaArtisHizi = 10f;
    
    private Rigidbody2D rb;
    private bool yerdeMi;
    private float yatayHareket;
    private bool kosuyorMu;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void Update()
    {
        if (!isLocalPlayer) return;
        
        // Hareket girişlerini al
        yatayHareket = Input.GetAxisRaw("Horizontal");
        kosuyorMu = Input.GetKey(KeyCode.LeftShift);
        
        // Zıplama kontrolü
        if (Input.GetButtonDown("Jump") && yerdeMi)
        {
            Zipla();
        }
        
        // Eşya kullanma
        if (Input.GetKeyDown(KeyCode.E))
        {
            EsyaKullan();
        }
    }
    
    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        
        // Hareketi uygula
        float hiz = kosuyorMu ? kosmaHizi : hareketHizi;
        rb.velocity = new Vector2(yatayHareket * hiz, rb.velocity.y);
    }
    
    private void Zipla()
    {
        rb.AddForce(Vector2.up * ziplamaGucu, ForceMode2D.Impulse);
        yerdeMi = false;
    }
    
    private void EsyaKullan()
    {
        // Eşya kullanma mantığı burada uygulanacak
        Debug.Log("Eşya kullanıldı!");
    }
    
    [Command]
    private void CmdAglamaSeviyesiniGuncelle(float yeniSeviye)
    {
        aglamaEslik = Mathf.Clamp(yeniSeviye, 0f, maksimumAglama);
        RpcAglamaEfektiGuncelle(aglamaEslik);
    }
    
    [ClientRpc]
    private void RpcAglamaEfektiGuncelle(float seviye)
    {
        // Ağlama efektlerini güncelle (yüz ifadesi, ses efektleri vb.)
        Debug.Log($"Ağlama seviyesi: {seviye}");
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            yerdeMi = true;
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            yerdeMi = false;
        }
    }
} 