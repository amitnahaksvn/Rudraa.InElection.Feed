import { useInfiniteQuery } from '@tanstack/react-query';
import { fetchNewsFeed } from '../../api/news';
import type { ArticleSourceType } from '../../api/newsTypes';

const PAGE_SIZE = 20;

export function useNewsFeed(sourceType: ArticleSourceType, country: string | null) {
  return useInfiniteQuery({
    queryKey: ['newsFeed', sourceType, country],
    queryFn: ({ pageParam }) =>
      fetchNewsFeed({ sourceType, country: country ?? undefined, skip: pageParam, count: PAGE_SIZE }),
    initialPageParam: 0,
    // A page shorter than requested means there's nothing left to fetch - the same "next button
    // disables once a page comes back short" signal used elsewhere in this app (see the crawl
    // report's RecentRunsTable), just driving infinite scroll instead of a pager.
    getNextPageParam: (lastPage, allPages) => (lastPage.length < PAGE_SIZE ? undefined : allPages.length * PAGE_SIZE),
  });
}
