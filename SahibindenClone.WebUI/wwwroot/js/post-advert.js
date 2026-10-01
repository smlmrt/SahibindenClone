// ═══════════════ BAŞLANGIÇ ═══════════════
document.addEventListener("DOMContentLoaded", () => {
    loadCategories();
    loadCities(); // ŞEHİRLER YÜKLENİYOR
    setupImagePreview();
});

// ═══════════════ ŞEHİRLERİ API'DEN YÜKLE ═══════════════
async function loadCities() {
    try {
        const response = await fetch('/api/cities');
        if (!response.ok) throw new Error('Şehirler yüklenemedi.');

        const cities = await response.json();
        const select = document.getElementById('cityId');

        cities.forEach(city => {
            select.add(new Option(city.name, city.id));
        });
    } catch (error) {
        console.error('Şehir yükleme hatası:', error);
        const select = document.getElementById('cityId');
        if (select) select.innerHTML = '<option value="">Şehirler yüklenemedi</option>';
    }
}

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
        if (select) select.innerHTML = '<option value="">Kategoriler yüklenemedi</option>';
    }
}

// ═══════════════ GÖRSEL ÖNİZLEME ═══════════════
function setupImagePreview() {
    const fileInput = document.getElementById('image');
    const preview = document.getElementById('filePreview');
    const wrapper = document.getElementById('fileWrapper');

    if (!fileInput) return;

    fileInput.addEventListener('change', () => {
        const files = fileInput.files;
        if (files && files.length > 0) {
            let html = '<div style="display: flex; gap: 8px; flex-wrap: wrap;">';
            for (let i = 0; i < files.length; i++) {
                const file = files[i];
                html += `<img src="${URL.createObjectURL(file)}" alt="Önizleme" style="max-width: 100px; max-height: 100px; border-radius: 4px; object-fit: cover;" />`;
            }
            html += '</div>';
            preview.innerHTML = html;
            wrapper.querySelector('.file-text').innerHTML = `<strong>${files.length} görsel seçildi</strong><br>Değiştirmek için tıklayın`;
        } else {
            preview.innerHTML = '';
            wrapper.querySelector('.file-text').innerHTML = '<strong>Dosya seçmek için tıklayın</strong><br>veya sürükleyip bırakın (JPG, PNG, max 5MB, Çoklu seçim yapılabilir)';
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
const form = document.getElementById('postAdvertForm');
if (form) {
    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        // 1. GİRİŞ KONTROLÜ VE KULLANICI BİLGİLERİNİ ALMA
        const userId = localStorage.getItem('userId');
        const token = localStorage.getItem('token');

        if (!userId || !token) {
            showMessage("İlan verebilmek için giriş yapmalısınız. Yönlendiriliyorsunuz...", "error");
            setTimeout(() => { window.location.href = "login.html"; }, 2000);
            return;
        }

        // Client-side validasyonlar
        const title = document.getElementById('title').value.trim();
        const description = document.getElementById('description').value.trim();
        const price = document.getElementById('price').value;
        const categoryId = document.getElementById('categoryId').value;
        const cityId = document.getElementById('cityId').value; // ŞEHİR VALIDASYONU

        if (title.length < 3) { showMessage('Başlık en az 3 karakter olmalıdır.', 'error'); return; }
        if (title.length > 150) { showMessage('Başlık en fazla 150 karakter olabilir.', 'error'); return; }
        if (description.length < 10) { showMessage('Açıklama en az 10 karakter olmalıdır.', 'error'); return; }
        if (!price || parseFloat(price) < 0) { showMessage('Geçerli bir fiyat giriniz.', 'error'); return; }
        if (!categoryId) { showMessage('Lütfen bir kategori seçin.', 'error'); return; }
        if (!cityId) { showMessage('Lütfen bir şehir seçin.', 'error'); return; } // ŞEHİR UYARISI

        // Görsel validasyonu
        const imageFiles = document.getElementById('image').files;
        for (let i = 0; i < imageFiles.length; i++) {
            const imageError = validateImageFile(imageFiles[i]);
            if (imageError) { showMessage(imageError, 'error'); return; }
        }

        const formData = new FormData();
        formData.append("Title", title);
        formData.append("Description", description);
        formData.append("Price", price);
        formData.append("Brand", document.getElementById('brand').value.trim());
        formData.append("Model", document.getElementById('model').value.trim());
        formData.append("CategoryId", categoryId);
        formData.append("CityId", cityId); // ŞEHİR VERİSİ EKLENDİ
        formData.append("UserId", userId);

        for (let i = 0; i < imageFiles.length; i++) {
            formData.append("images", imageFiles[i]);
        }

        try {
            const response = await fetch('/api/adverts', {
                method: 'POST',
                body: formData,
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                showMessage("İlanınız onaya gönderildi. Onaylandıktan sonra yayına alınacak.", "success");
                setTimeout(() => {
                    window.location.href = "index.html";
                }, 2000);
            } else {
                const errorData = await response.json().catch(() => null);
                let errorMsg = "Hata: İlan eklenemedi.";
                if (errorData?.errors) {
                    if (Array.isArray(errorData.errors)) {
                        errorMsg = errorData.errors.join('\n');
                    } else if (typeof errorData.errors === 'object') {
                        errorMsg = Object.values(errorData.errors).flat().join('\n');
                    }
                } else if (errorData?.error || errorData?.message) {
                    errorMsg = errorData.error || errorData.message;
                }
                showMessage(errorMsg, "error");
            }
        } catch (error) {
            console.error("Hata:", error);
            showMessage("Sunucuya ulaşılamadı.", "error");
        }
    });
}

function showMessage(text, type) {
    const el = document.getElementById('resultMessage');
    if (el) {
        el.textContent = text;
        el.className = type;
    }
}
