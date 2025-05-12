class Oyun {
    constructor() {
        this.oyuncular = new Map();
        this.soganRuhlari = new Set();
        this.platformlar = [];
        this.tezgahlar = [];
        this.cadirlar = [];
        this.kutular = [];
        this.kalanSure = 90;
        this.oyunAktif = false;
        this.aglamaSeviyesi = 0;
        this.ruzgarYonu = { x: 1, y: 0 };
        this.ruzgarGucu = 2;
        this.hiz = 8;
        this.oyuncuAdi = '';
        this.seciliRenk = 'mavi';
        this.esyalar = {
            parfum: 0,
            sogukHava: 0,
            maske: 0
        };
        
        // Sunucu URL'sini dinamik olarak al
        const serverUrl = window.location.hostname === 'localhost' ? 
            'http://localhost:8080' : 
            window.location.origin;
        
        this.socket = io(serverUrl);
        this.init();
    }
    
    init() {
        // Event listener'ları ekle
        document.getElementById('oyunaBasla').addEventListener('click', () => this.oyunuBaslat());
        document.getElementById('ayarlar').addEventListener('click', () => this.ayarlariAc());
        document.getElementById('cikis').addEventListener('click', () => this.cikisYap());
        document.getElementById('tekrarOyna').addEventListener('click', () => this.tekrarOyna());
        document.getElementById('anaMenuDon').addEventListener('click', () => this.anaMenuyeDon());
        document.getElementById('ayarlariKaydet').addEventListener('click', () => this.ayarlariKaydet());
        document.getElementById('ayarlardanCik').addEventListener('click', () => this.ayarlardanCik());
        
        // Karakter seçimi
        document.querySelectorAll('.karakter').forEach(karakter => {
            karakter.addEventListener('click', () => this.karakterSec(karakter));
        });
        
        // Eşya kullanımı
        document.querySelectorAll('.esya').forEach(esya => {
            esya.addEventListener('click', () => this.esyaKullan(esya.id));
        });
        
        // Klavye kontrollerini ekle
        document.addEventListener('keydown', (e) => this.klavyeKontrol(e));
        
        // Socket.io event'leri
        this.socket.on('oyuncuBaglandi', (data) => this.oyuncuBaglandi(data));
        this.socket.on('oyuncuAyrildi', (data) => this.oyuncuAyrildi(data));
        this.socket.on('oyuncuHareket', (data) => this.oyuncuHareket(data));
        this.socket.on('kokuYayildi', (data) => this.kokuYayildi(data));
        this.socket.on('esyaKullanildi', (data) => this.esyaKullanildi(data));
        this.socket.on('oyunBasladi', () => this.oyunBasladi());
        this.socket.on('oyunBitti', (data) => this.oyunBitti(data));
        
        // Haritayı oluştur
        this.haritayiOlustur();
    }
    
    karakterSec(karakter) {
        document.querySelectorAll('.karakter').forEach(k => k.classList.remove('secili'));
        karakter.classList.add('secili');
        this.seciliRenk = karakter.dataset.renk;
    }
    
    oyunuBaslat() {
        const oyuncuAdi = document.getElementById('oyuncuAdi').value.trim();
        if (!oyuncuAdi) {
            alert('Lütfen bir isim girin!');
            return;
        }
        
        this.oyuncuAdi = oyuncuAdi;
        this.socket.emit('oyunaKatil', {
            isim: oyuncuAdi,
            renk: this.seciliRenk
        });
    }
    
    oyunBasladi() {
        document.getElementById('anaMenu').classList.add('gizli');
        document.getElementById('oyunAlani').classList.remove('gizli');
        this.oyunAktif = true;
        this.ruzgarYonunuGuncelle();
        this.oyunDongusu();
    }
    
    oyunDongusu() {
        if (!this.oyunAktif) return;
        
        this.kalanSure--;
        document.getElementById('kalanSure').textContent = `Kalan Süre: ${this.kalanSure}`;
        
        if (this.kalanSure <= 0) {
            this.socket.emit('oyunBitti');
            return;
        }
        
        // Rüzgar yönünü periyodik olarak güncelle
        if (this.kalanSure % 10 === 0) {
            this.ruzgarYonunuGuncelle();
        }
        
        requestAnimationFrame(() => this.oyunDongusu());
    }
    
    haritayiOlustur() {
        const harita = document.getElementById('harita');
        const haritaGenislik = harita.clientWidth;
        const haritaYukseklik = harita.clientHeight;
        
        // Platformları oluştur
        for (let i = 0; i < 8; i++) {
            const platform = document.createElement('div');
            platform.className = 'platform';
            platform.style.width = '300px';
            platform.style.height = '30px';
            platform.style.left = `${Math.random() * (haritaGenislik - 300)}px`;
            platform.style.top = `${100 + i * 100}px`;
            harita.appendChild(platform);
            this.platformlar.push(platform);
        }
        
        // Tezgahları yerleştir
        this.platformlar.forEach(platform => {
            const tezgah = document.createElement('div');
            tezgah.className = 'tezgah';
            tezgah.style.width = '150px';
            tezgah.style.height = '80px';
            tezgah.style.left = `${parseInt(platform.style.left) + 75}px`;
            tezgah.style.top = `${parseInt(platform.style.top) - 80}px`;
            harita.appendChild(tezgah);
            this.tezgahlar.push(tezgah);
        });
        
        // Çadırları yerleştir
        for (let i = 0; i < 5; i++) {
            const cadir = document.createElement('div');
            cadir.className = 'cadir';
            cadir.style.width = '200px';
            cadir.style.height = '150px';
            cadir.style.left = `${Math.random() * (haritaGenislik - 200)}px`;
            cadir.style.top = '50px';
            harita.appendChild(cadir);
            this.cadirlar.push(cadir);
        }
        
        // Kutuları yerleştir
        this.platformlar.forEach(platform => {
            for (let i = 0; i < 3; i++) {
                const kutu = document.createElement('div');
                kutu.className = 'kutu';
                kutu.style.width = '50px';
                kutu.style.height = '50px';
                kutu.style.left = `${parseInt(platform.style.left) + 50 + i * 80}px`;
                kutu.style.top = `${parseInt(platform.style.top) - 50}px`;
                harita.appendChild(kutu);
                this.kutular.push(kutu);
            }
        });
    }
    
    klavyeKontrol(e) {
        if (!this.oyunAktif) return;
        
        let yeniX = this.oyuncular.get(this.socket.id)?.x || 100;
        let yeniY = this.oyuncular.get(this.socket.id)?.y || 100;
        
        switch(e.key) {
            case 'ArrowLeft':
                yeniX -= this.hiz;
                break;
            case 'ArrowRight':
                yeniX += this.hiz;
                break;
            case 'ArrowUp':
                yeniY -= this.hiz;
                break;
            case 'ArrowDown':
                yeniY += this.hiz;
                break;
            case ' ':
                if (this.soganRuhlari.has(this.socket.id)) {
                    this.socket.emit('kokuYay', { x: yeniX, y: yeniY });
                }
                break;
            case 'e':
                this.esyaKullan();
                break;
        }
        
        // Harita sınırlarını kontrol et
        const harita = document.getElementById('harita');
        const haritaGenislik = harita.clientWidth;
        const haritaYukseklik = harita.clientHeight;
        
        yeniX = Math.max(0, Math.min(yeniX, haritaGenislik - 50));
        yeniY = Math.max(0, Math.min(yeniY, haritaYukseklik - 50));
        
        // Engellerle çarpışma kontrolü
        if (!this.engelKontrol(yeniX, yeniY)) {
            this.socket.emit('hareket', { x: yeniX, y: yeniY });
        }
    }
    
    engelKontrol(x, y) {
        const oyuncuBoyut = 50;
        const oyuncuRect = {
            left: x,
            right: x + oyuncuBoyut,
            top: y,
            bottom: y + oyuncuBoyut
        };
        
        // Platformlarla çarpışma kontrolü
        for (const platform of this.platformlar) {
            const rect = platform.getBoundingClientRect();
            if (this.carpismaKontrol(oyuncuRect, rect)) {
                return true;
            }
        }
        
        // Tezgahlarla çarpışma kontrolü
        for (const tezgah of this.tezgahlar) {
            const rect = tezgah.getBoundingClientRect();
            if (this.carpismaKontrol(oyuncuRect, rect)) {
                return true;
            }
        }
        
        return false;
    }
    
    carpismaKontrol(rect1, rect2) {
        return !(rect1.right < rect2.left || 
                rect1.left > rect2.right || 
                rect1.bottom < rect2.top || 
                rect1.top > rect2.bottom);
    }
    
    oyuncuBaglandi(data) {
        const oyuncu = document.createElement('div');
        oyuncu.className = `oyuncu ${data.soganRuhu ? 'soganRuhu' : 'normalOyuncu'}`;
        if (data.id === this.socket.id) {
            oyuncu.classList.add('benimKarakterim');
        }
        oyuncu.style.left = `${data.x}px`;
        oyuncu.style.top = `${data.y}px`;
        
        const oyuncuAdi = document.createElement('div');
        oyuncuAdi.className = 'oyuncuAdi';
        oyuncuAdi.textContent = data.isim;
        oyuncu.appendChild(oyuncuAdi);
        
        document.getElementById('harita').appendChild(oyuncu);
        
        this.oyuncular.set(data.id, {
            element: oyuncu,
            x: data.x,
            y: data.y,
            isim: data.isim,
            aglamaSeviyesi: 0
        });
        
        if (data.soganRuhu) {
            this.soganRuhlari.add(data.id);
        }
        
        this.oyuncuSayisiniGuncelle();
    }
    
    oyuncuAyrildi(data) {
        const oyuncu = this.oyuncular.get(data.id);
        if (oyuncu) {
            oyuncu.element.remove();
            this.oyuncular.delete(data.id);
            this.soganRuhlari.delete(data.id);
            this.oyuncuSayisiniGuncelle();
        }
    }
    
    oyuncuHareket(data) {
        const oyuncu = this.oyuncular.get(data.id);
        if (oyuncu) {
            oyuncu.x = data.x;
            oyuncu.y = data.y;
            oyuncu.element.style.left = `${data.x}px`;
            oyuncu.element.style.top = `${data.y}px`;
        }
    }
    
    kokuYayildi(data) {
        const kokuBulutu = document.createElement('div');
        kokuBulutu.className = 'kokuBulutu';
        kokuBulutu.style.left = `${data.x}px`;
        kokuBulutu.style.top = `${data.y}px`;
        document.getElementById('harita').appendChild(kokuBulutu);
        
        // Koku bulutunu rüzgar yönünde hareket ettir
        const kokuHareketi = setInterval(() => {
            const x = parseInt(kokuBulutu.style.left);
            const y = parseInt(kokuBulutu.style.top);
            
            kokuBulutu.style.left = `${x + this.ruzgarYonu.x * this.ruzgarGucu}px`;
            kokuBulutu.style.top = `${y + this.ruzgarYonu.y * this.ruzgarGucu}px`;
            
            // Diğer oyuncularla çarpışma kontrolü
            this.oyuncular.forEach((hedef, id) => {
                if (id !== data.id && this.carpismaKontrol(kokuBulutu, hedef.element)) {
                    hedef.aglamaSeviyesi += 10;
                    this.aglamaSeviyesiniGuncelle(hedef);
                }
            });
        }, 50);
        
        // 2 saniye sonra koku bulutunu kaldır
        setTimeout(() => {
            clearInterval(kokuHareketi);
            kokuBulutu.remove();
        }, 2000);
    }
    
    esyaKullan(esyaId) {
        if (this.esyalar[esyaId] > 0) {
            this.socket.emit('esyaKullan', { esyaId });
        }
    }
    
    esyaKullanildi(data) {
        const oyuncu = this.oyuncular.get(data.id);
        if (oyuncu) {
            switch(data.esyaId) {
                case 'parfum':
                    oyuncu.aglamaSeviyesi = Math.max(0, oyuncu.aglamaSeviyesi - 30);
                    break;
                case 'sogukHava':
                    // Soğuk hava efekti
                    break;
                case 'maske':
                    // Maske efekti
                    break;
            }
            this.aglamaSeviyesiniGuncelle(oyuncu);
        }
    }
    
    aglamaSeviyesiniGuncelle(oyuncu) {
        if (oyuncu.id === this.socket.id) {
            document.getElementById('aglamaSeviyesi').style.width = `${oyuncu.aglamaSeviyesi}%`;
        }
        
        if (oyuncu.aglamaSeviyesi >= 100) {
            this.socket.emit('soganRuhuOldu', { id: oyuncu.id });
        }
    }
    
    oyuncuSayisiniGuncelle() {
        const normalOyuncu = this.oyuncular.size - this.soganRuhlari.size;
        const soganRuhu = this.soganRuhlari.size;
        document.getElementById('oyuncuSayisi').textContent = `Normal: ${normalOyuncu} | Soğan: ${soganRuhu}`;
    }
    
    ruzgarYonunuGuncelle() {
        const aci = Math.random() * Math.PI * 2;
        this.ruzgarYonu = {
            x: Math.cos(aci),
            y: Math.sin(aci)
        };
    }
    
    oyunBitti(data) {
        this.oyunAktif = false;
        document.getElementById('oyunAlani').classList.add('gizli');
        document.getElementById('oyunSonu').classList.remove('gizli');
        
        document.getElementById('kazanan').textContent = `Kazanan: ${data.kazanan}`;
        document.getElementById('skor').textContent = `Skor: ${data.skor}`;
        
        // Sıralamayı göster
        const siralama = document.getElementById('siralama');
        siralama.innerHTML = '<h3>Sıralama:</h3>';
        data.siralama.forEach((oyuncu, index) => {
            siralama.innerHTML += `<p>${index + 1}. ${oyuncu.isim} - ${oyuncu.skor}</p>`;
        });
    }
    
    ayarlariAc() {
        document.getElementById('anaMenu').classList.add('gizli');
        document.getElementById('ayarlarMenu').classList.remove('gizli');
    }
    
    ayarlariKaydet() {
        const sesSeviyesi = document.getElementById('sesSeviyesi').value;
        const muzikSeviyesi = document.getElementById('muzikSeviyesi').value;
        
        // Ayarları kaydet
        localStorage.setItem('sesSeviyesi', sesSeviyesi);
        localStorage.setItem('muzikSeviyesi', muzikSeviyesi);
        
        this.ayarlardanCik();
    }
    
    ayarlardanCik() {
        document.getElementById('ayarlarMenu').classList.add('gizli');
        document.getElementById('anaMenu').classList.remove('gizli');
    }
    
    tekrarOyna() {
        location.reload();
    }
    
    anaMenuyeDon() {
        document.getElementById('oyunSonu').classList.add('gizli');
        document.getElementById('anaMenu').classList.remove('gizli');
    }
    
    cikisYap() {
        window.close();
    }
}

// Oyunu başlat
const oyun = new Oyun(); 