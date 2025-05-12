const express = require('express');
const app = express();
const http = require('http').createServer(app);
const io = require('socket.io')(http, {
    cors: {
        origin: "*",
        methods: ["GET", "POST"]
    }
});
const path = require('path');

// Statik dosyaları serve et
app.use(express.static(path.join(__dirname, 'public')));

// Ana sayfa
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'index.html'));
});

// Oyun durumu
const oyunDurumu = {
    oyuncular: new Map(),
    soganRuhlari: new Set(),
    oyunAktif: false,
    kalanSure: 90,
    skorlar: new Map()
};

// Socket.io bağlantıları
io.on('connection', (socket) => {
    console.log('Yeni oyuncu bağlandı:', socket.id);
    
    // Oyuncu oyuna katıldığında
    socket.on('oyunaKatil', (data) => {
        const oyuncu = {
            id: socket.id,
            isim: data.isim,
            renk: data.renk,
            x: Math.random() * 800,
            y: Math.random() * 600,
            soganRuhu: false,
            skor: 0
        };
        
        oyunDurumu.oyuncular.set(socket.id, oyuncu);
        
        // İlk oyuncu soğan ruhu olsun
        if (oyunDurumu.oyuncular.size === 1) {
            oyuncu.soganRuhu = true;
            oyunDurumu.soganRuhlari.add(socket.id);
        }
        
        // Yeni oyuncuyu herkese bildir
        io.emit('oyuncuBaglandi', oyuncu);
        
        // Mevcut oyuncuları yeni oyuncuya bildir
        oyunDurumu.oyuncular.forEach((mevcutOyuncu) => {
            if (mevcutOyuncu.id !== socket.id) {
                socket.emit('oyuncuBaglandi', mevcutOyuncu);
            }
        });
        
        // Yeterli oyuncu varsa oyunu başlat
        if (oyunDurumu.oyuncular.size >= 2 && !oyunDurumu.oyunAktif) {
            oyunDurumu.oyunAktif = true;
            oyunDurumu.kalanSure = 90;
            io.emit('oyunBasladi');
            
            // Oyun süresini başlat
            const oyunZamanlayici = setInterval(() => {
                oyunDurumu.kalanSure--;
                
                if (oyunDurumu.kalanSure <= 0) {
                    clearInterval(oyunZamanlayici);
                    oyunuBitir();
                }
            }, 1000);
        }
    });
    
    // Oyuncu hareket ettiğinde
    socket.on('hareket', (data) => {
        const oyuncu = oyunDurumu.oyuncular.get(socket.id);
        if (oyuncu) {
            oyuncu.x = data.x;
            oyuncu.y = data.y;
            socket.broadcast.emit('oyuncuHareket', {
                id: socket.id,
                x: data.x,
                y: data.y
            });
        }
    });
    
    // Koku yayıldığında
    socket.on('kokuYay', (data) => {
        const oyuncu = oyunDurumu.oyuncular.get(socket.id);
        if (oyuncu && oyuncu.soganRuhu) {
            io.emit('kokuYayildi', {
                id: socket.id,
                x: data.x,
                y: data.y
            });
        }
    });
    
    // Eşya kullanıldığında
    socket.on('esyaKullan', (data) => {
        io.emit('esyaKullanildi', {
            id: socket.id,
            esyaId: data.esyaId
        });
    });
    
    // Oyuncu soğan ruhu olduğunda
    socket.on('soganRuhuOldu', (data) => {
        const oyuncu = oyunDurumu.oyuncular.get(data.id);
        if (oyuncu && !oyuncu.soganRuhu) {
            oyuncu.soganRuhu = true;
            oyunDurumu.soganRuhlari.add(data.id);
            io.emit('oyuncuSoganRuhuOldu', {
                id: data.id
            });
            
            // Soğan ruhu olan oyuncunun skorunu artır
            const soganRuhu = oyunDurumu.oyuncular.get(socket.id);
            if (soganRuhu) {
                soganRuhu.skor += 10;
                oyunDurumu.skorlar.set(socket.id, soganRuhu.skor);
            }
        }
    });
    
    // Oyuncu ayrıldığında
    socket.on('disconnect', () => {
        console.log('Oyuncu ayrıldı:', socket.id);
        
        const oyuncu = oyunDurumu.oyuncular.get(socket.id);
        if (oyuncu) {
            oyunDurumu.oyuncular.delete(socket.id);
            oyunDurumu.soganRuhlari.delete(socket.id);
            oyunDurumu.skorlar.delete(socket.id);
            
            io.emit('oyuncuAyrildi', {
                id: socket.id
            });
            
            // Yeterli oyuncu kalmadıysa oyunu bitir
            if (oyunDurumu.oyuncular.size < 2) {
                oyunuBitir();
            }
        }
    });
});

// Oyunu bitir
function oyunuBitir() {
    oyunDurumu.oyunAktif = false;
    
    // Kazananı belirle
    let kazanan = '';
    let enYuksekSkor = -1;
    
    oyunDurumu.oyuncular.forEach((oyuncu) => {
        if (oyuncu.skor > enYuksekSkor) {
            enYuksekSkor = oyuncu.skor;
            kazanan = oyuncu.isim;
        }
    });
    
    // Sıralamayı oluştur
    const siralama = Array.from(oyunDurumu.oyuncular.values())
        .sort((a, b) => b.skor - a.skor)
        .map(oyuncu => ({
            isim: oyuncu.isim,
            skor: oyuncu.skor
        }));
    
    // Sonuçları gönder
    io.emit('oyunBitti', {
        kazanan: kazanan,
        skor: enYuksekSkor,
        siralama: siralama
    });
    
    // Oyun durumunu sıfırla
    oyunDurumu.oyuncular.clear();
    oyunDurumu.soganRuhlari.clear();
    oyunDurumu.skorlar.clear();
}

// Sunucuyu başlat
const PORT = process.env.PORT || 8080;
http.listen(PORT, '0.0.0.0', () => {
    console.log(`Sunucu ${PORT} portunda çalışıyor`);
});
