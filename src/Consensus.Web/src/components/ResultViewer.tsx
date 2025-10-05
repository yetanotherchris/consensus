import React, { useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  Collapse,
} from '@mui/material';
import DownloadIcon from '@mui/icons-material/Download';
import VisibilityIcon from '@mui/icons-material/Visibility';
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff';
import type { LogEntryModel } from '../types/api';
import { LogViewer } from './LogViewer';

interface ResultViewerProps {
  html: string;
  logs: LogEntryModel[];
  runId: string;
}

export const ResultViewer: React.FC<ResultViewerProps> = ({ html, logs, runId }) => {
  const [showLogs, setShowLogs] = useState(false);

  const handleDownload = () => {
    const blob = new Blob([html], { type: 'text/html' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `consensus-${runId}.html`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Box sx={{ display: 'flex', gap: 2, justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h5" sx={{ fontWeight: 500 }}>
          Result
        </Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant="outlined"
            startIcon={showLogs ? <VisibilityOffIcon /> : <VisibilityIcon />}
            onClick={() => setShowLogs(!showLogs)}
          >
            {showLogs ? 'Hide Logs' : 'View Logs'}
          </Button>
          <Button
            variant="contained"
            startIcon={<DownloadIcon />}
            onClick={handleDownload}
          >
            Download HTML
          </Button>
        </Box>
      </Box>

      <Collapse in={showLogs}>
        <LogViewer logs={logs} />
      </Collapse>

      <Paper
        elevation={2}
        sx={{
          p: 0,
          height: '600px',
          borderRadius: 2,
          overflow: 'hidden',
        }}
      >
        <iframe
          srcDoc={html}
          title="Consensus Result"
          style={{
            width: '100%',
            height: '100%',
            border: 'none',
          }}
        />
      </Paper>
    </Box>
  );
};
