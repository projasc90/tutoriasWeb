type SectionEyebrowProps = {
  children: React.ReactNode;
  tone?: "primary";
  className?: string;
};

export function SectionEyebrow({ children, className }: SectionEyebrowProps) {
  return (
    <span
      className={`text-label-md uppercase tracking-widest text-primary font-bold ${className ?? ""}`}
    >
      {children}
    </span>
  );
}

type SectionTitleProps = {
  children: React.ReactNode;
  className?: string;
};

export function SectionTitle({ children, className }: SectionTitleProps) {
  return (
    <h2
      className={`text-headline-lg font-bold tracking-tight text-on-surface ${className ?? ""}`}
    >
      {children}
    </h2>
  );
}

type SectionSubtitleProps = {
  children: React.ReactNode;
  className?: string;
};

export function SectionSubtitle({ children, className }: SectionSubtitleProps) {
  return (
    <p
      className={`text-body-md text-on-surface-variant mt-2 ${className ?? ""}`}
    >
      {children}
    </p>
  );
}
