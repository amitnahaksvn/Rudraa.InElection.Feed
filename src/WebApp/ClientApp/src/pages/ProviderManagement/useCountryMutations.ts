import { useMutation, useQueryClient } from '@tanstack/react-query';
import { deleteCountry, upsertCountry } from '../../api/providers';
import type { CrawlPipelineName } from '../../api/providerTypes';

// Country Enabled folds into every provider's own computed Enabled (see GetRssProvidersQuery/
// GetApiProvidersQuery), so a country change also invalidates the provider list, not just the
// country list.
export function useUpsertCountry(pipeline: CrawlPipelineName) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ name, enabled }: { name: string; enabled: boolean }) => upsertCountry(pipeline, name, enabled),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['countries', pipeline] });
      queryClient.invalidateQueries({ queryKey: [pipeline === 'Api' ? 'apiProviders' : 'rssProviders'] });
    },
  });
}

export function useDeleteCountry(pipeline: CrawlPipelineName) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (name: string) => deleteCountry(pipeline, name),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['countries', pipeline] });
      queryClient.invalidateQueries({ queryKey: [pipeline === 'Api' ? 'apiProviders' : 'rssProviders'] });
    },
  });
}
