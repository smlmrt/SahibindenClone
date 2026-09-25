// ═══════════════ DURUM YÖNETİMİ ═══════════════
let currentState = {
    search: '',
    categoryId: null,
    minPrice: null,
    maxPrice: null,
    page: 1,
    pageSize: 20
};

// Kullanıcının favori ilan ID'leri (kalp ikonu için)
let favoriteIds = new Set();

// ═══════════════ BAŞLANGIÇ ═══════════════
document.addEventListener("DOMContentLoaded", () => {
    loadCategories();
    loadFavoriteIds().then(() => fetchAdverts());
    setupEventListeners();
});

// ═══════════════ FAVORİ ID'LERİNİ YÜKLE ═══════════════
async function loadFavoriteIds() {
    const token = localStorage.getItem('token');
    if (!token) return;

    try {
        const response = await fetch('/api/favorites/ids', {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.ok) {
            const ids = await response.json();
            favoriteIds = new Set(ids);
        }
    } catch (error) {
        console.error('Favori ID yükleme hatası:', error);
    }
}


// ═══════════════ EVENT LISTENER'LAR ═══════════════
function setupEventListeners() {
    // Arama butonu
    document.getElementById('searchBtn').addEventListener('click', () => {
        currentState.search = document.getElementById('searchInput').value.trim();
        currentState.page = 1;
        fetchAdverts();
    });

    // Enter ile arama
    document.getElementById('searchInput').addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            currentState.search = document.getElementById('searchInput').value.trim();
            currentState.page = 1;
            fetchAdverts();
        }
    });

    // Filtrele butonu
    document.getElementById('filterBtn').addEventListener('click', () => {
        const minVal = document.getElementById('minPrice').value;
        const maxVal = document.getElementById('maxPrice').value;
        currentState.minPrice = minVal ? parseFloat(minVal) : null;
        currentState.maxPrice = maxVal ? parseFloat(maxVal) : null;
        currentState.page = 1;
        fetchAdverts();
    });

    // Temizle butonu
    document.getElementById('clearFilterBtn').addEventListener('click', () => {
        document.getElementById('searchInput').value = '';
        document.getElementById('minPrice').value = '';
        document.getElementById('maxPrice').value = '';
        currentState = { search: '', categoryId: null, minPrice: null, maxPrice: null, page: 1, pageSize: 20 };
        
        // Kategori aktif durumunu sıfırla
        document.querySelectorAll('#categoryList a').forEach(a => a.classList.remove('active'));
        const allLink = document.querySelector('#categoryList a[data-category-id=""]');
        if (allLink) allLink.classList.add('active');

        fetchAdverts();
    });
}

// ═══════════════ KATEGORİLERİ YÜKLE ═══════════════
async function loadCategories() {
    try {
        const response = await fetch('/api/categories');
        if (!response.ok) throw new Error('Kategoriler yüklenemedi.');

        const categories = await response.json();
        const list = document.getElementById('categoryList');

        // "Tümü" linki zaten var, geri kalanını ekle
        categories.forEach(cat => {
            const li = document.createElement('li');
            const a = document.createElement('a');
            a.href = '#';
            a.textContent = cat.name;
            a.dataset.categoryId = cat.id;
            a.addEventListener('click', (e) => {
                e.preventDefault();
                selectCategory(cat.id, a);
            });
            li.appendChild(a);
            list.appendChild(li);

            // Alt kategoriler
            if (cat.subCategories && cat.subCategories.length > 0) {
                cat.subCategories.forEach(sub => {
                    const subLi = document.createElement('li');
                    const subA = document.createElement('a');
                    subA.href = '#';
                    subA.textContent = `  └ ${sub.name}`;
                    subA.dataset.categoryId = sub.id;
                    subA.style.paddingLeft = '28px';
                    subA.style.fontSize = '13px';
                    subA.addEventListener('click', (e) => {
                        e.preventDefault();
                        selectCategory(sub.id, subA);
                    });
                    subLi.appendChild(subA);
                    list.appendChild(subLi);
                });
            }
        });

        // "Tümü" linki click event
        const allLink = document.querySelector('#categoryList a[data-category-id=""]');
        if (allLink) {
            allLink.addEventListener('click', (e) => {
                e.preventDefault();
                selectCategory(null, allLink);
            });
        }
    } catch (error) {
        console.error('Kategori yükleme hatası:', error);
    }
}

