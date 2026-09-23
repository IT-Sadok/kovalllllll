import axios from 'axios';

interface ApiErrorResponse {
  message?: unknown;
  Message?: unknown;
  detail?: unknown;
  title?: unknown;
}

export const getErrorMessage = (error: unknown, fallback: string): string => {
  if (axios.isAxiosError<ApiErrorResponse>(error)) {
    const data = error.response?.data;
    // ProblemDetails carries the specific reason in `detail`; `title` is only the generic status label.
    const message = data?.message ?? data?.Message ?? data?.detail ?? data?.title;

    if (typeof message === 'string' && message.trim()) {
      return message;
    }
  }

  if (error instanceof Error && error.message.trim()) {
    return error.message;
  }

  return fallback;
};
