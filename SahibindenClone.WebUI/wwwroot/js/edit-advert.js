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
        // İlan ve kategorileri paralel yükle
        const [advertResponse, categoriesResponse] = await Promise.all([
            fetch(`/api/adverts/${id}`),
            fetch('/api/categories')
        ]);

        if (!advertResponse.ok) throw new Error('İlan bulunamadı.');
        if (!categoriesResponse.ok) throw new Error('Kategoriler yüklenemedi.');

        currentAdvert = await advertResponse.json();
        const categories = await categoriesResponse.json();

        renderEditForm(currentAdvert, categories);
    } catch (error) {
        showError("İlan yüklenirken bir hata oluştu: " + error.message);
    }
}

// ═══════════════ FORMU RENDER ET ═══════════════
function renderEditForm(advert, categories) {
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

    // Mevcut görsel
    const currentImageHtml = advert.imageUrl
        ? `<div class="current-image">
               <img src="${advert.imageUrl}" alt="Mevcut görsel" />
               <p>Mevcut görsel — Yeni bir görsel seçerseniz değiştirilir</p>
           </div>`
        : '';

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
                <label>Kategori</label>
                <select id="categoryId" class="form-control">
                    ${categoryOptions}
                </select>
            </div>
            <div class="form-group">
                <label>Yeni Görsel (opsiyonel)</label>
                <input type="file" id="image" class="form-control" accept="image/*">
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

// ═══════════════ DÜZENLEME GÖNDER ═══════════════
async function submitEdit(id) {
    const formData = new FormData();
    formData.append("Title", document.getElementById('title').value);
    formData.append("Description", document.getElementById('description').value);
    formData.append("Price", document.getElementById('price').value);
    formData.append("CategoryId", document.getElementById('categoryId').value);

    const imageFile = document.getElementById('image').files[0];
    if (imageFile) {
        formData.append("image", imageFile);
    }

    try {
        const response = await fetch(`/api/adverts/${id}`, {
            method: 'PUT',
            body: formData
        });

        if (response.ok) {
            showMessage("İlan başarıyla güncellendi! Detay sayfasına yönlendiriliyorsunuz...", "success");
            setTimeout(() => {
                window.location.href = `advert-detail.html?id=${id}`;
            }, 1500);
        } else {
            showMessage("Hata: İlan güncellenemedi.", "error");
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
