import type { PagedResult } from './types';
import type { ArticleSourceType } from './newsTypes';
import type { FilteredArticle } from './filteredArticlesTypes';
import { throwIfNotOk } from './httpUtils';

export interface FilteredArticlesQuery {
  page: number;
  pageSize: number;
  provider?: string;
  sourceType?: ArticleSourceType;
  category?: string;
}

export async function fetchFilteredArticles(query: FilteredArticlesQuery): Promise<PagedResult<FilteredArticle>> {
  const params = new URLSearchParams({ page: String(query.page), pageSize: String(query.pageSize) });
  if (query.provider) {
    params.set('provider', query.provider);
  }
  if (query.sourceType) {
    params.set('sourceType', query.sourceType);
  }
  if (query.category) {
    params.set('category', query.category);
  }
  const response = await fetch(`/api/filtered-articles?${params}`);
  await throwIfNotOk(response);
  return response.json();
}

export async function fetchFilteredArticleProviders(): Promise<string[]> {
  const response = await fetch('/api/filtered-articles/providers');
  await throwIfNotOk(response);
  return response.json();
}

export async function fetchFilteredArticleCategories(): Promise<string[]> {
  const response = await fetch('/api/filtered-articles/categories');
  await throwIfNotOk(response);
  return response.json();
}

/** Deletes one or more filtered-article rows by id - one call for both the per-row delete button and multi-select bulk delete. Returns how many were actually found and deleted. */
export async function deleteFilteredArticles(ids: string[]): Promise<number> {
  const response = await fetch('/api/filtered-articles', {
    method: 'DELETE',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ ids }),
  });
  await throwIfNotOk(response);
  return response.json();
}
