// 1. Mock Data representing tasks for the team
const mockWorkspaces = [
    { id: 1, name: "Frontend UI/UX", description: "User interface and JavaScript logic" },
    { id: 2, name: ".NET Backend API", description: "Building endpoints and controllers" },
    { id: 3, name: "Database Architecture", description: "SQL Server tables and relations" }
];

// 2. Select the HTML container
const container = document.getElementById('workspaces-container');

// 3. Function to render workspaces as cards
function renderWorkspaces() {
    
    // Clear the container first to prevent duplicates
    container.innerHTML = '';

    // Loop through the array and create a card for each item
    mockWorkspaces.forEach(workspace => {
        const card = `
            <div class="col-md-4 mb-3">
                <div class="card h-100 shadow-sm">
                    <div class="card-body">
                        <h5 class="card-title">${workspace.name}</h5>
                        <p class="card-text text-muted">${workspace.description}</p>
                        
                        <a href="project-details.html?workspaceId=${workspace.id}" class="btn btn-outline-primary btn-sm">
                            View Projects
                        </a>
                    </div>
                </div>
            </div>
        `;
        
        // Append the created card to the container
        container.innerHTML += card;
    });
}

// 4. Execute the function when the script loads
renderWorkspaces();

// 5. Select Modal elements
const createBtn = document.getElementById('create-workspace-btn');
const workspaceForm = document.getElementById('workspace-form');

// Initialize Bootstrap Modal
const workspaceModal = new bootstrap.Modal(document.getElementById('createWorkspaceModal'));

// 6. Open modal when clicking the create button
createBtn.addEventListener('click', () => {
    workspaceModal.show();
});

// 7. Handle form submission
workspaceForm.addEventListener('submit', (e) => {
    
    // Prevent page reload
    e.preventDefault();

    // Get values from inputs
    const newName = document.getElementById('workspace-name').value;
    const newDesc = document.getElementById('workspace-desc').value;

    // Create new object
    const newWorkspace = {
        id: mockWorkspaces.length + 1, // Generate a simple ID
        name: newName,
        description: newDesc
    };

    // Add new object to mock data
    mockWorkspaces.push(newWorkspace);

    // Re-render the interface to show the new card

    // Reset form fields and hide modal
    workspaceForm.reset();
    workspaceModal.hide();
});