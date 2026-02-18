using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.Services;
using Microsoft.EntityFrameworkCore;

public class PendingPaymentSyncJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PendingPaymentSyncJob> _logger;

    public PendingPaymentSyncJob(
        IServiceScopeFactory scopeFactory,
        ILogger<PendingPaymentSyncJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            await SyncStalePaymentsAsync();
        }
    }

    private async Task SyncStalePaymentsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DeWaveAPIDbContext>();
        var midtrans = scope.ServiceProvider.GetRequiredService<IMidtransService>();
        var enrollment = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

        var stale = await db.Payments
            .Where(p => p.Status == "Pending"
                     && p.CreatedAt < DateTime.UtcNow.AddMinutes(-10)
                     && p.CreatedAt > DateTime.UtcNow.AddHours(-24))
            .ToListAsync();

        _logger.LogInformation("Syncing {Count} stale pending payments.", stale.Count);

        foreach (var payment in stale)
        {
            try
            {
                var status = await midtrans.GetTransactionStatusAsync(payment.OrderId);

                payment.Status = status switch
                {
                    "settlement" or "capture" => "Success",
                    "expire" or "cancel" or "deny" => "Failed",
                    _ => payment.Status
                };

                if (payment.Status == "Success")
                    await enrollment.EnrollIfNotAlreadyAsync(
                        payment.StudentId, payment.CourseId, payment.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync payment {OrderId}.", payment.OrderId);
            }
        }

        await db.SaveChangesAsync();
    }
}