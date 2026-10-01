// ═══════════════ BAŞLANGIÇ ═══════════════
let currentAdvert = null;

document.addEventListener("DOMContentLoaded", () => {
    const urlParams = new URLSearchParams(window.location.search);
    const advertId = urlParams.get('id');

    if (advertId) {
        loadAdvertForEdit(advertId);
    } else {
        showError("Geçersiz ilan bağlantısı.");
    }
});

// ═══════════════ İLAN VERİLERİNİ YÜKLE ═══════════════
async function loadAdvertForEdit(id) {
    try {
        // İlan, kategoriler ve şehirleri paralel yükle
        const [advertResponse, categoriesResponse, citiesResponse] = await Promise.all([
            fetch(`/api/adverts/${id}`),
            fetch('/api/categories'),
            fetch('/api/cities')
        ]);

        if (!advertResponse.ok) throw new Error('İlan bulunamadı.');
        if (!categoriesResponse.ok) throw new Error('Kategoriler yüklenemedi.');
        if (!citiesResponse.ok) throw new Error('Şehirler yüklenemedi.');

        currentAdvert = await advertResponse.json();
        const categories = await categoriesResponse.json();
        const cities = await citiesResponse.json();

        renderEditForm(currentAdvert, categories, cities);
    } catch (error) {
        showError("İlan yüklenirken bir hata oluştu: " + error.message);
    }
}

