import type { ErrorLogDetail, ErrorLogFilters, PagedResult, ErrorLogSummary } from './types';

const PAGE_SIZE = 20;

async function throwIfNotOk(response: Response): Promise<Response> {
  if (!response.ok) {
    throw new Error(`Request failed: ${response.status} ${response.statusText}`);
  }
  return response;
}

function buildQuery(page: number, filters: ErrorLogFilters): string {
  const params = new URLSearchParams();
  params.set('page', String(page));
  params.set('pageSize', String(PAGE_SIZE));

  if (filters.status === 'unresolved') params.set('isResolved', 'false');
  if (filters.status === 'resolved') params.set('isResolved', 'true');
  if (filters.provider.trim()) params.set('provider', filters.provider.trim());
  if (filters.country.trim()) params.set('country', filters.country.trim());
  if (filters.source.trim()) params.set('source', filters.source.trim());
  if (filters.search.trim()) params.set('search', filters.search.trim());

  return params.toString();
}

export async function fetchErrorLogs(page: number, filters: ErrorLogFilters): Promise<PagedResult<ErrorLogSummary>> {
  const response = await fetch(`/api/errors?${buildQuery(page, filters)}`);
  await throwIfNotOk(response);
  return response.json();
}

export async function fetchErrorLogDetail(id: string): Promise<ErrorLogDetail> {
  const response = await fetch(`/api/errors/${encodeURIComponent(id)}`);
  await throwIfNotOk(response);
  return response.json();
}

export async function setErrorResolved(id: string, resolved: boolean): Promise<void> {
  const response = await fetch(`/api/errors/${encodeURIComponent(id)}/resolved`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ resolved }),
  });
  await throwIfNotOk(response);
}
