import { useState, useEffect } from 'react';
import {
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
import { Header } from './components/Header';
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
  const [currentPrompt, setCurrentPrompt] = useState<string>('');

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
    setCurrentPrompt(prompt);

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

  const handleReset = () => {
    setJobStatus(null);
    setLogs([]);
    setHtml('');
    setError('');
    setIsSubmitting(false);
    setCurrentPrompt('');
  };

  const isJobRunning = !!(jobStatus && jobStatus.status !== 2); // Not Finished
  const isJobFinished = !!(jobStatus && jobStatus.status === 2); // Finished

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Box
        sx={{
          minHeight: '100vh',
          height: isJobFinished ? '100vh' : 'auto',
          display: 'flex',
          alignItems: isJobFinished ? 'flex-start' : 'center',
          justifyContent: 'center',
          px: 3,
          py: isJobFinished ? 3 : 0,
        }}
      >
        <Box sx={{ 
          width: '100%', 
          maxWidth: '1040px', 
          mx: 'auto',
          height: isJobFinished ? '100%' : 'auto',
        }}>
          <Box sx={{ 
            display: 'flex', 
            flexDirection: 'column', 
            gap: 3,
            height: isJobFinished ? '100%' : 'auto',
          }}>
            {/* Header - Show only on processing page */}
            {isJobRunning && <Header align="left" />}

            {/* Header with subtitle - Only show when not finished and not running */}
            {!isJobFinished && !isJobRunning && (
              <Box sx={{ textAlign: 'center', mb: 2 }}>
                <Typography 
                  variant="h3" 
                  component="h1" 
                  sx={{ 
                    fontWeight: 200, 
                    fontFamily: '"Inter", "Segoe UI", "Roboto", sans-serif',
                    mb: 1 
                  }}
                >
                  consensus
                </Typography>
                <Typography variant="body1" color="text.secondary">
                  Submit a prompt to generate consensus from multiple AI agents
                </Typography>
              </Box>
            )}

            {/* Error Alert */}
            {error && (
              <Alert severity="error" onClose={() => setError('')}>
                {error}
              </Alert>
            )}

            {/* Prompt Input - Show when not finished and not running */}
            {!isJobFinished && !isJobRunning && (
              <PromptInput
                onSubmit={handleSubmitPrompt}
                disabled={isSubmitting || isJobRunning}
                value={currentPrompt}
                onChange={setCurrentPrompt}
              />
            )}

            {/* Show prompt as blockquote when running */}
            {isJobRunning && currentPrompt && (
              <Box 
                sx={{ 
                  borderLeft: '4px solid #e0e0e0',
                  paddingLeft: 2,
                  fontStyle: 'italic',
                  color: 'text.secondary',
                  backgroundColor: '#f9f9f9',
                  padding: 2,
                  borderRadius: 1,
                  mb: 2,
                  textAlign: 'left'
                }}
              >
                <Typography variant="body1" sx={{ fontSize: '1.125rem' }}>
                  "{currentPrompt}"
                </Typography>
              </Box>
            )}

            {/* Loading State */}
            {isSubmitting && (
              <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                <CircularProgress />
              </Box>
            )}

            {/* Job Running - Show Logs */}
            {isJobRunning && (
              <Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                  <Typography variant="h6">
                    Processing...
                  </Typography>
                  <CircularProgress size={24} />
                </Box>
                <LogViewer logs={logs} />
              </Box>
            )}

            {/* Job Finished - Show Result */}
            {isJobFinished && html && (
              <ResultViewer html={html} logs={logs} runId={jobStatus.runId} onReset={handleReset} />
            )}
          </Box>
        </Box>
      </Box>
    </ThemeProvider>
  );
}

export default App;
