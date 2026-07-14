const currentUser = {
    name: "User Name",
    email: "username@agileflow.com"
};

const profileNameInput = document.getElementById('profile-name');
const profileEmailInput = document.getElementById('profile-email');
const profilePasswordInput = document.getElementById('profile-password');
const toggleProfilePasswordButton = document.getElementById('toggle-profile-password');
const toggleProfilePasswordIcon = document.getElementById('toggle-profile-password-icon');
const deleteAccountButton = document.getElementById('delete-account-btn');
const logoutButton = document.getElementById('logout-btn');
const profileForm = document.getElementById('profile-form');

if (profileNameInput) {
    profileNameInput.value = currentUser.name;
}

if (profileEmailInput) {
    profileEmailInput.value = currentUser.email;
}

function togglePasswordVisibility(inputElement, iconElement, buttonElement) {
    if (!inputElement || !iconElement || !buttonElement) {
        return;
    }

    const isHidden = inputElement.type === 'password';
    inputElement.type = isHidden ? 'text' : 'password';
    iconElement.className = isHidden ? 'bi bi-eye-slash' : 'bi bi-eye';
    buttonElement.setAttribute('aria-label', isHidden ? 'Hide password' : 'Show password');
}

if (toggleProfilePasswordButton) {
    toggleProfilePasswordButton.addEventListener('click', () => {
        togglePasswordVisibility(profilePasswordInput, toggleProfilePasswordIcon, toggleProfilePasswordButton);
    });
}

if (deleteAccountButton) {
    deleteAccountButton.addEventListener('click', () => {
        const confirmed = confirm('Are you sure you want to delete your account? This action cannot be undone.');

        if (!confirmed) {
            return;
        }

        localStorage.removeItem('agileflow_token');
        alert('Your account has been deleted from this browser session.');
        window.location.href = 'login.html';
    });
}

if (logoutButton) {
    logoutButton.addEventListener('click', () => {
        const confirmed = confirm('Are you sure you want to logout?');

        if (!confirmed) {
            return;
        }

        localStorage.removeItem('agileflow_token');
        window.location.href = 'login.html';
    });
}

if (profileForm) {
    profileForm.addEventListener('submit', (e) => {
        e.preventDefault();

        const updatedName = profileNameInput?.value;
        const newPassword = profilePasswordInput?.value;

        console.log("Updating profile...");
        console.log("New Name:", updatedName);
        
        if (newPassword) {
            console.log("Password will be updated.");
        }

        alert("Profile updated successfully!");
    });
}

const logoutBtn = document.getElementById('logout-btn');

if (logoutBtn) {
    logoutBtn.addEventListener('click', () => {

        const isConfirmed = confirm("Are you sure you want to logout?");
        
        if (isConfirmed) {

            localStorage.removeItem('agileflow_token');
            
            window.location.href = 'login.html';
        }
    });
}