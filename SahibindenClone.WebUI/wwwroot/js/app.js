document.addEventListener("DOMContentLoaded", () => {
    fetchAdverts();
});

async function fetchAdverts() {
    try {
        const response = await fetch('/api/adverts');
        if (!response.ok) throw new Error("Veri çekilemedi.");
        
        const adverts = await response.json();
        const container = document.getElementById('advertList');
        
        container.innerHTML = ''; // Yükleniyor yazısını temizle
        
        if (adverts.length === 0) {
            container.innerHTML = '<p style="padding:15px; color:#666;">Şu an vitrinde hiç ilan bulunmuyor.</p>';
            return;
        }
        
        adverts.forEach(advert => {
            const card = document.createElement('div');
            card.className = 'advert-card';
            
            const priceFormatted = new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(advert.price);
            
            // Görsel kontrolü
            const imageHtml = advert.imageUrl 
                ? `<img src="${advert.imageUrl}" style="width:100%; height:140px; object-fit:cover;" />` 
                : `Görsel Yok`;

            // a href kısmını advert-detail.html'e yönlendirecek şekilde güncelledik
            card.innerHTML = `
                <div class="advert-image">${imageHtml}</div>
                <a href="advert-detail.html?id=${advert.id}" class="advert-title" title="${advert.title}">${advert.title}</a>
                <div class="advert-price">${priceFormatted}</div>
                <div class="advert-meta">${advert.categoryName} • ${advert.userName}</div>
            `;
            container.appendChild(card);
        });
    } catch (error) {
        console.error("Hata:", error);
        document.getElementById('advertList').innerHTML = 
            '<p style="color:#d0021b; padding:15px;">İlanlar yüklenirken bir hata oluştu. Lütfen projeyi "Live Server" ile değil, .NET üzerinden çalıştırdığınızdan emin olun.</p>';
    }
}