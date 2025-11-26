/**
 * Client-side pagination for admin tables
 * Provides pagination functionality for tables that don't have server-side pagination
 */
class ClientPagination {
    constructor(tableSelector, itemsPerPage = 3) {
        this.table = document.querySelector(tableSelector);
        this.itemsPerPage = itemsPerPage;
        this.currentPage = 1;
        this.allRows = [];
        
        if (this.table) {
            this.init();
        }
    }
    
    init() {
        // Get all data rows (exclude header and empty state rows)
        const tbody = this.table.querySelector('tbody');
        this.allRows = Array.from(tbody.querySelectorAll('tr')).filter(row => {
            return !row.querySelector('.text-center.text-muted'); // Exclude "no data" rows
        });
        
        this.totalItems = this.allRows.length;
        this.totalPages = Math.ceil(this.totalItems / this.itemsPerPage);
        
        // Show pagination if we have more than 1 page OR force show for demo purposes with 2+ items
        if (this.totalPages > 1 || this.totalItems >= 2) {
            this.createPaginationControls();
            this.showPage(1);
        } else if (this.totalItems > 0) {
            // Still show info for single page
            this.createInfoOnly();
        }
    }
    
    createInfoOnly() {
        const tableWrapper = this.table.closest('.asset-table-wrapper') || this.table.parentElement;
        const infoContainer = document.createElement('div');
        infoContainer.className = 'text-center text-muted mt-2';
        infoContainer.innerHTML = `<small>Showing all ${this.totalItems} entries</small>`;
        tableWrapper.appendChild(infoContainer);
    }
    
    showPage(page) {
        this.currentPage = page;
        
        // Hide all rows
        this.allRows.forEach(row => {
            row.style.display = 'none';
        });
        
        // Show rows for current page
        const startIndex = (page - 1) * this.itemsPerPage;
        const endIndex = startIndex + this.itemsPerPage;
        
        for (let i = startIndex; i < endIndex && i < this.allRows.length; i++) {
            this.allRows[i].style.display = '';
        }
        
        this.updatePaginationControls();
        this.updateInfoDisplay();
    }
    
    createPaginationControls() {
        const tableWrapper = this.table.closest('.asset-table-wrapper') || this.table.parentElement;
        const paginationContainer = document.createElement('nav');
        paginationContainer.className = 'mt-4';
        paginationContainer.setAttribute('aria-label', 'Page navigation');
        paginationContainer.innerHTML = `
            <ul class="pagination justify-content-center" id="client-pagination-${Date.now()}">
                <!-- Pagination will be generated here -->
            </ul>
        `;
        
        // Store the unique ID for later reference
        this.paginationId = paginationContainer.querySelector('ul').id;
        
        // Insert after the table
        tableWrapper.appendChild(paginationContainer);
        
        // Create info container
        const infoContainer = document.createElement('div');
        infoContainer.className = 'text-center text-muted mt-2';
        infoContainer.innerHTML = `<small id="pagination-info-${Date.now()}"></small>`;
        this.infoId = infoContainer.querySelector('small').id;
        tableWrapper.appendChild(infoContainer);
    }
    
    updatePaginationControls() {
        const paginationUl = document.getElementById(this.paginationId);
        if (!paginationUl) return;
        
        let paginationHTML = '';
        
        // Previous button
        const prevDisabled = this.currentPage === 1 ? 'disabled' : '';
        paginationHTML += `
            <li class="page-item ${prevDisabled}">
                <a class="page-link" href="#" data-page="${this.currentPage - 1}">
                    <i class="bi bi-chevron-left"></i> Prev
                </a>
            </li>
        `;
        
        // Page numbers
        const startPage = Math.max(1, this.currentPage - 2);
        const endPage = Math.min(this.totalPages, this.currentPage + 2);
        
        // First page link if needed
        if (startPage > 1) {
            paginationHTML += `<li class="page-item"><a class="page-link" href="#" data-page="1">1</a></li>`;
            if (startPage > 2) {
                paginationHTML += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
        }
        
        // Page number links
        for (let i = startPage; i <= endPage; i++) {
            const activeClass = i === this.currentPage ? 'active' : '';
            paginationHTML += `
                <li class="page-item ${activeClass}">
                    <a class="page-link" href="#" data-page="${i}">${i}</a>
                </li>
            `;
        }
        
        // Last page link if needed
        if (endPage < this.totalPages) {
            if (endPage < this.totalPages - 1) {
                paginationHTML += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
            paginationHTML += `<li class="page-item"><a class="page-link" href="#" data-page="${this.totalPages}">${this.totalPages}</a></li>`;
        }
        
        // Next button
        const nextDisabled = this.currentPage === this.totalPages ? 'disabled' : '';
        paginationHTML += `
            <li class="page-item ${nextDisabled}">
                <a class="page-link" href="#" data-page="${this.currentPage + 1}">
                    Next <i class="bi bi-chevron-right"></i>
                </a>
            </li>
        `;
        
        paginationUl.innerHTML = paginationHTML;
        
        // Add click event listeners
        paginationUl.addEventListener('click', (e) => {
            e.preventDefault();
            if (e.target.tagName === 'A' && e.target.dataset.page) {
                const page = parseInt(e.target.dataset.page);
                if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
                    this.showPage(page);
                }
            }
        });
    }
    
    updateInfoDisplay() {
        const infoElement = document.getElementById(this.infoId);
        if (!infoElement) return;
        
        const startItem = (this.currentPage - 1) * this.itemsPerPage + 1;
        const endItem = Math.min(this.currentPage * this.itemsPerPage, this.totalItems);
        
        infoElement.textContent = `Showing ${startItem} to ${endItem} of ${this.totalItems} entries`;
    }
}

// Auto-initialize pagination for admin tables
document.addEventListener('DOMContentLoaded', function() {
    // Check if we're on an admin index page that needs client-side pagination
    const currentPath = window.location.pathname;
    const isIndexPage = currentPath.includes('/Index') || currentPath.match(/\/[A-Za-z]+\/?$/);
    const isAdminArea = currentPath.includes('/Admin/');
    
    if (isAdminArea && isIndexPage) {
        const table = document.querySelector('.asset-table');
        if (table) {
            // Only add pagination if the table doesn't already have server-side pagination
            const existingPagination = document.querySelector('.pagination');
            if (!existingPagination) {
                // Use smaller page size to demonstrate pagination with few items
                const pageSize = 3; // Show 3 items per page
                
                console.log('Initializing client pagination with page size:', pageSize);
                new ClientPagination('.asset-table', pageSize);
            }
        }
    }
});

// Manually initialize pagination for testing
window.initClientPagination = function(pageSize = 3) {
    const table = document.querySelector('.asset-table');
    if (table) {
        new ClientPagination('.asset-table', pageSize);
    }
};