using SahibindenClone.Domain.Enums;

namespace SahibindenClone.Application.Helpers;

/// <summary>
/// İlan ve satın alma durumlarının Türkçe karşılıklarını döndüren yardımcı sınıf.
/// </summary>
public static class StatusNameHelper
{
    public static string AdvertStatusName(AdvertStatus status) => status switch
    {
        AdvertStatus.Active => "Aktif",
        AdvertStatus.Sold => "Satıldı",
        AdvertStatus.Expired => "Süresi Doldu",
        AdvertStatus.Rejected => "Reddedildi",
        _ => "Onay Bekliyor"
    };

    public static string PurchaseStatusName(PurchaseStatus status) => status switch
    {
        PurchaseStatus.Requested => "Satıcı yanıtı bekleniyor",
        PurchaseStatus.Accepted => "Satıcı kabul etti, alıcı onayı bekleniyor",
        PurchaseStatus.Rejected => "Reddedildi",
        _ => "Tamamlandı"
    };
}
