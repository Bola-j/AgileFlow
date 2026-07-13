// 1. Get Workspace ID from URL

const urlParams = new URLSearchParams(window.location.search);
const workspaceId = parseInt(urlParams.get('workspaceId'));

// 2. Mock Data for Projects
const mockProjects = [
    { id: 101, workspaceId: 1, name: "Login Interface", status: "In Progress" },
    { id: 102, workspaceId: 1, name: "Dashboard UI", status: "To Do" },
    { id: 103, workspaceId: 2, name: "Auth Endpoints", status: "Done" },
    { id: 104, workspaceId: 3, name: "User Tables", status: "In Progress" }
];

// 3. Select the HTML container
const container = document.getElementById('projects-container');

// 4. Function to render projects
function renderProjects() {
    
    // Clear the container
    container.innerHTML = '';

    // Filter projects to only show ones belonging to this workspace
    const filteredProjects = mockProjects.filter(project => project.workspaceId === workspaceId);

    // If no projects found
    if (filteredProjects.length === 0) {
        container.innerHTML = '<p class="text-muted">No projects found in this workspace.</p>';
    }

    // Loop through filtered projects and create cards
    filteredProjects.forEach(project => {
        const card = `
            <div class="col-md-4 mb-3">
                <div class="card h-100 shadow-sm border-start border-primary border-4">
                    <div class="card-body">
                        <h5 class="card-title">${project.name}</h5>
                        <p class="card-text text-muted">Status: ${project.status}</p>
                        
                        <!-- Button to navigate to sprint board-->
                        <a href="sprint-board.html?projectId=${project.id}" class="btn btn-outline-success btn-sm">
                            View Sprints
                        </a>
                    </div>
                </div>
            </div>
        `;
        
        container.innerHTML += card;
    });
}

// 5. Execute the function
renderProjects();

// 6. Select Modal elements
const createBtn = document.getElementById('create-project-btn');
const projectForm = document.getElementById('project-form');
const projectModal = new bootstrap.Modal(document.getElementById('createProjectModal'));

// 7. Open modal
createBtn.addEventListener('click', () => {
    projectModal.show();
});

// 8. Handle form submission
projectForm.addEventListener('submit', (e) => {
    e.preventDefault();

    const newName = document.getElementById('project-name').value;
    const newStatus = document.getElementById('project-status').value;

    const newProject = {
        id: mockProjects.length + 101,
        workspaceId: workspaceId,
        name: newName,
        status: newStatus
    };

    mockProjects.push(newProject);
    renderProjects();

    projectForm.reset();
    projectModal.hide();
});

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