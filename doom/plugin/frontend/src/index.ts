import { Doom } from "./Doom";

if (typeof window !== "undefined") {
  (window as unknown as Record<string, unknown>).Ivy_Tendril_Plugin_Doom = {
    Doom,
  };
}

export { Doom };
