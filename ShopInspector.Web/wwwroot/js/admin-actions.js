
(function () {
    'use strict';
    function log(...args) {
        if (window.console) console.log('[admin-actions]', ...args);
    }

    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenInput) return tokenInput.value;
        const meta = document.querySelector('meta[name="csrf-token"]');
        if (meta) return meta.getAttribute('content');
        return null;
    }

    function showToast(message, type = 'info', timeout = 3500) {
        let container = document.getElementById('admin-toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'admin-toast-container';
            container.style.position = 'fixed';
            container.style.top = '1rem';
            container.style.right = '1rem';
            container.style.zIndex = 99999;
            container.style.minWidth = '200px';
            document.body.appendChild(container);
        }

        const el = document.createElement('div');
        el.className = 'admin-toast';
        el.style.marginBottom = '0.5rem';
        el.style.padding = '0.6rem 0.9rem';
        el.style.borderRadius = '6px';
        el.style.color = '#fff';
        el.style.boxShadow = '0 2px 6px rgba(0,0,0,0.12)';
        el.style.fontSize = '0.9rem';
        el.style.opacity = '0';
        el.style.transition = 'opacity .18s ease';

        if (type === 'success') el.style.background = '#198754';
        else if (type === 'danger') el.style.background = '#dc3545';
        else if (type === 'warning') { el.style.background = '#ffc107'; el.style.color = '#222'; }
        else el.style.background = '#0d6efd';

        el.textContent = message;
        container.appendChild(el);
        requestAnimationFrame(() => (el.style.opacity = '1'));

        setTimeout(() => {
            el.style.opacity = '0';
            setTimeout(() => el.remove(), 200);
        }, timeout);
    }

    function fetchPost(url, body, headers = {}) {
        return fetch(url, {
            method: 'POST',
            body,
            headers,
            credentials: 'include'
        });
    }


    function ensureQrModal() {
        let modalEl = document.getElementById('qrModal');
        if (modalEl) return modalEl;

        const markup = `
<div class="modal fade" id="qrModal" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-dialog-centered">
    <div class="modal-content shadow">
      <div class="modal-header">
        <h5 class="modal-title">Asset QR Code</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
      </div>
      <div class="modal-body text-center">
        <img id="qrModalImage" class="img-fluid border rounded mb-3" alt="QR Code" style="background:#fff;max-width:320px;" />
        <div class="d-flex justify-content-center gap-3 flex-wrap">
            <a id="qrDownload" class="btn btn-primary" href="#" download="asset-qr.png">
                <i class="bi bi-download"></i> Download
            </a>
            <a id="qrOpen" class="btn btn-outline-secondary" href="#" target="_blank" rel="noopener">
                <i class="bi bi-box-arrow-up-right"></i> Open Image
            </a>
        </div>
        <div id="qrTargetUrl" class="small text-muted mt-3"></div>
      </div>
    </div>
  </div>
</div>`;
        const wrapper = document.createElement('div');
        wrapper.innerHTML = markup;
        document.body.appendChild(wrapper.firstElementChild);
        return document.getElementById('qrModal');
    }

    window.showQrModal = function (imageUrl, targetUrl) {
        const modalEl = ensureQrModal();
        const img = modalEl.querySelector('#qrModalImage');
        const dl = modalEl.querySelector('#qrDownload');
        const open = modalEl.querySelector('#qrOpen');
        const urlInfo = modalEl.querySelector('#qrTargetUrl');

        img.src = imageUrl;
        dl.href = imageUrl;
        open.href = imageUrl;
        urlInfo.textContent = targetUrl ? `URL: ${targetUrl}` : '';

        try {
            if (window.bootstrap?.Modal) {
                window.bootstrap.Modal.getOrCreateInstance(modalEl).show();
            } else {
                modalEl.style.display = 'block';
                modalEl.classList.add('show');
            }
        } catch (e) {
            console.error('QR modal show error', e);
            modalEl.style.display = 'block';
        }
    };

    async function handleNavigate(el) {
        const url = el.getAttribute('data-url') || el.getAttribute('href');
        if (!url) {
            log('navigate: no url', el);
            return;
        }

        window.location.assign(url);
    }

    async function handleDelete(el) {
        const url = el.getAttribute('data-url') || el.getAttribute('href') || (el.closest && el.closest('form') ? el.closest('form').getAttribute('action') : null);
        if (!url) {
            showToast('No delete URL specified', 'danger');
            log('delete: no url', el);
            return;
        }

        const confirmMessage = el.getAttribute('data-confirm') || 'Are you sure you want to delete this item?';
        if (!confirm(confirmMessage)) {
            log('delete cancelled by user');
            return;
        }

        const originalHtml = (el && el.innerHTML) ? el.innerHTML : '';
        try {
            el.disabled = true;
            el.innerHTML = el.getAttribute('data-loading') || 'Deleting...';

            const token = getAntiForgeryToken();
            const headers = {
                'X-Requested-With': 'XMLHttpRequest'
            };

            if (token) headers['RequestVerificationToken'] = token;

            const body = new URLSearchParams();

            const res = await fetchPost(url, body, headers);

            if (res.ok) {
                let json = null;
                const contentType = res.headers.get('content-type') || '';
                if (contentType.includes('application/json')) {
                    try {
                        json = await res.json();
                    } catch (ex) { log('json parse failed', ex); }
                }

                try {
                    const tr = el.closest && el.closest('tr');
                    if (tr && tr.parentNode) tr.parentNode.removeChild(tr);
                } catch (domEx) { log('failed to remove row', domEx); }

                if (json && json.redirect) {
                    window.location.assign(json.redirect);
                    return;
                }

                showToast(json && json.message ? json.message : 'Deleted', 'success');
            } else {
                if (res.status === 400 || res.status === 403) {
                    showToast('Delete failed: permission or validation error', 'danger');
                } else if (res.status === 404) {
                    showToast('Delete endpoint not found (404)', 'danger');
                } else {
                    showToast('Delete failed: ' + res.status, 'danger');
                }

                try {
                    const text = await res.text().catch(() => '');
                    log('delete failed', res.status, text);
                } catch (readErr) { log('error reading failure body', readErr); }
            }
        } catch (err) {
            console.error('[admin-actions] delete exception', err);
            showToast('Delete failed (network)', 'danger');
        } finally {
            try {
                if (el && el.parentNode) {
                    el.disabled = false;
                    if (originalHtml) el.innerHTML = el.getAttribute('data-original') || originalHtml;
                }
            } catch (finalErr) { log('error in finally restore', finalErr); }
        }
    }

    async function handleSave(el) {

        const url = el.getAttribute("data-url");
        if (!url) {
            showToast("Save failed: no URL provided", "danger");
            return;
        }

        const form = el.closest("form");
        if (!form) {
            showToast("Save failed: form not found", "danger");
            return;
        }

        const original = el.innerHTML;
        el.disabled = true;
        el.innerHTML = "Saving...";

        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            const fd = new FormData(form);

            const res = await fetch(url, {
                method: "POST",
                body: fd,
                headers: {
                    "RequestVerificationToken": token
                }
            });

            if (res.redirected) {
                  showToast("Saved successfully", "success");
                window.location.href = res.url;
                return;
            }

            if (res.ok) {
                showToast("Saved successfully", "success");
            } else {
                const text = await res.text();
                console.error(text);
                showToast("Save failed", "danger");
            }

        } catch (err) {
            console.error(err);
            showToast("Network error", "danger");

        } finally {
            el.disabled = false;
            el.innerHTML = original;
        }
    }

    async function handleAjaxDeleteForm(form) {
        const url = form.getAttribute('action') || form.getAttribute('data-url');
        if (!url) {
            showToast('Delete failed: no URL', 'danger');
            return;
        }
        if (!confirm(form.getAttribute('data-confirm') || 'Are you sure you want to delete this item?')) return;

        const btn = form.querySelector('button[type="submit"], input[type="submit"]');
        if (btn) { btn.disabled = true; const orig = btn.innerHTML; btn.innerHTML = 'Deleting...'; }

        try {
            const token = getAntiForgeryToken();
            const headers = {};
            if (token) headers['RequestVerificationToken'] = token;
            const res = await fetchPost(url, new URLSearchParams(), headers);
            if (res.ok) {
                const tr = form.closest('tr');
                if (tr) tr.remove();
                showToast('Delete Data Successfully', 'success');
            } else {
                showToast('Delete failed: ' + res.status, 'danger');
            }
        } catch (err) {
            console.error('[admin-actions] ajax-form-delete exception', err);
            showToast('Delete failed (network)', 'danger');
        } finally {
            if (btn) { btn.disabled = false;  }
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        log('initializing');

        document.querySelectorAll('table').forEach(tbl => {
            tbl.addEventListener('click', function (e) {
                if (e.target.closest && (e.target.closest('a') || e.target.closest('button') || e.target.closest('input'))) {
                    e.stopPropagation();
                    return;
                }
            }, true);
        });

 
        if (document.getElementById("AssetTypeID")) {
            const dropdown = document.getElementById("AssetTypeID");
            if (dropdown && dropdown.options.length <= 1) {
                dropdown.innerHTML = `<option value="">Loading...</option>`;
                fetch("/Admin/AssetType/GetAll")
                    .then(response => response.json())
                    .then(data => {
                        dropdown.innerHTML = `<option value="">-- Select Type --</option>`;
                        data.forEach(item => {
                            const opt = document.createElement("option");
                            opt.value = item.assetTypeID ?? item.assetTypeId ?? item.assetTypeID; 
                            opt.textContent = item.assetTypeName ?? item.assetType ?? item.name ?? 'Unknown';
                            dropdown.appendChild(opt);
                        });
                    })
                    .catch(err => {
                        console.error(err);
                        dropdown.innerHTML = `<option value="">Failed to load</option>`;
                    });
            }
        }

        if (document.getElementById("AssetID")) {
            const Assetdropdown = document.getElementById("AssetID");
            if (Assetdropdown && Assetdropdown.options.length <= 1) {
                Assetdropdown.innerHTML = `<option value="">Loading...</option>`;
                fetch("/Admin/AssetCheckList/getAllAsset")
                    .then(response => response.json())
                    .then(data => {
                        Assetdropdown.innerHTML = `<option value="">-- Select Asset --</option>`;
                        if (Array.isArray(data) && data.length > 0) {
                            data.forEach(item => {
                                const opt = document.createElement("option");
                                opt.value = item.assetID ?? item.assetId ?? item.id;
                                opt.textContent = item.assetName ?? item.name ?? 'Unknown';
                                Assetdropdown.appendChild(opt);
                            });
                        } else {
                            Assetdropdown.innerHTML = `<option value="">No Assets Found</option>`;
                        }
                    })
                    .catch(err => {
                        console.error(err);
                        Assetdropdown.innerHTML = `<option value="">Failed to load</option>`;
                    });
            }
        }

        if (document.getElementById("InspectionCheckListID")) {
            const Checklistdropdown = document.getElementById("InspectionCheckListID");
            if (Checklistdropdown && Checklistdropdown.options.length <= 1) {
                Checklistdropdown.innerHTML = `<option value="">Loading...</option>`;
                fetch("/Admin/AssetCheckList/getAllChecklist")
                    .then(response => response.json())
                    .then(data => {
                        Checklistdropdown.innerHTML = `<option value="">-- Select Type --</option>`;
                        data.forEach(item => {
                            const opt = document.createElement("option");
                            opt.value = item.inspectionCheckListID ?? item.id;
                            opt.textContent = item.inspectionCheckListName ?? item.name ?? 'Unknown';
                            Checklistdropdown.appendChild(opt);
                        });
                    })
                    .catch(err => {
                        console.error(err);
                        Checklistdropdown.innerHTML = `<option value="">Failed to load</option>`;
                    });
            }
        }
        document.documentElement.addEventListener('click', function (e) {
            const el = e.target.closest && e.target.closest('[data-action]');
            if (!el) return;

            const rawAction = (el.getAttribute('data-action') || '').toString();
            const action = rawAction.trim().toLowerCase();
            log('clicked data-action=', action, el);

            if (action === 'navigate' || action === 'cancel') {
                e.preventDefault();
                handleNavigate(el);
                return;
            }
            if (action === "save") {
                e.preventDefault();
                handleSave(el);
                return;
            }
            if (action === 'delete') {
                e.preventDefault();
                if (el.getAttribute && !el.getAttribute('data-original')) el.setAttribute('data-original', el.innerHTML);
                handleDelete(el);
                return;
            }
            if (action === 'generate-qr') {
                e.preventDefault();

                const url = el.getAttribute('data-url');
                if (!url) {
                    showToast("Missing QR URL", "danger");
                    return;
                }

                const originalHTML = el.innerHTML;
                el.disabled = true;
                el.innerHTML = "Generating...";

                fetch(url, {
                    method: "GET",
                    headers: { "X-Requested-With": "XMLHttpRequest" }
                })
                    .then(res => {
                        if (!res.ok) throw new Error("HTTP " + res.status);
                        const ct = (res.headers.get("content-type") || "").toLowerCase();
                        if (ct.includes("application/json")) {
                            return res.json().then(json => ({ json }));
                        }
                        return res.text().then(html => ({ html }));
                    })
                    .then(result => {
                        if (result.json) {
                            const imageUrl = result.json.imageUrl;
                            if (!imageUrl) { showToast("QR generated but no image returned", "warning"); return; }
                            showQrModal(imageUrl);
                            showToast("QR generated successfully", "success");
                            return;
                        }
                        const parser = new DOMParser();
                        const doc = parser.parseFromString(result.html, "text/html");
                        const img = doc.querySelector("img");
                        if (!img) {
                            showToast("QR generated but image missing", "warning");
                            return;
                        }
                        showQrModal(img.src);
                        showToast("QR generated successfully", "success");
                    })
                    .catch(err => {
                        console.error("QR Error:", err);
                        showToast("QR generation error", "danger");
                    })
                    .finally(() => {
                        el.disabled = false;
                        el.innerHTML = originalHTML;
                    });
                return;
            }

        }, true); 

        document.querySelectorAll('form.js-ajax-delete').forEach(form => {
            form.addEventListener('submit', function (e) {
                e.preventDefault();
                handleAjaxDeleteForm(form);
            });
        });

        log('admin-actions loaded');
    });

})();
