import { UserRound } from "lucide-react";
import { useEffect, useState } from "react";
import { cn, initials } from "@/lib/utils";
import { apiBaseURL } from "@/services/apiClient";

interface UserAvatarProps {
  src?: string | null;
  name?: string | null;
  email?: string | null;
  className?: string;
}

export function UserAvatar({ src, name, email, className }: UserAvatarProps) {
  const [failed, setFailed] = useState(false);
  const label = name || email || "User";
  const safeSrc = src?.trim();
  const imageSrc = safeSrc?.startsWith("/") ? `${apiBaseURL}${safeSrc}` : safeSrc;

  useEffect(() => {
    setFailed(false);
  }, [safeSrc]);

  return (
    <span
      className={cn(
        "inline-flex h-10 w-10 shrink-0 items-center justify-center overflow-hidden rounded-full border bg-muted text-sm font-semibold text-muted-foreground shadow-sm ring-2 ring-background",
        className,
      )}
      aria-label={label}
      title={label}
    >
      {imageSrc && !failed ? (
        <img
          src={imageSrc}
          alt={label}
          className="h-full w-full object-cover"
          referrerPolicy="no-referrer"
          onError={() => setFailed(true)}
        />
      ) : name || email ? (
        <span>{initials(label)}</span>
      ) : (
        <UserRound className="h-5 w-5" aria-hidden="true" />
      )}
    </span>
  );
}
