import type { AxiosResponse } from 'axios';
import type { ApiResponse, PagedResult } from '../types';

export const unwrap = <T>(response: AxiosResponse<ApiResponse<T>>): T => response.data.data as T;

export const unwrapPaged = <T>(response: AxiosResponse<ApiResponse<T[]>>): PagedResult<T> => {
  const { data, pagination } = response.data;

  return {
    items: data ?? [],
    totalCount: pagination?.totalCount ?? 0,
    page: pagination?.pageNumber ?? 1,
    pageSize: pagination?.pageSize ?? 0,
  };
};
