import { useMemo, useEffect, useRef } from 'react';
import type { LogEntryModel } from '../types/api';

interface LogViewerProps {
  logs: LogEntryModel[];
}

export function LogViewer({ logs }: LogViewerProps) {
  const logEndRef = useRef<HTMLDivElement>(null);

  const logText = useMemo(() => {
    if (logs.length === 0) {
      return [];
    }
    
    return logs.map(log => {
      const timestamp = new Date(log.timestamp).toLocaleTimeString();
      return `[${timestamp}] ${log.message}`;
    });
  }, [logs]);

  // Auto-scroll to bottom when new logs arrive
  useEffect(() => {
    logEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [logs]);

  return (
    <div className="p-4 bg-[#1e1e1e] rounded-lg shadow-lg w-full">
      <h2 className="text-lg font-semibold mb-4 text-[#d4d4d4]">
        Logs
      </h2>
      <div className="h-[400px] w-full bg-[#1e1e1e] overflow-auto">
        <pre className="text-[#d4d4d4] text-[13px] leading-relaxed whitespace-pre-wrap font-mono p-2">
          {logText.length === 0 ? (
            <div className="text-[#888]">No logs available yet...</div>
          ) : (
            logText.map((line, index) => (
              <div key={index} className="hover:bg-[#2d2d2d] select-text">
                {line}
              </div>
            ))
          )}
          <div ref={logEndRef} />
        </pre>
      </div>
    </div>
  );
}
