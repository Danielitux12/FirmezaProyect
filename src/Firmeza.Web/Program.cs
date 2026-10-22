var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();

// Mapea cualquier ruta no reconocida de vuelta a index.html para permitir el routing del lado del cliente de Angular (SPA)
app.MapFallbackToFile("index.html");

app.Run();