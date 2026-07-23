import { useQuery } from '@tanstack/react-query';
import { fetchFilteredArticleProviders } from '../../api/filteredArticles';

export function useFilteredArticleProviders() {
  return useQuery({ queryKey: ['filteredArticleProviders'], queryFn: fetchFilteredArticleProviders });
}
