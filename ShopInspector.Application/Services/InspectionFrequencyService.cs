using Microsoft.Extensions.Logging;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Services;

public class InspectionFrequencyService : IInspectionFrequencyService
{
    private readonly IInspectionFrequencyRepository _repository;
    private readonly ILogger<InspectionFrequencyService> _logger;

    public InspectionFrequencyService(IInspectionFrequencyRepository repository, ILogger<InspectionFrequencyService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<PaginatedList<InspectionFrequency>> GetAllAsync(int? pageIndex, int? pageSize, string searchTerm = "") =>
        _repository.GetAllAsync(pageIndex, pageSize, searchTerm);

    public Task<InspectionFrequency?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public Task AddAsync(InspectionFrequency entity) =>
        _repository.AddAsync(entity);

    public Task UpdateAsync(InspectionFrequency entity) =>
        _repository.UpdateAsync(entity);

    public Task DeleteAsync(int id) =>
        _repository.DeleteAsync(id);

    public async Task ForceDeleteAsync(int id)
    {
        try
        {
            await _repository.ForceDeleteAsync(id);
            _logger.LogWarning("Force deleted inspection frequency with ID {FrequencyId} and all associated data", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error force deleting inspection frequency with ID {FrequencyId}", id);
            throw;
        }
    }

    // Enhanced business logic methods with validation
    public async Task<bool> IsFrequencyNameExistsAsync(string frequencyName, int? excludeFrequencyId = null)
    {
        if (string.IsNullOrWhiteSpace(frequencyName))
        {
            _logger.LogWarning("Frequency name validation called with null or empty value");
            return false;
        }

        return await _repository.ExistsByNameAsync(frequencyName.Trim(), excludeFrequencyId);
    }

    public async Task<InspectionFrequency> CreateFrequencyAsync(string frequencyName)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(frequencyName))
            throw new ArgumentException("Frequency name cannot be empty", nameof(frequencyName));

        try
        {
            var frequency = new InspectionFrequency
            {
                FrequencyName = frequencyName.Trim()
            };

            await _repository.AddAsync(frequency);
            _logger.LogInformation("Successfully created inspection frequency: {FrequencyName}", frequency.FrequencyName);
            return frequency;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inspection frequency: {FrequencyName}", frequencyName);
            throw;
        }
    }

    public async Task<InspectionFrequency> UpdateFrequencyAsync(int frequencyId, string frequencyName)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(frequencyName))
            throw new ArgumentException("Frequency name cannot be empty", nameof(frequencyName));

        try
        {
            var existingFrequency = await _repository.GetByIdAsync(frequencyId);
            if (existingFrequency == null)
            {
                _logger.LogWarning("Attempted to update non-existent inspection frequency ID: {FrequencyId}", frequencyId);
                throw new InvalidOperationException($"Inspection frequency with ID {frequencyId} not found.");
            }

            existingFrequency.FrequencyName = frequencyName.Trim();

            await _repository.UpdateAsync(existingFrequency);
            _logger.LogInformation("Successfully updated inspection frequency: {FrequencyName} (ID: {FrequencyId})", 
                existingFrequency.FrequencyName, frequencyId);
            
            return existingFrequency;
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw to preserve the specific exception type
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inspection frequency ID: {FrequencyId}", frequencyId);
            throw;
        }
    }

    public async Task<bool> CanDeleteFrequencyAsync(int frequencyId)
    {
        try
        {
            var frequency = await _repository.GetByIdAsync(frequencyId);
            if (frequency == null)
            {
                _logger.LogWarning("Attempted to check delete eligibility for non-existent frequency ID: {FrequencyId}", frequencyId);
                return false;
            }

            return await _repository.CanDeleteFrequencyAsync(frequencyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if frequency {FrequencyId} can be deleted", frequencyId);
            return false;
        }
    }

    public async Task<InspectionFrequencyRelatedDataInfo> GetFrequencyRelatedInspectionsAsync(int frequencyId)
    {
        try
        {
            return await _repository.GetFrequencyRelatedDataAsync(frequencyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related data for frequency {FrequencyId}", frequencyId);
            return new InspectionFrequencyRelatedDataInfo();
        }
    }
}


