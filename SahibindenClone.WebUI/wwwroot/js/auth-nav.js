/**
 * auth-nav.js
 * Üst menüyü, oturum yönetimini ve SignalR Bildirim sistemini içerir.
 */
(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        const nav = document.querySelector('.nav-links');
        if (!nav) return;

        const separator = document.createElement('span');
        separator.className = 'nav-separator';
        nav.appendChild(separator);

        const token = localStorage.getItem('token');
        const userName = localStorage.getItem('userName');

        if (token && userName) {
            if (localStorage.getItem('userRole') === 'Admin') {
                const adminLink = document.createElement('a');
                adminLink.href = 'admin.html';
                adminLink.className = 'nav-item';
                adminLink.textContent = 'Admin Paneli';
                nav.appendChild(adminLink);
            }
            // Kullanıcı giriş yapmış - Bildirim zili, Adı ve Çıkış butonu
            const userInfo = document.createElement('div');
            userInfo.className = 'user-info';
            userInfo.style.display = 'flex';
            userInfo.style.alignItems = 'center';
            userInfo.style.gap = '15px';

            userInfo.innerHTML = `
                <!-- Bildirim Zili -->
                <div id="notificationBell" style="position:relative; cursor:pointer; color:rgba(255,255,255,0.9);">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="22" height="22">
                        <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path>
                        <path d="M13.73 21a2 2 0 0 1-3.46 0"></path>
                    </svg>
                    <span id="notificationBadge" style="display:none; position:absolute; top:-5px; right:-5px; background:#ef4444; color:white; font-size:10px; font-weight:bold; padding:2px 6px; border-radius:10px;">0</span>
                    
                    <!-- Bildirim Dropdown -->
                    <div id="notificationDropdown" style="display:none; position:absolute; right:0; top:35px; width:300px; background:white; border-radius:8px; box-shadow:0 10px 25px rgba(0,0,0,0.2); z-index:1000; overflow:hidden;">
                        <div style="padding:12px 15px; border-bottom:1px solid #f0f2f5; display:flex; justify-content:space-between; align-items:center;">
                            <h4 style="margin:0; color:#111827; font-size:14px;">Bildirimler</h4>
                            <button id="markReadBtn" style="background:none; border:none; color:#2563eb; font-size:12px; cursor:pointer;">Tümünü Okundu İşaretle</button>
                        </div>
                        <div id="notificationList" style="max-height:300px; overflow-y:auto; padding:0; margin:0; list-style:none;">
                            <div style="padding:15px; text-align:center; color:#6b7280; font-size:13px;">Yükleniyor...</div>
                        </div>
                    </div>
                </div>

                <span class="user-name">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="18" height="18">
                        <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
                        <circle cx="12" cy="7" r="4"/>
                    </svg>
                    ${userName}
                </span>
                <button class="btn-logout" id="logoutBtn">Çıkış Yap</button>
            `;
            nav.appendChild(userInfo);

            // Çıkış İşlemi
            document.getElementById('logoutBtn').addEventListener('click', function () {
                localStorage.clear();
                window.location.href = 'index.html';
            });

            // Bildirim Dropdown Aç/Kapa
            const bell = document.getElementById('notificationBell');
            const dropdown = document.getElementById('notificationDropdown');
            bell.addEventListener('click', (e) => {
                if (e.target.closest('#markReadBtn') || e.target.closest('a')) return;
                dropdown.style.display = dropdown.style.display === 'none' ? 'block' : 'none';
            });

            // Tıklayınca dışarı kapanma
            document.addEventListener('click', (e) => {
                if (!bell.contains(e.target)) dropdown.style.display = 'none';
            });

            // SignalR Kurulumu ve Başlatılması
            loadSignalRAndConnect(token);
        } else {
            // Kullanıcı giriş yapmamış
            const authLinks = document.createElement('div');
            authLinks.className = 'auth-links';
            authLinks.innerHTML = `
                <a href="login.html" class="btn-login">Giriş Yap</a>
                <a href="register.html" class="btn-register">Kayıt Ol</a>
            `;
            nav.appendChild(authLinks);
        }
    });

    function loadSignalRAndConnect(token) {
        // Dinamik olarak SignalR kütüphanesini sayfaya ekle
        const script = document.createElement('script');
        script.src = "https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js";
        script.onload = () => {
            initSignalR(token);
            fetchInitialNotifications(token);
        };
        document.head.appendChild(script);

        // Toast Container ekle
        const toastContainer = document.createElement('div');
        toastContainer.id = "toastContainer";
        toastContainer.style.cssText = "position:fixed; bottom:20px; right:20px; display:flex; flex-direction:column; gap:10px; z-index:9999;";
        document.body.appendChild(toastContainer);
    }

    function initSignalR(token) {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub", { accessTokenFactory: () => token })
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveNotification", (message, link) => {
            showToast(message, link);
            fetchInitialNotifications(token); // Listeyi ve rozeti (badge) güncelle
        });

        connection.start().catch(err => console.error("SignalR Bağlantı Hatası: ", err));
    }

    async function fetchInitialNotifications(token) {
        try {
            const response = await fetch('/api/notifications', {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!response.ok) return;

            const notifications = await response.json();
            const list = document.getElementById('notificationList');
            const badge = document.getElementById('notificationBadge');

            const unreadCount = notifications.filter(n => !n.isRead).length;

            if (unreadCount > 0) {
                badge.style.display = 'block';
                badge.textContent = unreadCount;
            } else {
                badge.style.display = 'none';
            }

            if (notifications.length === 0) {
                list.innerHTML = '<div style="padding:15px; text-align:center; color:#6b7280; font-size:13px;">Bildiriminiz bulunmuyor.</div>';
                return;
            }

            list.innerHTML = notifications.map(n => `
                <a href="${n.relatedLink}" style="display:block; padding:12px 15px; border-bottom:1px solid #f0f2f5; text-decoration:none; background:${n.isRead ? '#fff' : '#eff6ff'}; transition:background 0.2s;">
                    <p style="margin:0; font-size:13px; color:#111827; line-height:1.4;">${n.message}</p>
                    <small style="color:#6b7280; font-size:11px; margin-top:6px; display:block;">
                        ${new Date(n.createdAt).toLocaleDateString('tr-TR')}
                    </small>
                </a>
            `).join('');

            // Tümünü Okundu İşaretle butonu eventi
            document.getElementById('markReadBtn').onclick = async (e) => {
                e.stopPropagation();
                await fetch('/api/notifications/mark-read', {
                    method: 'PUT',
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                fetchInitialNotifications(token);
            };

        } catch (error) {
            console.error("Bildirimler çekilemedi", error);
        }
    }

    function showToast(message, link) {
        const container = document.getElementById('toastContainer');
        const toast = document.createElement('div');
        toast.style.cssText = "background:white; border-left:4px solid #3b82f6; box-shadow:0 4px 12px rgba(0,0,0,0.15); padding:16px; border-radius:8px; width:300px; cursor:pointer; transform:translateX(120%); transition:transform 0.4s ease;";
        toast.innerHTML = `
            <div style="font-weight:600; font-size:14px; color:#111827; margin-bottom:4px;">Yeni Bildirim</div>
            <div style="font-size:13px; color:#4b5563;">${message}</div>
        `;

        toast.onclick = () => window.location.href = link;

        container.appendChild(toast);

        // Animasyonla ekrana gir
        requestAnimationFrame(() => {
            toast.style.transform = "translateX(0)";
        });

        // 5 saniye sonra kaybol
        setTimeout(() => {
            toast.style.transform = "translateX(120%)";
            setTimeout(() => toast.remove(), 400);
        }, 5000);
    }
})();
