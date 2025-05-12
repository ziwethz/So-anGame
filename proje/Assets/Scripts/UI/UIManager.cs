using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class UIManager : MonoBehaviour
{
    [Header("Ana Menü")]
    [SerializeField] private GameObject anaMenuPanel;
    [SerializeField] private Button oyunaBaslaButton;
    [SerializeField] private Button ayarlarButton;
    [SerializeField] private Button cikisButton;
    
    [Header("Oyun İçi UI")]
    [SerializeField] private GameObject oyunIciPanel;
    [SerializeField] private Slider aglamaBar;
    [SerializeField] private TextMeshProUGUI kalanSureText;
    [SerializeField] private TextMeshProUGUI oyuncuSayisiText;
    [SerializeField] private Image parfumIcon;
    [SerializeField] private Image sogukHavaIcon;
    [SerializeField] private Image maskeIcon;
    
    [Header("Oyun Sonu")]
    [SerializeField] private GameObject oyunSonuPanel;
    [SerializeField] private TextMeshProUGUI kazananText;
    [SerializeField] private TextMeshProUGUI skorText;
    [SerializeField] private Button tekrarOynaButton;
    [SerializeField] private Button anaMenuButton;
    
    private void Start()
    {
        // Buton dinleyicilerini ekle
        oyunaBaslaButton.onClick.AddListener(OyunaBasla);
        ayarlarButton.onClick.AddListener(AyarlariAc);
        cikisButton.onClick.AddListener(CikisYap);
        tekrarOynaButton.onClick.AddListener(TekrarOyna);
        anaMenuButton.onClick.AddListener(AnaMenuyeDon);
        
        // Başlangıçta sadece ana menüyü göster
        anaMenuPanel.SetActive(true);
        oyunIciPanel.SetActive(false);
        oyunSonuPanel.SetActive(false);
    }
    
    private void OyunaBasla()
    {
        anaMenuPanel.SetActive(false);
        oyunIciPanel.SetActive(true);
        // NetworkManager üzerinden oyuna bağlanma işlemi
    }
    
    private void AyarlariAc()
    {
        // Ayarlar menüsünü aç
        Debug.Log("Ayarlar açılıyor...");
    }
    
    private void CikisYap()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void KalanSureyiGuncelle(float sure)
    {
        kalanSureText.text = $"Kalan Süre: {Mathf.CeilToInt(sure)}";
    }
    
    public void AglamaBariniGuncelle(float deger)
    {
        aglamaBar.value = deger;
    }
    
    public void OyuncuSayisiniGuncelle(int normalOyuncu, int soganRuhu)
    {
        oyuncuSayisiText.text = $"Normal: {normalOyuncu} | Soğan: {soganRuhu}";
    }
    
    public void EsyaDurumunuGuncelle(string esyaTuru, bool aktif)
    {
        switch (esyaTuru)
        {
            case "Parfum":
                parfumIcon.gameObject.SetActive(aktif);
                break;
            case "SogukHava":
                sogukHavaIcon.gameObject.SetActive(aktif);
                break;
            case "Maske":
                maskeIcon.gameObject.SetActive(aktif);
                break;
        }
    }
    
    public void OyunSonuGoster(string kazanan, int skor)
    {
        oyunIciPanel.SetActive(false);
        oyunSonuPanel.SetActive(true);
        
        kazananText.text = $"Kazanan: {kazanan}";
        skorText.text = $"Skor: {skor}";
    }
    
    private void TekrarOyna()
    {
        oyunSonuPanel.SetActive(false);
        oyunIciPanel.SetActive(true);
        // Yeni oyun başlatma işlemi
    }
    
    private void AnaMenuyeDon()
    {
        oyunSonuPanel.SetActive(false);
        anaMenuPanel.SetActive(true);
    }
} 