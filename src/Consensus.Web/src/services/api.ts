import type { PromptRequest, JobStatusModel, LogEntryModel } from '../types/api';

// In production, use same origin (served from API). In development, use env variable or default.
const API_BASE_URL = import.meta.env.PROD 
  ? '' 
  : (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000');

class ConsensusApiService {
  private baseUrl: string;

  constructor() {
    this.baseUrl = API_BASE_URL;
  }

  /**
   * Start a new consensus job
   */
  async startJob(prompt: string): Promise<JobStatusModel> {
    const request: PromptRequest = { prompt };
    
    const response = await fetch(`${this.baseUrl}/api/consensus/start`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to start job');
    }

    return response.json();
  }

  /**
   * Get the status of a job
   */
  async getJobStatus(runId: string): Promise<JobStatusModel> {
    const response = await fetch(`${this.baseUrl}/api/consensus/${runId}/status`);

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to get job status');
    }

    return response.json();
  }

  /**
   * Get logs for a job
   */
  async getLogs(runId: string): Promise<LogEntryModel[]> {
    const response = await fetch(`${this.baseUrl}/api/consensus/${runId}/logs`);

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to get logs');
    }

    return response.json();
  }

  /**
   * Get HTML output for a job
   */
  async getHtml(runId: string): Promise<string> {
    const response = await fetch(`${this.baseUrl}/api/consensus/${runId}/html`);

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to get HTML');
    }

    return response.text();
  }
}

export const consensusApi = new ConsensusApiService();
