const projects = [
  { key: "AF", name: "AgileFlow Platform" },
  { key: "MKT", name: "Marketing Site" }
];

export default function ProjectsPage() {
  return (
    <section className="rounded-lg border border-slate-800 bg-slate-900 p-6">
      <h2 className="text-lg font-semibold">Projects</h2>
      <div className="mt-4 grid gap-3 md:grid-cols-2">
        {projects.map((project) => (
          <div key={project.key} className="rounded-md border border-slate-800 p-4">
            <div className="text-sm text-slate-400">{project.key}</div>
            <div className="text-base font-medium">{project.name}</div>
          </div>
        ))}
      </div>
    </section>
  );
}
