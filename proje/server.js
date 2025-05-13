const express = require('express');
const app = express();
const http = require('http').createServer(app);
const io = require('socket.io')(http, {
    cors: {
        origin: "*",
        methods: ["GET", "POST"]
    }
});

// === Statik dosyaları doğru şekilde sun ===
// Bu satırda sadece tek bir 'proje' klasörünü kullanmalıyız
app.use(express.static(__dirname + '/proje'));

// === Ana sayfayı doğru şekilde göster ===
// Bu satırda da aynı şekilde doğru yol kullanılmalı
app.get('/', (req, res) => {
    res.sendFile(__dirname + '/proje/index.html');
});

// === Socket.IO bağlantısı ===
io.on('connection', (socket) => {
    console.log('Bir kullanıcı bağlandı');

    socket.on('disconnect', () => {
        console.log('Bir kullanıcı ayrıldı');
    });
});

// === Sunucuyu başlat ===
const PORT = process.env.PORT || 8080;
http.listen(PORT, '8.8.8.8', () => {
    console.log(`Sunucu ${PORT} portunda çalışıyor`);
});
