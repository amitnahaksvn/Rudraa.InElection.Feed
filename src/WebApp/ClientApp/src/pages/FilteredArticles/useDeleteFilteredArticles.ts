import { useMutation, useQueryClient } from '@tanstack/react-query';
import { deleteFilteredArticles } from '../../api/filteredArticles';

// One call for both the per-row delete button and multi-select bulk delete, mirroring
// News Feed's own useDeleteArticles.
export function useDeleteFilteredArticles() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (ids: string[]) => deleteFilteredArticles(ids),
    // A low-value diagnostic log, not a business record - a plain refetch of whichever page is
    // currently shown is proportionate here, unlike the News Feed page's more careful in-place
    // cache surgery for real article deletes. Provider/category filter options are invalidated
    // too, since deleting the last row under a given provider/category should drop it from those
    // dropdowns.
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['filteredArticles'] });
      queryClient.invalidateQueries({ queryKey: ['filteredArticleProviders'] });
      queryClient.invalidateQueries({ queryKey: ['filteredArticleCategories'] });
    },
  });
}
