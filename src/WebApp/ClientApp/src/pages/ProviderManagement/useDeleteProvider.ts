import { useMutation, useQueryClient } from '@tanstack/react-query';
import { deleteProvider } from '../../api/providers';
import type { CrawlPipelineName } from '../../api/providerTypes';

export function useDeleteProvider(pipeline: CrawlPipelineName) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (provider: string) => deleteProvider(pipeline, provider),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [pipeline === 'Api' ? 'apiProviders' : 'rssProviders'] }),
  });
}
