using Microsoft.Extensions.Logging;
using ShopInspector.Application.Interfaces;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IEmployeeRepository repository, ILogger<EmployeeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<PaginatedList<Employee>> GetAllAsync(int? pageIndex, int? pageSize, string searchTerm = "") => 
        _repository.GetAllAsync(pageIndex, pageSize, searchTerm);

    public Task<Employee?> GetByIdAsync(int id) => 
        _repository.GetByIdAsync(id);

    public Task AddAsync(Employee entity) => 
        _repository.AddAsync(entity);

    public Task UpdateAsync(Employee entity) => 
        _repository.UpdateAsync(entity);

    public Task DeleteAsync(int id) => 
        _repository.DeleteAsync(id);

    public async Task ForceDeleteAsync(int id)
    {
        try
        {
            await _repository.ForceDeleteAsync(id);
            _logger.LogWarning("Force deleted employee with ID {EmployeeId} and all associated data", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error force deleting employee with ID {EmployeeId}", id);
            throw;
        }
    }

    public Task<PaginatedList<Employee>> SerchEmployeeAsync(string seachterm, int? pageIndex, int? pageSize) => 
        _repository.SerchEmployeeAsync(seachterm, pageIndex, pageSize);

    // Enhanced business logic methods with validation
    public async Task<bool> IsEmployeeNameExistsInCompanyAsync(string employeeName, int companyId, int? excludeEmployeeId = null)
    {
        if (string.IsNullOrWhiteSpace(employeeName))
        {
            _logger.LogWarning("Employee name validation called with null or empty value");
            return false;
        }

        return await _repository.ExistsByNameAndCompanyAsync(employeeName.Trim(), companyId, excludeEmployeeId);
    }

    public async Task<Employee> CreateEmployeeAsync(string employeeName, int companyId, bool active, string createdBy)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(employeeName))
            throw new ArgumentException("Employee name cannot be empty", nameof(employeeName));
        if (companyId <= 0)
            throw new ArgumentException("Invalid company ID", nameof(companyId));

        try
        {
            var employee = new Employee
            {
                EmployeeName = employeeName.Trim(),
                CompanyID = companyId,
                Active = active,
                CreatedBy = createdBy ?? "System",
                CreatedOn = DateTime.UtcNow
            };

            await _repository.AddAsync(employee);
            _logger.LogInformation("Successfully created employee: {EmployeeName}", employee.EmployeeName);
            return employee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee: {EmployeeName}", employeeName);
            throw;
        }
    }

    public async Task<Employee> UpdateEmployeeAsync(int employeeId, string employeeName, int companyId, bool active)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(employeeName))
            throw new ArgumentException("Employee name cannot be empty", nameof(employeeName));
        if (companyId <= 0)
            throw new ArgumentException("Invalid company ID", nameof(companyId));

        try
        {
            var existingEmployee = await _repository.GetByIdAsync(employeeId);
            if (existingEmployee == null)
            {
                _logger.LogWarning("Attempted to update non-existent employee ID: {EmployeeId}", employeeId);
                throw new InvalidOperationException($"Employee with ID {employeeId} not found.");
            }

            existingEmployee.EmployeeName = employeeName.Trim();
            existingEmployee.CompanyID = companyId;
            existingEmployee.Active = active;

            await _repository.UpdateAsync(existingEmployee);
            _logger.LogInformation("Successfully updated employee: {EmployeeName} (ID: {EmployeeId})", 
                existingEmployee.EmployeeName, employeeId);
            
            return existingEmployee;
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw to preserve the specific exception type
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee ID: {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<bool> CanDeleteEmployeeAsync(int employeeId)
    {
        try
        {
            var employee = await _repository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                _logger.LogWarning("Attempted to check delete eligibility for non-existent employee ID: {EmployeeId}", employeeId);
                return false;
            }

            // Check if employee has any relationships that prevent deletion
            bool canDelete = await _repository.CanDeleteEmployeeAsync(employeeId);
            
            if (!canDelete)
            {
                _logger.LogInformation("Employee {EmployeeId} cannot be deleted due to existing relationships", employeeId);
            }
            
            return canDelete;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if employee {EmployeeId} can be deleted", employeeId);
            return false;
        }
    }

    public async Task<EmployeeRelatedDataInfo> GetEmployeeRelatedInspectionsAsync(int employeeId)
    {
        try
        {
            return await _repository.GetEmployeeRelatedDataAsync(employeeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related data for employee {EmployeeId}", employeeId);
            return new EmployeeRelatedDataInfo();
        }
    }
}

