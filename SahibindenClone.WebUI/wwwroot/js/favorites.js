// ═══════════════ FAVORİLERİM SAYFASI ═══════════════
document.addEventListener("DOMContentLoaded", () => {
    const token = localStorage.getItem('token');

    if (!token) {
        showEmptyState("Favorilerinizi görmek için giriş yapmalısınız.",
            `<a href="login.html" class="favorites-login-btn">Giriş Yap</a>`);
        document.getElementById('favoritesCount').textContent = '';
        return;
    }

    loadFavorites();
});

async function loadFavorites() {
    const token = localStorage.getItem('token');

    try {
        const response = await fetch('/api/favorites', {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.status === 401) {
            showEmptyState("Oturumunuz sona ermiş. Lütfen tekrar giriş yapın.",
                `<a href="login.html" class="favorites-login-btn">Giriş Yap</a>`);
            document.getElementById('favoritesCount').textContent = '';
            return;
        }

        if (!response.ok) throw new Error("Favoriler yüklenemedi.");

        const favorites = await response.json();
        const container = document.getElementById('favoritesList');
        const countEl = document.getElementById('favoritesCount');

        if (favorites.length === 0) {
            countEl.textContent = 'Henüz favori ilanınız yok.';
            showEmptyState("Beğendiğiniz ilanları kalp ikonuna tıklayarak favorilerinize ekleyebilirsiniz.",
                `<a href="index.html" class="favorites-browse-btn">İlanlara Göz At</a>`);
            return;
        }

        countEl.textContent = `${favorites.length} favori ilanınız var`;
        container.innerHTML = '';

        favorites.forEach((advert, index) => {
            const card = document.createElement('div');
            card.className = 'advert-card';
            card.style.animationDelay = `${index * 0.05}s`;
            card.style.animation = 'slideUp .4s ease both';

            const priceFormatted = new Intl.NumberFormat('tr-TR', {
                style: 'currency', currency: 'TRY', minimumFractionDigits: 0
            }).format(advert.price);

            const imageHtml = advert.imageUrl
                ? `<img src="${advert.imageUrl}" alt="${advert.title}" />`
                : `<span>Görsel Yok</span>`;

            card.innerHTML = `
                <div class="advert-image">${imageHtml}</div>
                <div class="advert-card-body">
                    <a href="advert-detail.html?id=${advert.id}" class="advert-title" title="${advert.title}">${advert.title}</a>
                    <div class="advert-price">${priceFormatted}</div>
                    <div class="advert-meta">${advert.categoryName} • ${advert.userName}</div>
                </div>
                <button class="favorite-btn favorited" onclick="removeFavorite(event, ${advert.id}, this)" title="Favorilerden çıkar">
                    <svg viewBox="0 0 24 24" fill="currentColor" width="20" height="20">
                        <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
                    </svg>
                </button>
            `;
            container.appendChild(card);
        });

    } catch (error) {
        console.error("Hata:", error);
        document.getElementById('favoritesList').innerHTML =
            '<p style="color:#ef4444; padding:15px; grid-column:1/-1;">Favoriler yüklenirken bir hata oluştu.</p>';
    }
}

async function removeFavorite(event, advertId, buttonEl) {
    event.preventDefault();
    event.stopPropagation();

    const token = localStorage.getItem('token');
    if (!token) return;

    try {
        const response = await fetch(`/api/favorites/${advertId}`, {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            // Kartı animasyonla kaldır
            const card = buttonEl.closest('.advert-card');
            card.style.transition = 'all 0.3s ease';
            card.style.transform = 'scale(0.9)';
            card.style.opacity = '0';

            setTimeout(() => {
                card.remove();
                // Kalan favori sayısını güncelle
                const remaining = document.querySelectorAll('#favoritesList .advert-card').length;
                const countEl = document.getElementById('favoritesCount');

                if (remaining === 0) {
                    countEl.textContent = 'Henüz favori ilanınız yok.';
                    showEmptyState("Beğendiğiniz ilanları kalp ikonuna tıklayarak favorilerinize ekleyebilirsiniz.",
                        `<a href="index.html" class="favorites-browse-btn">İlanlara Göz At</a>`);
                } else {
                    countEl.textContent = `${remaining} favori ilanınız var`;
                }
            }, 300);
        }
    } catch (error) {
        console.error("Favoriden çıkarma hatası:", error);
    }
}

function showEmptyState(message, actionHtml = '') {
    document.getElementById('favoritesList').innerHTML = `
        <div class="favorites-empty" style="grid-column: 1/-1;">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" width="72" height="72">
                <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
            </svg>
            <p>${message}</p>
            ${actionHtml}
        </div>
    `;
}
