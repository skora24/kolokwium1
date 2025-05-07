using System.Data.Common;
using System.Data.SqlClient;
using kolos1.Exceptions;
using kolos1.Models.DTOs;
using Microsoft.Extensions.Configuration;

namespace kolos1.Services
{
    public class VisitService : IVisitService
    {
        private readonly string _connectionString;

        public VisitService(IConfiguration configuration)
        {
            // Pobranie connection stringa z pliku konfiguracyjnego
            _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
        }

        // Pobieranie wizyty po ID
        public async Task<VisitDto> GetVisitByIdAsync(int id)
        {
            var query = @"
                SELECT v.date, c.first_name, c.last_name, c.date_of_birth, 
                       m.mechanic_id, m.licence_number, 
                       s.name, vs.service_fee
                FROM Visit v
                JOIN Client c ON v.client_id = c.client_id
                JOIN Mechanic m ON v.mechanic_id = m.mechanic_id
                JOIN Visit_Service vs ON v.visit_id = vs.visit_id
                JOIN Service s ON vs.service_id = s.service_id
                WHERE v.visit_id = @id";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            await connection.OpenAsync();
            var reader = await command.ExecuteReaderAsync();

            VisitDto visit = null;
            while (await reader.ReadAsync())
            {
                if (visit == null)
                {
                    visit = new VisitDto
                    {
                        Date = reader.GetDateTime(0),
                        Client = new ClientDto
                        {
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            DateOfBirth = reader.GetDateTime(3)
                        },
                        Mechanic = new MechanicDto
                        {
                            MechanicId = reader.GetInt32(4),
                            LicenceNumber = reader.GetString(5)
                        },
                        VisitServices = new List<VisitServiceDto>()
                    };
                }

                visit.VisitServices.Add(new VisitServiceDto
                {
                    Name = reader.GetString(6),
                    ServiceFee = reader.GetDecimal(7)
                });
            }

            if (visit == null)
            {
                throw new NotFoundException("Visit not found.");
            }

            return visit;
        }

        // Tworzenie nowej wizyty
        public async Task CreateVisitAsync(CreateVisitDto visitDto)
        {
            await using SqlConnection connection = new SqlConnection(_connectionString);
            await using SqlCommand command = new SqlCommand();

            await connection.OpenAsync();
            DbTransaction transaction = await connection.BeginTransactionAsync();
            command.Transaction = (SqlTransaction)transaction;

            try
            {
                // Sprawdzenie klienta
                command.CommandText = "SELECT 1 FROM Client WHERE client_id = @ClientId";
                command.Parameters.AddWithValue("@ClientId", visitDto.ClientId);
                var clientExists = await command.ExecuteScalarAsync();
                if (clientExists == null)
                {
                    throw new NotFoundException($"Client with ID {visitDto.ClientId} not found.");
                }

                // Sprawdzenie mechanika
                command.CommandText = "SELECT 1 FROM Mechanic WHERE licence_number = @LicenceNumber";
                command.Parameters.AddWithValue("@LicenceNumber", visitDto.MechanicLicenceNumber);
                var mechanicExists = await command.ExecuteScalarAsync();
                if (mechanicExists == null)
                {
                    throw new NotFoundException($"Mechanic with licence number {visitDto.MechanicLicenceNumber} not found.");
                }

                // Tworzenie wizyty
                command.CommandText = "INSERT INTO Visit (visit_id, client_id, mechanic_id, date) VALUES (@VisitId, @ClientId, @MechanicId, @Date)";
                command.Parameters.AddWithValue("@VisitId", visitDto.VisitId);
                command.Parameters.AddWithValue("@Date", DateTime.UtcNow); // Zakładając, że data wizyty to czas wykonania
                await command.ExecuteNonQueryAsync();

                // Dodawanie usług
                foreach (var service in visitDto.Services)
                {
                    command.CommandText = "SELECT service_id FROM Service WHERE name = @ServiceName";
                    command.Parameters.AddWithValue("@ServiceName", service.ServiceName);
                    var serviceId = await command.ExecuteScalarAsync();
                    if (serviceId == null)
                    {
                        throw new NotFoundException($"Service {service.ServiceName} not found.");
                    }

                    command.CommandText = "INSERT INTO Visit_Service (visit_id, service_id, service_fee) VALUES (@VisitId, @ServiceId, @ServiceFee)";
                    command.Parameters.AddWithValue("@ServiceId", serviceId);
                    command.Parameters.AddWithValue("@ServiceFee", service.ServiceFee);
                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
