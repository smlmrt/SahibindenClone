// ═══════════════ BAŞLANGIÇ ═══════════════
document.addEventListener("DOMContentLoaded", () => {
    loadCategories();
    setupImagePreview();
});

// ═══════════════ KATEGORİLERİ API'DEN YÜKLE ═══════════════
async function loadCategories() {
    try {
        const response = await fetch('/api/categories');
        if (!response.ok) throw new Error('Kategoriler yüklenemedi.');

        const categories = await response.json();
        const select = document.getElementById('categoryId');
        select.innerHTML = '<option value="">Kategori Seçin</option>';

        categories.forEach(cat => {
            const option = document.createElement('option');
            option.value = cat.id;
            option.textContent = cat.name;
            select.appendChild(option);

            // Alt kategoriler
            if (cat.subCategories && cat.subCategories.length > 0) {
                cat.subCategories.forEach(sub => {
                    const subOption = document.createElement('option');
                    subOption.value = sub.id;
                    subOption.textContent = `  └ ${sub.name}`;
                    select.appendChild(subOption);
                });
            }
        });
    } catch (error) {
        console.error('Kategori yükleme hatası:', error);
        const select = document.getElementById('categoryId');
        select.innerHTML = '<option value="">Kategoriler yüklenemedi</option>';
    }
}

// ═══════════════ GÖRSEL ÖNİZLEME ═══════════════
function setupImagePreview() {
    const fileInput = document.getElementById('image');
    const preview = document.getElementById('filePreview');
    const wrapper = document.getElementById('fileWrapper');

    fileInput.addEventListener('change', () => {
        const file = fileInput.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (e) => {
                preview.innerHTML = `<img src="${e.target.result}" alt="Önizleme" />`;
                wrapper.querySelector('.file-text').innerHTML = `<strong>${file.name}</strong><br>Değiştirmek için tıklayın`;
            };
            reader.readAsDataURL(file);
        } else {
            preview.innerHTML = '';
            wrapper.querySelector('.file-text').innerHTML = '<strong>Dosya seçmek için tıklayın</strong><br>veya sürükleyip bırakın (JPG, PNG, max 5MB)';
        }
    });
}

// ═══════════════ FORM GÖNDER ═══════════════
document.getElementById('postAdvertForm').addEventListener('submit', async function(e) {
    e.preventDefault();

    const categoryId = document.getElementById('categoryId').value;
    if (!categoryId) {
        showMessage('Lütfen bir kategori seçin.', 'error');
        return;
    }

    const formData = new FormData();
    formData.append("Title", document.getElementById('title').value);
    formData.append("Description", document.getElementById('description').value);
    formData.append("Price", document.getElementById('price').value);
    formData.append("CategoryId", categoryId);
    formData.append("UserId", 1); // Test amaçlı 1 numaralı kullanıcı

    const imageFile = document.getElementById('image').files[0];
    if (imageFile) {
        formData.append("image", imageFile);
    }

    try {
        const response = await fetch('/api/adverts', {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            showMessage("İlan başarıyla yayına alındı! Ana sayfaya yönlendiriliyorsunuz...", "success");
            setTimeout(() => {
                window.location.href = "index.html";
            }, 2000);
        } else {
            showMessage("Hata: İlan eklenemedi.", "error");
        }
    } catch (error) {
        console.error("Hata:", error);
        showMessage("Sunucuya ulaşılamadı.", "error");
    }
});

function showMessage(text, type) {
    const el = document.getElementById('resultMessage');
    el.textContent = text;
    el.className = type; // 'success' veya 'error'
}