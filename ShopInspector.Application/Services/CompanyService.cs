using Microsoft.Extensions.Logging;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _repository;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(ICompanyRepository repository, ILogger<CompanyService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<PaginatedList<Company>> GetAllAsync(int? pageIndex, int? pageSize, string searchTerm = "") =>
        _repository.GetAllAsync(pageIndex, pageSize, searchTerm);

    public Task<Company> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public Task AddAsync(Company entity) =>
        _repository.AddAsync(entity);

    public Task UpdateAsync(Company entity) =>
        _repository.UpdateAsync(entity);

    public Task DeleteAsync(int id) =>
        _repository.DeleteAsync(id);

    public async Task ForceDeleteAsync(int id)
    {
        try
        {
            await _repository.ForceDeleteAsync(id);
            _logger.LogWarning("Force deleted company with ID {CompanyId} and all associated data", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error force deleting company with ID {CompanyId}", id);
            throw;
        }
    }

    public Task<PaginatedList<Company>> SerchCompanyAsync(string seachterm, int? pageIndex, int? pageSize) =>
        _repository.SerchCompanyAsync(seachterm, pageIndex, pageSize);

    // Enhanced business logic methods with input validation
    public async Task<bool> IsCompanyNameExistsAsync(string companyName, int? excludeCompanyId = null)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            _logger.LogWarning("Company name validation called with null or empty value");
            return false;
        }
        
        return await _repository.ExistsByNameAsync(companyName.Trim(), excludeCompanyId);
    }

    public async Task<bool> IsEmailExistsAsync(string email, int? excludeCompanyId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Email validation called with null or empty value");
            return false;
        }
        
        return await _repository.ExistsByEmailAsync(email.Trim(), excludeCompanyId);
    }

    public async Task<bool> CanDeleteCompanyAsync(int companyId)
    {
        try
        {
            var company = await _repository.GetByIdWithEmployeesAsync(companyId);
            if (company == null)
            {
                _logger.LogWarning("Attempted to check delete eligibility for non-existent company ID: {CompanyId}", companyId);
                return false;
            }
            
            return company.Employees?.Any() != true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if company {CompanyId} can be deleted", companyId);
            return false;
        }
    }

    public async Task<CompanyRelatedDataInfo> GetCompanyRelatedDataAsync(int companyId)
    {
        try
        {
            return await _repository.GetCompanyRelatedDataAsync(companyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related data for company {CompanyId}", companyId);
            return new CompanyRelatedDataInfo();
        }
    }

    public async Task<Company> CreateCompanyAsync(string companyName, string adminEmail, string contactName, bool active, string createdBy)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name cannot be empty", nameof(companyName));
        if (string.IsNullOrWhiteSpace(adminEmail))
            throw new ArgumentException("Admin email cannot be empty", nameof(adminEmail));
        if (string.IsNullOrWhiteSpace(contactName))
            throw new ArgumentException("Contact name cannot be empty", nameof(contactName));

        try
        {
            var company = new Company
            {
                CompanyName = companyName.Trim(),
                CompanyAdminEmail = adminEmail.Trim().ToLowerInvariant(),
                CompanyContactName = contactName.Trim(),
                Active = active,
                CreatedBy = createdBy ?? "System",
                CreatedOn = DateTime.UtcNow
            };

            await _repository.AddAsync(company);
            _logger.LogInformation("Successfully created company: {CompanyName}", company.CompanyName);
            return company;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company: {CompanyName}", companyName);
            throw;
        }
    }

    public async Task<Company> UpdateCompanyAsync(int companyId, string companyName, string adminEmail, string contactName, bool active)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name cannot be empty", nameof(companyName));
        if (string.IsNullOrWhiteSpace(adminEmail))
            throw new ArgumentException("Admin email cannot be empty", nameof(adminEmail));
        if (string.IsNullOrWhiteSpace(contactName))
            throw new ArgumentException("Contact name cannot be empty", nameof(contactName));

        try
        {
            var existingCompany = await _repository.GetByIdAsync(companyId);
            if (existingCompany == null)
            {
                _logger.LogWarning("Attempted to update non-existent company ID: {CompanyId}", companyId);
                throw new InvalidOperationException($"Company with ID {companyId} not found.");
            }

            existingCompany.CompanyName = companyName.Trim();
            existingCompany.CompanyAdminEmail = adminEmail.Trim().ToLowerInvariant();
            existingCompany.CompanyContactName = contactName.Trim();
            existingCompany.Active = active;

            await _repository.UpdateAsync(existingCompany);
            _logger.LogInformation("Successfully updated company: {CompanyName} (ID: {CompanyId})", existingCompany.CompanyName, companyId);
            return existingCompany;
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw to preserve the specific exception type
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company ID: {CompanyId}", companyId);
            throw;
        }
    }
}

