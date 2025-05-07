using kolos1.Models.DTOs;

namespace kolos1.Services
{
    public interface IVisitService
    {
        Task<VisitDto> GetVisitByIdAsync(int id); // Pobieranie wizyty po ID
        Task CreateVisitAsync(CreateVisitDto visitDto); // Tworzenie nowej wizyty
    }
}