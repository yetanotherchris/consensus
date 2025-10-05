import React from 'react';
import { Box, Paper, Typography, List, ListItem, ListItemText } from '@mui/material';
import type { LogEntryModel } from '../types/api';

interface LogViewerProps {
  logs: LogEntryModel[];
}

export const LogViewer: React.FC<LogViewerProps> = ({ logs }) => {
  return (
    <Paper
      elevation={2}
      sx={{
        p: 2,
        maxHeight: '400px',
        overflowY: 'auto',
        bgcolor: '#1e1e1e',
        color: '#d4d4d4',
        fontFamily: 'monospace',
        borderRadius: 2,
      }}
    >
      <Typography variant="h6" sx={{ mb: 2, color: '#d4d4d4' }}>
        Logs
      </Typography>
      {logs.length === 0 ? (
        <Typography sx={{ color: '#888' }}>No logs available yet...</Typography>
      ) : (
        <List dense sx={{ p: 0 }}>
          {logs.map((log, index) => (
            <ListItem key={index} sx={{ px: 0, py: 0.5 }}>
              <ListItemText
                primary={
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    <Typography
                      component="span"
                      sx={{ color: '#4ec9b0', fontSize: '0.85rem' }}
                    >
                      [{new Date(log.timestamp).toLocaleTimeString()}]
                    </Typography>
                    <Typography
                      component="span"
                      sx={{ color: '#d4d4d4', fontSize: '0.85rem' }}
                    >
                      {log.message}
                    </Typography>
                  </Box>
                }
              />
            </ListItem>
          ))}
        </List>
      )}
    </Paper>
  );
};
