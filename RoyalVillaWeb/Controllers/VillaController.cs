using System.Diagnostics;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services;

namespace RoyalVillaWeb.Controllers;

public class VillaController(IVillaService villaService, IMapper mapper) : Controller
{
    private readonly IVillaService _villaService = villaService; 
    private readonly IMapper _mapper = mapper; 

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

    
    public IActionResult Create()
    {
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VillaCreateDTO createDTO)
    {
        if(!ModelState.IsValid)
        {
            return View(createDTO);
        }   
        
        try
        {
            var response = await _villaService.CreateAsync<ApiResponse<VillaDTO>>(createDTO, "");

            if(response != null && response.Success && response.Data != null)
            {
                TempData["success"] = "Villa created successfully";
                
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            TempData["error"] = $"An error occurred: {ex.Message}";
        }

        return View(createDTO);
    }


    public async Task<IActionResult> Delete(int id)
    {
        if(id <= 0)
        {
            TempData["error"] = "Invalid villa ID";
        }

        try
        {
            var response = await _villaService.GetAsync<ApiResponse<VillaDTO>>(id, "");
            if(response != null && response.Success && response.Data != null)
            {
                return View(response.Data);
            } 
        }
        catch (Exception ex)
        {
            TempData["error"] = $"An error occurred: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(VillaDTO villaDTO)
    {
        if(villaDTO.Id <= 0)
        {
            TempData["error"] = "Invalid villa ID";
            return RedirectToAction(nameof(Index));
        }
        try
        {
            var response = await _villaService.DeleteAsync<ApiResponse<VillaDTO>>(villaDTO.Id, "");
            
            if(response != null && response.Success)
            {
                TempData["success"] = "Villa deleted successfully";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            TempData["error"] = $"An error occurred: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
