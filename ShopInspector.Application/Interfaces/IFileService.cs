using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Interfaces;
public interface IFileService
{
    Task<string?> SaveImageAsync(IFormFile file, int inspectionId);
    bool IsAllowedImage(IFormFile file);
}