function selectCategory(categoryId, activeElement) {
    // Aktif sınıfını güncelle
    document.querySelectorAll('#categoryList a').forEach(a => a.classList.remove('active'));
    activeElement.classList.add('active');

    currentState.categoryId = categoryId;
    currentState.page = 1;

    // Sayfa başlığını güncelle
    const title = document.getElementById('pageTitle');
    title.textContent = categoryId ? activeElement.textContent.replace('└ ', '').trim() + ' İlanları' : 'Vitrin İlanları';

    fetchAdverts();
}

// ═══════════════ İLANLARI ÇEKME ═══════════════
async function fetchAdverts() {
    try {
        // URL parametrelerini oluştur
        const params = new URLSearchParams();
        if (currentState.search) params.append('search', currentState.search);
        if (currentState.categoryId) params.append('categoryId', currentState.categoryId);
        if (currentState.minPrice !== null) params.append('minPrice', currentState.minPrice);
        if (currentState.maxPrice !== null) params.append('maxPrice', currentState.maxPrice);
        params.append('page', currentState.page);
        params.append('pageSize', currentState.pageSize);

        const response = await fetch(`/api/adverts?${params.toString()}`);
        if (!response.ok) throw new Error("Veri çekilemedi.");
        
        const result = await response.json();
        const container = document.getElementById('advertList');
        
        container.innerHTML = '';
        
        if (result.items.length === 0) {
            container.innerHTML = `
                <div class="no-results" style="grid-column: 1/-1;">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                        <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
                    </svg>
                    <p>Aramanızla eşleşen ilan bulunamadı.</p>
                </div>`;
            document.getElementById('pagination').style.display = 'none';
            return;
        }
        
        result.items.forEach((advert, index) => {
            const card = document.createElement('div');
            card.className = 'advert-card';
            card.style.animationDelay = `${index * 0.05}s`;
            card.style.animation = 'slideUp .4s ease both';
            
            const priceFormatted = new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', minimumFractionDigits: 0 }).format(advert.price);
            
            const imageHtml = advert.imageUrl 
                ? `<img src="${advert.imageUrl}" alt="${advert.title}" />` 
                : `<span>Görsel Yok</span>`;

            const isFav = favoriteIds.has(advert.id);
            const token = localStorage.getItem('token');
            const heartHtml = token ? `
                <button class="favorite-btn ${isFav ? 'favorited' : ''}" 
                        onclick="toggleFavoriteFromCard(event, ${advert.id}, this)" 
                        title="${isFav ? 'Favorilerden çıkar' : 'Favorilere ekle'}">
                    <svg viewBox="0 0 24 24" width="20" height="20"
                         fill="${isFav ? 'currentColor' : 'none'}" 
                         stroke="currentColor" stroke-width="2">
                        <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
                    </svg>
                </button>
            ` : '';

            card.innerHTML = `
                <div class="advert-image">${imageHtml}</div>
                <div class="advert-card-body">
                    <a href="advert-detail.html?id=${advert.id}" class="advert-title" title="${advert.title}">${advert.title}</a>
                    <div class="advert-price">${priceFormatted}</div>
                    <div class="advert-meta">${advert.categoryName} • ${advert.userName}</div>
                </div>
                ${heartHtml}
            `;
            container.appendChild(card);
        });

        // Sayfalama render
        renderPagination(result);
    } catch (error) {
        console.error("Hata:", error);
        document.getElementById('advertList').innerHTML = 
            '<p style="color:#ef4444; padding:15px; grid-column:1/-1;">İlanlar yüklenirken bir hata oluştu.</p>';
    }
}

