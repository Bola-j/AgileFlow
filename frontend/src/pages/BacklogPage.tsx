const backlog = [
  { key: "AF-101", title: "Create project header", status: "Todo" },
  { key: "AF-102", title: "Wire API base URL", status: "In Progress" },
  { key: "AF-103", title: "Add board view", status: "Todo" }
];

export default function BacklogPage() {
  return (
    <section className="rounded-lg border border-slate-800 bg-slate-900 p-6">
      <h2 className="text-lg font-semibold">Backlog</h2>
      <div className="mt-4 space-y-2">
        {backlog.map((item) => (
          <div
            key={item.key}
            className="flex items-center justify-between rounded-md border border-slate-800 p-3 text-sm"
          >
            <div>
              <span className="text-slate-400">{item.key}</span>
              <span className="ml-3">{item.title}</span>
            </div>
            <span className="rounded-full bg-slate-800 px-2 py-1 text-xs text-slate-300">
              {item.status}
            </span>
          </div>
        ))}
      </div>
    </section>
  );
}
