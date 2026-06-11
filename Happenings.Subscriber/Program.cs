using Happenings.Services.Database;
using Happenings.Services.Services;
using Microsoft.EntityFrameworkCore;
using Happenings.Subscriber;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

builder.Services.AddDbContext<HappeningsContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Isti QR servis koji koristi i WebAPI (TicketService) � ujednacen QR format
builder.Services.AddScoped<QrCodeService>();

var host = builder.Build();
host.Run();
