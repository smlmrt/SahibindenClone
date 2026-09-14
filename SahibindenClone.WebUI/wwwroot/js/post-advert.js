document.getElementById('postAdvertForm').addEventListener('submit', async function(e) {
    e.preventDefault(); // Sayfanın yenilenmesini engeller

    // Formdaki verileri alıp DTO'ya uygun hale getiriyoruz
    const advertData = {
        title: document.getElementById('title').value,
        description: document.getElementById('description').value,
        price: parseFloat(document.getElementById('price').value),
        categoryId: parseInt(document.getElementById('categoryId').value),
        userId: 1 // Test amaçlı 1 numaralı kullanıcıyı gönderiyoruz
    };

    try {
        // API'mizin POST metoduna veriyi gönderiyoruz
        const response = await fetch('/api/adverts', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(advertData)
        });

        const result = await response.json();

        if (response.ok) {
            document.getElementById('resultMessage').style.color = "green";
            document.getElementById('resultMessage').innerText = "İlan başarıyla yayına alındı! Ana sayfaya yönlendiriliyorsunuz...";
            
            // 2 saniye sonra ana sayfaya yönlendir
            setTimeout(() => {
                window.location.href = "index.html";
            }, 2000);
        } else {
            document.getElementById('resultMessage').style.color = "red";
            document.getElementById('resultMessage').innerText = "Hata: İlan eklenemedi.";
        }
    } catch (error) {
        console.error("Hata:", error);
        document.getElementById('resultMessage').style.color = "red";
        document.getElementById('resultMessage').innerText = "Sunucuya ulaşılamadı.";
    }
});