import type { PromptRequest, JobStatusModel, LogEntryModel } from '../types/api';

// In production, use same origin (served from API). In development, use env variable or default.
const API_BASE_URL = import.meta.env.PROD
  ? ''
  : (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000');

// Helper function to extract error message from response
async function getErrorMessage(response: Response, defaultMessage: string): Promise<string> {
  const contentType = response.headers.get('content-type');

  try {
    if (contentType && contentType.includes('application/json')) {
      const error = await response.json();
      return error.message || error.title || defaultMessage;
    } else {
      const text = await response.text();
      return text || defaultMessage;
    }
  } catch {
    // If parsing fails, use default message
    return defaultMessage;
  }
}

class ConsensusApiService {
  private baseUrl: string;

  constructor() {
    this.baseUrl = API_BASE_URL;
  }

  /**
   * Start a new consensus job
   */
  async startJob(prompt: string, cheatcode?: string): Promise<JobStatusModel> {
    const request: PromptRequest = { prompt };

    // Build URL with optional cheatcode query parameter
    const url = new URL(`${this.baseUrl}/api/consensus/start`, window.location.origin);
    if (cheatcode) {
      url.searchParams.append('cheatcode', cheatcode);
    }

    const response = await fetch(url.toString(), {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const message = await getErrorMessage(response, 'Failed to start job');
      throw new Error(message);
    }

    return response.json();
  }

  /**
   * Get the status of a job
   */
  async getJobStatus(runId: string): Promise<JobStatusModel> {
    const response = await fetch(`${this.baseUrl}/api/consensus/${runId}/status`);

    if (!response.ok) {
      const message = await getErrorMessage(
        response,
        response.status === 404 ? 'Response not found' : 'Failed to get job status'
      );
      throw new Error(message);
    }

    return response.json();
  }

  /**
   * Get logs for a job
   */
  async getLogs(runId: string): Promise<LogEntryModel[]> {
    const response = await fetch(`${this.baseUrl}/api/consensus/${runId}/logs`);

    if (!response.ok) {
      const message = await getErrorMessage(
        response,
        response.status === 404 ? 'Response not found' : 'Failed to get logs'
      );
      throw new Error(message);
    }

    return response.json();
  }

  /**
   * Get HTML output for a job
   */
  async getHtml(runId: string): Promise<string> {
    const response = await fetch(`${this.baseUrl}/api/consensus/${runId}/html`);

    if (!response.ok) {
      const message = await getErrorMessage(
        response,
        response.status === 404 ? 'Response not found' : 'Failed to get HTML'
      );
      throw new Error(message);
    }

    return response.text();
  }
}

export const consensusApi = new ConsensusApiService();
