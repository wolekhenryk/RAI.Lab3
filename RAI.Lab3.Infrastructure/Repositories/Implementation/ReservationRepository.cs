using RAI.Lab3.Domain.Models;
using RAI.Lab3.Infrastructure.Data;
using RAI.Lab3.Infrastructure.Repositories.Interfaces;

namespace RAI.Lab3.Infrastructure.Repositories.Implementation;

public class ReservationRepository(AppDbContext dbContext)
    : BaseRepository<Reservation>(dbContext), IReservationRepository;