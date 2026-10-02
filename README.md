
## Gereksinimler

- .NET 10 SDK
- SQLite için ayrıca sunucu kurmanız gerekmez; veritabanı dosya tabanlıdır.

## Kurulum ve çalıştırma

Depoyu klonlayıp WebUI projesini başlatın:

```bash
git clone https://github.com/smlmrt/SahibindenClone.git
cd SahibindenClone
dotnet run --project SahibindenClone.WebUI
```

Varsayılan geliştirme adresleri `http://localhost:5264` ve `https://localhost:7076` şeklindedir. Tarayıcı kök adrese gittiğinde statik arayüz açılır. Uygulama başlarken EF Core migration'larını veritabanına uygular ve başlangıç kategorileri ile şehirlerini ekler. Veritabanı varsayılan olarak `SahibindenClone.WebUI/sahibindenclone.db` konumunda oluşturulur.

## Yapılandırma

Temel ayarlar `SahibindenClone.WebUI/appsettings.json` dosyasındadır. Yerel geliştirme ve dağıtım ortamlarında sırları kaynak koda yazmak yerine .NET yapılandırma sağlayıcılarını veya ortam değişkenlerini kullanın. İlgili ortam değişkenleri:

| Ayar | Ortam değişkeni | Açıklama |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | SQLite bağlantı dizesi |
| `JwtSettings:SecretKey` | `JwtSettings__SecretKey` | JWT imzalama anahtarı; dağıtımda güçlü ve özel bir değer kullanın |
| `JwtSettings:Issuer` | `JwtSettings__Issuer` | Token issuer değeri |
| `JwtSettings:Audience` | `JwtSettings__Audience` | Token audience değeri |
| `JwtSettings:ExpiryMinutes` | `JwtSettings__ExpiryMinutes` | Token geçerlilik süresi (dakika) |
| `Admin:Email` | `Admin__Email` | Kayıt sırasında bu e-postayı kullanan hesaba Admin rolü verilir |

Örnek:

```bash
export JwtSettings__SecretKey='uzun-ve-benzersiz-bir-gizli-anahtar'
export Admin__Email='admin@example.com'
dotnet run --project SahibindenClone.WebUI
```

Admin rolüne sahip olacak kullanıcıyı, `Admin:Email` ile aynı e-posta adresini kullanarak kaydedin. Başlangıç verileri yalnızca ilgili tablo boşken eklenir.

## API özeti

API adresleri `/api` altında sunulur. Kimlik doğrulaması gereken uç noktalara `Authorization: Bearer <token>` başlığıyla giriş yanıtındaki JWT gönderilir.

| Alan | Uç noktalar | Erişim |
| --- | --- | --- |
| Kimlik | `POST /api/auth/register`, `POST /api/auth/login` | Herkese açık |
| İlanlar | `GET /api/adverts`, `GET /api/adverts/{id}`, `POST /api/adverts`, `PUT /api/adverts/{id}`, `DELETE /api/adverts/{id}`, `PUT /api/adverts/{id}/mark-sold`, `GET /api/adverts/{id}/history` | Okuma herkese açık; değişiklikler giriş gerektirir |
| Kategoriler ve şehirler | `GET /api/categories`, `GET /api/cities` | Herkese açık |
| Favoriler | `GET /api/favorites`, `GET /api/favorites/ids`, `POST /api/favorites/{advertId}` | Giriş gerekir |
| Alışveriş | `GET/POST /api/purchases`, `PUT /api/purchases/{id}/decision`, `PUT /api/purchases/{id}/confirm`, `POST /api/purchases/{id}/review` | Giriş gerekir |
| Kullanıcılar | `GET /api/users/{id}/profile`, `PUT /api/users/{id}/profile`, `GET /api/users/{id}/reviews` | Profil ve yorumlar herkese açık; güncelleme yalnızca profil sahibine açık |
| Bildirimler | `GET /api/notifications`, `PUT /api/notifications/mark-read` | Giriş gerekir |
| Yönetim | `/api/admin/dashboard`, `/api/admin/adverts`, `/api/admin/users`, `/api/admin/categories`, `/api/admin/audit-logs` ve ilgili `POST`/`PUT`/`DELETE` işlemleri | Admin rolü gerekir |

`GET /api/adverts` arama, kategori, şehir, fiyat aralığı, marka, model, oluşturulma tarihi, sıralama ve sayfalama parametrelerini destekler. İlan görseli yükleme uç noktaları JSON yerine `multipart/form-data` kabul eder. Gerçek zamanlı bildirim bağlantısı `/notificationHub` adresindedir.

## İlan ve alışveriş durumları

- Yeni ilanlar `PendingApproval` durumunda oluşturulur. Yönetici `approve` kararıyla ilanı `Active`, `reject` kararıyla `Rejected` yapar.
- Aktif ilanlar 30 gün geçerlidir. Arka plan servisi süresi dolan ilanları `Expired` durumuna geçirir.
- Alışveriş talebi `Requested` ile başlar. Satıcı kabul veya ret verir; kabulden sonra alıcı işlemi tamamlar ve değerlendirme bırakabilir.
- Bir ilan için tekrar eden favori veya eşzamanlı aktif satın alma talepleri veritabanı/servis kurallarıyla sınırlandırılır.

## Depo yapısı

```text
SahibindenClone.Domain/
SahibindenClone.Application/
SahibindenClone.Infrastructure/
SahibindenClone.WebUI/
  Controllers/
  Hubs/
  Services/
  wwwroot/       # Statik web arayüzü
```

EF Core migration dosyaları `SahibindenClone.Infrastructure/Migrations` altında tutulur. Uygulama bunları başlangıçta otomatik uygular.
