import { useState, useEffect } from 'react';
import {
  Container,
  Box,
  Typography,
  ThemeProvider,
  createTheme,
  CssBaseline,
  CircularProgress,
  Alert,
} from '@mui/material';
import { PromptInput } from './components/PromptInput';
import { LogViewer } from './components/LogViewer';
import { ResultViewer } from './components/ResultViewer';
import { consensusApi } from './services/api';
import type { JobStatusModel, LogEntryModel } from './types/api';

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#10a37f',
    },
    background: {
      default: '#f7f7f8',
    },
  },
  typography: {
    fontFamily: '"Inter", "Segoe UI", "Roboto", sans-serif',
  },
});

function App() {
  const [jobStatus, setJobStatus] = useState<JobStatusModel | null>(null);
  const [logs, setLogs] = useState<LogEntryModel[]>([]);
  const [html, setHtml] = useState<string>('');
  const [error, setError] = useState<string>('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Poll for job status
  useEffect(() => {
    if (!jobStatus || jobStatus.status === 2) { // Finished
      return;
    }

    const interval = setInterval(async () => {
      try {
        const status = await consensusApi.getJobStatus(jobStatus.runId);
        setJobStatus(status);

        // If job is finished, fetch the HTML result
        if (status.status === 2) { // Finished
          const htmlResult = await consensusApi.getHtml(status.runId);
          setHtml(htmlResult);
        }
      } catch (err) {
        console.error('Error polling job status:', err);
      }
    }, 10000); // Poll every 10 seconds

    return () => clearInterval(interval);
  }, [jobStatus]);

  // Poll for logs
  useEffect(() => {
    if (!jobStatus || jobStatus.status === 2) { // Finished
      return;
    }

    const interval = setInterval(async () => {
      try {
        const newLogs = await consensusApi.getLogs(jobStatus.runId);
        setLogs(newLogs);
      } catch (err) {
        console.error('Error polling logs:', err);
      }
    }, 10000); // Poll every 10 seconds

    return () => clearInterval(interval);
  }, [jobStatus]);

  const handleSubmitPrompt = async (prompt: string) => {
    setIsSubmitting(true);
    setError('');
    setLogs([]);
    setHtml('');
    setJobStatus(null);

    try {
      const status = await consensusApi.startJob(prompt);
      setJobStatus(status);

      // Immediately fetch logs after starting the job
      const initialLogs = await consensusApi.getLogs(status.runId);
      setLogs(initialLogs);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to start job');
    } finally {
      setIsSubmitting(false);
    }
  };

  const isJobRunning = !!(jobStatus && jobStatus.status !== 2); // Not Finished
  const isJobFinished = !!(jobStatus && jobStatus.status === 2); // Finished

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          py: 4,
        }}
      >
        <Container maxWidth="lg">
          <Box
            sx={{
              bgcolor: 'background.paper',
              borderRadius: 4,
              boxShadow: 3,
              p: 4,
              border: '1px solid',
              borderColor: 'divider',
            }}
          >
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
              {/* Header */}
              <Box sx={{ textAlign: 'center', mb: 2 }}>
                <Typography variant="h3" component="h1" sx={{ fontWeight: 600, mb: 1 }}>
                  Consensus Builder
                </Typography>
                <Typography variant="body1" color="text.secondary">
                  Submit a prompt to generate consensus from multiple AI agents
                </Typography>
              </Box>

              {/* Error Alert */}
              {error && (
                <Alert severity="error" onClose={() => setError('')}>
                  {error}
                </Alert>
              )}

              {/* Prompt Input */}
              <PromptInput
                onSubmit={handleSubmitPrompt}
                disabled={isSubmitting || isJobRunning}
              />

              {/* Loading State */}
              {isSubmitting && (
                <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                  <CircularProgress />
                </Box>
              )}

              {/* Job Running - Show Logs */}
              {isJobRunning && (
                <Box>
                  <Typography variant="h6" sx={{ mb: 2 }}>
                    Processing... (Run ID: {jobStatus.runId})
                  </Typography>
                  <LogViewer logs={logs} />
                </Box>
              )}

              {/* Job Finished - Show Result */}
              {isJobFinished && html && (
                <ResultViewer html={html} logs={logs} runId={jobStatus.runId} />
              )}
            </Box>
          </Box>
        </Container>
      </Box>
    </ThemeProvider>
  );
}

export default App;
