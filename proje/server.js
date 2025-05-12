const express = require('express');
const app = express();
const http = require('http').createServer(app);
const io = require('socket.io')(http, {
    cors: {
        origin: "*",
        methods: ["GET", "POST"]
    }
});

// === Statik dosyaları sun ===
app.use(express.static(__dirname + '/proje'));

// === Ana sayfayı göster ===
app.get('/', (req, res) => {
    res.sendFile(__dirname + '/proje/index.html');
});

// === Socket.IO bağlantısı ===
io.on('connection', (socket) => {
    console.log('Bir kullanıcı bağlandı');

    // Buraya oyunla ilgili socket olaylarını ekleyebilirsin
    socket.on('disconnect', () => {
        console.log('Bir kullanıcı ayrıldı');
    });
});

// === Sunucuyu başlat ===
const PORT = process.env.PORT || 8080;
http.listen(PORT, '0.0.0.0', () => {
    console.log(`Sunucu ${PORT} portunda çalışıyor`);
});
