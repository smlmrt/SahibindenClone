document.addEventListener("DOMContentLoaded", () => {
    const urlParams = new URLSearchParams(window.location.search);
    const advertId = urlParams.get('id');

    if (advertId) {
        fetchAdvertDetail(advertId);
    } else {
        showError("Geçersiz ilan bağlantısı.");
    }
});

async function fetchAdvertDetail(id) {
    try {
        const response = await fetch(`/api/adverts/${id}`);
        if (!response.ok) throw new Error("İlan bulunamadı.");

        const advert = await response.json();
        renderAdvert(advert);
    } catch (error) {
        showError("İlan yüklenirken bir hata oluştu.");
    }
}

function renderAdvert(advert) {
    const priceFormatted = new Intl.NumberFormat('tr-TR', {
        style: 'currency', currency: 'TRY', minimumFractionDigits: 0
    }).format(advert.price);

    const dateFormatted = new Date(advert.createdAt).toLocaleDateString('tr-TR', {
        day: 'numeric', month: 'long', year: 'numeric'
    });

    const initials = getInitials(advert.userName);

    // Breadcrumb güncelle
    const breadcrumb = document.getElementById('breadcrumb');
    breadcrumb.innerHTML = `
        <a href="index.html">Ana Sayfa</a>
        <span class="sep">›</span>
        <a href="index.html">${advert.categoryName}</a>
        <span class="sep">›</span>
        <span class="current">${truncate(advert.title, 50)}</span>
    `;

    // Görsel
    const imageHtml = advert.imageUrl
        ? `<img src="${advert.imageUrl}" alt="${advert.title}">`
        : `<div class="no-image-placeholder">
               <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="m21 15-3.086-3.086a2 2 0 0 0-2.828 0L6 21"/></svg>
               <span>Görsel eklenmemiş</span>
           </div>`;

    // Description
    const descriptionHtml = advert.description
        ? `<div class="description-card fade-up fade-up-delay-2">
               <h2>Açıklama</h2>
               <div class="description-text">${escapeHtml(advert.description)}</div>
           </div>`
        : '';

    // İçeriği oluştur
    const container = document.getElementById('detailContent');
    container.innerHTML = `
        <!-- SOL KOLON -->
        <div class="detail-left">
            <div class="image-showcase fade-up">
                ${imageHtml}
            </div>

            <div class="title-card fade-up fade-up-delay-1">
                <h1>${escapeHtml(advert.title)}</h1>
                <div class="price-badge">${priceFormatted}</div>
                <div class="meta-row">
                    <span class="meta-chip">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20.59 13.41l-7.17 7.17a2 2 0 01-2.83 0L2 12V2h10l8.59 8.59a2 2 0 010 2.82z"/><line x1="7" y1="7" x2="7.01" y2="7"/></svg>
                        ${advert.categoryName}
                    </span>
                    <span class="meta-chip">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
                        ${dateFormatted}
                    </span>
                    <span class="meta-chip">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                        İlan No: ${advert.id}
                    </span>
                </div>
            </div>

            ${descriptionHtml}
        </div>

        <!-- SAĞ KOLON -->
        <div class="detail-right">
            <div class="seller-card fade-up fade-up-delay-1">
                <div class="seller-avatar">${initials}</div>
                <div class="seller-name">${escapeHtml(advert.userName)}</div>
                <div class="seller-since">Bireysel Satıcı</div>
                <button class="contact-btn" onclick="alert('Bu özellik henüz aktif değil.')">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07 19.5 19.5 0 01-6-6 19.79 19.79 0 01-3.07-8.67A2 2 0 014.11 2h3a2 2 0 012 1.72c.127.96.361 1.903.7 2.81a2 2 0 01-.45 2.11L8.09 9.91a16 16 0 006 6l1.27-1.27a2 2 0 012.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0122 16.92z"/></svg>
                    İletişime Geç
                </button>

                <!-- Düzenleme & Silme Butonları -->
                <div class="action-buttons">
                    <a href="edit-advert.html?id=${advert.id}" class="btn-edit">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                        Düzenle
                    </a>
                    <button class="btn-delete" onclick="confirmDelete(${advert.id})">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/></svg>
                        Sil
                    </button>
                </div>
            </div>

            <div class="info-card fade-up fade-up-delay-2">
                <h3>İlan Bilgileri</h3>
                <ul class="info-list">
                    <li>
                        <span class="info-label">İlan No</span>
                        <span class="info-value">#${advert.id}</span>
                    </li>
                    <li>
                        <span class="info-label">Kategori</span>
                        <span class="info-value">${advert.categoryName}</span>
                    </li>
                    <li>
                        <span class="info-label">İlan Tarihi</span>
                        <span class="info-value">${dateFormatted}</span>
                    </li>
                    <li>
                        <span class="info-label">İlan Sahibi</span>
                        <span class="info-value">${escapeHtml(advert.userName)}</span>
                    </li>
                </ul>
            </div>

            <a href="index.html" class="back-link fade-up fade-up-delay-3">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="19" y1="12" x2="5" y2="12"/><polyline points="12 19 5 12 12 5"/></svg>
                Tüm İlanlara Dön
            </a>
        </div>
    `;
}

// ═══════════════ İLAN SİLME ═══════════════
function confirmDelete(id) {
    // Modal oluştur
    const overlay = document.createElement('div');
    overlay.className = 'modal-overlay';
    overlay.innerHTML = `
        <div class="modal-box">
            <h3>İlanı Sil</h3>
            <p>Bu ilanı silmek istediğinize emin misiniz? Bu işlem geri alınamaz.</p>
            <div class="modal-actions">
                <button class="modal-cancel" onclick="this.closest('.modal-overlay').remove()">Vazgeç</button>
                <button class="modal-confirm" id="confirmDeleteBtn">Evet, Sil</button>
            </div>
        </div>
    `;
    document.body.appendChild(overlay);

    // Overlay'a tıklayınca kapat
    overlay.addEventListener('click', (e) => {
        if (e.target === overlay) overlay.remove();
    });

    // Sil butonuna tıklayınca
    document.getElementById('confirmDeleteBtn').addEventListener('click', async () => {
        try {
            const response = await fetch(`/api/adverts/${id}`, { method: 'DELETE' });
            if (response.ok) {
                overlay.querySelector('.modal-box').innerHTML = `
                    <h3 style="color: #059669;">İlan Silindi!</h3>
                    <p>Ana sayfaya yönlendiriliyorsunuz...</p>
                `;
                setTimeout(() => {
                    window.location.href = 'index.html';
                }, 1500);
            } else {
                overlay.querySelector('.modal-box p').textContent = 'Silme işlemi başarısız oldu.';
                overlay.querySelector('.modal-box p').style.color = '#ef4444';
            }
        } catch (error) {
            console.error('Silme hatası:', error);
            overlay.querySelector('.modal-box p').textContent = 'Sunucuya ulaşılamadı.';
        }
    });
}

// ═══════════════ HATA GÖSTER ═══════════════
function showError(message) {
    document.getElementById('detailContent').innerHTML = `
        <div class="error-state">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                <circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/>
            </svg>
            <p>${message}</p>
            <a href="index.html" class="back-link">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="19" y1="12" x2="5" y2="12"/><polyline points="12 19 5 12 12 5"/></svg>
                Ana Sayfaya Dön
            </a>
        </div>
    `;
}

// ═══════════════ YARDIMCI FONKSİYONLAR ═══════════════
function getInitials(name) {
    if (!name) return '?';
    return name.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2);
}

function truncate(str, max) {
    if (!str) return '';
    return str.length > max ? str.slice(0, max) + '...' : str;
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}