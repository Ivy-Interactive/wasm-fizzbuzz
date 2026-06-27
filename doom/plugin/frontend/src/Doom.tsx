import React, { useRef, useEffect, useState } from "react";

interface DoomProps {
  id: string;
  canvasWidth?: number;
  canvasHeight?: number;
}

const DOOM_SCREEN_WIDTH = 320 * 2;
const DOOM_SCREEN_HEIGHT = 200 * 2;

function doomKeyCode(keyCode: number): number {
  switch (keyCode) {
    case 8:
      return 127; // KEY_BACKSPACE
    case 17:
      return 0x80 + 0x1d; // KEY_RCTRL
    case 18:
      return 0x80 + 0x38; // KEY_RALT
    case 37:
      return 0xac; // KEY_LEFTARROW
    case 38:
      return 0xad; // KEY_UPARROW
    case 39:
      return 0xae; // KEY_RIGHTARROW
    case 40:
      return 0xaf; // KEY_DOWNARROW
    default:
      if (keyCode >= 65 && keyCode <= 90) {
        return keyCode + 32; // ASCII to lower case
      }
      if (keyCode >= 112 && keyCode <= 123) {
        return keyCode + 75; // KEY_F1
      }
      return keyCode;
  }
}

export const Doom: React.FC<DoomProps> = ({
  canvasWidth = 640,
  canvasHeight = 400,
}) => {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [status, setStatus] = useState<string>("Click to load DOOM");
  const [loaded, setLoaded] = useState(false);
  const instanceRef = useRef<WebAssembly.Instance | null>(null);
  const memoryRef = useRef<WebAssembly.Memory | null>(null);
  const animFrameRef = useRef<number>(0);

  const startDoom = async () => {
    if (loaded) {
      canvasRef.current?.focus();
      return;
    }

    setStatus("Loading DOOM...");

    try {
      const memory = new WebAssembly.Memory({ initial: 108 });
      memoryRef.current = memory;

      const canvas = canvasRef.current!;
      const ctx = canvas.getContext("2d")!;

      const drawCanvas = (ptr: number) => {
        const doomScreen = new Uint8ClampedArray(
          memory.buffer,
          ptr,
          DOOM_SCREEN_WIDTH * DOOM_SCREEN_HEIGHT * 4
        );
        const renderScreen = new ImageData(
          doomScreen,
          DOOM_SCREEN_WIDTH,
          DOOM_SCREEN_HEIGHT
        );
        ctx.putImageData(renderScreen, 0, 0);
      };

      const readWasmString = (offset: number, length: number): string => {
        const bytes = new Uint8Array(memory.buffer, offset, length);
        return new TextDecoder("utf8").decode(bytes);
      };

      const appendOutput = (_style: string) => {
        return (offset: number, length: number) => {
          const str = readWasmString(offset, length);
          console.log(`[DOOM ${_style}]`, str);
        };
      };

      const importObject: WebAssembly.Imports = {
        js: {
          js_console_log: appendOutput("log"),
          js_stdout: appendOutput("stdout"),
          js_stderr: appendOutput("stderr"),
          js_milliseconds_since_start: () => performance.now(),
          js_draw_screen: drawCanvas,
        },
        env: {
          memory,
        },
      };

      const response = await WebAssembly.instantiateStreaming(
        fetch("/ivy/plugins/doom/doom.wasm"),
        importObject
      );

      instanceRef.current = response.instance;

      // Initialize DOOM
      (response.instance.exports.main as Function)();

      setStatus("");
      setLoaded(true);
      canvas.focus();

      // Game loop
      const step = () => {
        (response.instance.exports.doom_loop_step as Function)();
        animFrameRef.current = requestAnimationFrame(step);
      };
      animFrameRef.current = requestAnimationFrame(step);
    } catch (err) {
      setStatus(`Failed to load DOOM: ${err}`);
      console.error("DOOM load error:", err);
    }
  };

  useEffect(() => {
    return () => {
      if (animFrameRef.current) {
        cancelAnimationFrame(animFrameRef.current);
      }
    };
  }, []);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas || !instanceRef.current) return;

    const instance = instanceRef.current;
    const keyDown = (keyCode: number) => {
      (instance.exports.add_browser_event as Function)(0, keyCode);
    };
    const keyUp = (keyCode: number) => {
      (instance.exports.add_browser_event as Function)(1, keyCode);
    };

    const handleKeyDown = (e: KeyboardEvent) => {
      keyDown(doomKeyCode(e.keyCode));
      e.preventDefault();
    };
    const handleKeyUp = (e: KeyboardEvent) => {
      keyUp(doomKeyCode(e.keyCode));
      e.preventDefault();
    };

    canvas.addEventListener("keydown", handleKeyDown);
    canvas.addEventListener("keyup", handleKeyUp);

    return () => {
      canvas.removeEventListener("keydown", handleKeyDown);
      canvas.removeEventListener("keyup", handleKeyUp);
    };
  }, [loaded]);

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        gap: "8px",
      }}
    >
      <canvas
        ref={canvasRef}
        width={canvasWidth}
        height={canvasHeight}
        tabIndex={0}
        onClick={startDoom}
        style={{
          border: "2px solid var(--border)",
          borderRadius: "8px",
          cursor: loaded ? "default" : "pointer",
          background: "#000",
          imageRendering: "pixelated",
        }}
      />
      {status && (
        <span style={{ color: "var(--muted-foreground)", fontSize: "14px" }}>
          {status}
        </span>
      )}
    </div>
  );
};
