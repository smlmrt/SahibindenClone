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

// ═══════════════ CLIENT-SIDE DOSYA DOĞRULAMA ═══════════════
const ALLOWED_EXTENSIONS = ['.jpg', '.jpeg', '.png', '.webp'];
const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB

function validateImageFile(file) {
    if (!file) return null;

    const ext = '.' + file.name.split('.').pop().toLowerCase();
    if (!ALLOWED_EXTENSIONS.includes(ext)) {
        return `Desteklenmeyen dosya türü: ${ext}. İzin verilen: ${ALLOWED_EXTENSIONS.join(', ')}`;
    }

    if (file.size > MAX_FILE_SIZE) {
        return `Görsel boyutu en fazla 5MB olabilir. Yüklenen: ${(file.size / (1024 * 1024)).toFixed(1)}MB`;
    }

    return null;
}

// ═══════════════ FORM GÖNDER ═══════════════
document.getElementById('postAdvertForm').addEventListener('submit', async function(e) {
    e.preventDefault();

    // Client-side validasyonlar
    const title = document.getElementById('title').value.trim();
    const description = document.getElementById('description').value.trim();
    const price = document.getElementById('price').value;
    const categoryId = document.getElementById('categoryId').value;

    if (title.length < 3) { showMessage('Başlık en az 3 karakter olmalıdır.', 'error'); return; }
    if (title.length > 150) { showMessage('Başlık en fazla 150 karakter olabilir.', 'error'); return; }
    if (description.length < 10) { showMessage('Açıklama en az 10 karakter olmalıdır.', 'error'); return; }
    if (!price || parseFloat(price) < 0) { showMessage('Geçerli bir fiyat giriniz.', 'error'); return; }
    if (!categoryId) { showMessage('Lütfen bir kategori seçin.', 'error'); return; }

    // Görsel validasyonu
    const imageFile = document.getElementById('image').files[0];
    const imageError = validateImageFile(imageFile);
    if (imageError) { showMessage(imageError, 'error'); return; }

    const formData = new FormData();
    formData.append("Title", title);
    formData.append("Description", description);
    formData.append("Price", price);
    formData.append("CategoryId", categoryId);
    formData.append("UserId", 1); // Test amaçlı 1 numaralı kullanıcı

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
            const errorData = await response.json().catch(() => null);
            const errorMsg = errorData?.errors?.join('\n') || "Hata: İlan eklenemedi.";
            showMessage(errorMsg, "error");
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