import axios from 'axios';

interface ApiErrorResponse {
  message?: unknown;
  Message?: unknown;
  title?: unknown;
}

export const getErrorMessage = (error: unknown, fallback: string): string => {
  if (axios.isAxiosError<ApiErrorResponse>(error)) {
    const data = error.response?.data;
    const message = data?.message ?? data?.Message ?? data?.title;

    if (typeof message === 'string' && message.trim()) {
      return message;
    }
  }

  if (error instanceof Error && error.message.trim()) {
    return error.message;
  }

  return fallback;
};
