/**
 * verify-email.js
 * Reads userId and token from the URL query string, calls the confirm-email
 * API endpoint, then transitions to a success or error state.
 */

const API_BASE = 'https://localhost:7001/api';

const stateLoading = document.getElementById('state-loading');
const stateSuccess = document.getElementById('state-success');
const stateError   = document.getElementById('state-error');
const successMsg   = document.getElementById('success-message');
const errorMsg     = document.getElementById('error-message');
const resendBtn    = document.getElementById('resend-btn');

function showState(name) {
    stateLoading.style.display = name === 'loading' ? '' : 'none';
    stateSuccess.style.display = name === 'success' ? '' : 'none';
    stateError.style.display   = name === 'error'   ? '' : 'none';
}

async function confirmEmail() {
    const params = new URLSearchParams(window.location.search);
    const userId = params.get('userId');
    const token  = params.get('token');

    if (!userId || !token) {
        errorMsg.textContent = 'This confirmation link is missing required parameters.';
        showState('error');
        return;
    }

    try {
        const resp = await fetch(
            `${API_BASE}/auth/confirm-email?userId=${encodeURIComponent(userId)}&token=${encodeURIComponent(token)}`,
            { method: 'GET', headers: { 'Content-Type': 'application/json' } }
        );

        const data = await resp.json();

        if (resp.ok && data.confirmed) {
            successMsg.textContent = data.message || 'Email confirmed. You can now log in.';
            showState('success');
        } else {
            errorMsg.textContent = data.message || 'The confirmation link is invalid or has expired.';
            // Store email for resend (if server returned it)
            if (data.email) resendBtn.dataset.email = data.email;
            showState('error');
        }
    } catch (err) {
        console.error('Confirm-email fetch failed:', err);
        errorMsg.textContent = 'Network error. Please check your connection and try again.';
        showState('error');
    }
}

// Resend button handler
resendBtn?.addEventListener('click', async () => {
    const email = resendBtn.dataset.email;
    if (!email) {
        alert('Please go back to the login page and use the "Resend verification email" option.');
        return;
    }

    resendBtn.disabled = true;
    resendBtn.textContent = 'Sending…';

    try {
        await fetch(`${API_BASE}/auth/resend-confirmation`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email }),
        });
        resendBtn.textContent = 'Email sent!';
    } catch {
        resendBtn.textContent = 'Failed — try again';
        resendBtn.disabled = false;
    }
});

// Kick off on load
confirmEmail();
