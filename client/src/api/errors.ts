import axios from 'axios';
import type { ApiResponse } from '../types';

export const getErrorMessage = (error: unknown, fallback: string): string => {
  if (axios.isAxiosError<ApiResponse>(error)) {
    const message = error.response?.data?.errors?.[0]?.message;

    if (typeof message === 'string' && message.trim()) {
      return message;
    }
  }

  if (error instanceof Error && error.message.trim()) {
    return error.message;
  }

  return fallback;
};