// ═══════════════ SAYFALAMA ═══════════════
function renderPagination(result) {
    const container = document.getElementById('pagination');
    
    if (result.totalPages <= 1) {
        container.style.display = 'none';
        return;
    }

    container.style.display = 'flex';
    container.innerHTML = '';

    // Önceki sayfa butonu
    const prevBtn = document.createElement('button');
    prevBtn.innerHTML = '← Önceki';
    prevBtn.disabled = result.page <= 1;
    prevBtn.addEventListener('click', () => goToPage(result.page - 1));
    container.appendChild(prevBtn);

    // Sayfa numaraları
    const maxButtons = 5;
    let startPage = Math.max(1, result.page - Math.floor(maxButtons / 2));
    let endPage = Math.min(result.totalPages, startPage + maxButtons - 1);
    
    if (endPage - startPage < maxButtons - 1) {
        startPage = Math.max(1, endPage - maxButtons + 1);
    }

    if (startPage > 1) {
        addPageButton(container, 1, result.page);
        if (startPage > 2) {
            const dots = document.createElement('button');
            dots.textContent = '...';
            dots.disabled = true;
            container.appendChild(dots);
        }
    }

    for (let i = startPage; i <= endPage; i++) {
        addPageButton(container, i, result.page);
    }

    if (endPage < result.totalPages) {
        if (endPage < result.totalPages - 1) {
            const dots = document.createElement('button');
            dots.textContent = '...';
            dots.disabled = true;
            container.appendChild(dots);
        }
        addPageButton(container, result.totalPages, result.page);
    }

    // Sonraki sayfa butonu
    const nextBtn = document.createElement('button');
    nextBtn.innerHTML = 'Sonraki →';
    nextBtn.disabled = result.page >= result.totalPages;
    nextBtn.addEventListener('click', () => goToPage(result.page + 1));
    container.appendChild(nextBtn);

    // Bilgi metni
    const info = document.createElement('span');
    info.className = 'pagination-info';
    info.textContent = `${result.totalCount} ilandan ${(result.page - 1) * result.pageSize + 1}-${Math.min(result.page * result.pageSize, result.totalCount)} arası gösteriliyor`;
    container.appendChild(info);
}

function addPageButton(container, pageNum, currentPage) {
    const btn = document.createElement('button');
    btn.textContent = pageNum;
    if (pageNum === currentPage) btn.classList.add('active');
    btn.addEventListener('click', () => goToPage(pageNum));
    container.appendChild(btn);
}

function goToPage(page) {
    currentState.page = page;
    fetchAdverts();
    // Sayfanın üstüne scroll
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// ═══════════════ FAVORİ TOGGLE (KART İÇİ) ═══════════════
async function toggleFavoriteFromCard(event, advertId, buttonEl) {
    event.preventDefault();
    event.stopPropagation();

    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = 'login.html';
        return;
    }

    try {
        const response = await fetch(`/api/favorites/${advertId}`, {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const data = await response.json();
            const svg = buttonEl.querySelector('svg');

            if (data.isFavorited) {
                buttonEl.classList.add('favorited');
                svg.setAttribute('fill', 'currentColor');
                buttonEl.title = 'Favorilerden çıkar';
                favoriteIds.add(advertId);
            } else {
                buttonEl.classList.remove('favorited');
                svg.setAttribute('fill', 'none');
                buttonEl.title = 'Favorilere ekle';
                favoriteIds.delete(advertId);
            }

            // Kalp animasyonu
            buttonEl.classList.add('favorite-pulse');
            setTimeout(() => buttonEl.classList.remove('favorite-pulse'), 400);
        }
    } catch (error) {
        console.error('Favori toggle hatası:', error);
    }
}