import { useMutation, useQueryClient } from '@tanstack/react-query';
import { setErrorResolved } from '../../api/errorLogs';

export function useSetErrorResolved() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, resolved }: { id: string; resolved: boolean }) => setErrorResolved(id, resolved),
    onSuccess: () => {
      // A resolved row can leave the "unresolved" filter's result set entirely (and the sort
      // order is unresolved-first), so a targeted cache patch would still need a full page
      // re-derivation in the common case - simplest correct behavior is just refetching.
      queryClient.invalidateQueries({ queryKey: ['errorLogs'] });
      queryClient.invalidateQueries({ queryKey: ['errorLogDetail'] });
    },
  });
}
