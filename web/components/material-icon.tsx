import type { CSSProperties } from "react";

const VARIABLE_AXIS_STYLE: CSSProperties = {
  fontVariationSettings: "'FILL' 1, 'wght' 400, 'GRAD' 0, 'opsz' 24",
};

type MaterialIconProps = {
  name: string;
  className?: string;
  style?: CSSProperties;
  filled?: boolean;
  "aria-hidden"?: boolean | "true" | "false";
};

/**
 * Thin wrapper around Google Material Symbols Outlined icon font.
 * Loaded once via the MaterialSymbols component in the root layout.
 */
export function MaterialIcon({
  name,
  className,
  style,
  filled = false,
  "aria-hidden": ariaHidden = true,
}: MaterialIconProps) {
  const mergedStyle: CSSProperties = {
    ...(filled ? VARIABLE_AXIS_STYLE : {}),
    ...(style ?? {}),
  };
  return (
    <span
      className={`material-symbols-outlined leading-none ${className ?? ""}`}
      style={mergedStyle}
      aria-hidden={ariaHidden}
    >
      {name}
    </span>
  );
}

/**
 * Loads Material Symbols Outlined + the variable axis (weight/fill/grade)
 * stylesheets from Google Fonts.  Rendered once in the root layout.
 */
export function MaterialSymbols() {
  return (
    <>
      <link
        rel="stylesheet"
        href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:opsz,wght,FILL,GRAD@24,400,0,0&display=swap"
      />
      <link
        rel="stylesheet"
        href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&display=swap"
      />
    </>
  );
}
