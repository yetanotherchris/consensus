import { useState, useEffect } from 'react';
import { PromptInput } from './components/PromptInput';
import { LogViewer } from './components/LogViewer';
import { ResultViewer } from './components/ResultViewer';
import { Header } from './components/Header';
import { consensusApi } from './services/api';
import type { JobStatusModel, LogEntryModel } from './types/api';

// Helper function to validate UUID format
const isValidUuid = (str: string): boolean => {
  const uuidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
  return uuidRegex.test(str);
};

// Helper function to get runId from URL path (/answer/{runId})
const getRunIdFromUrl = (): string | null => {
  const pathMatch = window.location.pathname.match(/\/answer\/([0-9a-f-]+)$/i);
  return pathMatch && isValidUuid(pathMatch[1]) ? pathMatch[1] : null;
};

function App() {
  const [jobStatus, setJobStatus] = useState<JobStatusModel | null>(null);
  const [logs, setLogs] = useState<LogEntryModel[]>([]);
  const [html, setHtml] = useState<string>('');
  const [error, setError] = useState<string>('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [currentPrompt, setCurrentPrompt] = useState<string>('');
  const [isLoadingFromUrl, setIsLoadingFromUrl] = useState(false);

  // Load response from URL path on mount
  useEffect(() => {
    const runIdFromUrl = getRunIdFromUrl();
    if (!runIdFromUrl) {
      return;
    }

    const loadFromUrl = async () => {
      setIsLoadingFromUrl(true);
      setError('');

      try {
        // Fetch job status
        const status = await consensusApi.getJobStatus(runIdFromUrl);
        setJobStatus(status);

        // Fetch logs
        const jobLogs = await consensusApi.getLogs(runIdFromUrl);
        setLogs(jobLogs);

        // If job is finished, fetch HTML
        if (status.status === 2) {
          const htmlResult = await consensusApi.getHtml(runIdFromUrl);
          setHtml(htmlResult);
        }
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : 'Failed to load response';
        setError(errorMessage);
        window.history.replaceState({}, '', '/'); // Update URL to home without adding history entry
      } finally {
        setIsLoadingFromUrl(false);
      }
    };

    loadFromUrl();
  }, []); // Run only on mount

  // Handle browser back/forward navigation
  useEffect(() => {
    const handlePopState = () => {
      const runIdFromUrl = getRunIdFromUrl();

      if (!runIdFromUrl) {
        // User navigated back to home, reset the app
        setJobStatus(null);
        setLogs([]);
        setHtml('');
        setError('');
        setIsSubmitting(false);
        setCurrentPrompt('');
      } else if (!jobStatus || jobStatus.runId !== runIdFromUrl) {
        // User navigated to a different runId, reload
        window.location.reload();
      }
    };

    window.addEventListener('popstate', handlePopState);
    return () => window.removeEventListener('popstate', handlePopState);
  }, [jobStatus]);

  // Poll for job status
  useEffect(() => {
    if (!jobStatus || jobStatus.status === 2) { // Finished
      return;
    }

    const interval = setInterval(async () => {
      try {
        const status = await consensusApi.getJobStatus(jobStatus.runId);
        setJobStatus(status);

        // If job is finished, fetch the HTML result and update URL
        if (status.status === 2) { // Finished
          const htmlResult = await consensusApi.getHtml(status.runId);
          setHtml(htmlResult);
          window.history.pushState({}, '', `/answer/${status.runId}`); // Update URL with runId
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
      window.history.pushState({}, '', `/answer/${status.runId}`); // Update URL with new runId

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
    window.history.pushState({}, '', '/'); // Navigate back to home
  };

  const isJobRunning = !!(jobStatus && jobStatus.status !== 2); // Not Finished
  const isJobFinished = !!(jobStatus && jobStatus.status === 2); // Finished

  return (
    <div className={`min-h-screen ${isJobFinished ? 'h-screen' : 'h-auto'} flex ${isJobFinished ? 'items-start' : 'items-center'} justify-center px-6 ${isJobFinished ? 'py-6' : 'py-0'} bg-[#f7f7f8]`}>
      <div className={`w-full max-w-[1040px] mx-auto ${isJobFinished ? 'h-full' : 'h-auto'}`}>
        <div className={`flex flex-col gap-6 ${isJobFinished ? 'h-full' : 'h-auto'}`}>
          {/* Header - Show only on processing page */}
          {isJobRunning && <Header align="left" />}

          {/* Header with subtitle - Only show when not finished and not running */}
          {!isJobFinished && !isJobRunning && (
            <div className="text-center mb-4">
              <h1 className="text-3xl font-extralight font-sans mb-2">
                consensus
              </h1>
              <p className="text-base text-gray-600">
                Generate a consensus from multiple AI models
              </p>
            </div>
          )}

          {/* Error Alert */}
          {error && (
            <div className="w-full max-w-[700px] mx-auto p-4 bg-gray-100 border border-gray-300 rounded-lg flex items-center justify-between">
              <span className="text-gray-700">{error}</span>
              <button
                onClick={() => setError('')}
                className="text-gray-500 hover:text-gray-700 font-bold cursor-pointer font-sans"
              >
                ×
              </button>
            </div>
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
            <div className="border-l-4 border-gray-300 pl-4 italic text-gray-600 bg-gray-50 p-4 rounded mb-4 text-left">
              <p className="text-lg">
                "{currentPrompt}"
              </p>
            </div>
          )}

          {/* Loading State */}
          {(isSubmitting || isLoadingFromUrl) && (
            <div className="flex flex-col items-center justify-center py-8 gap-3">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
              {isLoadingFromUrl && (
                <p className="text-sm text-gray-600">Loading response...</p>
              )}
            </div>
          )}

          {/* Job Running - Show Logs */}
          {isJobRunning && (
            <div>
              <div className="flex items-center gap-4 mb-4">
                <h2 className="text-xl font-semibold">
                  Processing...
                </h2>
                <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
              </div>
              <LogViewer logs={logs} />
            </div>
          )}

          {/* Job Finished - Show Result */}
          {isJobFinished && html && (
            <ResultViewer html={html} logs={logs} runId={jobStatus.runId} onReset={handleReset} />
          )}
        </div>
      </div>
    </div>
  );
}

export default App;
