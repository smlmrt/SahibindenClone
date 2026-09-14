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
        
        adverts.forEach(advert => {
            const item = document.createElement('div');
            item.className = 'advert-item';
            
            // Fiyatı TL formatına çevir
            const priceFormatted = new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(advert.price);

            item.innerHTML = `
                <div>
                    <strong>${advert.title}</strong>
                    <div style="font-size: 0.85em; color: #777;">${advert.categoryName} - ${advert.userName}</div>
                </div>
                <div class="price">${priceFormatted}</div>
            `;
            container.appendChild(item);
        });
    } catch (error) {
        console.error("Hata:", error);
        document.getElementById('advertList').innerText = "İlanlar yüklenirken bir hata oluştu.";
    }
}