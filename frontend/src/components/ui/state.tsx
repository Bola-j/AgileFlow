import { AlertTriangle, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export function Spinner({ className }: { className?: string }) {
  return <Loader2 className={cn("h-4 w-4 animate-spin", className)} />;
}

export function Skeleton({ className }: { className?: string }) {
  return <div className={cn("animate-pulse rounded-md bg-muted", className)} />;
}

export function EmptyState({ title, description, action }: { title: string; description?: string; action?: React.ReactNode }) {
  return (
    <div className="flex min-h-48 flex-col items-center justify-center rounded-lg border border-dashed p-8 text-center">
      <p className="text-sm font-semibold">{title}</p>
      {description ? <p className="mt-1 max-w-md text-sm text-muted-foreground">{description}</p> : null}
      {action ? <div className="mt-4">{action}</div> : null}
    </div>
  );
}

export function ErrorState({ title = "Unable to load data", onRetry }: { title?: string; onRetry?: () => void }) {
  return (
    <div className="flex min-h-48 flex-col items-center justify-center rounded-lg border border-destructive/30 p-8 text-center">
      <AlertTriangle className="mb-3 h-8 w-8 text-destructive" />
      <p className="text-sm font-semibold">{title}</p>
      {onRetry ? <Button className="mt-4" variant="outline" onClick={onRetry}>Retry</Button> : null}
    </div>
  );
}
