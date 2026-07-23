import { useQuery } from '@tanstack/react-query';
import { fetchFilteredArticleCategories } from '../../api/filteredArticles';

export function useFilteredArticleCategories() {
  return useQuery({ queryKey: ['filteredArticleCategories'], queryFn: fetchFilteredArticleCategories });
}
