import { useMutation } from '@tanstack/react-query';
import { testApiEndpoint } from '../../api/providers';

export function useTestApiEndpoint() {
  return useMutation({
    mutationFn: (endpointId: string) => testApiEndpoint(endpointId),
  });
}
