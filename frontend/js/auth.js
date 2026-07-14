console.log("Auth.js is loaded successfully!");

const loginForm = document.getElementById('login-form');
const passwordInput = document.getElementById('password');

if (loginForm) {
    loginForm.addEventListener('submit', (e) => {
        e.preventDefault();
        
        const email = document.getElementById('email').value;
        const password = document.getElementById('password').value;
        console.log("Logging in...", email);

        localStorage.setItem('agileflow_token', 'dummy-jwt-token-12345');
        window.location.href = 'workspaces.html';
    });
}

const registerForm = document.getElementById('register-form');
const registerPassword = document.getElementById('reg-password');
const registerConfirmPassword = document.getElementById('reg-confirm-password');
const toggleRegisterPasswordButton = document.getElementById('toggle-reg-password');
const toggleRegisterPasswordIcon = document.getElementById('toggle-reg-password-icon');
const toggleRegisterConfirmPasswordButton = document.getElementById('toggle-reg-confirm-password');
const toggleRegisterConfirmPasswordIcon = document.getElementById('toggle-reg-confirm-password-icon');
const toggleLoginPasswordButton = document.getElementById('toggle-password');
const toggleLoginPasswordIcon = document.getElementById('toggle-password-icon');

function togglePasswordVisibility(inputElement, iconElement, buttonElement, labelBase) {
    if (!inputElement || !iconElement || !buttonElement) {
        return;
    }

    const isHidden = inputElement.type === 'password';
    inputElement.type = isHidden ? 'text' : 'password';
    iconElement.className = isHidden ? 'bi bi-eye-slash' : 'bi bi-eye';
    buttonElement.setAttribute('aria-label', isHidden ? `Hide ${labelBase}` : `Show ${labelBase}`);
}

if (toggleRegisterPasswordButton) {
    toggleRegisterPasswordButton.addEventListener('click', () => {
        togglePasswordVisibility(registerPassword, toggleRegisterPasswordIcon, toggleRegisterPasswordButton, 'password');
    });
}

if (toggleRegisterConfirmPasswordButton) {
    toggleRegisterConfirmPasswordButton.addEventListener('click', () => {
        togglePasswordVisibility(registerConfirmPassword, toggleRegisterConfirmPasswordIcon, toggleRegisterConfirmPasswordButton, 'confirm password');
    });
}

if (toggleLoginPasswordButton) {
    toggleLoginPasswordButton.addEventListener('click', () => {
        togglePasswordVisibility(passwordInput, toggleLoginPasswordIcon, toggleLoginPasswordButton, 'password');
    });
}

if (registerForm) {
    registerForm.addEventListener('submit', (e) => {
        e.preventDefault();
        
        const name = document.getElementById('reg-name').value;
        const email = document.getElementById('reg-email').value;
        const password = registerPassword?.value;
        const confirmPassword = registerConfirmPassword?.value;

        if (password !== confirmPassword) {
            alert('Passwords do not match. Please re-enter them.');
            return;
        }

        console.log("Registering...", name, email);

        localStorage.setItem('agileflow_token', 'dummy-jwt-token-12345');
        window.location.href = 'workspaces.html';
    });
}
