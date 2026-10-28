using System.Diagnostics;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services;

namespace RoyalVillaWeb.Controllers;

public class HomeController(IVillaService villaService, IMapper mapper) : Controller
{
    private readonly IVillaService _villaService = villaService; 
    private readonly IMapper _mapper = mapper; 

    [Authorize]
    public async Task<IActionResult> Index()
    {
        List<VillaDTO> villaList = [];
        try
        {
            var response = await _villaService.GetAllAsync<ApiResponse<List<VillaDTO>>>("");
            if(response != null && response.Success && response.Data != null)
            {
                villaList = response.Data; 
            }
        }
        catch (Exception ex)
        {
            TempData["error"] = $"An error occurred: {ex.Message}";
        }
        return View(villaList);
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
}
