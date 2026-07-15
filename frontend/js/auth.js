/**
 * auth.js — AgileFlow authentication flows
 * Handles: register, login, password-visibility toggles.
 * Register now shows a "check your email" panel instead of logging in.
 * Login detects the requiresEmailConfirmation flag and shows a resend banner.
 */

const API_BASE = 'https://localhost:7001/api';

// ── Utility helpers ────────────────────────────────────────────────────────

function togglePasswordVisibility(inputEl, iconEl, btnEl, labelBase) {
    if (!inputEl || !iconEl || !btnEl) return;
    const isHidden = inputEl.type === 'password';
    inputEl.type = isHidden ? 'text' : 'password';
    iconEl.className = isHidden ? 'bi bi-eye-slash' : 'bi bi-eye';
    btnEl.setAttribute('aria-label', isHidden ? `Hide ${labelBase}` : `Show ${labelBase}`);
}

function showAlert(containerId, message, type = 'danger') {
    const el = document.getElementById(containerId);
    if (!el) return;
    el.innerHTML = `<div class="alert alert-${type} py-2 mb-0" role="alert">${message}</div>`;
    el.style.display = '';
}

function hideAlert(containerId) {
    const el = document.getElementById(containerId);
    if (el) { el.innerHTML = ''; el.style.display = 'none'; }
}

// ── Password-toggle wiring ─────────────────────────────────────────────────

const bindToggle = (btnId, iconId, inputId, label) => {
    const btn  = document.getElementById(btnId);
    const icon = document.getElementById(iconId);
    const inp  = document.getElementById(inputId);
    if (btn) btn.addEventListener('click', () => togglePasswordVisibility(inp, icon, btn, label));
};

bindToggle('toggle-password',              'toggle-password-icon',              'password',              'password');
bindToggle('toggle-reg-password',          'toggle-reg-password-icon',          'reg-password',          'password');
bindToggle('toggle-reg-confirm-password',  'toggle-reg-confirm-password-icon',  'reg-confirm-password',  'confirm password');

// ── Register flow ──────────────────────────────────────────────────────────

const registerForm       = document.getElementById('register-form');
const checkEmailPanel    = document.getElementById('check-email-panel');
const checkEmailAddress  = document.getElementById('check-email-address');

if (registerForm) {
    registerForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        hideAlert('register-alert');

        const fullName        = document.getElementById('reg-name').value.trim();
        const email           = document.getElementById('reg-email').value.trim();
        const password        = document.getElementById('reg-password').value;
        const confirmPassword = document.getElementById('reg-confirm-password').value;

        if (password !== confirmPassword) {
            showAlert('register-alert', 'Passwords do not match. Please re-enter them.');
            return;
        }

        // Split full name into first / last (fallback: last = "")
        const parts     = fullName.split(' ');
        const firstName = parts[0] || fullName;
        const lastName  = parts.slice(1).join(' ') || '.';

        const submitBtn = registerForm.querySelector('button[type="submit"]');
        submitBtn.disabled = true;
        submitBtn.textContent = 'Creating account…';

        try {
            const resp = await fetch(`${API_BASE}/auth/register`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ firstName, lastName, email, password }),
            });

            const data = await resp.json();

            if (!resp.ok) {
                const msg = data.message || 'Registration failed. Please try again.';
                showAlert('register-alert', msg);
                return;
            }

            // Success → show the "check your email" panel
            if (checkEmailAddress) checkEmailAddress.textContent = email;
            registerForm.style.display = 'none';
            if (checkEmailPanel) checkEmailPanel.style.display = '';

        } catch (err) {
            console.error('Register error:', err);
            showAlert('register-alert', 'Network error. Please check your connection and try again.');
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = 'Sign Up';
        }
    });
}

// ── Login flow ─────────────────────────────────────────────────────────────

const loginForm           = document.getElementById('login-form');
const resendBanner        = document.getElementById('resend-verification-banner');
const resendEmailDisplay  = document.getElementById('resend-email-display');
const resendBtn           = document.getElementById('resend-verification-btn');

let unverifiedEmail = '';

if (loginForm) {
    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        hideAlert('login-alert');
        if (resendBanner) resendBanner.style.display = 'none';

        const email    = document.getElementById('email').value.trim();
        const password = document.getElementById('password').value;

        const submitBtn = loginForm.querySelector('button[type="submit"]');
        submitBtn.disabled = true;
        submitBtn.textContent = 'Logging in…';

        try {
            const resp = await fetch(`${API_BASE}/auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password }),
            });

            const data = await resp.json();

            if (resp.status === 403 && data.requiresEmailConfirmation) {
                // Unverified email — show resend banner
                unverifiedEmail = data.email || email;
                if (resendEmailDisplay) resendEmailDisplay.textContent = unverifiedEmail;
                if (resendBanner) resendBanner.style.display = '';
                showAlert('login-alert', data.message || 'Please verify your email before logging in.', 'warning');
                return;
            }

            if (!resp.ok) {
                showAlert('login-alert', data.message || 'Invalid email or password.');
                return;
            }

            // Successful login — store tokens and redirect
            localStorage.setItem('agileflow_token',         data.accessToken);
            localStorage.setItem('agileflow_refresh_token', data.refreshToken);
            localStorage.setItem('agileflow_user_id',       data.userId);
            localStorage.setItem('agileflow_role',          data.role);
            window.location.href = 'workspaces.html';

        } catch (err) {
            console.error('Login error:', err);
            showAlert('login-alert', 'Network error. Please check your connection and try again.');
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = 'Login';
        }
    });
}

// Resend verification button
if (resendBtn) {
    resendBtn.addEventListener('click', async () => {
        resendBtn.disabled = true;
        resendBtn.textContent = 'Sending…';

        try {
            await fetch(`${API_BASE}/auth/resend-confirmation`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email: unverifiedEmail }),
            });
            resendBtn.textContent = '✓ Sent!';
            showAlert('login-alert', 'Verification email resent. Please check your inbox.', 'success');
        } catch {
            resendBtn.disabled = false;
            resendBtn.textContent = 'Resend email';
            showAlert('login-alert', 'Could not resend email. Please try again later.');
        }
    });
}
