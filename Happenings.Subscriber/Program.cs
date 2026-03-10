using Happenings.Services.Database;
using Microsoft.EntityFrameworkCore;
using Happenings.Subscriber;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

builder.Services.AddDbContext<HappeningsContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var host = builder.Build();
host.Run();
