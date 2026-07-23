import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createFeed, deleteFeed, updateFeed, type CreateFeedFields, type UpdateFeedFields } from '../../api/providers';
import type { CrawlPipelineName } from '../../api/providerTypes';

// Invalidates the whole provider list for the affected pipeline rather than trying to patch the
// nested feed/endpoint array in the cache in place - simpler, and Provider Management isn't
// updated often enough for a full refetch to be a real cost.
function listQueryKey(pipeline: CrawlPipelineName) {
  return [pipeline === 'Api' ? 'apiProviders' : 'rssProviders'];
}

export function useCreateFeed(pipeline: CrawlPipelineName) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (fields: CreateFeedFields) => createFeed(fields),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: listQueryKey(pipeline) }),
  });
}

export function useUpdateFeed(pipeline: CrawlPipelineName) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, fields }: { id: string; fields: UpdateFeedFields }) => updateFeed(id, fields),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: listQueryKey(pipeline) }),
  });
}

export function useDeleteFeed(pipeline: CrawlPipelineName) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteFeed(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: listQueryKey(pipeline) }),
  });
}
