using CLDV6212P1.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddScoped<FunctionApiService>();

// Add session management
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();


//Microsoft (2025) �Introduction to Azure Storage�, Microsoft Learn.
//Available at: https://learn.microsoft.com/en-us/azure/storage/common/storage-introduction (Accessed: 26 August 2025).

//Microsoft (2023) �Design a scalable partitioning strategy for Azure Table storage�, Microsoft Learn.
//Available at: https://learn.microsoft.com/en-us/rest/api/storageservices/designing-a-scalable-partitioning-strategy-for-azure-table-storage? (Accessed: 26 August 2025).

//Microsoft (2024) �Quickstart: Use .NET to create a table in Azure Table storage�, Microsoft Learn.
//Available at: https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-quickstart-dotnet?tabs=dotnet (Accessed: 26 August 2025).

//C-Sharp Corner (2025) �Best Practices for Azure Blob, Table, Queue, File Storage with C#�, C-Sharp Corner.
//Available at: https://www.c-sharpcorner.com/article/best-practices-for-azure-blob-table-queue-file-storage-with-c-sharp/ (Accessed: 26 August 2025).

//Stuart Bale (2021) �Use Azure.Storage.Queues in ASP.Net � configure for DI�, Stack Overflow,
//Available at: [Stack Overflow] (Accessed: 28 August 2025).
