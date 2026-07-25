import { useQuery } from '@tanstack/react-query';
import { fetchCrawlHistory } from '../../api/crawl';
import type { CrawlPipelineName } from '../../api/crawlTypes';

export function useCrawlHistory(
  pipeline: CrawlPipelineName,
  from: string,
  to: string,
  page: number,
  pageSize: number,
  providers: string[] = [],
) {
  return useQuery({
    queryKey: ['crawlHistory', pipeline, from, to, page, pageSize, providers],
    queryFn: () => fetchCrawlHistory({ pipeline, from, to, count: pageSize, skip: page * pageSize, providers }),
    // Same cross-pipeline guard as useCrawlReport - an empty page (no runs in a fresh pipeline's
    // window) has no [0].pipeline to check, so it falls back to "no placeholder" safely rather
    // than assuming a match.
    placeholderData: (previous) => (previous === undefined || previous.length === 0 || previous[0].pipeline === pipeline ? previous : undefined),
  });
}
