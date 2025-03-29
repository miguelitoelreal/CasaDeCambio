using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CasaDeCambio.Models;

namespace CasaDeCambio.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public IActionResult CalcularCambio(decimal cantidad, string origen, string destino)
    {
        // Definir las tasas de cambio
        var tasasCambio = new Dictionary<(string, string), decimal>
        {
            { ("BRL", "PEN"), 0.75m }, // 1 BRL = 0.75 PEN
            { ("USD", "PEN"), 3.80m }, // 1 USD = 3.80 PEN
            { ("BRL", "USD"), 0.20m }, // 1 BRL = 0.20 USD
            { ("USD", "BRL"), 5.00m }, // 1 USD = 5.00 BRL
            { ("PEN", "USD"), 0.26m }, // 1 PEN = 0.26 USD
            { ("PEN", "BRL"), 1.33m }  // 1 PEN = 1.33 BRL
        };

        // Verificar si la combinación de monedas es válida
        if (!tasasCambio.TryGetValue((origen, destino), out var tasaCambio))
        {
            ViewBag.Error = "No se puede realizar el cambio entre las monedas seleccionadas.";
            return View("Index");
        }

        // Calcular el resultado
        decimal resultado = cantidad * tasaCambio;

        // Pasar los datos calculados a la vista
        ViewBag.Cantidad = cantidad;
        ViewBag.Origen = origen;
        ViewBag.Destino = destino;
        ViewBag.Resultado = resultado;

        return View("Resultado");
    }

    [HttpPost]
    public IActionResult FormularioBoleta(decimal cantidad, decimal resultado, string origen, string destino)
    {
        ViewBag.Cantidad = cantidad;
        ViewBag.Resultado = resultado;
        ViewBag.Origen = origen;
        ViewBag.Destino = destino;
        return View("FormularioBoleta");
    }

    [HttpPost]
    public IActionResult GenerarBoleta(string nombre, string email, decimal cantidad, decimal resultado, string origen, string destino)
    {
        ViewBag.Nombre = nombre;
        ViewBag.Email = email;
        ViewBag.Cantidad = cantidad;
        ViewBag.Resultado = resultado;
        ViewBag.Origen = origen;
        ViewBag.Destino = destino;
        return View("Boleta");
    }
}