import { useState, useEffect } from 'react';
import { PromptInput } from './components/PromptInput';
import { LogViewer } from './components/LogViewer';
import { ResultViewer } from './components/ResultViewer';
import { Header } from './components/Header';
import { consensusApi } from './services/api';
import type { JobStatusModel, LogEntryModel } from './types/api';

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
            <div className="p-4 bg-red-50 border border-red-200 rounded-lg flex items-center justify-between">
              <span className="text-red-800">{error}</span>
              <button 
                onClick={() => setError('')}
                className="text-red-600 hover:text-red-800 font-bold cursor-pointer font-sans"
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
          {isSubmitting && (
            <div className="flex justify-center py-8">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
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
