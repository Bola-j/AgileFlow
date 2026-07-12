
const urlParams = new URLSearchParams(window.location.search);
const projectId = parseInt(urlParams.get('projectId'));


const mockTasks = [
    { id: 1001, projectId: 101, title: "Design Login Page", status: "Done" },
    { id: 1002, projectId: 101, title: "Validate User Input", status: "In Progress" },
    { id: 1003, projectId: 101, title: "Connect to .NET API", status: "To Do" },
    { id: 1004, projectId: 102, title: "Setup Database Tables", status: "To Do" }
];


const todoCol = document.getElementById('todo-column');
const inProgressCol = document.getElementById('inprogress-column');
const doneCol = document.getElementById('done-column');


function renderTasks() {

    todoCol.innerHTML = '';
    inProgressCol.innerHTML = '';
    doneCol.innerHTML = '';


    const projectTasks = mockTasks.filter(task => task.projectId === projectId);

    projectTasks.forEach(task => {
  
        const taskCard = `
            <div class="card task-card mb-3 shadow-sm border-start border-4 ${getBorderColor(task.status)}">
                <div class="card-body p-2">
                    <h6 class="card-title m-0">${task.title}</h6>
                </div>
            </div>
        `;


        if (task.status === "To Do") todoCol.innerHTML += taskCard;
        else if (task.status === "In Progress") inProgressCol.innerHTML += taskCard;
        else if (task.status === "Done") doneCol.innerHTML += taskCard;
    });
}


function getBorderColor(status) {
    if (status === "To Do") return "border-secondary";
    if (status === "In Progress") return "border-primary";
    if (status === "Done") return "border-success";
    return "";
}

renderTasks();

const createBtn = document.getElementById('create-task-btn');
const taskForm = document.getElementById('task-form');
const taskModal = new bootstrap.Modal(document.getElementById('createTaskModal'));

createBtn.addEventListener('click', () => taskModal.show());

taskForm.addEventListener('submit', (e) => {
    e.preventDefault();

    const newTask = {
        id: Math.floor(Math.random() * 10000),
        projectId: projectId,
        title: document.getElementById('task-title').value,
        status: document.getElementById('task-status').value
    };

    mockTasks.push(newTask);
    renderTasks();

    taskForm.reset();
    taskModal.hide();
});