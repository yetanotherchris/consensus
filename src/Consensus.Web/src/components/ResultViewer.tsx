import React, { useState } from 'react';
import {
  Box,
  Paper,
  Button,
  Collapse,
} from '@mui/material';
import DownloadIcon from '@mui/icons-material/Download';
import VisibilityIcon from '@mui/icons-material/Visibility';
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import type { LogEntryModel } from '../types/api';
import { LogViewer } from './LogViewer';
import { Header } from './Header';

interface ResultViewerProps {
  html: string;
  logs: LogEntryModel[];
  runId: string;
  onReset: () => void;
}

export const ResultViewer: React.FC<ResultViewerProps> = ({ html, logs, runId, onReset }) => {
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
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, height: '100%' }}>
      <Box sx={{ display: 'flex', gap: 1, justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Header noMargin />
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant="outlined"
            onClick={onReset}
            startIcon={<ArrowForwardIcon />}
            sx={{ textTransform: 'none' }}
          >
            New Prompt
          </Button>
          <Button
            variant="outlined"
            startIcon={showLogs ? <VisibilityOffIcon /> : <VisibilityIcon />}
            onClick={() => setShowLogs(!showLogs)}
            sx={{ textTransform: 'none' }}
          >
            {showLogs ? 'Hide Logs' : 'View Logs'}
          </Button>
          <Button
            variant="contained"
            startIcon={<DownloadIcon />}
            onClick={handleDownload}
            sx={{ textTransform: 'none' }}
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
          flex: 1,
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
