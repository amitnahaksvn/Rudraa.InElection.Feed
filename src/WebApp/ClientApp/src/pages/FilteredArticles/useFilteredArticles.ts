import { useQuery } from '@tanstack/react-query';
import { fetchFilteredArticles } from '../../api/filteredArticles';
import type { ArticleSourceType } from '../../api/newsTypes';

export interface FilteredArticlesFilters {
  provider: string | null;
  sourceType: ArticleSourceType | null;
  category: string | null;
}

// `page` here is 0-based (matching MUI's TablePagination), converted to the 1-based page the
// backend expects (see WebPlatform.Endpoints.FilteredArticles.GetFilteredArticles).
export function useFilteredArticles(page: number, pageSize: number, filters: FilteredArticlesFilters) {
  return useQuery({
    queryKey: ['filteredArticles', page, pageSize, filters],
    queryFn: () =>
      fetchFilteredArticles({
        page: page + 1,
        pageSize,
        provider: filters.provider ?? undefined,
        sourceType: filters.sourceType ?? undefined,
        category: filters.category ?? undefined,
      }),
    placeholderData: (previous) => previous,
  });
}
