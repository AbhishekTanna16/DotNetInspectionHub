// Enhanced Form Validation Utilities for ShopInspector Admin
// Provides client-side validation with visual feedback

(function() {
    'use strict';

    // Initialize enhanced validation for all forms
    $(document).ready(function() {
        // Apply validation enhancement to all forms with validation
        $('form[data-enhance-validation]').each(function() {
            enhanceFormValidation($(this));
        });
    });

    // Enhanced form validation setup
    window.enhanceFormValidation = function(form) {
        if (!form.length) return;

        // Add visual feedback for required fields
        form.find("input[data-val-required], select[data-val-required], textarea[data-val-required]").each(function() {
            $(this).on('blur', function() {
                if ($(this).val() === '' || $(this).val() === null) {
                    $(this).addClass("is-invalid");
                } else {
                    $(this).removeClass("is-invalid");
                }
            });
        });

        // Real-time validation feedback
        form.find("input, select, textarea").on('keyup change paste', function() {
            const $this = $(this);
            
            // Remove validation classes after user interaction
            if ($this.hasClass("is-invalid") && $this.val() !== '') {
                if ($this.valid && $this.valid()) {
                    $this.removeClass("is-invalid").addClass("is-valid");
                }
            }
            
            // Clear valid class if field becomes empty and is required
            if ($this.val() === '' && $this.attr('data-val-required')) {
                $this.removeClass("is-valid");
            }
        });

        // Enhance submit button with loading state
        form.on('submit', function(e) {
            const submitButton = form.find('button[type="submit"]');
            
            if (submitButton.length && $(this).valid && $(this).valid()) {
                const originalHtml = submitButton.html();
                const isCreate = originalHtml.includes('Save Asset') || originalHtml.includes('Create') || originalHtml.includes('Add');
                const loadingText = isCreate ? 
                    '<i class="bi bi-hourglass-split"></i> Creating...' : 
                    '<i class="bi bi-hourglass-split"></i> Saving...';
                
                submitButton.prop("disabled", true)
                           .data('original-html', originalHtml)
                           .html(loadingText);
            }
        });

        // Reset button state if form submission fails or page reloads
        $(window).on('beforeunload', function() {
            form.find('button[type="submit"]').each(function() {
                const $btn = $(this);
                const originalHtml = $btn.data('original-html');
                if (originalHtml) {
                    $btn.prop("disabled", false).html(originalHtml);
                }
            });
        });
    };

    // Auto-enhance forms with validation attributes
    window.autoEnhanceValidation = function() {
        $('form').each(function() {
            const form = $(this);
            
            // Check if form has validation attributes
            if (form.find('[data-val="true"]').length > 0) {
                form.attr('data-enhance-validation', 'true');
                enhanceFormValidation(form);
            }
        });
    };

    // Validation helper functions
    window.validationHelpers = {
        // Show field error
        showFieldError: function(fieldName, message) {
            const field = $(`[name="${fieldName}"]`);
            field.addClass('is-invalid');
            const errorSpan = field.siblings('.text-danger').first();
            if (errorSpan.length) {
                errorSpan.text(message);
            }
        },

        // Clear field error
        clearFieldError: function(fieldName) {
            const field = $(`[name="${fieldName}"]`);
            field.removeClass('is-invalid');
            const errorSpan = field.siblings('.text-danger').first();
            if (errorSpan.length) {
                errorSpan.text('');
            }
        },

        // Validate field
        validateField: function(fieldName, rules) {
            const field = $(`[name="${fieldName}"]`);
            const value = field.val();
            
            for (let rule in rules) {
                const ruleValue = rules[rule];
                switch(rule) {
                    case 'required':
                        if (ruleValue && (!value || value.trim() === '')) {
                            this.showFieldError(fieldName, `${fieldName.replace(/([A-Z])/g, ' $1')} is required`);
                            return false;
                        }
                        break;
                    case 'minlength':
                        if (value && value.length < ruleValue) {
                            this.showFieldError(fieldName, `Minimum ${ruleValue} characters required`);
                            return false;
                        }
                        break;
                    case 'maxlength':
                        if (value && value.length > ruleValue) {
                            this.showFieldError(fieldName, `Maximum ${ruleValue} characters allowed`);
                            return false;
                        }
                        break;
                }
            }
            
            this.clearFieldError(fieldName);
            return true;
        }
    };

})();