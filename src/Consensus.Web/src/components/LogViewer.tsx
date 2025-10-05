import React, { useMemo } from 'react';
import { Paper, Typography } from '@mui/material';
import { LazyLog } from 'react-lazylog';
import type { LogEntryModel } from '../types/api';

// Theme colors
const COLORS = {
  background: '#1e1e1e',
  text: '#d4d4d4',
  textMuted: '#888',
} as const;

// Style configurations
const PAPER_STYLES = {
  p: 2,
  bgcolor: COLORS.background,
  borderRadius: 2,
  width: '100%',
} as const;

interface LogViewerProps {
  logs: LogEntryModel[];
}

export const LogViewer: React.FC<LogViewerProps> = ({ logs }) => {
  const logText = useMemo(() => {
    if (logs.length === 0) {
      return 'No logs available yet...';
    }
    
    return logs
      .map(log => {
        const timestamp = new Date(log.timestamp).toLocaleTimeString();
        return `[${timestamp}] ${log.message}`;
      })
      .join('\n');
  }, [logs]);

  return (
    <Paper elevation={2} sx={PAPER_STYLES}>
      <Typography variant="h6" sx={{ mb: 2, color: COLORS.text }}>
        Logs
      </Typography>
      <div style={{ height: '400px', backgroundColor: COLORS.background }}>
        <LazyLog
          text={logText}
          enableSearch={false}
          follow
          selectableLines
          extraLines={1}
          style={{
            backgroundColor: COLORS.background,
            color: COLORS.text,
            fontSize: '13px',
            lineHeight: '1.4',
          }}
        />
      </div>
    </Paper>
  );
};
