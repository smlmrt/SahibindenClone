namespace SahibindenClone.Application.Interfaces
{
    public interface INotificationService
    {
        Task NotifyPriceDropAsync(int advertId, string advertTitle, decimal newPrice);
    }
}