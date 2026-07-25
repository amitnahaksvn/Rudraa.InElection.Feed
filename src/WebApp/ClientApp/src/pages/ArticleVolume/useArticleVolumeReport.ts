import { useQuery } from '@tanstack/react-query';
import { fetchArticleVolumeReport } from '../../api/crawl';

export function useArticleVolumeReport(pipeline: 'Rss' | 'Api', from: string, to: string, providers: string[] = []) {
  return useQuery({
    queryKey: ['articleVolumeReport', pipeline, from, to, providers],
    queryFn: () => fetchArticleVolumeReport(pipeline, from, to, providers),
    // Same cross-pipeline placeholder guard as useCrawlReport - avoids briefly showing RSS data
    // mislabeled under the API tab (or vice versa) while the new pipeline's fetch is in flight.
    placeholderData: (previous) => (previous?.pipeline === pipeline ? previous : undefined),
  });
}
