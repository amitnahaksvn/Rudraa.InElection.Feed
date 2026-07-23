import { useMutation } from '@tanstack/react-query';
import { testRssFeed } from '../../api/providers';

export function useTestRssFeed() {
  return useMutation({
    mutationFn: (feedId: string) => testRssFeed(feedId),
  });
}
