export interface PromptRequest {
  prompt: string;
}

export interface JobStatusModel {
  runId: string;
  status: 0 | 1 | 2; // NotStarted, Running, Finished
  createdAt: string;
  startedAt?: string;
  finishedAt?: string;
}



export interface LogEntryModel {
  timestamp: string;
  message: string;
}

export const JobStatusEnum = {
  NotStarted: 0,
  Running: 1,
  Finished: 2,
} as const;
