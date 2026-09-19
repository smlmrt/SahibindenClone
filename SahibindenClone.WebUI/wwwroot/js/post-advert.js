document.getElementById('postAdvertForm').addEventListener('submit', async function(e) {
    e.preventDefault(); // Sayfanın yenilenmesini engeller

    // Verileri FormData objesine ekliyoruz (Görsel yükleme için zorunlu format)
    const formData = new FormData();
    formData.append("Title", document.getElementById('title').value);
    formData.append("Description", document.getElementById('description').value);
    formData.append("Price", document.getElementById('price').value);
    formData.append("CategoryId", document.getElementById('categoryId').value);
    formData.append("UserId", 1); // Test amaçlı 1 numaralı kullanıcı

    // Görsel dosyasını alıp FormData'ya ekliyoruz
    const imageFile = document.getElementById('image').files[0];
    if (imageFile) {
        formData.append("image", imageFile);
    }

    try {
        // Fetch API ile FormData'yı POST ediyoruz. (Content-Type otomatik olarak ayarlanır)
        const response = await fetch('/api/adverts', {
            method: 'POST',
            body: formData
        });

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