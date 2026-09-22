/**
 * auth-nav.js
 * Tüm sayfalarda navbar'a Giriş Yap / Kayıt Ol veya Kullanıcı bilgisi ekler.
 * Her sayfada <script src="js/auth-nav.js"></script> ile yüklenir.
 */
(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        const nav = document.querySelector('.nav-links');
        if (!nav) return;

        // Separator ekle
        const separator = document.createElement('span');
        separator.className = 'nav-separator';
        nav.appendChild(separator);

        const token = localStorage.getItem('token');
        const userName = localStorage.getItem('userName');

        if (token && userName) {
            // Kullanıcı giriş yapmış — kullanıcı adı + çıkış butonu göster
            const userInfo = document.createElement('div');
            userInfo.className = 'user-info';
            userInfo.innerHTML = `
                <span class="user-name">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
                        <circle cx="12" cy="7" r="4"/>
                    </svg>
                    ${userName}
                </span>
                <button class="btn-logout" id="logoutBtn">Çıkış Yap</button>
            `;
            nav.appendChild(userInfo);

            // Çıkış butonu işlevi
            document.getElementById('logoutBtn').addEventListener('click', function () {
                localStorage.removeItem('token');
                localStorage.removeItem('userId');
                localStorage.removeItem('userName');
                window.location.href = 'index.html';
            });
        } else {
            // Kullanıcı giriş yapmamış — Giriş Yap + Kayıt Ol butonları göster
            const authLinks = document.createElement('div');
            authLinks.className = 'auth-links';
            authLinks.innerHTML = `
                <a href="login.html" class="btn-login">Giriş Yap</a>
                <a href="register.html" class="btn-register">Kayıt Ol</a>
            `;
            nav.appendChild(authLinks);
        }
    });
})();