// ═══════════════ FORMU RENDER ET ═══════════════
function renderEditForm(advert, categories, cities) {
    const container = document.getElementById('formContainer');

    // Kategori option'larını oluştur
    let categoryOptions = '<option value="">Kategori Seçin</option>';
    categories.forEach(cat => {
        const selected = cat.id === advert.categoryId ? 'selected' : '';
        categoryOptions += `<option value="${cat.id}" ${selected}>${cat.name}</option>`;
        if (cat.subCategories && cat.subCategories.length > 0) {
            cat.subCategories.forEach(sub => {
                const subSelected = sub.id === advert.categoryId ? 'selected' : '';
                categoryOptions += `<option value="${sub.id}" ${subSelected}>  └ ${sub.name}</option>`;
            });
        }
    });

    // Şehir option'larını oluştur
    let cityOptions = '<option value="">Şehir Seçin</option>';
    if (cities) {
        cities.forEach(city => {
            const selected = city.id === advert.cityId ? 'selected' : '';
            cityOptions += `<option value="${city.id}" ${selected}>${city.name}</option>`;
        });
    }

    // Mevcut görseller
    let currentImageHtml = '';
    if (advert.images && advert.images.length > 0) {
        currentImageHtml += '<div class="current-image"><div style="display: flex; gap: 8px; flex-wrap: wrap; justify-content: center;">';
        advert.images.forEach(img => {
            currentImageHtml += `<img src="${img.imageUrl}" alt="Mevcut görsel" style="max-width: 100px; max-height: 100px; object-fit: cover; border-radius: 4px;" />`;
        });
        currentImageHtml += '</div><p>Mevcut görseller — Yeni görsel(ler) eklerseniz üzerine eklenir/değiştirilir.</p></div>';
    }

    container.innerHTML = `
        <h2>İlan Düzenle <span class="badge">#${advert.id}</span></h2>
        <form id="editAdvertForm">
            <div class="form-group">
                <label>İlan Başlığı</label>
                <input type="text" id="title" class="form-control" required value="${escapeAttr(advert.title)}">
            </div>
            <div class="form-group">
                <label>Açıklama</label>
                <textarea id="description" class="form-control" rows="4" required>${escapeHtml(advert.description || '')}</textarea>
            </div>
            <div class="form-group">
                <label>Fiyat (TL)</label>
                <input type="number" id="price" class="form-control" required value="${advert.price}">
            </div>
            <div class="form-group">
                <label>Marka (isteğe bağlı)</label>
                <input type="text" id="brand" class="form-control" maxlength="100" value="${escapeAttr(advert.brand || '')}">
            </div>
            <div class="form-group">
                <label>Model (isteğe bağlı)</label>
                <input type="text" id="model" class="form-control" maxlength="100" value="${escapeAttr(advert.model || '')}">
            </div>
            <div class="form-group">
                <label>Kategori</label>
                <select id="categoryId" class="form-control">
                    ${categoryOptions}
                </select>
            </div>
            <div class="form-group">
                <label>Şehir</label>
                <select id="cityId" class="form-control" required>
                    ${cityOptions}
                </select>
            </div>
            <div class="form-group">
                <label>Yeni Görsel (opsiyonel)</label>
                <input type="file" id="image" class="form-control" accept=".jpg,.jpeg,.png,.webp" multiple>
                ${currentImageHtml}
            </div>
            <div class="btn-row">
                <a href="advert-detail.html?id=${advert.id}" class="btn-cancel">Vazgeç</a>
                <button type="submit" class="btn-submit">Değişiklikleri Kaydet</button>
            </div>
        </form>
        <div id="resultMessage"></div>
    `;

    // Form submit event
    document.getElementById('editAdvertForm').addEventListener('submit', (e) => {
        e.preventDefault();
        submitEdit(advert.id);
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

// ═══════════════ DÜZENLEME GÖNDER ═══════════════
async function submitEdit(id) {
    // Client-side validasyonlar
    const title = document.getElementById('title').value.trim();
    const description = document.getElementById('description').value.trim();
    const price = document.getElementById('price').value;
    const categoryId = document.getElementById('categoryId').value;
    const cityId = document.getElementById('cityId').value;

    if (title.length < 3) { showMessage('Başlık en az 3 karakter olmalıdır.', 'error'); return; }
    if (title.length > 150) { showMessage('Başlık en fazla 150 karakter olabilir.', 'error'); return; }
    if (description.length < 10) { showMessage('Açıklama en az 10 karakter olmalıdır.', 'error'); return; }
    if (!price || parseFloat(price) < 0) { showMessage('Geçerli bir fiyat giriniz.', 'error'); return; }
    if (!categoryId) { showMessage('Lütfen bir kategori seçin.', 'error'); return; }
    if (!cityId) { showMessage('Lütfen bir şehir seçin.', 'error'); return; }

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
    formData.append("CityId", cityId);

    for (let i = 0; i < imageFiles.length; i++) {
        formData.append("images", imageFiles[i]);
    }

    // Token'ı localStorage'dan alıyoruz
    const token = localStorage.getItem('token');

    try {
        const response = await fetch(`/api/adverts/${id}`, {
            method: 'PUT',
            body: formData,
            headers: {
                'Authorization': `Bearer ${token}` // Token eklendi
            }
        });

        if (response.ok) {
            showMessage("İlan başarıyla güncellendi! Detay sayfasına yönlendiriliyorsunuz...", "success");
            setTimeout(() => {
                window.location.href = `advert-detail.html?id=${id}`;
            }, 1500);
        } else {
            const errorData = await response.json().catch(() => null);
            // 403 Forbidden veya 401 Unauthorized hataları için özel mesaj
            if (response.status === 401 || response.status === 403) {
                showMessage("Bu ilanı düzenleme yetkiniz yok.", "error");
            } else {
                let errorMsg = "Hata: İlan güncellenemedi.";
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
        }
    } catch (error) {
        console.error("Hata:", error);
        showMessage("Sunucuya ulaşılamadı.", "error");
    }
}

// ═══════════════ YARDIMCI FONKSİYONLAR ═══════════════
function showError(message) {
    document.getElementById('formContainer').innerHTML = `
        <div style="text-align:center; padding:40px; color:#6b7280;">
            <p style="font-size:16px; margin-bottom:16px;">${message}</p>
            <a href="index.html" style="color:#2563eb; text-decoration:none; font-weight:600;">← Ana Sayfaya Dön</a>
        </div>
    `;
}

function showMessage(text, type) {
    const el = document.getElementById('resultMessage');
    el.textContent = text;
    el.className = type;
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function escapeAttr(text) {
    if (!text) return '';
    return text.replace(/"/g, '&quot;').replace(/'/g, '&#39;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}
